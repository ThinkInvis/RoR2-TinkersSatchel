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
        protected override string GetDescString(string langid = null) => $"<style=cIsDamage>Ignites</style>, <style=cIsDamage>freezes</style>, and <style=cIsUtility>stuns</style> have a {Pct(procChance)} <style=cStack>(+{Pct(procChance)} per stack)</style> chance to also cause one of the other effects listed for <style=cIsDamage>{Pct(procBaseDamage)} base damage <style=cStack>(+{Pct(procStackDamage)} per stack)</style></style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc chance per stack (linear, percentage).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float procChance { get; private set; } = 9f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc damage, as base damage fraction, at first stack.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float procBaseDamage { get; private set; } = 1f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc damage, as base damage fraction, per additional stack.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float procStackDamage { get; private set; } = 0.25f;



        ////// Other Fields/Properties //////

        bool isInternalIgnite = false;



        ////// TILER2 Module Setup //////
        public TriBrooch() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/TriBrooch.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/triBroochIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
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
                if(count <= 0) return;
                bool isFreeze = (report.damageInfo.damageType & DamageType.Freeze2s) != DamageType.Generic;
                bool isStun = (report.damageInfo.damageType & DamageType.Stun1s) != DamageType.Generic;
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
                    if(count <= 0 || !Util.CheckRoll(procChance * (float)count, report.attackerMaster)) return;
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
            var count = GetCount(inflictDotInfo.attackerObject.GetComponent<CharacterBody>());
            if(count <= 0 || !victimBody || !victimSSOH) return;
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
            var attackerDamage = inflictDotInfo.attackerObject.GetComponent<CharacterBody>()?.damage ?? inflictDotInfo.totalDamage ?? 1f;
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
}