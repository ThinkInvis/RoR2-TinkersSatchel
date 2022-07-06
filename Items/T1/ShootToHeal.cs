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
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Healing});

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
                childName = "HandR",
                localPos = new Vector3(-0.03464F, 0.10259F, -0.06757F),
                localAngles = new Vector3(319.4068F, 347.8731F, 46.87115F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(-0.0897F, 0.14081F, 0.03271F),
                localAngles = new Vector3(300.2318F, 295.2138F, 197.7983F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "LowerArmR",
                localPos = new Vector3(0.66226F, 5.14175F, -0.22211F),
                localAngles = new Vector3(331.4982F, 326.7393F, 8.18496F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "LowerArmR",
                localPos = new Vector3(0.02233F, 0.14033F, -0.0751F),
                localAngles = new Vector3(336.5551F, 330.0712F, 39.69568F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "LowerArmR",
                localPos = new Vector3(0.04393F, -0.04636F, 0.01517F),
                localAngles = new Vector3(39.93605F, 179.8943F, 198.899F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.16672F, -0.03254F, 0.373F),
                localAngles = new Vector3(316.9363F, 2.65794F, 139.6746F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(0.0239F, 0.07276F, 0.07745F),
                localAngles = new Vector3(343.6281F, 49.36037F, 17.41755F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "LowerArmR",
                localPos = new Vector3(-0.04336F, 0.31317F, -0.0799F),
                localAngles = new Vector3(318.2455F, 308.9852F, 36.96745F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "UpperArmR",
                localPos = new Vector3(-0.24752F, 3.97012F, -0.48912F),
                localAngles = new Vector3(319.2452F, 10.75099F, 353.5247F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FlowerBase",
                localPos = new Vector3(0.42347F, -0.11766F, 0.57757F),
                localAngles = new Vector3(323.8121F, 171.688F, 162.5838F),
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