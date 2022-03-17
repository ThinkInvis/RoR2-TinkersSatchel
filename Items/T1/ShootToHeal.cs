using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    public class ShootToHeal : Item<ShootToHeal> {

        ////// Item Data //////

        public override string displayName => "Percussive Maintenance";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Healing});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Hit allies to heal them.";
        protected override string GetDescString(string langid = null) => $"Hitting an ally with a direct attack heals them for <style=cIsHealing>1 <style=cStack>(+1 per stack)</style></style>.";
        protected override string GetLoreString(string langid = null) => "";



        /////// Other Fields/Properties //////

        internal UnlockableDef unlockable;



        ////// TILER2 Module Setup //////
        public ShootToHeal() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/ShootToHeal.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/shootToHealIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            unlockable = UnlockableAPI.AddUnlockable<TkSatShootToHealAchievement>();
            LanguageAPI.Add("TKSAT_SHOOTTOHEAL_ACHIEVEMENT_NAME", "One-Man Band");
            LanguageAPI.Add("TKSAT_SHOOTTOHEAL_ACHIEVEMENT_DESCRIPTION", "Have 3 different musical instruments at once.");

            itemDef.unlockableDef = unlockable;
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

            var ILFound = c.TryGotoNext(MoveType.Before,
                x => x.MatchCall<OverlapAttack>(nameof(OverlapAttack.HurtBoxPassesFilter)));
            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<HurtBox, OverlapAttack, HurtBox>>((cpt, self) => {
                    if(cpt && cpt.healthComponent
                    && !self.ignoredHealthComponentList.Contains(cpt.healthComponent)
                    && cpt.healthComponent.gameObject != self.attacker
                    && cpt.teamIndex == TeamComponent.GetObjectTeam(self.attacker)) {
                        self.ignoredHealthComponentList.Add(cpt.healthComponent);
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
            if(hitInfo.hitHurtBox?.healthComponent && count > 0 && hitInfo.hitHurtBox.healthComponent != self.owner.GetComponent<HealthComponent>() && hitInfo.hitHurtBox.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                hitInfo.hitHurtBox.healthComponent.Heal(count, default);
            }
            return retv;
        }

        private void ProjectileController_OnCollisionEnter(On.RoR2.Projectile.ProjectileController.orig_OnCollisionEnter orig, RoR2.Projectile.ProjectileController self, Collision collision) {
            orig(self, collision);
            if(!collision.gameObject) return;
            var hb = collision.gameObject.GetComponent<HurtBox>();
            var count = GetCount(self.owner?.GetComponent<CharacterBody>());
            if(hb && hb.healthComponent && count > 0 && hb.healthComponent != self.owner.GetComponent<HealthComponent>() && hb.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                hb.healthComponent.Heal(count, default);
            }
        }

        private void ProjectileController_OnTriggerEnter(On.RoR2.Projectile.ProjectileController.orig_OnTriggerEnter orig, RoR2.Projectile.ProjectileController self, Collider collider) {
            orig(self, collider);
            if(!collider.gameObject) return;
            var hb = collider.gameObject.GetComponent<HurtBox>();
            var count = GetCount(self.owner?.GetComponent<CharacterBody>());
            if(hb && hb.healthComponent && count > 0 && hb.healthComponent != self.owner.GetComponent<HealthComponent>() && hb.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                hb.healthComponent.Heal(count, default);
            }
        }
    }

    public class TkSatShootToHealAchievement : RoR2.Achievements.BaseAchievement, IModdedUnlockableDataProvider {
        public string AchievementIdentifier => "TKSAT_SHOOTTOHEAL_ACHIEVEMENT_ID";
        public string UnlockableIdentifier => "TKSAT_SHOOTTOHEAL_UNLOCKABLE_ID";
        public string PrerequisiteUnlockableIdentifier => "";
        public string AchievementNameToken => "TKSAT_SHOOTTOHEAL_ACHIEVEMENT_NAME";
        public string AchievementDescToken => "TKSAT_SHOOTTOHEAL_ACHIEVEMENT_DESCRIPTION";
        public string UnlockableNameToken => ShootToHeal.instance.nameToken;

        public Sprite Sprite => TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/shootToHealIcon.png");

        public System.Func<string> GetHowToUnlock => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

        public System.Func<string> GetUnlocked => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
        }

        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
        }

        private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self) {
            if(this.localUser.cachedMaster == self
                && self.inventory.GetItemCount(RoR2Content.Items.ChainLightning) > 0
                && self.inventory.GetItemCount(RoR2Content.Items.EnergizedOnEquipmentUse) > 0
                && (self.inventory.currentEquipmentIndex == RoR2Content.Equipment.TeamWarCry.equipmentIndex
                || self.inventory.alternateEquipmentIndex == RoR2Content.Equipment.TeamWarCry.equipmentIndex))
                Grant();
        }
    }
}