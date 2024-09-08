using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class DamageBuffer : Item<DamageBuffer> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Healing });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            bufferFrac.ToString("0%"), bufferDuration.ToString("N0"), GetBestLanguage(langID).GetLocalizedFormattedStringByToken(bufferRate <= 0f ? "TINKERSSATCHEL_DAMAGEBUFFER_DESC_CTICK" : "TINKERSSATCHEL_DAMAGEBUFFER_DESC_DTICK", bufferRate.ToString("N2"))
        };



        ////// Config ///////

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of damage to convert per stack (linear).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float bufferFrac { get; private set; } = 0.08f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of regen multiplier to add per stack (linear) at full barrier.", AutoConfigFlags.PreventNetMismatch, 0f, 10f)]
        public float regenFrac { get; private set; } = 0.2f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 60f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time over which each barrier instance is delayed, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float bufferDuration { get; private set; } = 3f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Tick interval of the granted barrier, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
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
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.02365F, 0.13215F, -0.05885F),
                localAngles = new Vector3(272.0608F, 159.5406F, 196.2444F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechHandL",
                localPos = new Vector3(-0.0212F, 0.31621F, 0.06463F),
                localAngles = new Vector3(343.0469F, 323.0367F, 1.51606F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.01718F, 0.16946F, -0.02332F),
                localAngles = new Vector3(330.8368F, 17.82179F, 168.1396F),
                localScale = new Vector3(0.04F, 0.04F, 0.04F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(-0.03175F, 0.16708F, 0.0162F),
                localAngles = new Vector3(286.9122F, 184.1758F, 153.3215F),
                localScale = new Vector3(0.05F, 0.05F, 0.05F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Finger21R",
                localPos = new Vector3(0.05146F, 0.64353F, 0.04065F),
                localAngles = new Vector3(342.0906F, 230.3716F, 14.18674F),
                localScale = new Vector3(1.2F, 1.2F, 1.2F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FootFrontL",
                localPos = new Vector3(-0.00757F, 0.29597F, 0.01972F),
                localAngles = new Vector3(355.2064F, 177.3112F, 193.8584F),
                localScale = new Vector3(0.7F, 0.7F, 0.7F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.01945F, 0.12268F, -0.00211F),
                localAngles = new Vector3(275.5996F, 215.576F, 249.6473F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "RingFinger",
                localPos = new Vector3(0.00282F, 0.04216F, 0.00255F),
                localAngles = new Vector3(345.4385F, 92.49622F, 0.94164F),
                localScale = new Vector3(0.08F, 0.08F, 0.08F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.ServerFixedUpdate += HealthComponent_ServerFixedUpdate;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }



        ////// Hooks //////

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            orig(self, damageInfo);
            if(!self || !self.body || damageInfo == null || (damageInfo.damageType & DamageType.FallDamage) != 0) return;
            var count = GetCount(self.body);
            if(count <= 0) return;
            var cpt = self.GetComponent<DelayedBarrierComponent>();
            if(!cpt) cpt = self.gameObject.AddComponent<DelayedBarrierComponent>();
            var frac = bufferFrac * (float)count;
            var reduc = damageInfo.damage * frac;
            cpt.ApplyDamage(reduc);
        }

        private void HealthComponent_ServerFixedUpdate(On.RoR2.HealthComponent.orig_ServerFixedUpdate orig, HealthComponent self, float deltaTime) {
            if(self && self.body)
                self.regenAccumulator +=
                    self.body.regen
                    * (self.barrier / self.fullBarrier)
                    * GetCount(self.body) * regenFrac;
            orig(self, deltaTime);
        }
    }

    [RequireComponent(typeof(HealthComponent))]
    public class DelayedBarrierComponent : MonoBehaviour {
        HealthComponent hc;
        public List<(float curr, float max)> bufferDamage = new();
        float stopwatch = 0f;

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

                    hc.AddBarrier(accum);
                }
            }
        }

        public void ApplyDamage(float amount) {
            if(amount > 0f)
                bufferDamage.Add((amount, amount));
        }
    }
}