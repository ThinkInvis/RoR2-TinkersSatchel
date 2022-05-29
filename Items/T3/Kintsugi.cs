using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using System.Linq;

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
		[AutoConfig("Stat bonus per T3/Boss item per stack.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float tier3Bonus { get; private set; } = 0.05f;



		////// Other Fields/Properties //////
		
		internal static UnlockableDef unlockable;



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

		public override void Install() {
			base.Install();
			RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
		}

        public override void Uninstall() {
			base.Uninstall();
			RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
		}



		////// Public API //////

		public static int GetConsumedItemCount(Inventory inventory) {
			if(!inventory) return 0;
			int count = 0;
			count += inventory.GetItemCount(RoR2Content.Items.ExtraLifeConsumed);
			count += inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoidConsumed);
			count += inventory.GetItemCount(DLC1Content.Items.FragileDamageBonusConsumed);
			count += inventory.GetItemCount(DLC1Content.Items.HealingPotionConsumed);
			count += inventory.GetItemCount(DLC1Content.Items.RegeneratingScrapConsumed);
			count += inventory.GetItemCount(RoR2Content.Items.ScrapWhite);
			count += inventory.GetItemCount(RoR2Content.Items.ScrapGreen);
			count += inventory.GetItemCount(RoR2Content.Items.ScrapRed);
			count += inventory.GetItemCount(RoR2Content.Items.ScrapYellow);
			return count;
		}

		public static (int t1, int t2, int t3plus) GetConsumedItemCountByTier(Inventory inventory) {
			if(!inventory) return (0, 0, 0);
			int count1 = 0;
			int count2 = 0;
			int count3 = 0;
			count3 += inventory.GetItemCount(RoR2Content.Items.ExtraLifeConsumed);
			count3 += inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoidConsumed);
			count1 += inventory.GetItemCount(DLC1Content.Items.FragileDamageBonusConsumed);
			count1 += inventory.GetItemCount(DLC1Content.Items.HealingPotionConsumed);
			count2 += inventory.GetItemCount(DLC1Content.Items.RegeneratingScrapConsumed);
			count1 += inventory.GetItemCount(RoR2Content.Items.ScrapWhite);
			count2 += inventory.GetItemCount(RoR2Content.Items.ScrapGreen);
			count3 += inventory.GetItemCount(RoR2Content.Items.ScrapRed);
			count3 += inventory.GetItemCount(RoR2Content.Items.ScrapYellow);
			return (count1, count2, count3);
		}



		////// Hooks //////

		private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
			if(!sender) return;
			var multCount = GetCount(sender);
			var (t1, t2, t3plus) = GetConsumedItemCountByTier(sender.inventory);
			var totalBonus = multCount * (t1 * tier1Bonus + t2 * tier2Bonus + t3plus * tier3Bonus);
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