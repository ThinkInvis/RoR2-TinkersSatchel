using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class DamageBuffer : Item<DamageBuffer> {

        ////// Item Data //////

        public override string displayName => "Negative Feedback Loop";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Healing });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Some incoming damage is dealt over time.";
        protected override string GetDescString(string langid = null) => $"<style=cIsDamage>{Pct(bufferFrac)} <style=cStack>(+{Pct(bufferFrac)} per stack, hyperbolic)</style> of incoming damage</style> is <style=cIsHealing>applied gradually</style> over {bufferDuration} seconds, ticking {(bufferRate <= 0f ? "continuously" : $"every {bufferRate:N2} seconds")}. <style=cIsHealing>Healing</style> past <style=cIsHealth>max health</style> <style=cIsHealing>will apply</style> to the pool of delayed damage.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of damage to absorb per stack (hyperbolic).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float bufferFrac { get; private set; } = 0.2f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 60f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time over which each damage instance is delayed, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float bufferDuration { get; private set; } = 5f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Tick interval of the damage buffer, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float bufferRate { get; private set; } = 0f;



        ////// Other Fields/Properties

        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public DamageBuffer() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/DamageBuffer.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/damageBufferIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/DamageBuffer.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.04446F, 0.12915F, -0.01916F),
                localAngles = new Vector3(277.754F, 43.95369F, 323.1906F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.03663F, 0.19062F, -0.00511F),
                localAngles = new Vector3(3.42107F, 256.1434F, 351.7726F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.01744F, 0.16387F, 0.01867F),
                localAngles = new Vector3(323.7762F, 23.74602F, 343.3208F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Finger21L",
                localPos = new Vector3(-0.06918F, 0.36992F, 0.01639F),
                localAngles = new Vector3(4.23909F, 273.218F, 357.2444F),
                localScale = new Vector3(1.5F, 1.5F, 1.5F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.01514F, 0.17934F, -0.01492F),
                localAngles = new Vector3(329.151F, 354.4431F, 1.15425F),
                localScale = new Vector3(0.08F, 0.08F, 0.08F)
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
        }

        public override void Install() {
            base.Install();
            IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;
        }

        public override void Uninstall() {
            base.Uninstall();
            IL.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
            IL.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
        }



        ////// Hooks //////

        private void HealthComponent_TakeDamage(ILContext il) {
            ILCursor c = new(il);

            int locIndex = 0;
            if(c.TryGotoNext(
                x => x.MatchLdloc(out locIndex),
                x => x.MatchLdcR4(0),
                x => x.MatchBleUn(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.barrier))
                )
                && c.TryGotoPrev(MoveType.After,
                x => x.MatchStloc((byte)locIndex))
                ) {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldloc_S, (byte)locIndex);
                c.EmitDelegate<Func<HealthComponent, DamageInfo, float, float>>((hc, damageInfo, origFinalDamage) => {
                    if(!hc || damageInfo == null || (damageInfo.damageType & DamageType.FallDamage) != 0) return origFinalDamage;
                    var count = GetCount(hc.body);
                    if(count <= 0) return origFinalDamage;
                    var cpt = hc.GetComponent<DelayedDamageBufferComponent>();
                    if(!cpt) cpt = hc.gameObject.AddComponent<DelayedDamageBufferComponent>();
                    if(cpt.isApplying) return origFinalDamage;
                    var frac = Mathf.Clamp01(1f-1f/(1f + bufferFrac * (float)count));
                    var reduc = origFinalDamage * frac;
                    cpt.ApplyDamage(reduc);
                    return origFinalDamage - reduc;
                });
                c.Emit(OpCodes.Stloc_S, (byte)locIndex);
            } else {
                TinkersSatchelPlugin._logger.LogError("Failed to apply IL patch (target instructions not found): DamageBuffer::HealthComponent_TakeDamage");
            }
        }

        private void HealthComponent_Heal(ILContext il) {
            ILCursor c = new(il);

            int locIndex = 0;
            if(c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(out locIndex),
                x => x.MatchLdcR4(0),
                x => x.MatchBleUn(out _),
                x => x.MatchLdarg(out _),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(out _),
                x => x.MatchLdflda<HealthComponent>(nameof(HealthComponent.itemCounts)),
                x => x.MatchLdfld<HealthComponent.ItemCounts>(nameof(HealthComponent.ItemCounts.barrierOnOverHeal))
                )) {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, locIndex);
                c.EmitDelegate<Action<HealthComponent, float>>((hc, overheal) => {
                    if(!hc) return;
                    var cpt = hc.GetComponent<DelayedDamageBufferComponent>();
                    if(cpt)
                        cpt.ApplyOverheal(overheal);
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("Failed to apply IL patch (target instructions not found): DamageBuffer::HealthComponent_Heal");
            }
        }
    }

    [RequireComponent(typeof(HealthComponent))]
    public class DelayedDamageBufferComponent : MonoBehaviour {
        HealthComponent hc;
        public List<(float curr, float max)> bufferDamage = new();
        float stopwatch = 0f;
        public bool isApplying { get; private set; } = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            hc = GetComponent<HealthComponent>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(bufferDamage.Count > 0) {
                stopwatch -= Time.fixedDeltaTime;
                if(stopwatch <= 0f) {
                    stopwatch = DamageBuffer.instance.bufferRate;
                    float accum = 0f;
                    var frac = Mathf.Max(DamageBuffer.instance.bufferRate, Time.fixedDeltaTime) / DamageBuffer.instance.bufferDuration;
                    for(var i = 0; i < bufferDamage.Count; i++) {
                        var rem = Mathf.Min(bufferDamage[i].max * frac, bufferDamage[i].curr);
                        accum += rem;
                        bufferDamage[i] = (bufferDamage[i].curr - rem, bufferDamage[i].max);
                    }
                    bufferDamage.RemoveAll(x => x.curr <= 0f);
                    isApplying = true;

                    if(accum > 0f && hc.barrier > 0f) {
                        if(accum <= hc.barrier) {
                            hc.Networkbarrier = hc.barrier - accum;
                            accum = 0f;
                        } else {
                            accum -= hc.barrier;
                            hc.Networkbarrier = 0f;
                        }
                    }
                    if(accum > 0f && hc.shield > 0f) {
                        if(accum <= hc.shield) {
                            hc.Networkshield = hc.shield - accum;
                            accum = 0f;
                        } else {
                            accum -= hc.shield;
                            hc.Networkshield = 0f;
                            EffectManager.SpawnEffect(HealthComponent.AssetReferences.shieldBreakEffectPrefab, new EffectData {
                                origin = base.transform.position,
                                scale = hc.body ? hc.body.radius : 1f
                            }, true);
                        }
                    }
                    if(accum > 0f)
                        hc.Networkhealth -= accum;

                    isApplying = false;
                }
            }
        }

        public void ApplyDamage(float amount) {
            if(amount > 0f)
                bufferDamage.Add((amount, amount));
        }

        public void ApplyOverheal(float amount) {
            if(bufferDamage.Count == 0 || amount <= 0f) return;
            var total = bufferDamage.Sum(x => x.curr);
            var frac = amount / total;
            for(var i = 0; i < bufferDamage.Count; i++) {
                var reduc = bufferDamage[i].curr * frac;
                var remaining = bufferDamage[i].curr - reduc;
                bufferDamage[i] = (remaining, Mathf.Max(bufferDamage[i].max - reduc, remaining));
            }
            bufferDamage.RemoveAll(x => x.curr <= 0f);
        }
    }
}