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
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Damage});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Return some damage to sender. <style=cIsVoid>Corrupts all Negative Feedback Loops</style>.";
        protected override string GetDescString(string langid = null) => $"<style=cIsDamage>{Pct(damageFrac)} of damage taken <style=cStack>(+{Pct(damageFrac)} per stack, hyperbolic)</style></style> is also taken by the inflictor. <style=cIsVoid>Corrupts all Negative Feedback Loops</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 0.999f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of damage to reflect. Stacks hyperbolically.", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float damageFrac { get; private set; } = 0.2f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 4f)]
        [AutoConfig("Multiplier to damageFrac vs players.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float vsPlayerScaling { get; private set; } = 0.25f;



        ////// Other Fields/Properties //////
        
        public DamageAPI.ModdedDamageType damageType;
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public BrambleRing() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/BrambleRing.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/brambleRingIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/BrambleRing.prefab");
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
                localPos = new Vector3(0.0437F, 0.12845F, -0.01743F),
                localAngles = new Vector3(270.0839F, 334.7889F, 0F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.03648F, 0.18758F, -0.00522F),
                localAngles = new Vector3(0F, 0F, 0F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.01785F, 0.16431F, 0.01802F),
                localAngles = new Vector3(330.1184F, 4.00532F, 351.3972F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Finger21L",
                localPos = new Vector3(-0.00153F, 0.36557F, 0.01404F),
                localAngles = new Vector3(13.40797F, 29.89699F, 356.7747F),
                localScale = new Vector3(1.4F, 1.4F, 1.4F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.01245F, 0.17834F, -0.0126F),
                localAngles = new Vector3(332.5649F, 350.0944F, 10.69863F),
                localScale = new Vector3(0.08F, 0.08F, 0.08F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.02588F, 0.13494F, -0.05978F),
                localAngles = new Vector3(313.258F, 261.7107F, 87.99889F),
                localScale = new Vector3(0.05F, 0.05F, 0.05F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechHandL",
                localPos = new Vector3(-0.02166F, 0.31159F, 0.0611F),
                localAngles = new Vector3(2.51191F, 75.04926F, 333.3042F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
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
                var attackerTeam = TeamComponent.GetObjectTeam(damageInfo.attacker);
                var frac = Mathf.Clamp01(1f - 1f / (1f + damageFrac * ((attackerTeam == TeamIndex.Player) ? vsPlayerScaling: 1f) * (float)count));
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