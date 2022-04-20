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
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Healing });

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



        ////// TILER2 Module Setup //////

        public BismuthFlask() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/BismuthFlask.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/bismuthFlaskIcon.png");
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