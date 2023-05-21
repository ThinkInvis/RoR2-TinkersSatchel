using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using System.Linq;
using System.Collections.Generic;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
	public class Kintsugi : Item<Kintsugi> {

		////// Item Data //////

		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility, ItemTag.Damage });

		protected override string[] GetDescStringArgs(string langID = null) => new[] {
			tier1Bonus.ToString("0%"), tier2Bonus.ToString("0%"), tier3Bonus.ToString("0%")
		};



		////// Config //////

		[AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Stat bonus per T1 item per stack.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float tier1Bonus { get; private set; } = 0.01f;

		[AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Stat bonus per T2 item per stack.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float tier2Bonus { get; private set; } = 0.03f;

		[AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Stat bonus per any other item (e.g. T3, Boss) per stack.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float tier3Bonus { get; private set; } = 0.05f;

		[AutoConfigRoOString()]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateStats)]
		[AutoConfig("Items to count towards Kintsugi, as a comma-delimited list of name tokens or internal names (will be automatically trimmed, prefix name tokens with @).", AutoConfigFlags.PreventNetMismatch)]
		public string validItemNameTokens { get; private set; } = "ScrapWhite, ScrapGreen, ScrapRed, ScrapYellow, RegeneratingScrap, RegeneratingScrapConsumed, HealingPotionConsumed, FragileDamageBonusConsumed, ExtraLifeVoidConsumed, ExtraLifeConsumed, TKSATKintsugi";

		[AutoConfigRoOIntSlider("{0:B7}", 0, 127)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Binary flag determining which stats to effect. 1: Attack Speed, 2: Damage, 4: Move Speed, 8: Jump Power, 16: Regen Mult., 32: Crit Chance, 64: Armor", AutoConfigFlags.PreventNetMismatch, 0, 127)]
		public int buffEffectFlags { get; private set; } = 127;


		////// Other Fields/Properties //////

		internal static UnlockableDef unlockable;
		private readonly HashSet<ItemDef> validItems = new();
		public GameObject idrPrefab { get; private set; }



		////// TILER2 Module Setup //////

		public Kintsugi() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Kintsugi.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/kintsugiIcon.png");
			idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Kintsugi.prefab");
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
				localPos = new Vector3(0.12567F, -0.08697F, 0.20119F),
				localAngles = new Vector3(312.9724F, 73.70127F, 16.40451F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CaptainBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.31281F, -0.00462F, 0.14926F),
				localAngles = new Vector3(351.1349F, 236.5051F, 26.06075F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CommandoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.23084F, -0.01033F, 0.08094F),
				localAngles = new Vector3(4.32157F, 286.1034F, 52.17296F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CrocoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "SpineStomach1",
				localPos = new Vector3(1.18663F, 3.31978F, 0.68969F),
				localAngles = new Vector3(343.7948F, 26.87664F, 353.0051F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("EngiBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(0.28395F, 0.07248F, -0.08704F),
				localAngles = new Vector3(354.6513F, 302.0988F, 211.8762F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("HuntressBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(0.20056F, -0.05875F, -0.07142F),
				localAngles = new Vector3(40.30397F, 17.12854F, 159.5616F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("LoaderBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "MechBase",
				localPos = new Vector3(0.19923F, -0.14524F, 0.25555F),
				localAngles = new Vector3(308.887F, 74.29995F, 36.2258F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("MageBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(0.14796F, 0.01064F, -0.16038F),
				localAngles = new Vector3(4.44165F, 294.8552F, 208.2559F),
				localScale = new Vector3(0.2F, 0.2F, 0.2F)
			});
			displayRules.Add("MercBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.2219F, 0.00249F, 0.09567F),
				localAngles = new Vector3(8.58844F, 225.0816F, 24.13489F),
				localScale = new Vector3(0.2F, 0.2F, 0.2F)
			});
			displayRules.Add("ToolbotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(2.1294F, -0.79742F, 1.86715F),
				localAngles = new Vector3(342.3761F, 258.168F, 58.53947F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("TreebotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "PlatformBase",
				localPos = new Vector3(0.72213F, 0.23556F, 0.26992F),
				localAngles = new Vector3(339.7497F, 310.3758F, 56.41169F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("RailgunnerBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(-0.03362F, -0.03905F, 0.18169F),
				localAngles = new Vector3(352.4358F, 63.85438F, 6.83272F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Center",
				localPos = new Vector3(0.20091F, -0.0092F, 0.16045F),
				localAngles = new Vector3(309.0165F, 121.6112F, 332.4587F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			#endregion
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
			unlockable.cachedName = $"TkSat_{name}Unlockable";
			unlockable.sortScore = 200;
			unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/kintsugiIcon.png");
			ContentAddition.AddUnlockableDef(unlockable);
			itemDef.unlockableDef = unlockable;
		}

        public override void SetupConfig() {
            base.SetupConfig();

			this.ConfigEntryChanged += (nv, args) => {
				if(args.target.boundProperty.Name == nameof(validItemNameTokens)) {
					UpdateValidItems();
                }
			};
        }

        public override void SetupCatalogReady() {
            base.SetupCatalogReady();
			UpdateValidItems();
        }

        public override void Install() {
			base.Install();
			RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
		}

        public override void Uninstall() {
			base.Uninstall();
			RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
			On.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
		}



		////// Public API //////

		public static bool GetIsItemValid(ItemDef item) {
			return Kintsugi.instance.validItems.Contains(item);
        }

		public static int GetConsumedItemCount(Inventory inventory) {
			if(!inventory) return 0;
			int count = 0;
			foreach(var idef in Kintsugi.instance.validItems) {
				count += inventory.GetItemCount(idef);
            }
			return count;
		}

		public static Dictionary<ItemTier, int> GetConsumedItemCountByTier(Inventory inventory) {
			var retv = new Dictionary<ItemTier, int>();
			if(!inventory) return retv;
			foreach(var idef in Kintsugi.instance.validItems) {
				var c = inventory.GetItemCount(idef);
				if(retv.ContainsKey(idef.tier))
					retv[idef.tier] += c;
				else
					retv.Add(idef.tier, c);
			}
			return retv;
		}



		////// Private API //////

		private void UpdateValidItems() {
			if(!ItemCatalog.availability.available) return;
			var nameTokens = validItemNameTokens.Split(',').Select(x => {
				bool isInternalName = x.Length > 0 && (x[0] == '@');
				return (content: (isInternalName ? x : x.Substring(1)).Trim(), isInternalName);
				});
			validItems.Clear();
			validItems.UnionWith(
				ItemCatalog.allItemDefs.Where(
					idef => nameTokens.Any(
						(kvp) => (kvp.isInternalName ? idef.name : idef.nameToken)
						== kvp.content
						)
					)
				);
		}



		////// Hooks //////

		private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
			if(!sender) return;
			var multCount = GetCount(sender);
			var consumedItems = GetConsumedItemCountByTier(sender.inventory);
			float totalBonus = 0;
			foreach(var (k, v) in consumedItems.Select(x => (x.Key, x.Value))) {
                totalBonus += k switch {
                    ItemTier.Tier1 or ItemTier.VoidTier1 => tier1Bonus * v,
                    ItemTier.Tier2 or ItemTier.VoidTier2 => tier2Bonus * v,
                    _ => tier3Bonus * v,
                };
            }
			totalBonus *= multCount;
			if((buffEffectFlags & 1) != 0) args.attackSpeedMultAdd += totalBonus;
			if((buffEffectFlags & 2) != 0) args.damageMultAdd += totalBonus;
			if((buffEffectFlags & 4) != 0) args.moveSpeedMultAdd += totalBonus;
			if((buffEffectFlags & 8) != 0) args.jumpPowerMultAdd += totalBonus;
			if((buffEffectFlags & 16) != 0) args.regenMultAdd += totalBonus;
			if((buffEffectFlags & 32) != 0) args.critAdd += totalBonus * 100;
			if((buffEffectFlags & 64) != 0) args.armorAdd += totalBonus * 100;
		}

		private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self) {
			orig(self);
		}
	}

	[RegisterAchievement("TkSat_Kintsugi", "TkSat_KintsugiUnlockable", "")]
	public class TkSatKintsugiAchievement : RoR2.Achievements.BaseAchievement {
		public override void OnInstall() {
			base.OnInstall();
			On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
		}

        public override void OnUninstall() {
			base.OnUninstall();
			On.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
		}

		private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self) {
			orig(self);
			if(this.localUser.cachedMaster != self) return;
			int count = Kintsugi.GetConsumedItemCount(self.inventory);
			int totalCount = self.inventory.itemStacks.Sum();
			if(totalCount >= 20 && count > Mathf.CeilToInt((float)totalCount / 2f))
				Grant();
		}
	}
}