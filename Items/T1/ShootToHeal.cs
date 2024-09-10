using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using R2API;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class ShootToHeal : Item<ShootToHeal> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Healing});

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            baseHealMod.ToString("0%"), returnHealMod.ToString("0%")
        };



        ////// Config //////

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, AI with nothing better to do will attempt to shoot teammates if they have this item, either of them are injured, and Artifact of Chaos is not enabled.",
            AutoConfigFlags.PreventNetMismatch)]
        public bool aiOverride { get; private set; } = true;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Healing amount multiplier, relative to attack power+proc and target health, per stack.",
            AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseHealMod { get; private set; } = 0.01f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Self-healing amount on healing an ally, relative to the healing the ally received, per stack.",
            AutoConfigFlags.None, 0f, float.MaxValue)]
        public float returnHealMod { get; private set; } = 0.5f;



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
                childName = "LowerArmR",
                localPos = new Vector3(-0.01167F, 0.24784F, -0.08091F),
                localAngles = new Vector3(302.9008F, 102.0814F, 317.6239F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "ForeArmL",
                localPos = new Vector3(0.13678F, -0.03975F, 0.03643F),
                localAngles = new Vector3(56.25161F, 198.1408F, 178.4731F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/shootToHealIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            itemDef.unlockableDef = unlockable;

            R2API.Networking.NetworkingAPI.RegisterMessageType<MsgHealTargetAndSelf>();
        }

        public override void Install() {
            base.Install();
            On.RoR2.BulletAttack.ProcessHit += BulletAttack_ProcessHit;
            On.RoR2.Projectile.ProjectileController.OnCollisionEnter += ProjectileController_OnCollisionEnter;
            On.RoR2.Projectile.ProjectileControllerTrigger.OnTriggerEnter += ProjectileControllerTrigger_OnTriggerEnter;
            IL.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyInputs += BaseAI_UpdateBodyInputs;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.BulletAttack.ProcessHit -= BulletAttack_ProcessHit;
            On.RoR2.Projectile.ProjectileController.OnCollisionEnter -= ProjectileController_OnCollisionEnter;
            On.RoR2.Projectile.ProjectileControllerTrigger.OnTriggerEnter -= ProjectileControllerTrigger_OnTriggerEnter;
            IL.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyInputs -= BaseAI_UpdateBodyInputs;
        }



        ////// Hooks //////

        private void BaseAI_UpdateBodyInputs(On.RoR2.CharacterAI.BaseAI.orig_UpdateBodyInputs orig, RoR2.CharacterAI.BaseAI self) {
            orig(self);
            if(!aiOverride) return;
            if(GetCount(self.master) > 0
                && RunArtifactManager.instance && !RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.friendlyFireArtifactDef)
                && self.hasAimTarget && self.hasAimConfirmation && TeamComponent.GetObjectTeam(self.skillDriverEvaluation.aimTarget.gameObject) == self.master.teamIndex
                && self.bodyInputBank
                && (
                    (self.skillDriverEvaluation.aimTarget.gameObject.TryGetComponent<HealthComponent>(out var otherHC) && otherHC.health < otherHC.fullHealth)
                    || (self.bodyHealthComponent && self.bodyHealthComponent.health < self.bodyHealthComponent.fullHealth)
                )) {
                self.bodyInputBank.skill1.PushState(true);
                self.bodyInputBank.skill2.PushState(true);
                self.bodyInputBank.skill3.PushState(true);
                self.bodyInputBank.skill4.PushState(true);
            }
        }

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
                            var relativeAttackDamage = (self.attacker.TryGetComponent<CharacterBody>(out var acb)) ? (self.damage / acb.damage) : 1f;
                            new MsgHealTargetAndSelf(cpt.healthComponent, ob.healthComponent, count * self.procCoefficient * relativeAttackDamage, self.procChainMask)
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
                var relativeAttackDamage = ob ? (self.damage / ob.damage) : 1f;
                new MsgHealTargetAndSelf(hitInfo.hitHurtBox.healthComponent, ob.healthComponent, count * self.procCoefficient * relativeAttackDamage, self.procChainMask)
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
                var relativeAttackDamage = (ob && self.TryGetComponent<RoR2.Projectile.ProjectileDamage>(out var pd)) ? (pd.damage / ob.damage) : 1f;
                new MsgHealTargetAndSelf(hb.healthComponent, ob.healthComponent, count * self.procCoefficient * relativeAttackDamage, self.procChainMask)
                    .Send(R2API.Networking.NetworkDestination.Server);
            }
        }

        private void ProjectileControllerTrigger_OnTriggerEnter(On.RoR2.Projectile.ProjectileControllerTrigger.orig_OnTriggerEnter orig, RoR2.Projectile.ProjectileControllerTrigger self, Collider collider) {
            orig(self, collider);
            if(collider == null || !collider.gameObject || !self || !self.owner || !self.canImpactOnTrigger) return;
            var hb = collider.gameObject.GetComponent<HurtBox>();
            var ob = self.owner.GetComponent<CharacterBody>();
            var count = GetCount(ob);
            if(hb && hb.healthComponent && count > 0 && hb.healthComponent != self.owner.GetComponent<HealthComponent>() && hb.teamIndex == TeamComponent.GetObjectTeam(self.owner)) {
                var relativeAttackDamage = (ob && self.TryGetComponent<RoR2.Projectile.ProjectileDamage>(out var pd)) ? (pd.damage / ob.damage) : 1f;
                new MsgHealTargetAndSelf(hb.healthComponent, ob.healthComponent, count * self.procCoefficient * relativeAttackDamage, self.procChainMask)
                    .Send(R2API.Networking.NetworkDestination.Server);
            }
        }

        public struct MsgHealTargetAndSelf : INetMessage {
            HealthComponent _target;
            HealthComponent _self;
            float _adjustedCoeffs;
            ProcChainMask _pcm;

            public MsgHealTargetAndSelf(HealthComponent target, HealthComponent self, float amount, ProcChainMask pcm) {
                _target = target;
                _self = self;
                _adjustedCoeffs = amount;
                _pcm = pcm;
            }

            public void OnReceived() {
                var th = _adjustedCoeffs * ShootToHeal.instance.baseHealMod;
                if(_self && _self.body) Defib.instance.healingSourceStack.Push(_self.body);
                _target.Heal(th * _target.fullHealth, _pcm);
                if(_self) _self.Heal(th * _self.fullHealth * ShootToHeal.instance.returnHealMod, _pcm);
                if(_self && _self.body) Defib.instance.healingSourceStack.Pop();
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
                _adjustedCoeffs = reader.ReadSingle();
                _pcm = reader.ReadProcChainMask();
            }

            public void Serialize(NetworkWriter writer) {
                writer.Write(_target.gameObject);
                writer.Write(_self.gameObject);
                writer.Write(_adjustedCoeffs);
                writer.Write(_pcm);
            }
        }
    }

    [RegisterAchievement("TkSat_ShootToHeal", "TkSat_ShootToHealUnlockable", "", 1u)]
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
            var ees = self.GetComponent<ExtraEquipmentStash>();
            if((self.inventory.currentEquipmentIndex == RoR2Content.Equipment.TeamWarCry.equipmentIndex
                || self.inventory.alternateEquipmentIndex == RoR2Content.Equipment.TeamWarCry.equipmentIndex)
                || ees && ees.readOnlyStashedEquipment.Any(x => x.equipmentIndex == RoR2Content.Equipment.TeamWarCry.equipmentIndex)) count++;
            if(self.inventory.GetItemCount(RoR2Content.Items.Behemoth) > 0) count++;
            if(self.inventory.GetItemCount(HurdyGurdy.instance.itemDef) > 0) count++;
            if(count >= 3)
                Grant();
        }
    }
}