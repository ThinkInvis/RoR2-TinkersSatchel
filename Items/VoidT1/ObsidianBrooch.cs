using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using System;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
    public class ObsidianBrooch : Item<ObsidianBrooch> {

        ////// Item Data //////

        public override string displayName => "Obsidian Brooch";
        public override ItemTier itemTier => ItemTier.VoidTier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage, ItemTag.Utility });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to spread DoTs on hit. <style=cIsVoid>Corrupts all Triskelion Brooches</style>.";
        protected override string GetDescString(string langid = null) => $"Whenever you hit an enemy, {procChance:N0}% chance <style=cStack>(rolls once per stack)</style> to mirror one of their <style=cIsDamage>damage-over-time effects</style> to another random enemy within {range:N0} m. <style=cIsVoid>Corrupts all Triskelion Brooches</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////
        
        [AutoConfigRoOSlider("{0:N1}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Chance to trigger the effect. Effect can proc once per stack.", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procChance { get; private set; } = 9f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 300f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Range to spread debuffs within.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float range { get; private set; } = 50f;



        ////// Other Fields/Properties //////



        ////// TILER2 Module Setup //////
        public ObsidianBrooch() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ObsidianBrooch.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/obsidianBroochIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            itemDef.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset")
                .WaitForCompletion();

            On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
                var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                isp.relationships = new[] {new ItemDef.Pair {
                    itemDef1 = TriBrooch.instance.itemDef,
                    itemDef2 = itemDef
                }};
                orig(providers.Concat(new[] { isp }).ToArray());
            };
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
            itemDef.unlockableDef = TriBrooch.unlockable;
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

            if(damageInfo == null || !damageInfo.attacker || (damageInfo.damageType & DamageType.DoT) != 0) return;

            var dc = DotController.FindDotController(self.gameObject);
            if(!dc || dc.dotStackList.Count <= 0) return;

            var body = damageInfo.attacker.GetComponent<CharacterBody>();
            var count = GetCount(body);
            if(count <= 0) return;

            var enemies = GatherEnemies(body.teamComponent.teamIndex, TeamIndex.Neutral)
                .Select(x => MiscUtil.GetRootWithLocators(x.gameObject))
                .Where(obj => {
                    var hc = obj.GetComponent<HealthComponent>();
                    if(!hc || !hc.alive || hc == self) return false;
                    var dvec = (obj.transform.position - self.transform.position);
                    var ddist = dvec.magnitude;
                    if(ddist > range) return false;
                    return true;
                })
                .ToArray();

            if(enemies.Length <= 0) return;

            for(var i = 0; i < count; i++) {
                if(!Util.CheckRoll(procChance, body.master)) continue;
                var tgt = rng.NextElementUniform(enemies);
                var dot = rng.NextElementUniform(dc.dotStackList);
                var idi = new InflictDotInfo {
                    attackerObject = damageInfo.attacker,
                    victimObject = tgt,
                    duration = dot.timer,
                    dotIndex = dot.dotIndex,
                    totalDamage = dot.damage,
                    damageMultiplier = 1f
                };
                DotController.InflictDot(ref idi);
            }
        }
    }
}