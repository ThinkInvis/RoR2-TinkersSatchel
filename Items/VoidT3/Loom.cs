using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
	public class Loom : Item<Loom> {

		////// Item Data //////

		public override string displayName => "Unraveling Loom";
		public override ItemTier itemTier => ItemTier.VoidTier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) =>
			"Combos become progressively heavier.";
		protected override string GetDescString(string langid = null) =>
			$"Each <style=cIsDamage>combat skill</style> use gives you <style=cIsDamage>-{stackAttack:P1} attack speed and +{stackDamage:P1} damage</style> for {window} seconds <style=cStack>(+{window} s per stack)</style>, up to {maxStacks} times <style=cStack>(+{maxStacks} per stack)</style>.";
		protected override string GetLoreString(string langid = null) => "";



		////// Config //////

		[AutoConfigRoOIntSlider("{0:N0}", 1, 10)]
		[AutoConfig("Maximum number of buff stacks per item stack.", AutoConfigFlags.None, 1, int.MaxValue)]
		public int maxStacks { get; private set; } = 10;

		[AutoConfigRoOSlider("{0:P1}", 0f, 0.999f)]
		[AutoConfig("Attack speed multiplier reduction per buff stack.", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
		public float stackAttack { get; private set; } = 0.05f;

		[AutoConfigRoOSlider("{0:P1}", 0f, 10f)]
		[AutoConfig("Damage multiplier addition per buff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float stackDamage { get; private set; } = 0.125f;

		[AutoConfigRoOSlider("{0:N0} s", 0f, 30f)]
		[AutoConfig("Time before effect expires.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float window { get; private set; } = 3f;



		////// Other Fields/Properties //////

		public BuffDef loomBuff { get; private set; }



		////// TILER2 Module Setup //////

		public Loom() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Loom.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/loomIcon.png");
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			loomBuff = ScriptableObject.CreateInstance<BuffDef>();
			loomBuff.buffColor = Color.white;
			loomBuff.canStack = true;
			loomBuff.isDebuff = false;
			loomBuff.name = "TKSATLoomBuff";
			loomBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/loomBuffIcon.png");
			ContentAddition.AddBuffDef(loomBuff);

			itemDef.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset")
				.WaitForCompletion();

			On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
				var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
				isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
				isp.relationships = new[] {new ItemDef.Pair {
					itemDef1 = Moustache.instance.itemDef,
					itemDef2 = itemDef
				}};
				orig(providers.Concat(new[] { isp }).ToArray());
			};
		}

		public override void Install() {
			base.Install();
			RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
		}

        public override void Uninstall() {
			base.Uninstall();
			RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
			On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
		}



		////// Hooks //////

		private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill) {
			orig(self, skill);
			var count = GetCount(self);
			if(count > 0 && skill && skill.isCombatSkill && (skill.baseRechargeInterval > 0f || self.skillLocator.FindSkillSlot(skill) == SkillSlot.Primary))
				self.AddTimedBuff(loomBuff, window, count * maxStacks);
		}

		private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
			if(!sender) return;
			var buffCount = sender.GetBuffCount(loomBuff);
			args.attackSpeedMultAdd -= buffCount * stackAttack;
			args.damageMultAdd += buffCount * stackDamage;
		}
	}
}