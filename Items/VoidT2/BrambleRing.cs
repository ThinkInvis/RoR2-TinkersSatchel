using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using R2API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;

namespace ThinkInvisible.TinkersSatchel {
    public class BrambleRing : Item<BrambleRing> {

        ////// Item Data //////

        public override string displayName => "Bramble Ring";
        public override ItemTier itemTier => ItemTier.VoidTier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Damage});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Return some damage to sender. <style=cIsVoid>Corrupts all Negative Feedback Loops</style>.";
        protected override string GetDescString(string langid = null) => $"<style=cIsDamage>{Pct(damageFrac)} of damage taken <style=cStack>(+{Pct(damageFrac)} per stack, hyperbolic)</style></style> is also taken by the inflictor. <style=cIsVoid>Corrupts all Negative Feedback Loops</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Amount of damage to reflect. Stacks hyperbolically.", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float damageFrac { get; private set; } = 0.2f;



        ////// Other Fields/Properties //////
        public DamageAPI.ModdedDamageType damageType;



        ////// TILER2 Module Setup //////

        public BrambleRing() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/BrambleRing.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/brambleRingIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            itemDef.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset")
                .WaitForCompletion();

            On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
                var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                isp.relationships = new[] {new ItemDef.Pair {
                    itemDef1 = DamageBuffer.instance.itemDef,
                    itemDef2 = itemDef
                }};
                orig(providers.Concat(new[] {isp}).ToArray());
            };

            damageType = DamageAPI.ReserveDamageType();
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
            orig(self, damageInfo);
            if(!self || !damageInfo.attacker || damageInfo.HasModdedDamageType(damageType) || !damageInfo.attacker.TryGetComponent<HealthComponent>(out var attackerHC)) return;
            var count = GetCount(self.body);
            if(count > 0) {
                var frac = Mathf.Clamp01(1f - 1f / (1f + damageFrac * (float)count));
                var di = new DamageInfo {
                    attacker = self.gameObject,
                    canRejectForce = true,
                    crit = false,
                    damage = damageInfo.damage * frac,
                    damageColorIndex = DamageColorIndex.Item,
                    force = Vector3.zero,
                    position = attackerHC.body ? attackerHC.body.corePosition : attackerHC.transform.position,
                    procChainMask = damageInfo.procChainMask,
                    procCoefficient = 0f
                };
                di.AddModdedDamageType(damageType);
                attackerHC.TakeDamage(di);
            }
        }
    }
}