using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
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
			"All your attacks become progressively slower and more powerful. <style=cIsVoid>Corrupts all Spacetime Skeins</style>.";
		protected override string GetDescString(string langid = null) =>
			$"Each <style=cIsDamage>combat skill</style> use gives you <style=cIsDamage>-{stackAttack:P1} attack speed and +{stackDamage:P1} damage</style> for {window} seconds <style=cStack>(+{window} s per stack)</style>, up to {maxStacks} times <style=cStack>(+{maxStacks} per stack)</style>. <style=cIsVoid>Corrupts all Spacetime Skeins</style>.";
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
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public Loom() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Loom.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/loomIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Loom.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MuzzleShotgun",
                localPos = new Vector3(0.0182F, -0.11949F, 0.17513F),
                localAngles = new Vector3(312.5091F, 15.56333F, 254.7946F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MuzzleGun",
                localPos = new Vector3(0.15787F, -0.04485F, 0.19995F),
                localAngles = new Vector3(275.6631F, 222.1807F, 70.54312F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
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
					itemDef1 = Skein.instance.itemDef,
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
			if(count > 0 && skill && skill.isCombatSkill && (skill.baseRechargeInterval > 0f || self.skillLocator.FindSkillSlot(skill) == SkillSlot.Primary)) {
				int oldCount = self.GetBuffCount(loomBuff);
				self.ClearTimedBuffs(loomBuff.buffIndex);
				self.SetBuffCount(loomBuff.buffIndex, 0);
				for(var i = 0; i < System.Math.Min(oldCount + 1, count * maxStacks); i++)
					self.AddTimedBuff(loomBuff, window, count * maxStacks);
			}
		}

		private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
			if(!sender) return;
			var buffCount = sender.GetBuffCount(loomBuff);
			args.attackSpeedMultAdd -= buffCount * stackAttack;
			args.damageMultAdd += buffCount * stackDamage;
		}
	}
}