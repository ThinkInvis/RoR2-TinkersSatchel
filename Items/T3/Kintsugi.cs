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

		public override string displayName => "Kintsugi";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility, ItemTag.Damage });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) =>
			"Your broken/consumed/scrapped items increase all your stats.";
		protected override string GetDescString(string langid = null) =>
			$"Each of your broken, consumed, or scrapped items increases <style=cIsUtility>ALL stats</style> based on its tier <style=cStack>(stacks linearly)</style>: {Pct(tier1Bonus)}% for Common items, {Pct(tier2Bonus)}% for Uncommon items, and {Pct(tier3Bonus)}% for Rare/Boss items.";
		protected override string GetLoreString(string langid = null) => "";



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
		[AutoConfig("Items to count towards Kintsugi, as a comma-delimited list of name tokens (will be automatically trimmed).", AutoConfigFlags.PreventNetMismatch)]
		public string validItemNameTokens { get; private set; } = "ITEM_SCRAPWHITE_NAME, ITEM_SCRAPGREEN_NAME, ITEM_SCRAPRED_NAME, ITEM_SCRAPYELLOW_NAME, ITEM_REGENERATINGSCRAP_NAME, ITEM_REGENERATINGSCRAPCONSUMED_NAME, ITEM_HEALINGPOTIONCONSUMED_NAME, ITEM_FRAGILEDAMAGEBONUSCONSUMED_NAME, ITEM_EXTRALIFEVOIDCONSUMED_NAME, ITEM_EXTRALIFECONSUMED_NAME";



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

			var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
			var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
			unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
			unlockable.cachedName = $"TkSat_{name}Unlockable";
			unlockable.sortScore = 200;
			unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/kintsugiIcon.png");
			ContentAddition.AddUnlockableDef(unlockable);
			LanguageAPI.Add(achiNameToken, "Planned Obsolescence");
			LanguageAPI.Add(achiDescToken, "Have at least 20 items, at least half of which are broken/consumed/scrapped.");
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
		}

        public override void Uninstall() {
			base.Uninstall();
			RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
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
			var nameTokens = validItemNameTokens.Split(',').Select(x => x.Trim());
			validItems.Clear();
			validItems.UnionWith(ItemCatalog.allItemDefs.Where(idef => nameTokens.Contains(idef.nameToken)));
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
			args.attackSpeedMultAdd += totalBonus;
			args.damageMultAdd += totalBonus;
			args.moveSpeedMultAdd += totalBonus;
			args.jumpPowerMultAdd += totalBonus;
			args.regenMultAdd += totalBonus;
			args.critAdd += totalBonus;
			args.armorAdd += totalBonus;
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