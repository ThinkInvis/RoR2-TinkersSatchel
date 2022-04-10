using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using R2API;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
    public class ShootToHeal : Item<ShootToHeal> {

        ////// Item Data //////

        public override string displayName => "Percussive Maintenance";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Healing});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Hit allies to heal them.";
        protected override string GetDescString(string langid = null) => $"Hitting an ally with a direct attack heals them for <style=cIsHealing>{healAmount:N1} health <style=cStack>(+{healAmount:N1} per stack)</style></style> and you for {Pct(healAmount / returnHealingAmount)} as much.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Healing amount, in flat HP, per stack.",
            AutoConfigFlags.None, 0f, float.MaxValue)]
        public float healAmount { get; private set; } = 2f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Self-healing amount on healing an ally, in flat HP, per stack.",
            AutoConfigFlags.None, 0f, float.MaxValue)]
        public float returnHealingAmount { get; private set; } = 0.5f;



        ////// Other Fields/Properties //////

        internal static UnlockableDef unlockable;



        ////// TILER2 Module Setup //////
        public ShootToHeal() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ShootToHeal.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/shootToHealIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            unlockable = UnlockableAPI.AddUnlockable<TkSatShootToHealAchievement>();
            LanguageAPI.Add("TKSAT_SHOOTTOHEAL_ACHIEVEMENT_NAME", "One-Man Band");
            LanguageAPI.Add("TKSAT_SHOOTTOHEAL_ACHIEVEMENT_DESCRIPTION", "Item Set: Musical instruments. Have ALL 3 at once.");

            itemDef.unlockableDef = unlockable;
        }

        public override void Install() {
            base.Install();
            On.RoR2.BulletAttack.ProcessHit += BulletAttack_ProcessHit;
            On.RoR2.Projectile.ProjectileController.OnCollisionEnter += ProjectileController_OnCollisionEnter;
            On.RoR2.Projectile.ProjectileController.OnTriggerEnter += ProjectileController_OnTriggerEnter;
            IL.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.BulletAttack.ProcessHit -= BulletAttack_ProcessHit;
            On.RoR2.Projectile.ProjectileController.OnCollisionEnter -= ProjectileController_OnCollisionEnter;
            On.RoR2.Projectile.ProjectileController.OnTriggerEnter -= ProjectileController_OnTriggerEnter;
            IL.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;
        }



        ////// Hooks //////
        private void OverlapAttack_Fire(ILContext il) {
            var c = new ILCursor(il);

            var ILFound = c.TryGotoNext(MoveType.Before,
                x => x.MatchCall<OverlapAttack>(nameof(OverlapAttack.HurtBoxPassesFilter)));
            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<HurtBox, OverlapAttack, HurtBox>>((cpt, self) => {
                    if(self != null && self.attacker && cpt && cpt.healthComponent
                    && !self.ignoredHealthComponentList.Contains(cpt.healthComponent)
                    && cpt.healthComponent.gameObject != self.attacker
                    && cpt.teamIndex == TeamComponent.GetObjectTeam(self.attacker)) {
                        self.ignoredHealthComponentList.Add(cpt.healthComponent);
                        var ob = self.attacker.GetComponent<CharacterBody>();
                        var count = GetCount(ob);
                        if(count > 0) {
                            cpt.healthComponent.Heal(count * self.procCoefficient * healAmount, self.procChainMask);
                            if(ob.healthComponent) ob.healthComponent.Heal(count * self.procCoefficient * returnHealingAmount, self.procChainMask);
                        }
                    }

                    return cpt;
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("ShootToHeal: Failed to apply OverlapAttack.Fire IL patch (target instructions not found)");
            }
        }

        private bool BulletAttack_ProcessHit(On.RoR2.BulletAttack.orig_ProcessHit orig, BulletAttack self, ref BulletAttack.BulletHit hitInfo) {
            var retv = orig(self, ref hitInfo);
            if(!self.owner || !hitInfo.hitHurtBox) return retv;
            var ob = self.owner.GetComponent<CharacterBody>();
            var count = GetCount(ob);
            if(hitInfo.hitHurtBox.healthComponent && count > 0 && hitInfo.hitHurtBox.healthComponent != self.owner.GetComponent<HealthComponent>() && hitInfo.hitHurtBox.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                hitInfo.hitHurtBox.healthComponent.Heal(count * self.procCoefficient * healAmount, self.procChainMask);
                if(ob.healthComponent) ob.healthComponent.Heal(count * self.procCoefficient * returnHealingAmount, self.procChainMask);
            }
            return retv;
        }

        private void ProjectileController_OnCollisionEnter(On.RoR2.Projectile.ProjectileController.orig_OnCollisionEnter orig, RoR2.Projectile.ProjectileController self, Collision collision) {
            orig(self, collision);
            if(collision == null || !collision.gameObject || !self || !self.owner) return;
            var hb = collision.gameObject.GetComponent<HurtBox>();
            var ob = self.owner.GetComponent<CharacterBody>();
            var count = GetCount(ob);
            if(hb && hb.healthComponent && count > 0 && hb.healthComponent != self.owner.GetComponent<HealthComponent>() && hb.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                hb.healthComponent.Heal(count * self.procCoefficient * healAmount, self.procChainMask);
                if(ob.healthComponent) ob.healthComponent.Heal(count * self.procCoefficient * returnHealingAmount, self.procChainMask);
            }
        }

        private void ProjectileController_OnTriggerEnter(On.RoR2.Projectile.ProjectileController.orig_OnTriggerEnter orig, RoR2.Projectile.ProjectileController self, Collider collider) {
            orig(self, collider);
            if(collider == null || !collider.gameObject || !self || !self.owner) return;
            var hb = collider.gameObject.GetComponent<HurtBox>();
            var ob = self.owner.GetComponent<CharacterBody>();
            var count = GetCount(ob);
            if(hb && hb.healthComponent && count > 0 && hb.healthComponent != self.owner.GetComponent<HealthComponent>() && hb.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                hb.healthComponent.Heal(count * self.procCoefficient * healAmount, self.procChainMask);
                if(ob.healthComponent) ob.healthComponent.Heal(count * self.procCoefficient * returnHealingAmount, self.procChainMask);
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

        public Sprite Sprite => TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/shootToHealIcon.png");

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
            orig(self);
            if(this.localUser.cachedMaster == self
                && self.inventory.GetItemCount(RoR2Content.Items.ChainLightning) > 0
                && self.inventory.GetItemCount(RoR2Content.Items.EnergizedOnEquipmentUse) > 0
                && (self.inventory.currentEquipmentIndex == RoR2Content.Equipment.TeamWarCry.equipmentIndex
                || self.inventory.alternateEquipmentIndex == RoR2Content.Equipment.TeamWarCry.equipmentIndex))
                Grant();
        }
    }
}