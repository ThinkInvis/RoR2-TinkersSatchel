using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class FudgeDice : Item<FudgeDice> {

        ////// Item Data //////

        public override string displayName => "Fudge Dice";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility });
        public override bool itemIsAIBlacklisted { get; protected set; } = true;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Periodically guarantee luck.";
        protected override string GetDescString(string langid = null) => $"Once every {icd} seconds <style=cStack>(-{cdrStack:P0} per stack, hyperbolic)</style>, your next random effect is <style=cIsUtility>guaranteed to have a favorable outcome</style>.";
        protected override string GetLoreString(string langid = null) => "Don't tell the DM.";



        ////// Config ///////

        [AutoConfigRoOSlider("{0:P0}", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Base cooldown at first stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float icd { get; private set; } = 20f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplicative internal cooldown reduction per stack past the first.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float cdrStack { get; private set; } = 0.1f;



        ////// Other Fields/Properties //////
        
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////
        public FudgeDice() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/FudgeDice.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/fudgeDiceIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/FudgeDice.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.04417F, 0.19067F, -0.24033F),
                localAngles = new Vector3(337.4471F, 55.56866F, 354.1383F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00054F, 0.27487F, -0.29389F),
                localAngles = new Vector3(320.018F, 64.74491F, 342.704F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.01826F, 0.41296F, -0.21866F),
                localAngles = new Vector3(6.28242F, 43.10916F, 36.10896F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "SpineChest3",
                localPos = new Vector3(-0.08684F, 0.67153F, -1.08192F),
                localAngles = new Vector3(44.63957F, 264.7216F, 107.5511F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.01093F, 0.05395F, -0.36182F),
                localAngles = new Vector3(314.4274F, 93.80039F, 295.7014F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.15785F, 0.13082F, -0.11723F),
                localAngles = new Vector3(322.6137F, 19.11888F, 332.5494F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.0131F, -0.01474F, -0.22271F),
                localAngles = new Vector3(328.3462F, 59.25051F, 349.7125F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.00568F, 0.22235F, -0.35905F),
                localAngles = new Vector3(334.1837F, 59.43953F, 3.43586F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00072F, 0.34057F, -0.30971F),
                localAngles = new Vector3(345.0132F, 50.15996F, 12.58943F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.1298F, 1.52182F, -2.26367F),
                localAngles = new Vector3(320.2272F, 71.04354F, 329.5704F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(1.01798F, 0.30345F, -0.23827F),
                localAngles = new Vector3(320.4994F, 321.4309F, 4.27371F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.12059F, 0.06472F, -0.16892F),
                localAngles = new Vector3(321.9678F, 54.92611F, 353.519F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00089F, 0.05696F, -0.30533F),
                localAngles = new Vector3(318.8723F, 56.99937F, 349.1709F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            On.RoR2.Util.CheckRoll_float_float_CharacterMaster += Util_CheckRoll_float_float_CharacterMaster;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.Util.CheckRoll_float_float_CharacterMaster -= Util_CheckRoll_float_float_CharacterMaster;
        }



        ////// Hooks //////

        private bool Util_CheckRoll_float_float_CharacterMaster(On.RoR2.Util.orig_CheckRoll_float_float_CharacterMaster orig, float percentChance, float luck, CharacterMaster effectOriginMaster) {
            var count = GetCount(effectOriginMaster);
            if(count > 0 && percentChance > 0f) {
                var icdCpt = effectOriginMaster.GetComponent<FudgeDiceICD>();
                if(!icdCpt) icdCpt = effectOriginMaster.gameObject.AddComponent<FudgeDiceICD>();
                if(icdCpt.stopwatch <= 0f) {
                    icdCpt.stopwatch = icd * (Mathf.Pow(1f - cdrStack, count - 1));
                    return true;
                }
            }
            return orig(percentChance, luck, effectOriginMaster);
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class FudgeDiceICD : MonoBehaviour {
        public float stopwatch = 0f;
        CharacterBody body;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(!body.healthComponent.alive) return;
            if(stopwatch > 0f) {
                stopwatch -= Time.fixedDeltaTime;
            }
        }
    }
}