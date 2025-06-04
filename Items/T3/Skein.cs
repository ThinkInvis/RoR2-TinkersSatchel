using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
	public class Skein : Item<Skein> {

		////// Item Data //////

		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility, ItemTag.Damage });

		protected override string[] GetDescStringArgs(string langID = null) => new[] {
			highMassFrac.ToString("0%"), lowMassFracMove.ToString("0%"), lowMassFracAttack.ToString("0%"), massChangeDuration.ToString("N0"), graceRate.ToString("0%"), hitIcd.ToString("N2")
		};



		////// Config //////

		[AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Maximum damage/knockback to block per stack (hyperbolic).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float highMassFrac { get; private set; } = 0.5f;

		[AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
		[AutoConfig("Maximum speed to add per stack (linear).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float lowMassFracAttack { get; private set; } = 0.3f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Maximum speed to add per stack (linear).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float lowMassFracMove { get; private set; } = 0.4f;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 30f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Time required to reach maximum buff, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float massChangeDuration { get; private set; } = 5f;

		[AutoConfigRoOSlider("{0:P0}", 0f, 4f)]
		[AutoConfig("Rate at which buffs decay, relative to the charge rate.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float graceRate { get; private set; } = 2f;

		[AutoConfigRoOSlider("{0:N0} s", 0f, 10f)]
		[AutoConfig("Time after being hit to force movement state.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float hitIcd { get; private set; } = 0.25f;



		////// Other Fields/Properties //////

		public BuffDef speedBuff { get; private set; }
		public BuffDef resistBuff { get; private set; }
		internal static UnlockableDef unlockable;
		public GameObject idrPrefab { get; private set; }



		////// TILER2 Module Setup //////

		public Skein() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Skein.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/skeinIcon.png");
			idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Skein.prefab");
		}

		public override void SetupModifyItemDef() {
			base.SetupModifyItemDef();

			CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

			#region ItemDisplayRule Definitions

			/// Survivors ///
			displayRules.Add("Bandit2Body", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.16662F, 0.23603F, -0.2328F),
				localAngles = new Vector3(283.0797F, 259.6789F, 87.20558F),
				localScale = new Vector3(0.23F, 0.23F, 0.23F)
			});
			displayRules.Add("CaptainBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "Head",
				localPos = new Vector3(-0.21289F, 0.14872F, -0.10543F),
				localAngles = new Vector3(53.26719F, 171.0046F, 197.6588F),
				localScale = new Vector3(0.12F, 0.12F, 0.12F)
			});
			displayRules.Add("CommandoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(-0.00118F, 0.178F, -0.28754F),
				localAngles = new Vector3(359.0837F, 93.76512F, 332.8747F),
				localScale = new Vector3(0.5F, 0.5F, 0.5F)
			});
			displayRules.Add("CrocoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "SpineChest1",
				localPos = new Vector3(0.06337F, 1.26103F, -0.69081F),
				localAngles = new Vector3(2.39033F, 269.3893F, 47.91893F),
				localScale = new Vector3(4F, 4F, 4F)
			});
			displayRules.Add("EngiBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "CannonHeadR",
				localPos = new Vector3(-0.18005F, 0.30757F, 0.18712F),
				localAngles = new Vector3(86.21277F, 286.0484F, 278.2581F),
				localScale = new Vector3(0.25F, 0.25F, 0.25F)
			});
			displayRules.Add("HuntressBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "BowHinge1R",
				localPos = new Vector3(-0.00901F, 0.15326F, -0.09964F),
				localAngles = new Vector3(85.83322F, 347.1405F, 331.8423F),
				localScale = new Vector3(0.45F, 0.45F, 0.45F)
			});
			displayRules.Add("LoaderBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "MechUpperArmR",
				localPos = new Vector3(-0.15892F, 0.20699F, -0.01099F),
				localAngles = new Vector3(274.9109F, 12.15518F, 58.75174F),
				localScale = new Vector3(0.45F, 0.45F, 0.45F)
			});
			displayRules.Add("MageBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.00715F, 0.37187F, -0.16102F),
				localAngles = new Vector3(358.1779F, 265.442F, 97.43761F),
				localScale = new Vector3(0.4F, 0.4F, 0.4F)
			});
			displayRules.Add("MercBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.00421F, 0.18537F, -0.31537F),
				localAngles = new Vector3(358.024F, 269.6058F, 22.69296F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("ToolbotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.15033F, 1.48124F, -2.13107F),
				localAngles = new Vector3(359.6307F, 85.72971F, 269.808F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("TreebotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "PlatformBase",
				localPos = new Vector3(0.14563F, 0.83292F, 0.7369F),
				localAngles = new Vector3(357.9048F, 91.49792F, 254.4164F),
				localScale = new Vector3(1.22F, 1.2F, 1.2F)
			});
			displayRules.Add("RailgunnerBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "Backpack",
				localPos = new Vector3(-0.33099F, -0.03768F, -0.0103F),
				localAngles = new Vector3(273.8419F, 305.2371F, 146.085F),
				localScale = new Vector3(0.25F, 0.25F, 0.25F)
			});
			displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefabAddress = new AssetReferenceGameObject(""),
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.00086F, 0.31781F, -0.34701F),
				localAngles = new Vector3(0.06097F, 89.70734F, 340.4815F),
				localScale = new Vector3(0.45F, 0.45F, 0.45F)
			});
			#endregion
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			speedBuff = ScriptableObject.CreateInstance<BuffDef>();
			speedBuff.buffColor = Color.white;
			speedBuff.canStack = true;
			speedBuff.isDebuff = false;
			speedBuff.name = "TKSATSkeinSpeed";
			speedBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/skeinSpeedBuffIcon.png");
			ContentAddition.AddBuffDef(speedBuff);

			resistBuff = ScriptableObject.CreateInstance<BuffDef>();
			resistBuff.buffColor = Color.white;
			resistBuff.canStack = true;
			resistBuff.isDebuff = false;
			resistBuff.name = "TKSATSkeinResist";
			resistBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/skeinResistBuffIcon.png");
			ContentAddition.AddBuffDef(resistBuff);

			unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
			unlockable.cachedName = $"TkSat_{name}Unlockable";
			unlockable.sortScore = 200;
			unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/skeinIcon.png");
			ContentAddition.AddUnlockableDef(unlockable);
			itemDef.unlockableDef = unlockable;
		}

		public override void Install() {
			base.Install();
			CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
			RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		}

        public override void Uninstall() {
			base.Uninstall();
			CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
			RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
			On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
		}



		////// Hooks //////
		
		private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
			var hasItem = GetCount(body) > 0;
			var component = body.GetComponent<SkeinTracker>();
			if(hasItem && !component)
				body.gameObject.AddComponent<SkeinTracker>();
			else if(!hasItem && component) {
				GameObject.Destroy(component);
				body.SetBuffCount(Skein.instance.speedBuff.buffIndex, 0);
				body.SetBuffCount(Skein.instance.resistBuff.buffIndex, 0);
			}
		}

		private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
			if(!sender) return;
			var count = GetCount(sender);
			var cpt = sender.GetComponent<SkeinTracker>();
			if(count > 0 && cpt) {
                args.moveSpeedMultAdd += cpt.GetMovementScalar() * count * lowMassFracMove;
				args.attackSpeedMultAdd += cpt.GetMovementScalar() * count * lowMassFracAttack;
            }
		}

		private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
			if(self && self.body) {
				var count = GetCount(self.body);
				var cpt = self.GetComponent<SkeinTracker>();
				if(count > 0 && cpt) {
					var fac = 1f - (1f - Mathf.Pow(highMassFrac, count)) * cpt.GetResistanceScalar();
					damageInfo.damage *= fac;
					if(damageInfo.canRejectForce)
						damageInfo.force *= fac;
					cpt.forcedMovingStopwatch += hitIcd;
                }
            }
			orig(self, damageInfo);
		}
	}

	[RequireComponent(typeof(CharacterBody))]
	public class SkeinTracker : MonoBehaviour {
		const float RECALC_TICK_RATE = 0.2f;

		float movingStopwatch = 0f;
		float stoppedStopwatch = 0f;
		float recalcStopwatch = 0f;
		public float forcedMovingStopwatch = 0f;

		Vector3 prevPos;

		CharacterBody body;

		public float GetMovementScalar() {
			return Mathf.Clamp01(movingStopwatch / Skein.instance.massChangeDuration);
        }

		public float GetResistanceScalar() {
			return Mathf.Clamp01(stoppedStopwatch / Skein.instance.massChangeDuration);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void Awake() {
			body = GetComponent<CharacterBody>();
			prevPos = body.transform.position;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void FixedUpdate() {
			if(!body || !NetworkServer.active) return;
			float minMove = 0.1f * Time.fixedDeltaTime;
			if((body.transform.position - prevPos).sqrMagnitude <= minMove * minMove && forcedMovingStopwatch <= 0f) {
				stoppedStopwatch += Time.fixedDeltaTime;
				if(stoppedStopwatch > Skein.instance.massChangeDuration) stoppedStopwatch = Skein.instance.massChangeDuration;
				movingStopwatch -= Time.fixedDeltaTime * Skein.instance.graceRate;
				if(movingStopwatch < 0f) movingStopwatch = 0f;
			} else {
				if(forcedMovingStopwatch > 0f) forcedMovingStopwatch -= Time.fixedDeltaTime;
				movingStopwatch += Time.fixedDeltaTime;
				if(movingStopwatch > Skein.instance.massChangeDuration) movingStopwatch = Skein.instance.massChangeDuration;
				stoppedStopwatch -= Time.fixedDeltaTime * Skein.instance.graceRate;
				if(stoppedStopwatch < 0f) stoppedStopwatch = 0f;
			}

			prevPos = body.transform.position;

			recalcStopwatch -= Time.fixedDeltaTime;
			if(recalcStopwatch <= 0f) {
				recalcStopwatch = RECALC_TICK_RATE;
				body.statsDirty = true;
				body.SetBuffCount(Skein.instance.resistBuff.buffIndex,
					Mathf.FloorToInt(GetResistanceScalar() * 100));
				body.SetBuffCount(Skein.instance.speedBuff.buffIndex,
					Mathf.FloorToInt(GetMovementScalar() * 100));
			}
        }
    }

	[RegisterAchievement("TkSat_Skein", "TkSat_SkeinUnlockable", "", 3u)]
	public class TkSatSkeinAchievement : RoR2.Achievements.BaseAchievement {
		public override void OnInstall() {
			base.OnInstall();
            On.RoR2.RoR2Application.Update += RoR2Application_Update;
		}

        public override void OnUninstall() {
			base.OnUninstall();
			On.RoR2.RoR2Application.Update -= RoR2Application_Update;
		}

		float stopwatch = 0f;
		private void RoR2Application_Update(On.RoR2.RoR2Application.orig_Update orig, RoR2Application self) {
			orig(self);
			stopwatch -= Time.deltaTime;
			if(stopwatch <= 0f) {
				stopwatch = 1f;
				if(userProfile.HasUnlockable(Defib.unlockable)
					&& userProfile.HasUnlockable(ShootToHeal.unlockable)
					&& userProfile.HasUnlockable(Pinball.unlockable)
					&& userProfile.HasUnlockable(Lodestone.unlockable))
					Grant();
			}
		}
	}
}