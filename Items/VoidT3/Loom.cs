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
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

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
                childName = "GunR",
                localPos = new Vector3(-0.38451F, 0.0469F, 0.02783F),
                localAngles = new Vector3(285.653F, 207.5622F, 333.654F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            }, new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "GunL",
                localPos = new Vector3(0.38474F, 0.07447F, -0.02576F),
                localAngles = new Vector3(295.1262F, 204.6935F, 333.7171F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "LowerArmR",
                localPos = new Vector3(-1.37052F, 6.40454F, 0.33925F),
                localAngles = new Vector3(37.42741F, 311.1624F, 268.5512F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "CannonHeadR",
                localPos = new Vector3(-0.09853F, 0.65828F, -0.02925F),
                localAngles = new Vector3(357.861F, 24.97364F, 107.2528F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Muzzle",
                localPos = new Vector3(-0.10289F, -0.12588F, 0.28935F),
                localAngles = new Vector3(66.76357F, 100.0334F, 201.5606F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechHandR",
                localPos = new Vector3(-0.07617F, 0.49021F, 0.04608F),
                localAngles = new Vector3(28.23814F, 65.30327F, 71.42656F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MuzzleRight",
                localPos = new Vector3(-0.09397F, 0.09521F, 0.07436F),
                localAngles = new Vector3(53.29458F, 56.86463F, 159.9321F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(-0.4703F, 0.12868F, -0.17719F),
                localAngles = new Vector3(339.133F, 322.2292F, 330.3595F),
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