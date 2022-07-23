using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class MountainToken : Item<MountainToken> {

        ////// Item Data //////
        
        public override string displayName => "Celestial Gambit";
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.HoldoutZoneRelated });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Gain an extra item reward from teleporters... <color=#FF7F7F>BUT the teleporter zone weakens you.</color>";
        protected override string GetDescString(string langid = null) => $"The teleporter boss drops 1 <style=cIsUtility>extra item</style> when killed <style=cStack>(+1 per stack)</style>. As long as you remain in the teleporter zone, you receive <color=#FF7F7F>50% less healing and health regen</color> <style=cStack>(+50% per stack, hyperbolic)</style> and become <color=#FF7F7F>unable to jump</color>.";
        protected override string GetLoreString(string langid = null) => "This monument dedicated to those / Who perished in the storm";



        ////// Config //////

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fractional healing reduction per stack, hyperbolic.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float debuffAmount { get; private set; } = 0.5f;



        ////// Other Fields/Properties //////

        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public MountainToken() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/MountainToken.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/mountainTokenIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/MountainToken.prefab");
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

            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += GenericCharacterMain_ApplyJumpVelocity;
            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
            On.EntityStates.GenericCharacterMain.ApplyJumpVelocity -= GenericCharacterMain_ApplyJumpVelocity;
            IL.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
        }



        ////// Hooks //////

        private void BossGroup_DropRewards(ILContext il) {
            ILCursor c = new(il);

            int locRewardCount = -1;
            if(c.TryGotoNext(
                i => i.MatchCall<BossGroup>("get_bonusRewardCount"),
                i => i.MatchAdd(),
                i => i.MatchStloc(out locRewardCount))
                && c.TryGotoNext(MoveType.After,
                i => i.MatchLdcR4(360f),
                i => i.MatchLdloc(locRewardCount))) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<int, BossGroup, int>>((origRewards, bg) => {
                    int extraRewardsSingular = 0;
                    foreach(var nu in NetworkUser.readOnlyInstancesList) {
                        if(!nu.isParticipating) continue;
                        var body = nu.GetCurrentBody();
                        extraRewardsSingular += GetCount(body);
                    }
                    return origRewards + extraRewardsSingular;
                });
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Stloc, locRewardCount);
            } else {
                TinkersSatchelPlugin._logger.LogError("MountainToken: Failed to apply IL hook (BossGroup_DropRewards), item will not provide extra teleporter rewards");
            }
        }

        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen) {
            var count = GetCount(self.body);
            if(count > 0
                && TeleporterInteraction.instance
                && TeleporterInteraction.instance.activationState == TeleporterInteraction.ActivationState.Charging
                && TeleporterInteraction.instance.holdoutZoneController.IsBodyInChargingRadius(self.body))
                amount *= Mathf.Pow(debuffAmount, count);
            return orig(self, amount, procChainMask, nonRegen);
        }

        private void GenericCharacterMain_ApplyJumpVelocity(On.EntityStates.GenericCharacterMain.orig_ApplyJumpVelocity orig, CharacterMotor characterMotor, CharacterBody characterBody, float horizontalBonus, float verticalBonus, bool vault) {
            if(GetCount(characterBody) > 0
                && TeleporterInteraction.instance
                && TeleporterInteraction.instance.activationState == TeleporterInteraction.ActivationState.Charging
                && TeleporterInteraction.instance.holdoutZoneController.IsBodyInChargingRadius(characterBody))
                return;
            orig(characterMotor, characterBody, horizontalBonus, verticalBonus, vault);
        }
    }
}