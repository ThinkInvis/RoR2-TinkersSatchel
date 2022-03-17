using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using R2API;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;

namespace ThinkInvisible.TinkersSatchel {
    public class ShootToHeal : Item<ShootToHeal> {

        ////// Item Data //////

        public override string displayName => "Percussive Maintenance";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Healing});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Shoot allies to heal them.";
        protected override string GetDescString(string langid = null) => $"Hitting an ally with any attack heals them for <style=cIsHealing>1 <style=cStack>(+1 per stack)</style></style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// TILER2 Module Setup //////
        public ShootToHeal() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/ShootToHeal.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/shootToHealIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            On.RoR2.BulletAttack.ProcessHit += BulletAttack_ProcessHit;
            On.RoR2.Projectile.ProjectileController.OnCollisionEnter += ProjectileController_OnCollisionEnter;
            On.RoR2.Projectile.ProjectileController.OnTriggerEnter += ProjectileController_OnTriggerEnter;
            IL.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
        }

        private void OverlapAttack_Fire(ILContext il) {
            var c = new ILCursor(il);

            var ILFound = c.TryGotoNext(MoveType.After,
                x => x.MatchCallvirt<Component>("GetComponent<class RoR2.HurtBox>"));
            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<HurtBox, OverlapAttack, HurtBox>>((cpt, self) => {
                    //!hurtBox.healthComponent || ((!(hurtBox.healthComponent.gameObject == this.attacker) || this.attackerFiltering != AttackerFiltering.NeverHitSelf) && (!(this.attacker == null) || !(hurtBox.healthComponent.gameObject.GetComponent<MaulingRock>() != null)) && !this.ignoredHealthComponentList.Contains(hurtBox.healthComponent) && FriendlyFireManager.ShouldDirectHitProceed(hurtBox.healthComponent, this.teamIndex));
                    if(cpt && cpt.healthComponent && cpt.healthComponent.gameObject != self.attacker
                    && cpt.teamIndex == TeamComponent.GetObjectTeam(self.attacker)) {
                        var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                        if(count > 0) {
                            cpt.healthComponent.Heal(count, default);
                        }
                    }

                    return cpt;
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("ShootToHeal: Failed to apply OverlapAttack.Fire IL patch (target instructions not found)");
            }
        }

        public override void Uninstall() {
            base.Uninstall();
        }



        ////// Hooks //////
        private bool BulletAttack_ProcessHit(On.RoR2.BulletAttack.orig_ProcessHit orig, BulletAttack self, ref BulletAttack.BulletHit hitInfo) {
            var retv = orig(self, ref hitInfo);
            var count = GetCount(self.owner?.GetComponent<CharacterBody>());
            if(hitInfo.hitHurtBox?.healthComponent && count > 0 && hitInfo.hitHurtBox.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                hitInfo.hitHurtBox.healthComponent.Heal(count, default);
            }
            return retv;
        }

        private void ProjectileController_OnCollisionEnter(On.RoR2.Projectile.ProjectileController.orig_OnCollisionEnter orig, RoR2.Projectile.ProjectileController self, Collision collision) {
            orig(self, collision);
            if(!collision.gameObject) return;
            var hb = collision.gameObject.GetComponent<HurtBox>();
            var count = GetCount(self.owner?.GetComponent<CharacterBody>());
            if(hb && hb.healthComponent && count > 0 && hb.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                hb.healthComponent.Heal(count, default);
            }
        }

        private void ProjectileController_OnTriggerEnter(On.RoR2.Projectile.ProjectileController.orig_OnTriggerEnter orig, RoR2.Projectile.ProjectileController self, Collider collider) {
            orig(self, collider);
            if(!collider.gameObject) return;
            var hb = collider.gameObject.GetComponent<HurtBox>();
            var count = GetCount(self.owner?.GetComponent<CharacterBody>());
            if(hb && hb.healthComponent && count > 0 && hb.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                hb.healthComponent.Heal(count, default);
            }
        }
    }
}