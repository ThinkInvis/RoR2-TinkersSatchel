using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;

namespace ThinkInvisible.TinkersSatchel {
    public class BismuthFlask : Item<BismuthFlask> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Healing });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            (1f - debuffReduction).ToString("P1"), (1f - buffReduction).ToString("P1")
        };



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Reduction to duration of debuffs/DoTs per stack (hyperbolic).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float debuffReduction { get; private set; } = 0.125f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Reduction to duration of buffs per stack (hyperbolic).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float buffReduction { get; private set; } = 0.1f;



        ////// Other Fields/Properties //////

        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public BismuthFlask() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/BismuthFlask.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/bismuthFlaskIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/BismuthFlask.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.13229F, -0.06816F, 0.11903F),
                localAngles = new Vector3(302.8133F, 27.57598F, 47.27724F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.21951F, 0.2191F, -0.10604F),
                localAngles = new Vector3(22.69201F, 69.13578F, 164.9348F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.10944F, 0.13985F, 0.21713F),
                localAngles = new Vector3(319.5523F, 246.8393F, 99.51362F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(2.28292F, 1.43586F, -0.91167F),
                localAngles = new Vector3(299.1197F, 57.21257F, 51.63428F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(-0.1162F, -0.05832F, 0.18106F),
                localAngles = new Vector3(294.9001F, 247.6723F, 103.4995F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.12451F, 0.13473F, 0.10913F),
                localAngles = new Vector3(294.6539F, 315.7283F, 135.0021F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.10361F, 0.03687F, 0.15813F),
                localAngles = new Vector3(300.456F, 307.2197F, 122.3479F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.12205F, -0.0469F, 0.09215F),
                localAngles = new Vector3(318.3096F, 336.7556F, 109.7998F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.13921F, -0.07294F, 0.10236F),
                localAngles = new Vector3(3.3846F, 343.6192F, 120.1624F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(1.2443F, 3.29632F, -1.388F),
                localAngles = new Vector3(355.6995F, 147.9706F, 208.2942F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FlowerBase",
                localPos = new Vector3(-0.43748F, 0.77839F, 0.59737F),
                localAngles = new Vector3(318.3699F, 161.7631F, 192.5741F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.37926F, 0.04539F, 0.04029F),
                localAngles = new Vector3(19.18333F, 170.5805F, 275.3323F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.16383F, 0.21559F, -0.00468F),
                localAngles = new Vector3(16.63539F, 61.77726F, 186.1561F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();

            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += CharacterBody_AddTimedBuff_BuffDef_float_int;
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float -= CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int -= CharacterBody_AddTimedBuff_BuffDef_float_int;
            On.RoR2.DotController.InflictDot_refInflictDotInfo -= DotController_InflictDot_refInflictDotInfo;
        }



        ////// Hooks //////

        private void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo) {
            if(inflictDotInfo.victimObject && inflictDotInfo.victimObject.TryGetComponent<CharacterBody>(out var victimBody)) {
                var count = GetCount(victimBody);
                if(count > 0) {
                    inflictDotInfo.duration *= Mathf.Pow(1f - debuffReduction, count);
                }
            }
            orig(ref inflictDotInfo);
        }

        private void CharacterBody_AddTimedBuff_BuffDef_float_int(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, CharacterBody self, BuffDef buffDef, float duration, int maxStacks) {
            if(self) {
                var count = GetCount(self);
                if(count > 0) {
                    duration *= Mathf.Pow(1f - (buffDef.isDebuff ? debuffReduction : buffReduction), count);
                }
            }
            orig(self, buffDef, duration, maxStacks);
        }

        private void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration) {
            if(self) {
                var count = GetCount(self);
                if(count > 0) {
                    duration *= Mathf.Pow(1f - (buffDef.isDebuff ? debuffReduction : buffReduction), count);
                }
            }
            orig(self, buffDef, duration);
        }
    }
}