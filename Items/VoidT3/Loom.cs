﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
	public class Loom : Item<Loom> {

		////// Item Data //////

		public override ItemTier itemTier => ItemTier.VoidTier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            stackAttack.ToString("0.0%"), stackDamage.ToString("0.0%"), window.ToString("N1"), maxStacks.ToString("N0")
        };



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
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "MuzzleShotgun",
                localPos = new Vector3(0.0182F, -0.11949F, 0.17513F),
                localAngles = new Vector3(312.5091F, 15.56333F, 254.7946F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "MuzzleGun",
                localPos = new Vector3(0.15787F, -0.04485F, 0.19995F),
                localAngles = new Vector3(275.6631F, 222.1807F, 70.54312F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "GunR",
                localPos = new Vector3(-0.38451F, 0.0469F, 0.02783F),
                localAngles = new Vector3(285.653F, 207.5622F, 333.654F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            }, new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "GunL",
                localPos = new Vector3(0.38474F, 0.07447F, -0.02576F),
                localAngles = new Vector3(295.1262F, 204.6935F, 333.7171F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "LowerArmR",
                localPos = new Vector3(-1.37052F, 6.40454F, 0.33925F),
                localAngles = new Vector3(37.42741F, 311.1624F, 268.5512F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "CannonHeadR",
                localPos = new Vector3(-0.09853F, 0.65828F, -0.02925F),
                localAngles = new Vector3(357.861F, 24.97364F, 107.2528F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Muzzle",
                localPos = new Vector3(-0.10289F, -0.12588F, 0.28935F),
                localAngles = new Vector3(66.76357F, 100.0334F, 201.5606F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "MechHandR",
                localPos = new Vector3(-0.07617F, 0.49021F, 0.04608F),
                localAngles = new Vector3(28.23814F, 65.30327F, 71.42656F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "MuzzleRight",
                localPos = new Vector3(-0.09397F, 0.09521F, 0.07436F),
                localAngles = new Vector3(53.29458F, 56.86463F, 159.9321F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(-0.4703F, 0.12868F, -0.17719F),
                localAngles = new Vector3(339.133F, 322.2292F, 330.3595F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "LowerArmL",
                localPos = new Vector3(1.13055F, 11.41728F, -0.8727F),
                localAngles = new Vector3(341.9F, 325.8047F, 70.39042F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "MuzzleSyringe",
                localPos = new Vector3(0.16624F, -0.18172F, 0.56866F),
                localAngles = new Vector3(288.9777F, 80.45465F, 195.4706F),
                localScale = new Vector3(0.8F, 0.8F, 0.8F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "MuzzlePistol",
                localPos = new Vector3(-0.06666F, -0.15216F, 0.40861F),
                localAngles = new Vector3(348.49F, 78.64636F, 349.3781F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "CannonEnd",
                localPos = new Vector3(0.06157F, 0.34745F, -0.02382F),
                localAngles = new Vector3(14.44064F, 349.8533F, 86.79057F),
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