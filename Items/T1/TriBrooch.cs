using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class TriBrooch : Item<TriBrooch> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            (procBaseChance/100f).ToString("P0"), (procStackChance/100f).ToString("P0"), procBaseDamage.ToString("P0"), procStackDamage.ToString("P0")
        };



        ////// Config ///////

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc chance at first stack (linear, percentage).", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procBaseChance { get; private set; } = 9f;

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc chance per additional stack (linear, percentage).", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procStackChance { get; private set; } = 9f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc damage, as base damage fraction, at first stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float procBaseDamage { get; private set; } = 1f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc damage, as base damage fraction, per additional stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float procStackDamage { get; private set; } = 0.25f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, will never proc from self damage.", AutoConfigFlags.PreventNetMismatch)]
        public bool preventSelfProc { get; private set; } = false;



        ////// Other Fields/Properties //////

        bool isInternalIgnite = false;
        public static UnlockableDef unlockable { get; private set; }
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////
        public TriBrooch() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/TriBrooch.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/triBroochIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/TriBrooch.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "ClavicleL",
                localPos = new Vector3(0.01587F, 0.03075F, -0.0454F),
                localAngles = new Vector3(12.95898F, 265.3185F, 111.5319F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "ClavicleL",
                localPos = new Vector3(0.18501F, 0.0208F, -0.04388F),
                localAngles = new Vector3(336.0666F, 351.0449F, 287.884F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.10938F, 0.3308F, 0.16379F),
                localAngles = new Vector3(77.74458F, 9.37155F, 56.06641F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.85748F, 2.27521F, -2.00212F),
                localAngles = new Vector3(74.22634F, 295.739F, 150.861F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.14128F, 0.28981F, 0.22255F),
                localAngles = new Vector3(30.07084F, 237.6799F, 307.643F),
                localScale = new Vector3(0.15F, 0.15F, 0.15F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.0258F, 0.2852F, 0.1511F),
                localAngles = new Vector3(72.71474F, 228.6517F, 288.4894F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(-0.00898F, 0.34109F, 0.45765F),
                localAngles = new Vector3(30.64921F, 124.9147F, 150.8836F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "ClavicleR",
                localPos = new Vector3(-0.08299F, -0.02196F, 0.11205F),
                localAngles = new Vector3(328.7179F, 352.2592F, 110.2025F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.00012F, 0.26978F, 0.16404F),
                localAngles = new Vector3(75.41711F, 88.73151F, 126.3319F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.05477F, 0.9587F, 3.27361F),
                localAngles = new Vector3(29.15617F, 134.4333F, 169.5063F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(0.0312F, 0.31848F, 0.94812F),
                localAngles = new Vector3(323.8579F, 146.4679F, 83.51678F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(-0.08226F, 0.27109F, 0.2853F),
                localAngles = new Vector3(56.90765F, 35.79788F, 94.39868F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.00823F, 0.11072F, 0.21924F),
                localAngles = new Vector3(41.15704F, 115.7881F, 117.0112F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/triBroochIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            itemDef.unlockableDef = unlockable;
        }

        public override void Install() {
            base.Install();
            IL.RoR2.SetStateOnHurt.OnTakeDamageServer += SetStateOnHurt_OnTakeDamageServer;
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
        }

        public override void Uninstall() {
            base.Uninstall();
            IL.RoR2.SetStateOnHurt.OnTakeDamageServer -= SetStateOnHurt_OnTakeDamageServer;
            On.RoR2.DotController.InflictDot_refInflictDotInfo -= DotController_InflictDot_refInflictDotInfo;
        }



        ////// Private Methods //////
        
        void InflictFreezeOrStun(int count, SetStateOnHurt ssoh, DamageReport report, bool isStun) {
            if(!report.victim.isInFrozenState) {
                if(isStun) {
                    if(ssoh.canBeStunned)
                        ssoh.SetStun(1f);
                } else {
                    if(ssoh.canBeFrozen)
                        ssoh.SetFrozen(2f * report.damageInfo.procCoefficient);
                }
            }
            report.victim.TakeDamage(new DamageInfo {
                attacker = report.attacker,
                crit = report.damageInfo.crit,
                damage = ((float)(count - 1) * procStackDamage + procBaseDamage) * report.attackerBody.damage,
                damageColorIndex = DamageColorIndex.Item,
                damageType = DamageType.Generic,
                force = Vector3.zero,
                inflictor = report.damageInfo.inflictor,
                position = report.damageInfo.position,
                procChainMask = report.damageInfo.procChainMask,
                procCoefficient = 0f
            });
        }

        void InflictBurn(int count, DamageReport report) {
            var dot = new InflictDotInfo {
                victimObject = report.victim.gameObject,
                attackerObject = report.attacker,
                totalDamage = new float?(((float)(count - 1) * procStackDamage + procBaseDamage) * report.attackerBody.damage),
                dotIndex = DotController.DotIndex.Burn,
                damageMultiplier = 1f
            };
            if(report.attackerMaster)
                StrengthenBurnUtils.CheckDotForUpgrade(report.attackerMaster.inventory, ref dot);
            isInternalIgnite = true;
            DotController.InflictDot(ref dot);
            isInternalIgnite = false;
        }



        ////// Hooks //////

        private void SetStateOnHurt_OnTakeDamageServer(ILContext il) {
            ILCursor c = new(il);

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<SetStateOnHurt, DamageReport>>((self, report) => {
                if(!self.targetStateMachine || !self.spawnedOverNetwork || !report.attackerBody)
                    return;
                var count = GetCount(report.attackerBody);
                if(count <= 0 || (preventSelfProc && report.attacker == report.victim) || !Util.CheckRoll(procBaseChance + (float)(count - 1) * procStackChance, report.attackerMaster)) return;
                bool isFreeze = (report.damageInfo.damageType & DamageType.Freeze2s) != DamageType.Generic;
                bool isStun = (report.damageInfo.damageType & DamageType.Stun1s) != DamageType.Generic || (report.damageInfo.damageType & DamageType.Shock5s) != DamageType.Generic;
                if(isFreeze) {
                    bool doStun = rng.nextBool;
                    if(doStun) {
                        InflictFreezeOrStun(count, self, report, true);
                    } else {
                        InflictBurn(count, report);
                    }
                }
                if(isStun) {
                    bool doFreeze = rng.nextBool;
                    if(doFreeze) {
                        InflictFreezeOrStun(count, self, report, false);
                    } else {
                        InflictBurn(count, report);
                    }
                }
            });

            if(c.TryGotoNext(MoveType.Before,
                x => x.MatchLdstr("Prefabs/Effects/ImpactEffects/ImpactStunGrenade")
                )) {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Action<SetStateOnHurt, DamageReport>>((self, report) => {
                    if(report == null || !report.attackerBody || !report.victimBody) return;
                    var count = GetCount(report.attackerBody);
                    if(count <= 0 || (preventSelfProc && report.attacker == report.victim) || !Util.CheckRoll(procBaseChance + (float)(count - 1) * procStackChance, report.attackerMaster)) return;
                    bool doFreeze = rng.nextBool;
                    if(doFreeze) {
                        InflictFreezeOrStun(count, self, report, false);
                    } else {
                        InflictBurn(count, report);
                    }
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("TriBrooch failed to apply IL hook (SSOH OnTakeDamageServer): target instructions not found (ldstr).");
            }
        }

        private void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo) {
            orig(ref inflictDotInfo);
            if(isInternalIgnite || !inflictDotInfo.attackerObject || !inflictDotInfo.victimObject) return;
            var victimBody = inflictDotInfo.victimObject.GetComponent<CharacterBody>();
            var victimSSOH = inflictDotInfo.victimObject.GetComponent<SetStateOnHurt>();
            var atkb = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
            var count = GetCount(atkb);
            if(count <= 0 || !victimBody || !victimSSOH || (preventSelfProc && victimBody == atkb)) return;
            if(!Util.CheckRoll(procBaseChance + (float)(count - 1) * procStackChance, atkb.master))
                return;
            bool isStun = rng.nextBool;
            if(!victimBody.healthComponent.isInFrozenState) {
                if(isStun) {
                    if(victimSSOH.canBeStunned)
                        victimSSOH.SetStun(1f);
                } else {
                    if(victimSSOH.canBeFrozen)
                        victimSSOH.SetFrozen(2f);
                }
            }
            var attackerDamage = 1f;
            if(inflictDotInfo.attackerObject) {
                var body = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
                if(body)
                    attackerDamage = body.damage;
            } else if(inflictDotInfo.totalDamage != null) {
                attackerDamage = inflictDotInfo.totalDamage.Value;
            }
            victimBody.healthComponent.TakeDamage(new DamageInfo {
                attacker = inflictDotInfo.attackerObject,
                crit = false,
                damage = ((float)(count - 1) * procStackDamage + procBaseDamage) * attackerDamage,
                damageColorIndex = DamageColorIndex.Item,
                damageType = DamageType.Generic,
                force = Vector3.zero,
                position = victimBody.corePosition,
                procChainMask = default,
                procCoefficient = 0f
            });
        }
    }

    [RegisterAchievement("TkSat_TriBrooch", "TkSat_TriBroochUnlockable", "")]
    public class TkSatTriBroochAchievement : RoR2.Achievements.BaseAchievement {
        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.SetStateOnHurt.SetStunInternal += SetStateOnHurt_SetStunInternal;
            On.RoR2.SetStateOnHurt.SetFrozenInternal += SetStateOnHurt_SetFrozenInternal;
            On.RoR2.CharacterBody.OnClientBuffsChanged += CharacterBody_OnClientBuffsChanged;
        }

        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.SetStateOnHurt.SetStunInternal -= SetStateOnHurt_SetStunInternal;
            On.RoR2.SetStateOnHurt.SetFrozenInternal -= SetStateOnHurt_SetFrozenInternal;
            On.RoR2.CharacterBody.OnClientBuffsChanged -= CharacterBody_OnClientBuffsChanged;
        }

        private void SetStateOnHurt_SetStunInternal(On.RoR2.SetStateOnHurt.orig_SetStunInternal orig, SetStateOnHurt self, float duration) {
            orig(self, duration);
            var cpt = self.gameObject.GetComponent<TriBroochAchievementTracker>();
            if(!cpt) cpt = self.gameObject.AddComponent<TriBroochAchievementTracker>();
            cpt.stunStopwatch = 3f;
        }

        private void SetStateOnHurt_SetFrozenInternal(On.RoR2.SetStateOnHurt.orig_SetFrozenInternal orig, SetStateOnHurt self, float duration) {
            orig(self, duration);
            var cpt = self.gameObject.GetComponent<TriBroochAchievementTracker>();
            if(!cpt) cpt = self.gameObject.AddComponent<TriBroochAchievementTracker>();
            if(cpt.stunStopwatch <= 0f) {
                cpt.freezeStopwatch = 0f;
            } else {
                cpt.freezeStopwatch = 3f;
            }
        }

        private void CharacterBody_OnClientBuffsChanged(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody self) {
            orig(self);
            if(self.HasBuff(RoR2Content.Buffs.OnFire)) {
                var cpt = self.gameObject.GetComponent<TriBroochAchievementTracker>();
                if(cpt) {
                    if(cpt.stunStopwatch > 0f && cpt.freezeStopwatch > 0f) {
                        Grant();
                    } else {
                        cpt.stunStopwatch = 0f;
                        cpt.freezeStopwatch = 0f;
                    }
                }
            }
        }
    }

    public class TriBroochAchievementTracker : MonoBehaviour {
        public float stunStopwatch = 0f;
        public float freezeStopwatch = 0f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(stunStopwatch > 0f)
                stunStopwatch -= Time.fixedDeltaTime;
            if(freezeStopwatch > 0f)
                freezeStopwatch -= Time.fixedDeltaTime;
        }
    }
}