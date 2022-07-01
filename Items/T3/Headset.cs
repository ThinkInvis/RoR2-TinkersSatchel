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

		[AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Fraction of base damage dealt by H3AD-53T procs with a single item copy.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float baseDamagePct { get; private set; } = 4f;

		[AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Fraction of base damage dealt by H3AD-53T procs per additional item copy.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float stackDamagePct { get; private set; } = 1.5f;

		[AutoConfigRoOSlider("{0:N0} s", 0f, 30f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Stun time (in seconds) dealt by H3AD-53T procs.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float stunDuration { get; private set; } = 5f;

		[AutoConfigRoOIntSlider("{0:N0}", 0, 20)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Procs added on Utility skill cast with a single item copy.",
			AutoConfigFlags.None, 1, int.MaxValue)]
		public int procCount { get; private set; } = 5;

		[AutoConfigRoOIntSlider("{0:N0}", 0, 20)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Procs added on Utility skill cast per additional item copy.",
			AutoConfigFlags.None, 1, int.MaxValue)]
		public int stackProcCount { get; private set; } = 3;



		////// Other Fields/Properties //////
		
		public BuffDef headsetBuff { get; private set; }

		internal UnlockableDef unlockable;

		private const float HITBOX_RADIUS = 3f;
		private const float HIT_INTERVAL = 0.5f;
		public GameObject idrPrefab { get; private set; }




		////// TILER2 Module Setup //////

		public Headset() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Headset.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/headsetIcon.png");
			idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Headset.prefab");
		}

		public override void SetupModifyItemDef() {
			base.SetupModifyItemDef();

			CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

			#region ItemDisplayRule Definitions

			/// Survivors ///
			displayRules.Add("Bandit2Body", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.22045F, -0.06626F, 0.11193F),
				localAngles = new Vector3(359.0299F, 357.3219F, 25.2928F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CaptainBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.38728F, 0.00965F, -0.06446F),
				localAngles = new Vector3(31.87035F, 332.9695F, 3.18838F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CommandoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.23353F, -0.00868F, -0.08696F),
				localAngles = new Vector3(27.00084F, 326.5775F, 4.93487F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CrocoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(-0.6739F, -1.47899F, 1.63122F),
				localAngles = new Vector3(354.4511F, 7.12517F, 355.0916F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("EngiBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(0.24835F, 0.13692F, 0.12219F),
				localAngles = new Vector3(19.74273F, 338.7649F, 343.2596F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("HuntressBody", new ItemDisplayRule {
				childName = "Stomach",
				localPos = new Vector3(0.17437F, -0.01902F, 0.11239F),
				localAngles = new Vector3(14.62809F, 338.0782F, 18.2589F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F),
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab
			});
			displayRules.Add("LoaderBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "MechBase",
				localPos = new Vector3(0.28481F, -0.22564F, -0.12889F),
				localAngles = new Vector3(0.98176F, 51.91312F, 23.00177F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("MageBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(0.16876F, -0.10376F, 0.02998F),
				localAngles = new Vector3(357.5521F, 355.006F, 105.9485F),
				localScale = new Vector3(0.25F, 0.25F, 0.25F)
			});
			displayRules.Add("MercBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "ThighR",
				localPos = new Vector3(-0.08794F, 0.03176F, -0.06409F),
				localAngles = new Vector3(350.6662F, 317.2625F, 21.97947F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("ToolbotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(2.33895F, -0.34548F, 0.80107F),
				localAngles = new Vector3(311.4177F, 7.89006F, 354.1869F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("TreebotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "PlatformBase",
				localPos = new Vector3(0.75783F, -0.10773F, 0.00385F),
				localAngles = new Vector3(308.2326F, 10.8672F, 329.0782F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("RailgunnerBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Backpack",
				localPos = new Vector3(0.28636F, -0.3815F, -0.06912F),
				localAngles = new Vector3(352.4358F, 63.85439F, 6.83272F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.17554F, -0.13447F, -0.0436F),
				localAngles = new Vector3(15.08189F, 9.51543F, 15.89409F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			#endregion
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			headsetBuff = ScriptableObject.CreateInstance<BuffDef>();
			headsetBuff.buffColor = new Color(0.5f, 0.575f, 0.95f);
			headsetBuff.canStack = true;
			headsetBuff.isDebuff = false;
			headsetBuff.name = "TKSATHeadset";
			headsetBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ShockNearby/texBuffTeslaIcon.tif")
				.WaitForCompletion();
			ContentAddition.AddBuffDef(headsetBuff);

			var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
			var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
			unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
			unlockable.cachedName = $"TkSat_{name}Unlockable";
			unlockable.sortScore = 200;
			unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/headsetIcon.png");
			ContentAddition.AddUnlockableDef(unlockable);
			LanguageAPI.Add(achiNameToken, "You Broke It");
			LanguageAPI.Add(achiDescToken, "Kill a boss with a maximum damage H3AD-5T v2 explosion.");
			itemDef.unlockableDef = unlockable;
		}

		public override void Install() {
			base.Install();

			On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
		}

        public override void Uninstall() {
			base.Uninstall();

			On.RoR2.CharacterBody.FixedUpdate -= CharacterBody_FixedUpdate;
			On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
		}



		////// Hooks //////
		#region Hooks

		private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill) {
			orig(self, skill);
			if(!NetworkServer.active) return;
			if(self && self.skillLocator
				&& self.skillLocator.FindSkillSlot(skill) == SkillSlot.Utility) {
				var count = GetCount(self);
				if(count > 0) {
					var cpt = self.GetComponent<HeadsetComponent>();
					if(!cpt) cpt = self.gameObject.AddComponent<HeadsetComponent>();
					cpt.hitsRemaining = procCount + stackProcCount * (count - 1);
				}
			}
		}

		private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
			orig(self);
			if(!NetworkServer.active || !self) {
				return;
			}
			var cpt = self.gameObject.GetComponent<HeadsetComponent>();
			if(!cpt) cpt = self.gameObject.AddComponent<HeadsetComponent>();
			var count = GetCount(self);
			if(count <= 0) {
				cpt.hitsRemaining = 0;
			} else if(cpt.hitsRemaining > 0) {
				var atkTeam = TeamComponent.GetObjectTeam(self.gameObject);
				var p1 = cpt.previousPos;
				var p2 = self.transform.position;

				var extendedRadius = HITBOX_RADIUS * ExtendoArms.GetRangeMultiplier(self);

				Collider[] res;
				if(p1 == p2)
					res = Physics.OverlapSphere(p1, extendedRadius, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore);
				else
					res = Physics.OverlapCapsule(p1, p2, extendedRadius, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore);

				var damageInfo = new DamageInfo {
					attacker = self.gameObject,
					inflictor = self.gameObject,
					crit = false,
					damage = self.damage * (baseDamagePct + stackDamagePct * (count - 1)) * ExtendoArms.GetDamageMultiplier(self),
					damageColorIndex = DamageColorIndex.Item,
					damageType = DamageType.Generic,
					force = Vector3.zero,
					procCoefficient = 1f
				};

				foreach(var hit in res) {
					if(!hit) continue;
					var hurtbox = hit.GetComponent<HurtBox>();
					if(!hurtbox
						|| !hurtbox.healthComponent
						|| hurtbox.healthComponent == self.healthComponent
						|| !FriendlyFireManager.ShouldSplashHitProceed(hurtbox.healthComponent, atkTeam)) continue;
					var icd = hurtbox.healthComponent.gameObject.GetComponent<HeadsetICDComponent>();
					if(!icd) icd = hurtbox.healthComponent.gameObject.AddComponent<HeadsetICDComponent>();
					if(Time.fixedTime - icd.lastHit < HIT_INTERVAL) continue;
					icd.lastHit = Time.fixedTime;

					damageInfo.position = hit.transform.position;
					hurtbox.healthComponent.TakeDamage(damageInfo);
					var ssoh = hurtbox.healthComponent.GetComponent<SetStateOnHurt>();
					if(ssoh && ssoh.canBeStunned) {
						ssoh.SetStun(stunDuration);
					}

					cpt.hitsRemaining--;
					if(cpt.hitsRemaining <= 0)
						break;
				}

				cpt.previousPos = p2;
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
	}

	public class HeadsetICDComponent : MonoBehaviour {
		public float lastHit = 0f;
    }

	[RegisterAchievement("TkSat_Headset", "TkSat_HeadsetUnlockable", "")]
	public class TkSatHeadsetAchievement : RoR2.Achievements.BaseAchievement {
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