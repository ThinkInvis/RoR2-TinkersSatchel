using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class TriBrooch : Item<TriBrooch> {

        ////// Item Data //////

        public override string displayName => "Triskelion Brooch";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to combine ignite, freeze, and stun.";
        protected override string GetDescString(string langid = null) => $"<style=cIsDamage>Ignites</style>, <style=cIsDamage>freezes</style>, and <style=cIsUtility>stuns</style> have a {Pct(procBaseChance, 0, 1f)} <style=cStack>(+{Pct(procStackChance, 0, 1f)} per stack)</style> chance to also cause one of the other effects listed for <style=cIsDamage>{Pct(procBaseDamage)} base damage <style=cStack>(+{Pct(procStackDamage)} per stack)</style></style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc chance at first stack (linear, percentage).", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procBaseChance { get; private set; } = 9f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc chance per additional stack (linear, percentage).", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procStackChance { get; private set; } = 9f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc damage, as base damage fraction, at first stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float procBaseDamage { get; private set; } = 1f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc damage, as base damage fraction, per additional stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float procStackDamage { get; private set; } = 0.25f;



        ////// Other Fields/Properties //////

        bool isInternalIgnite = false;



        ////// TILER2 Module Setup //////
        public TriBrooch() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/TriBrooch.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/triBroochIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var unlockable = UnlockableAPI.AddUnlockable<TkSatTriBroochAchievement>();
            LanguageAPI.Add("TKSAT_TRIBROOCH_ACHIEVEMENT_NAME", "Rasputin");
            LanguageAPI.Add("TKSAT_TRIBROOCH_ACHIEVEMENT_DESCRIPTION", "As a team: stun, then freeze, then ignite the same enemy within 3 seconds.");

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

        void InflictBurn(int count, SetStateOnHurt ssoh, DamageReport report) {
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
            ILCursor c = new ILCursor(il);

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<SetStateOnHurt, DamageReport>>((self, report) => {
                if(!self.targetStateMachine || !self.spawnedOverNetwork || !report.attackerBody)
                    return;
                var count = GetCount(report.attackerBody);
                if(count <= 0 || !Util.CheckRoll(procBaseChance + (float)(count - 1) * procStackChance, report.attackerMaster)) return;
                bool isFreeze = (report.damageInfo.damageType & DamageType.Freeze2s) != DamageType.Generic;
                bool isStun = (report.damageInfo.damageType & DamageType.Stun1s) != DamageType.Generic || (report.damageInfo.damageType & DamageType.Shock5s) != DamageType.Generic;
                if(isFreeze) {
                    bool doStun = rng.nextBool;
                    if(doStun) {
                        InflictFreezeOrStun(count, self, report, true);
                    } else {
                        InflictBurn(count, self, report);
                    }
                }
                if(isStun) {
                    bool doFreeze = rng.nextBool;
                    if(doFreeze) {
                        InflictFreezeOrStun(count, self, report, false);
                    } else {
                        InflictBurn(count, self, report);
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
                    if(count <= 0 || !Util.CheckRoll(procBaseChance + (float)(count - 1) * procStackChance, report.attackerMaster)) return;
                    bool doFreeze = rng.nextBool;
                    if(doFreeze) {
                        InflictFreezeOrStun(count, self, report, false);
                    } else {
                        InflictBurn(count, self, report);
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
            if(count <= 0 || !victimBody || !victimSSOH) return;
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

    public class TkSatTriBroochAchievement : RoR2.Achievements.BaseAchievement, IModdedUnlockableDataProvider {
        public string AchievementIdentifier => "TKSAT_TRIBROOCH_ACHIEVEMENT_ID";
        public string UnlockableIdentifier => "TKSAT_TRIBROOCH_UNLOCKABLE_ID";
        public string PrerequisiteUnlockableIdentifier => "";
        public string AchievementNameToken => "TKSAT_TRIBROOCH_ACHIEVEMENT_NAME";
        public string AchievementDescToken => "TKSAT_TRIBROOCH_ACHIEVEMENT_DESCRIPTION";
        public string UnlockableNameToken => TriBrooch.instance.nameToken;

        public Sprite Sprite => TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/triBroochIcon.png");

        public System.Func<string> GetHowToUnlock => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

        public System.Func<string> GetUnlocked => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

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
        void FixedUpdate() {
            if(stunStopwatch > 0f)
                stunStopwatch -= Time.fixedDeltaTime;
            if(freezeStopwatch > 0f)
                freezeStopwatch -= Time.fixedDeltaTime;
        }
    }
}