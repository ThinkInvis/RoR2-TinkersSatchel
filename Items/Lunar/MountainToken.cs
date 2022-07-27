using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class MountainToken : Item<MountainToken> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.HoldoutZoneRelated });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            debuffAmount.ToString("P0")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fractional healing reduction per stack, hyperbolic.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float debuffAmount { get; private set; } = 0.5f;



        ////// Other Fields/Properties //////



        ////// TILER2 Module Setup //////

        public MountainToken() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/MountainToken.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/mountainTokenIcon.png");
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