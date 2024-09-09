using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using TILER2;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public class KnockbackFinFloat : T2Module<KnockbackFinFloat> {

        ////// Config //////
        
        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Damage multiplier stat of the reworked Knockback Fin attack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFrac { get; private set; } = 1.8f;

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance for reworked Knockback Fin to proc; stacks hyperbolically.", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procChance { get; private set; } = 6f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 10f)]
        [AutoConfig("Vertical hold distance of the reworked Knockback Fin float effect.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pullHeight { get; private set; } = 4f;
        
        [AutoConfigRoOSlider("{0:N0} m", 0f, 10f)]
        [AutoConfig("Radius of random motion of the reworked Knockback Fin float effect.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pullWobble { get; private set; } = 0.5f;
        
        [AutoConfigRoOSlider("{0:N0} m", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the reworked Knockback Fin float effect.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pullTime { get; private set; } = 2f;



        ////// TILER2 Module Setup //////

        public override bool managedEnable => true;

        bool hookSuccess = false;

        public override void Install() {
            base.Install();

            IL.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_ProcessHitEnemy;
        }

        public override void Uninstall() {
            base.Uninstall();

            IL.RoR2.GlobalEventManager.ProcessHitEnemy -= GlobalEventManager_ProcessHitEnemy;
        }

        public override void InstallLanguage() {
            base.InstallLanguage();
            if(hookSuccess) {
                languageOverlays.Add(R2API.LanguageAPI.AddOverlay("ITEM_KNOCKBACKHITENEMIES_PICKUP",
                    Language.GetString("TKSAT_OVERLAY_KNOCKBACKHITENEMIES_PICKUP")));
                languageOverlays.Add(R2API.LanguageAPI.AddOverlay("ITEM_KNOCKBACKHITENEMIES_DESC",
                    Language.GetStringFormatted("TKSAT_OVERLAY_KNOCKBACKHITENEMIES_DESC", procChance.ToString("N0"), pullTime.ToString("N1"), (damageFrac * 100f).ToString("N0"))));
            }
        }



        ////// Hooks //////

        private void GlobalEventManager_ProcessHitEnemy(ILContext il) {
            ILCursor c = new(il);

            if(c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.KnockBackHitEnemies)),
                x => x.MatchCallOrCallvirt(typeof(Inventory), nameof(Inventory.GetItemCount))
                )) {
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldarg_2);
                c.EmitDelegate<Func<int, DamageInfo, GameObject, int>>((itemCount, damageInfo, victim) => {
                    var victimBody = victim ? victim.GetComponent<CharacterBody>() : null;
                    var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    if(victimBody && attackerBody
                    && Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance * (float)itemCount * damageInfo.procCoefficient), attackerBody.master ? attackerBody.master.luck : 0f, null)) {
                        FloatDebuffModule.Inflict(victimBody.healthComponent,
                            new DamageInfo {
                                attacker = damageInfo.attacker,
                                canRejectForce = true,
                                crit = attackerBody.RollCrit(),
                                damage = attackerBody.damage * damageFrac,
                                damageColorIndex = DamageColorIndex.Item,
                                damageType = DamageType.Generic,
                                force = Vector3.zero,
                                inflictor = null,
                                position = victimBody.corePosition,
                                procChainMask = damageInfo.procChainMask,
                                procCoefficient = 0 //no custom ProcChainMask? ,':(
                            }, new FloatDebuffController.FloatDebuffParams {
                                duration = pullTime,
                                height = pullHeight,
                                slamForce = 20f,
                                wobbleForce = 20f,
                                wobbleRadius = pullWobble,
                                wobbleSpeed = 0.5f
                            });
                    }
                    return 0; //bypass default behavior by pretending item count is 0
                });
                hookSuccess = true;
            } else {
                TinkersSatchelPlugin._logger.LogError("KnockbackFinFloat: failed to apply IL hook (GlobalEventManager_ProcessHitEnemy), could not find target instructions. Tweak will not apply.");
                hookSuccess = false;
            }
        }
    }
}