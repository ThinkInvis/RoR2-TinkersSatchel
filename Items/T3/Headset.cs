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
	public class Headset : Item<Headset> {

		////// Item Data //////

		public override string displayName => "H3AD-53T";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) => "Your Utility skill builds a stunning static charge.";
		protected override string GetDescString(string langid = null) => $"After activating your <style=cIsUtility>Utility skill</style>, the next {procCount} enemies <style=cStack>(+{stackProcCount} per stack)</style> your path crosses will <style=cIsDamage>take {Pct(baseDamagePct)} damage</style> <style=cStack>(+{Pct(stackDamagePct)} per stack)</style> and be <style=cIsUtility>stunned for {stunDuration} seconds</style>.";
		protected override string GetLoreString(string langid = null) => "You finally found a fall too long for your long fall boots. While you could probably jury-rig the parts back together for their original purpose, this is the perfect opportunity for... innovation.\r\n\r\nUse your head.";



		////// Config //////

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Fraction of base damage dealt by H3AD-53T procs with a single item copy.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float baseDamagePct { get; private set; } = 4f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Fraction of base damage dealt by H3AD-53T procs per additional item copy.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float stackDamagePct { get; private set; } = 1.5f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Stun time (in seconds) dealt by H3AD-53T procs.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float stunDuration { get; private set; } = 5f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Procs added on Utility skill cast with a single item copy.",
			AutoConfigFlags.None, 1, int.MaxValue)]
		public int procCount { get; private set; } = 5;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Procs added on Utility skill cast per additional item copy.",
			AutoConfigFlags.None, 1, int.MaxValue)]
		public int stackProcCount { get; private set; } = 3;



		////// Other Fields/Properties //////
		
		public BuffDef headsetBuff { get; private set; }

		internal UnlockableDef unlockable;

		private const float HITBOX_RADIUS = 3f;
		private const float HIT_INTERVAL = 0.5f;

		readonly HashSet<GameObject> recentlyHit = new HashSet<GameObject>();




		////// TILER2 Module Setup //////

		public Headset() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Headset.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/headsetIcon.png");
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			headsetBuff = ScriptableObject.CreateInstance<BuffDef>();
			headsetBuff.buffColor = new Color(0.5f, 0.575f, 0.95f);
			headsetBuff.canStack = true;
			headsetBuff.isDebuff = false;
			headsetBuff.name = "TKSATVoidGoldenGear";
			headsetBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ShockNearby/texBuffTeslaIcon.tif")
				.WaitForCompletion();
			ContentAddition.AddBuffDef(headsetBuff);

			unlockable = UnlockableAPI.AddUnlockable<TkSatHeadsetAchievement>();
			LanguageAPI.Add("TKSAT_HEADSET_ACHIEVEMENT_NAME", "You Broke It");
			LanguageAPI.Add("TKSAT_HEADSET_ACHIEVEMENT_DESCRIPTION", "Kill a boss with a maximum damage H3AD-5T v2 explosion.");

			itemDef.unlockableDef = unlockable;
		}

		public override void Install() {
			base.Install();

			On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            On.RoR2.GenericSkill.OnExecute += GenericSkill_OnExecute;
		}

        public override void Uninstall() {
			base.Uninstall();

			On.RoR2.CharacterBody.FixedUpdate -= CharacterBody_FixedUpdate;
			On.RoR2.GenericSkill.OnExecute -= GenericSkill_OnExecute;
		}



        ////// Hooks //////
        #region Hooks
        private void GenericSkill_OnExecute(On.RoR2.GenericSkill.orig_OnExecute orig, GenericSkill self) {
			if(self.characterBody && self.characterBody.skillLocator
				&& self.characterBody.skillLocator.FindSkillSlot(self) == SkillSlot.Utility) {
				var count = GetCount(self.characterBody);
				if(count > 0) {
					var cpt = self.characterBody.GetComponent<HeadsetComponent>();
					if(!cpt) cpt = self.characterBody.gameObject.AddComponent<HeadsetComponent>();
					cpt.hitsRemaining = procCount + stackProcCount * (count - 1);
				}
			}
			orig(self);
		}

		private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
			orig(self);
			if(!NetworkServer.active || !self) {
				return;
			}
			var cpt = self.GetComponent<HeadsetComponent>();
			if(!cpt) cpt = self.gameObject.AddComponent<HeadsetComponent>();
			var atkTeam = TeamComponent.GetObjectTeam(self.gameObject);
			var count = GetCount(self);
			if(count <= 0) {
				cpt.hitsRemaining = 0;
			} else if(cpt.hitsRemaining > 0) {
				var p1 = cpt.previousPos;
				var p2 = self.transform.position;

				Collider[] res = Physics.OverlapCapsule(p1, p2, HITBOX_RADIUS, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);

				if(self.healthComponent) recentlyHit.Add(self.healthComponent.gameObject);

				foreach(var hit in res) {
					if(!hit) continue;
					var hurtbox = hit.GetComponent<HurtBox>();
					if(!hurtbox || !hurtbox.healthComponent
						|| recentlyHit.Contains(hurtbox.healthComponent.gameObject)
						|| !FriendlyFireManager.ShouldSplashHitProceed(hurtbox.healthComponent, atkTeam)) continue;
					recentlyHit.Add(hurtbox.healthComponent.gameObject);
					cpt.hitsRemaining--;
					if(cpt.hitsRemaining <= 0)
						break;
				}

				cpt.previousPos = p2;
			}

			if(recentlyHit.Count > 1) {
				cpt.procCooldown--;
				if(cpt.procCooldown <= 0) {
					cpt.procCooldown = HIT_INTERVAL;

                    var damageInfo = new DamageInfo {
                        attacker = self.gameObject,
                        inflictor = self.gameObject,
                        crit = false,
                        damage = self.damage * (baseDamagePct + stackDamagePct * (count - 1)),
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = DamageType.Generic,
                        force = Vector3.zero,
                        procCoefficient = 0f
                    };

                    foreach(var hit in recentlyHit) {
						if(!hit) continue;
						var hurtbox = hit.GetComponent<HurtBox>();
						if(!hurtbox || !hurtbox.healthComponent
							|| !FriendlyFireManager.ShouldSplashHitProceed(hurtbox.healthComponent, atkTeam)) continue;
						damageInfo.position = hit.transform.position;
						hurtbox.healthComponent.TakeDamage(damageInfo);
						var ssoh = hurtbox.healthComponent.GetComponent<SetStateOnHurt>();
						if(ssoh && ssoh.canBeStunned) {
							ssoh.SetStun(stunDuration);
						}
					}
					recentlyHit.Clear();
				}
			}

			int currBuffStacks = self.GetBuffCount(headsetBuff);
			if(cpt.hitsRemaining != currBuffStacks)
				self.SetBuffCount(headsetBuff.buffIndex, cpt.hitsRemaining);
		}
        #endregion
    }

    public class HeadsetComponent : MonoBehaviour {
        public Vector3 previousPos;
        public int hitsRemaining;
		public float procCooldown = 0f;
	}

	public class TkSatHeadsetAchievement : RoR2.Achievements.BaseAchievement, IModdedUnlockableDataProvider {
		public string AchievementIdentifier => "TKSAT_HEADSET_ACHIEVEMENT_ID";
		public string UnlockableIdentifier => "TKSAT_HEADSET_UNLOCKABLE_ID";
		public string PrerequisiteUnlockableIdentifier => "";
		public string AchievementNameToken => "TKSAT_HEADSET_ACHIEVEMENT_NAME";
		public string AchievementDescToken => "TKSAT_HEADSET_ACHIEVEMENT_DESCRIPTION";
		public string UnlockableNameToken => Headset.instance.nameToken;

		public Sprite Sprite => TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/headsetIcon.png");

		public System.Func<string> GetHowToUnlock => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new[] {
			Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

		public System.Func<string> GetUnlocked => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new[] {
			Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

		public override void OnInstall() {
			base.OnInstall();
            On.EntityStates.Headstompers.HeadstompersFall.DoStompExplosionAuthority += HeadstompersFall_DoStompExplosionAuthority;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		}

        public override void OnUninstall() {
			base.OnUninstall();
			On.EntityStates.Headstompers.HeadstompersFall.DoStompExplosionAuthority -= HeadstompersFall_DoStompExplosionAuthority;
			On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
		}

		bool currentDamageIsHeadstompers = false;
		private void HeadstompersFall_DoStompExplosionAuthority(On.EntityStates.Headstompers.HeadstompersFall.orig_DoStompExplosionAuthority orig, EntityStates.Headstompers.HeadstompersFall self) {
			if(self.body && self.body.inventory && self.body.inventory.GetItemCount(RoR2Content.Items.FallBoots) > 0) {
				var dist = Mathf.Max(0f, self.initialY - self.body.footPosition.y);
				if(dist >= EntityStates.Headstompers.HeadstompersFall.maxDistance) {
					currentDamageIsHeadstompers = true;
				}
			}
			orig(self);
			currentDamageIsHeadstompers = false;
		}

		private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
			orig(self, damageInfo);
			if(currentDamageIsHeadstompers && self && !self.alive && self.body && self.body.isChampion) {
				Grant();
			}
		}
	}
}