﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class ConcentratingAlembic : Item<ConcentratingAlembic> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            damageBuff.ToString("0%"), durationBuff.ToString("0%"), rangeReduc.ToString("0%"), speedReduc.ToString("0%"), hitscanReduc.ToString("0%")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Reduction to melee/blast range/radius per stack (linear divisor).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float rangeReduc { get; private set; } = 0.1f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Reduction to projectile speed per stack (linear divisor).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float speedReduc { get; private set; } = 0.1f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Reduction to hitscan range per stack (linear divisor).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float hitscanReduc { get; private set; } = 0.1f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("DoT damage multiplier per stack (linear).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageBuff { get; private set; } = 0.25f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Debuff duration multiplier per stack (linear).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float durationBuff { get; private set; } = 0.25f;



        ////// Other Fields/Properties //////




        ////// TILER2 Module Setup //////

        public ConcentratingAlembic() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ConcentratingAlembic.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/concentratingAlembicIcon.png");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            IL.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
            On.RoR2.BlastAttack.Fire += BlastAttack_Fire;
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += CharacterBody_AddTimedBuff_BuffDef_float_int;
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
            On.RoR2.Projectile.ProjectileManager.InitializeProjectile += ProjectileManager_InitializeProjectile;
        }

        public override void Uninstall() {
            base.Uninstall();
            IL.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;
            On.RoR2.BlastAttack.Fire -= BlastAttack_Fire;
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float -= CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int -= CharacterBody_AddTimedBuff_BuffDef_float_int;
            On.RoR2.DotController.InflictDot_refInflictDotInfo -= DotController_InflictDot_refInflictDotInfo;
            On.RoR2.Projectile.ProjectileManager.InitializeProjectile -= ProjectileManager_InitializeProjectile;
        }



        ////// Hooks //////

        private void ProjectileManager_InitializeProjectile(On.RoR2.Projectile.ProjectileManager.orig_InitializeProjectile orig, RoR2.Projectile.ProjectileController projectileController, RoR2.Projectile.FireProjectileInfo fireProjectileInfo) {
            orig(projectileController, fireProjectileInfo);

            if(fireProjectileInfo.owner && fireProjectileInfo.owner.TryGetComponent<CharacterBody>(out var ownerBody) && GetCount(ownerBody) > 0) {
                float speedDiv = 1f + GetCount(ownerBody) * speedReduc;

                if(projectileController.TryGetComponent<RoR2.Projectile.ProjectileSimple>(out var ps)) {
                    ps.desiredForwardSpeed /= speedDiv;
                    ps.oscillateSpeed /= speedDiv;
                    ps.oscillateMagnitude /= speedDiv;
                    var vol = ps.velocityOverLifetime;
                    if(vol != null) {
                        for(var i = 0; i < vol.length; i++) {
                            vol.keys[i].value /= speedDiv;
                        }
                    }
                }

                if(projectileController.TryGetComponent<RoR2.Projectile.BoomerangProjectile>(out var bp)) {
                    bp.travelSpeed /= speedDiv;
                }

                if(projectileController.TryGetComponent<RoR2.Projectile.MissileController>(out var mc)) {
                    mc.maxVelocity /= speedDiv;
                    mc.acceleration /= speedDiv;
                }
            }
        }

        private void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo) {
            if(inflictDotInfo.attackerObject && inflictDotInfo.attackerObject.TryGetComponent<CharacterBody>(out var attackerBody))
                inflictDotInfo.damageMultiplier *= 1f + damageBuff * GetCount(attackerBody);
            orig(ref inflictDotInfo);
        }

        private void CharacterBody_AddTimedBuff_BuffDef_float_int(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, CharacterBody self, BuffDef buffDef, float duration, int maxStacks) {
            if(self && buffDef.isDebuff) {
                var targets = MiscUtil.GatherEnemies(TeamComponent.GetObjectTeam(self.gameObject));
                var combatantStacks = targets.Sum(x => x.body ? GetCount(x.body) : 0);
                duration *= 1f + combatantStacks * durationBuff;
            }
            orig(self, buffDef, duration, maxStacks);
        }

        private void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration) {
            if(self && buffDef.isDebuff) {
                var targets = MiscUtil.GatherEnemies(TeamComponent.GetObjectTeam(self.gameObject));
                var combatantStacks = targets.Sum(x => x.body ? GetCount(x.body) : 0);
                duration *= 1f + combatantStacks * durationBuff;
            }
            orig(self, buffDef, duration);
        }

        private BlastAttack.Result BlastAttack_Fire(On.RoR2.BlastAttack.orig_Fire orig, BlastAttack self) {
            var origRadius = self.radius;
            if(self.attacker && self.attacker.TryGetComponent<CharacterBody>(out var attackerBody)) {
                var count = GetCount(attackerBody);
                self.radius /= 1f + count * rangeReduc;
            }
            var retv = orig(self);
            self.radius = origRadius;
            return retv;
        }

        private void OverlapAttack_Fire(ILContext il) {
            var c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Transform>("get_lossyScale"))) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Vector3, OverlapAttack, Vector3>>((origFullExtents, self) => {
                    if(!self.attacker) return origFullExtents;
                    var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                    return origFullExtents / (1f + rangeReduc * count);
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("ConcentratingAlembic: failed to apply IL hook (OverlapAttack_Fire), target instructions not found. Item will not apply a hitbox scale malus.");
            }
        }

        private void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self) {
            float mdDiv = 1f;
            if(self.owner && self.owner.TryGetComponent<CharacterBody>(out var ownerBody))
                mdDiv += GetCount(ownerBody) * hitscanReduc;
            self.maxDistance /= mdDiv;
            orig(self);
            self.maxDistance *= mdDiv;
        }
    }
}