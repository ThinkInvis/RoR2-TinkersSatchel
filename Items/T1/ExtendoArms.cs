using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using System.Collections.Generic;
using System.Linq;
using R2API;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class ExtendoArms : Item<ExtendoArms> {

        ////// Item Data //////
        
        public override string displayName => "Extendo-Arms";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Damage});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Melee attacks reach farther and deal slightly more damage.";
        protected override string GetDescString(string langid = null) => $"Increases the <style=cIsUtility>range</style> of all your melee attacks by <style=cIsUtility>{Pct(resizeAmount)} <style=cStack>(+{Pct(resizeAmount)} per stack)</style></style>. Increases <style=cIsDamage>damage</style> of all melee attacks by <style=cIsDamage>{Pct(damageAmount)} <style=cStack>(+{Pct(damageAmount)} per stack)</style></style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:P0}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Hitbox scale increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float resizeAmount { get; private set; } = 0.15f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Melee attack damage increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageAmount { get; private set; } = 0.05f;



        ////// TILER2 Module Setup //////
        
        public ExtendoArms() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ExtendoArms.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/extendoArmsIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            IL.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
            On.RoR2.OverlapAttack.ctor += OverlapAttack_ctor;
        }


        public override void Uninstall() {
            base.Uninstall();
        }



        ////// Hooks //////

        private void OverlapAttack_ctor(On.RoR2.OverlapAttack.orig_ctor orig, OverlapAttack self) {
            orig(self);
            if(self.attacker) {
                var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                self.damage *= 1f + count * damageAmount;
            }
        }

        private void OverlapAttack_Fire(ILContext il) {
            var c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Transform>("get_lossyScale"))) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Vector3, OverlapAttack, Vector3>>((origFullExtents, self) => {
                    if(!self.attacker) return origFullExtents;
                    var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                    return origFullExtents * (1f + resizeAmount * count);
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("ExtendoArms: failed to apply IL hook (OverlapAttack_Fire), target instructions not found. Item will not apply a hitbox scale bonus.");
            }
        }
    }
}