using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    //todo: aim assist with tracking (may need to build velocity table for turret projectiles?), make drones follow aim
    public class BismuthFlask : Item<BismuthFlask> {

        ////// Item Data //////
        
        public override string displayName => "Bismuth Tonic";
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Healing });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Gain resistance when hit by one enemy type... <color=#FF7F7F>BUT gain weakness to the others.</color>";
        protected override string GetDescString(string langid = null) => $"On being hit by one <style=cIsDamage>type of enemy</style>: take <style=cIsHealing>{Pct(resistAmount, 1)} less damage</style> from subsequent attacks from that type, but <style=cIsDamage>{Pct(weakAmount, 1)} more damage</style> from all other types. Wears off after {duration:N0} seconds.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigRoOSlider("{0:P1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fractional damage reduction per stack for the resisted attack type, linear: damage = original / (1 + resistAmount * stacks).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float resistAmount { get; private set; } = 0.125f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fractional damage increase per stack for unresisted attack types, linear: damage = original * (1 + resistAmount * stacks)", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float weakAmount { get; private set; } = 0.2f;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the item's effect.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float duration { get; private set; } = 10f;



        ////// Other Fields/Properties //////

        public BuffDef bismuthFlaskBuff { get; private set; }
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public BismuthFlask() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/BismuthFlask.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/bismuthFlaskIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/BismuthFlask.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.13229F, -0.06816F, 0.11903F),
                localAngles = new Vector3(302.8133F, 27.57598F, 47.27724F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.21951F, 0.2191F, -0.10604F),
                localAngles = new Vector3(22.69201F, 69.13578F, 164.9348F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.10944F, 0.13985F, 0.21713F),
                localAngles = new Vector3(319.5523F, 246.8393F, 99.51362F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(2.28292F, 1.43586F, -0.91167F),
                localAngles = new Vector3(299.1197F, 57.21257F, 51.63428F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(-0.1162F, -0.05832F, 0.18106F),
                localAngles = new Vector3(294.9001F, 247.6723F, 103.4995F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.12451F, 0.13473F, 0.10913F),
                localAngles = new Vector3(294.6539F, 315.7283F, 135.0021F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.10361F, 0.03687F, 0.15813F),
                localAngles = new Vector3(300.456F, 307.2197F, 122.3479F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.12205F, -0.0469F, 0.09215F),
                localAngles = new Vector3(318.3096F, 336.7556F, 109.7998F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.13921F, -0.07294F, 0.10236F),
                localAngles = new Vector3(3.3846F, 343.6192F, 120.1624F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(1.2443F, 3.29632F, -1.388F),
                localAngles = new Vector3(355.6995F, 147.9706F, 208.2942F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FlowerBase",
                localPos = new Vector3(-0.43748F, 0.77839F, 0.59737F),
                localAngles = new Vector3(318.3699F, 161.7631F, 192.5741F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.37926F, 0.04539F, 0.04029F),
                localAngles = new Vector3(19.18333F, 170.5805F, 275.3323F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.16383F, 0.21559F, -0.00468F),
                localAngles = new Vector3(16.63539F, 61.77726F, 186.1561F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            bismuthFlaskBuff = ScriptableObject.CreateInstance<BuffDef>();
            bismuthFlaskBuff.buffColor = Color.white;
            bismuthFlaskBuff.canStack = false;
            bismuthFlaskBuff.isDebuff = false;
            bismuthFlaskBuff.name = "TKSATBismuthFlask";
            bismuthFlaskBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(bismuthFlaskBuff);
        }

        public override void Install() {
            base.Install();

            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }



        ////// Hooks //////

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(self && self.body && damageInfo.attacker && GetCount(self.body) > 0) {
                var atkb = damageInfo.attacker.GetComponent<CharacterBody>();
                if(!atkb) {
                    orig(self, damageInfo);
                    return;
                }
                var cpt = self.body.gameObject.GetComponent<DamageSourceResistanceTracker>();
                if(!cpt) cpt = self.body.gameObject.AddComponent<DamageSourceResistanceTracker>();
                damageInfo.damage = cpt.ModifyDamage(damageInfo.damage, atkb.bodyIndex);
            }
            orig(self, damageInfo);
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class DamageSourceResistanceTracker  : MonoBehaviour {
        BodyIndex lastHitBodyIndex = BodyIndex.None;
        float stopwatch = 0f;

        CharacterBody body;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(stopwatch > 0f) {
                stopwatch -= Time.fixedDeltaTime;
            } else lastHitBodyIndex = BodyIndex.None;
        }

        public float ModifyDamage(float damage, BodyIndex sourceBodyIndex) {
            var count = BismuthFlask.instance.GetCount(body);
            if(count <= 0 || sourceBodyIndex == BodyIndex.None) {
                lastHitBodyIndex = BodyIndex.None;
                return damage;
            }
            if(lastHitBodyIndex != BodyIndex.None) {
                if(lastHitBodyIndex == sourceBodyIndex) {
                    damage /= 1f + BismuthFlask.instance.resistAmount * count;
                } else {
                    damage *= 1f + BismuthFlask.instance.weakAmount * count;
                }
            }
            lastHitBodyIndex = sourceBodyIndex;
            stopwatch = BismuthFlask.instance.duration;
            body.AddTimedBuff(BismuthFlask.instance.bismuthFlaskBuff, stopwatch);
            return damage;
        }
    }
}