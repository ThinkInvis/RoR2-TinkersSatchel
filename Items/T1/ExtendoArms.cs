using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
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
        protected override string GetPickupString(string langid = null) => "Melee and PBAoE attacks reach farther and deal slightly more damage.";
        protected override string GetDescString(string langid = null) => $"All your melee attacks, as well as AoEs centered within {pbaoeRange:N1} meters of you, gain <style=cIsUtility>{Pct(resizeAmount)} range <style=cStack>(+{Pct(resizeAmount)} per stack)</style></style> and <style=cIsDamage>{Pct(damageAmount)} damage <style=cStack>(+{Pct(damageAmount)} per stack)</style></style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:P0}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Hitbox scale increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float resizeAmount { get; private set; } = 0.125f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Melee attack damage increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageAmount { get; private set; } = 0.0625f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum distance between character and center of an AoE to count as a PBAoE.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pbaoeRange { get; private set; } = 5f;



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
            On.RoR2.BlastAttack.Fire += BlastAttack_Fire;
        }

        public override void Uninstall() {
            base.Uninstall();
            IL.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;
            On.RoR2.BlastAttack.Fire -= BlastAttack_Fire;
        }



        ////// Hooks //////

        private BlastAttack.Result BlastAttack_Fire(On.RoR2.BlastAttack.orig_Fire orig, BlastAttack self) {
            var origDamage = self.baseDamage;
            var origRadius = self.radius;
            if(self.attacker && self.attacker.TryGetComponent<CharacterBody>(out var attackerBody)) {
                if(Vector3.Distance(attackerBody.corePosition, self.position) < pbaoeRange) {
                    var count = GetCount(attackerBody);
                    self.baseDamage *= 1f + count * damageAmount;
                    self.radius *= 1f + count * resizeAmount;
                }
            }
            var retv = orig(self);
            self.baseDamage = origDamage;
            self.radius = origRadius;
            return retv;
        }

        private void OverlapAttack_Fire(ILContext il) {
            var c = new ILCursor(il);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<OverlapAttack>>((self) => {
                if(self.attacker) {
                    var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                    self.damage *= 1f + count * damageAmount;
                }
            });
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
            c.Index = 0;
            while(c.TryGotoNext(MoveType.Before, x => x.MatchRet())) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<OverlapAttack>>((self) => {
                    if(self.attacker) {
                        var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                        self.damage /= 1f + count * damageAmount;
                    }
                });
                c.Index++;
            }
        }
    }
}