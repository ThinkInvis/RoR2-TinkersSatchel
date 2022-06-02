using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using System.Linq;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
	public class Kintsugi : Item<Kintsugi> {

		////// Item Data //////

		public override string displayName => "Kintsugi";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility, ItemTag.Damage });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) =>
			"Your broken/consumed/scrapped items increase all your stats.";
		protected override string GetDescString(string langid = null) =>
			$"Each of your broken, consumed, or scrapped items increases <style=cIsUtility>ALL stats</style> based on its tier <style=cStack>(stacks linearly)</style>: 1% for Common items, 3% for Uncommon items, and 5% for Rare/Boss items.";
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
		private readonly HashSet<ItemDef> validItems = new HashSet<ItemDef>();



		////// TILER2 Module Setup //////

		public Kintsugi() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Kintsugi.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/kintsugiIcon.png");
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
				switch(k) {
					case ItemTier.Tier1:
					case ItemTier.VoidTier1:
						totalBonus += tier1Bonus * v;
						break;
					case ItemTier.Tier2:
					case ItemTier.VoidTier2:
						totalBonus += tier2Bonus * v;
						break;
					default:
						totalBonus += tier3Bonus * v;
						break;
				}
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