using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
	public class ShrinkRay : Item<ShrinkRay> {

		////// Item Data //////

		public override string displayName => "Shrink Ray";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) => "Suppress a single target's non-primary skills and damage.";
		protected override string GetDescString(string langid = null) => $"Once every {icd:N1} seconds, hitting an enemy disables their <style=cIsUtility>non-primary skills</style> and reduces their <style=cIsDamage>damage</style> by 50% for {duration:N1} seconds <style=cStack>(+{duration:N1} seconds per stack)</style>.";
		protected override string GetLoreString(string langid = null) => "";



		////// Config //////

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Effect duration per stack.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float duration { get; private set; } = 3f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Internal cooldown of applying the effect. Does not stack.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float icd { get; private set; } = 2.5f;



		////// Other Fields/Properties //////

		public BuffDef shrinkDebuff { get; private set; }



		////// TILER2 Module Setup //////

		public ShrinkRay() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ShrinkRay.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/shrinkRayIcon.png");
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			shrinkDebuff = ScriptableObject.CreateInstance<BuffDef>();
			shrinkDebuff.buffColor = Color.white;
			shrinkDebuff.canStack = true;
			shrinkDebuff.isDebuff = true;
			shrinkDebuff.name = "TKSATShrink";
			shrinkDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texDifficultyEasyIcon.png")
				.WaitForCompletion();
			ContentAddition.AddBuffDef(shrinkDebuff);
		}

		public override void Install() {
			base.Install();

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
		}

        public override void Uninstall() {
			base.Uninstall();

			On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
			On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
			On.RoR2.CharacterBody.OnBuffFinalStackLost -= CharacterBody_OnBuffFinalStackLost;
			On.RoR2.CharacterBody.OnBuffFirstStackGained -= CharacterBody_OnBuffFirstStackGained;
		}



		////// Hooks //////

		private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
			orig(self, damageInfo, victim);
			if(NetworkServer.active && damageInfo != null && damageInfo.attacker) {
				var count = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
				if(count > 0) {
					var sricd = damageInfo.attacker.GetComponent<ShrinkRayICDComponent>();
					if(!sricd)
						sricd = damageInfo.attacker.AddComponent<ShrinkRayICDComponent>();
					if(Time.fixedTime - sricd.lastHit > icd) {
						sricd.lastHit = Time.fixedTime;
						var stsd = victim.GetComponent<ServerTimedSkillDisable>();
						if(!stsd) stsd = victim.AddComponent<ServerTimedSkillDisable>();
						stsd.ServerApply(duration * count, SkillSlot.Secondary);
						stsd.ServerApply(duration * count, SkillSlot.Utility);
						stsd.ServerApply(duration * count, SkillSlot.Special);
						if(victim.TryGetComponent<CharacterBody>(out var vbody)) {
							vbody.AddTimedBuff(shrinkDebuff, duration * count);
						}
					}
				}
			}
		}

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef) {
			orig(self, buffDef);
			if(self && buffDef == shrinkDebuff && self.modelLocator) {
				self.modelLocator.modelTransform.localScale *= 0.5f;
			}
		}

		private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef) {
			orig(self, buffDef);
			if(self && buffDef == shrinkDebuff && self.modelLocator) {
				self.modelLocator.modelTransform.localScale *= 2f;
			}
		}

		private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
			orig(self);
			if(self && self.HasBuff(shrinkDebuff)) {
				self.damage *= 0.5f;
			}
		}
	}

	public class ShrinkRayICDComponent : MonoBehaviour {
		public float lastHit = 0f;
    }
}