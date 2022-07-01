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
        protected override string GetDescString(string langid = null) => $"Hitting an ally with a direct attack heals them for <style=cIsHealing>{healAmount:N1} health <style=cStack>(+{healAmount:N1} per stack)</style></style> and you for {Pct(returnHealingAmount / healAmount)} as much.";
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
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////
        public ShootToHeal() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ShootToHeal.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/shootToHealIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/ShootToHeal.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MainWeapon",
                localPos = new Vector3(-0.01191F, 0.94731F, 0.01098F),
                localAngles = new Vector3(316.5172F, 180.416F, 17.75818F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.38728F, 0.00965F, -0.06446F),
                localAngles = new Vector3(31.87035F, 332.9695F, 3.18838F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.23353F, -0.00868F, -0.08696F),
                localAngles = new Vector3(27.00084F, 326.5775F, 4.93487F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.6739F, -1.47899F, 1.63122F),
                localAngles = new Vector3(354.4511F, 7.12517F, 355.0916F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.24835F, 0.13692F, 0.12219F),
                localAngles = new Vector3(19.74273F, 338.7649F, 343.2596F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                childName = "Stomach",
                localPos = new Vector3(0.17437F, -0.01902F, 0.11239F),
                localAngles = new Vector3(14.62809F, 338.0782F, 18.2589F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F),
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.28481F, -0.22564F, -0.12889F),
                localAngles = new Vector3(0.98176F, 51.91312F, 23.00177F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.16876F, -0.10376F, 0.02998F),
                localAngles = new Vector3(357.5521F, 355.006F, 105.9485F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "ThighR",
                localPos = new Vector3(-0.08794F, 0.03176F, -0.06409F),
                localAngles = new Vector3(350.6662F, 317.2625F, 21.97947F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(2.33895F, -0.34548F, 0.80107F),
                localAngles = new Vector3(311.4177F, 7.89006F, 354.1869F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(0.75783F, -0.10773F, 0.00385F),
                localAngles = new Vector3(308.2326F, 10.8672F, 329.0782F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.28636F, -0.3815F, -0.06912F),
                localAngles = new Vector3(352.4358F, 63.85439F, 6.83272F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.17554F, -0.13447F, -0.0436F),
                localAngles = new Vector3(15.08189F, 9.51543F, 15.89409F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
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
            LanguageAPI.Add(achiDescToken, "Item Set: Musical instruments. Have 3 or more (of 5) at once.");
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
            if(this.localUser.cachedMaster != self) return;
            int count = 0;
            if(self.inventory.GetItemCount(RoR2Content.Items.ChainLightning) > 0 || self.inventory.GetItemCount(DLC1Content.Items.ChainLightningVoid) > 0) count++;
            if(self.inventory.GetItemCount(RoR2Content.Items.EnergizedOnEquipmentUse) > 0) count++;
            if(self.inventory.GetItemCount(RoR2Content.Items.ShockNearby) > 0) count++;
            if((self.inventory.currentEquipmentIndex == RoR2Content.Equipment.TeamWarCry.equipmentIndex
                || self.inventory.alternateEquipmentIndex == RoR2Content.Equipment.TeamWarCry.equipmentIndex)) count++;
            if(self.inventory.GetItemCount(RoR2Content.Items.Behemoth) > 0) count++;
            if(count >= 3)
                Grant();
        }
    }
}