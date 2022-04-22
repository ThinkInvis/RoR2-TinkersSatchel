using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using R2API;
using static TILER2.MiscUtil;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

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

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Healing amount, in flat HP, per stack.",
            AutoConfigFlags.None, 0f, float.MaxValue)]
        public float healAmount { get; private set; } = 2f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
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

            var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
            var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/shootToHealIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            LanguageAPI.Add(achiNameToken, "One-Man Band");
            LanguageAPI.Add(achiDescToken, "Item Set: Musical instruments. Have ALL 3 at once.");
            itemDef.unlockableDef = unlockable;

            R2API.Networking.NetworkingAPI.RegisterMessageType<MsgHealTargetAndSelf>();
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
                            new MsgHealTargetAndSelf(cpt.healthComponent, ob.healthComponent, count * self.procCoefficient, self.procChainMask)
                                .Send(R2API.Networking.NetworkDestination.Server);
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
                new MsgHealTargetAndSelf(hitInfo.hitHurtBox.healthComponent, ob.healthComponent, count * self.procCoefficient, self.procChainMask)
                    .Send(R2API.Networking.NetworkDestination.Server);
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
                new MsgHealTargetAndSelf(hb.healthComponent, ob.healthComponent, count * self.procCoefficient, self.procChainMask)
                    .Send(R2API.Networking.NetworkDestination.Server);
            }
        }

        private void ProjectileController_OnTriggerEnter(On.RoR2.Projectile.ProjectileController.orig_OnTriggerEnter orig, RoR2.Projectile.ProjectileController self, Collider collider) {
            orig(self, collider);
            if(collider == null || !collider.gameObject || !self || !self.owner) return;
            var hb = collider.gameObject.GetComponent<HurtBox>();
            var ob = self.owner.GetComponent<CharacterBody>();
            var count = GetCount(ob);
            if(hb && hb.healthComponent && count > 0 && hb.healthComponent != self.owner.GetComponent<HealthComponent>() && hb.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                new MsgHealTargetAndSelf(hb.healthComponent, ob.healthComponent, count * self.procCoefficient, self.procChainMask)
                    .Send(R2API.Networking.NetworkDestination.Server);
            }
        }

        public struct MsgHealTargetAndSelf : INetMessage {
            HealthComponent _target;
            HealthComponent _self;
            float _adjustedCount;
            ProcChainMask _pcm;

            public MsgHealTargetAndSelf(HealthComponent target, HealthComponent self, float amount, ProcChainMask pcm) {
                _target = target;
                _self = self;
                _adjustedCount = amount;
                _pcm = pcm;
            }

            public void OnReceived() {
                _target.Heal(_adjustedCount * ShootToHeal.instance.healAmount, _pcm);
                if(_self) _self.Heal(_adjustedCount * ShootToHeal.instance.returnHealingAmount, _pcm);
            }

            public void Deserialize(NetworkReader reader) {
                var tgto = reader.ReadGameObject();
                if(!tgto) {
                    TinkersSatchelPlugin._logger.LogError("Received MsgHealTargetAndSelf for nonexistent target object");
                    return;
                }
                if(!tgto.TryGetComponent<HealthComponent>(out _target)) {
                    TinkersSatchelPlugin._logger.LogError("Received MsgHealTargetAndSelf for target object with no HealthComponent");
                    return;
                }
                var selfo = reader.ReadGameObject();
                if(!selfo) {
                    TinkersSatchelPlugin._logger.LogWarning("Received MsgHealTargetAndSelf for nonexistent self object");
                    _self = null;
                } else if(!selfo.TryGetComponent<HealthComponent>(out _self)) {
                    TinkersSatchelPlugin._logger.LogWarning("Received MsgHealTargetAndSelf for self object with no HealthComponent");
                    _self = null;
                }
                _adjustedCount = reader.ReadSingle();
                _pcm = reader.ReadProcChainMask();
            }

            public void Serialize(NetworkWriter writer) {
                writer.Write(_target.gameObject);
                writer.Write(_self.gameObject);
                writer.Write(_adjustedCount);
                writer.Write(_pcm);
            }
        }
    }

    [RegisterAchievement("TkSat_ShootToHeal", "TkSat_ShootToHealUnlockable", "")]
    public class TkSatShootToHealAchievement : RoR2.Achievements.BaseAchievement {
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