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
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

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
				childName = "Hat",
				localPos = new Vector3(0.00014F, 0.02091F, -0.01037F),
				localAngles = new Vector3(0F, 0F, 0F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CaptainBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Head",
				localPos = new Vector3(0.01348F, 0.20851F, 0.01613F),
				localAngles = new Vector3(338.5889F, 344.4471F, 18.27958F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CommandoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Head",
				localPos = new Vector3(0.00504F, 0.24495F, 0.01284F),
				localAngles = new Vector3(0F, 0F, 0F),
				localScale = new Vector3(0.4F, 0.4F, 0.4F)
			});
			displayRules.Add("CrocoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Head",
				localPos = new Vector3(-0.28014F, 1.14252F, 1.01174F),
				localAngles = new Vector3(7.38828F, 76.73765F, 113.5267F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("EngiBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "HeadCenter",
				localPos = new Vector3(0.00426F, 0.03067F, 0.00167F),
				localAngles = new Vector3(20.40316F, 317.4127F, 347.4659F),
				localScale = new Vector3(0.4F, 0.4F, 0.4F)
			});
			displayRules.Add("HuntressBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "HeadCenter",
				localPos = new Vector3(0.00449F, 0.01231F, -0.04551F),
				localAngles = new Vector3(340.5608F, 314.136F, 15.74504F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("LoaderBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Head",
				localPos = new Vector3(-0.00138F, 0.13256F, 0.00663F),
				localAngles = new Vector3(356.0245F, 32.47648F, 347.6966F),
				localScale = new Vector3(0.35F, 0.35F, 0.35F)
			});
			displayRules.Add("MageBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "HeadCenter",
				localPos = new Vector3(-0.01204F, 0.05384F, -0.08743F),
				localAngles = new Vector3(18.0701F, 269.2518F, 73.73166F),
				localScale = new Vector3(0.25F, 0.25F, 0.25F)
			});
			displayRules.Add("MercBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "HeadCenter",
				localPos = new Vector3(0.0015F, 0.009F, -0.00323F),
				localAngles = new Vector3(357.8618F, 317.1738F, 359.0508F),
				localScale = new Vector3(0.325F, 0.325F, 0.325F)
			});
			displayRules.Add("ToolbotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Head",
				localPos = new Vector3(0.05776F, 2.43676F, 0.83379F),
				localAngles = new Vector3(75.36604F, 69.47244F, 84.15331F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("TreebotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "FlowerBase",
				localPos = new Vector3(0.15641F, 1.50413F, 0.01745F),
				localAngles = new Vector3(16.90676F, 25.22818F, 351.2803F),
				localScale = new Vector3(1.5F, 1.5F, 1.5F)
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