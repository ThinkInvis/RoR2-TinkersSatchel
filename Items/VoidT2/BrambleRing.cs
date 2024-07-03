﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using System.Linq;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using RoR2.Orbs;

namespace ThinkInvisible.TinkersSatchel {
    public class BrambleRing : Item<BrambleRing> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.VoidTier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Damage});

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            damageFrac.ToString("0%"), barrierFrac.ToString("0%")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 0.999f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of damage to reflect. Will be doubled by bleed proc. Stacks hyperbolically.", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float damageFrac { get; private set; } = 0.08f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 0.999f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of damage to convert to barrier. Stacks hyperbolically.", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float barrierFrac { get; private set; } = 0.08f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 4f)]
        [AutoConfig("Multiplier to damageFrac vs players.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float vsPlayerScaling { get; private set; } = 0.25f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc coefficient of the retaliation attack.", AutoConfigFlags.None, 0f, 1f)]
        public float procCoefficient { get; private set; } = 0f;



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
                childName = "HandL",
                localPos = new Vector3(0.01728F, 0.1692F, -0.02313F),
                localAngles = new Vector3(327.2125F, 18.24322F, 166.5908F),
                localScale = new Vector3(0.04F, 0.04F, 0.04F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(-0.03269F, 0.16912F, 0.01925F),
                localAngles = new Vector3(323.6763F, 241.3513F, 106.1961F),
                localScale = new Vector3(0.05F, 0.05F, 0.05F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Finger21R",
                localPos = new Vector3(0.03214F, 0.65431F, 0.06196F),
                localAngles = new Vector3(352.9313F, 353.4738F, 339.2639F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FootFrontL",
                localPos = new Vector3(0.01097F, 0.28171F, 0.02874F),
                localAngles = new Vector3(3.52974F, 348.55F, 352.2132F),
                localScale = new Vector3(0.7F, 0.7F, 0.7F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.01874F, 0.12282F, -0.0035F),
                localAngles = new Vector3(296.2707F, 22.61896F, 82.80862F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "RingFinger",
                localPos = new Vector3(0.00702F, 0.04274F, 0.00275F),
                localAngles = new Vector3(3.66348F, 8.43588F, 5.24985F),
                localScale = new Vector3(0.06F, 0.06F, 0.06F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

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
            if(!self || !self.alive) { orig(self, damageInfo); return; }
            var barrierPre = self.barrier;
            orig(self, damageInfo);
            var count = GetCount(self.body);
            if(count <= 0) return;

            //barrier retaliation
            var deltaBarrier = self.barrier - barrierPre;
            if(deltaBarrier < 0 && damageInfo.attacker && !damageInfo.HasModdedDamageType(damageType) && damageInfo.attacker.TryGetComponent<HealthComponent>(out var attackerHC) && attackerHC.body) {
                var attackerTeam = TeamComponent.GetObjectTeam(damageInfo.attacker);
                var frac = Mathf.Clamp01(1f - 1f / (1f + damageFrac * ((attackerTeam == TeamIndex.Player) ? vsPlayerScaling : 1f) * (float)count));

                var vlo = new GenericDamageOrb {
                    origin = damageInfo.position,
                    damageValue = deltaBarrier * frac,
                    damageType = default,
                    isCrit = false,
                    teamIndex = self.body.teamComponent.teamIndex,
                    attacker = self.gameObject,
                    target = attackerHC.body.mainHurtBox,
                    procCoefficient = procCoefficient,
                    procChainMask = default,
                    damageColorIndex = DamageColorIndex.Item
                };
                vlo.AddModdedDamageType(damageType);
                OrbManager.instance.AddOrb(vlo);

                DotController.InflictDot(damageInfo.attacker, self.gameObject, DotController.DotIndex.Bleed, 3f, deltaBarrier * frac / self.body.damage);
            }

            //conversion to barrier
            self.AddBarrier(damageInfo.damage * Mathf.Clamp01(1f - 1f / (1f + barrierFrac * (float)count)));
        }
    }
}