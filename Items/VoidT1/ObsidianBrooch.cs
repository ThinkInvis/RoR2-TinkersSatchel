using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
    public class ObsidianBrooch : Item<ObsidianBrooch> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.VoidTier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage, ItemTag.Utility });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            (procChance/100f).ToString("0%"), range.ToString("N0")
        };



        ////// Config ///////
        
        [AutoConfigRoOSlider("{0:N1}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Chance to trigger the effect. Effect can proc once per stack.", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procChance { get; private set; } = 9f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 300f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Range to spread debuffs within.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float range { get; private set; } = 50f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, self-damage will not proc this item.", AutoConfigFlags.PreventNetMismatch)]
        public bool disableSelfDamage { get; private set; } = true;



        ////// Other Fields/Properties //////

        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////
        public ObsidianBrooch() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ObsidianBrooch.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/obsidianBroochIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/ObsidianBrooch.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "ClavicleL",
                localPos = new Vector3(0.00602F, 0.02158F, -0.05845F),
                localAngles = new Vector3(326.667F, 46.21878F, 309.2319F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "ClavicleL",
                localPos = new Vector3(0.17638F, 0.01995F, -0.04308F),
                localAngles = new Vector3(337.2352F, 25.6461F, 235.3091F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.11596F, 0.3307F, 0.15588F),
                localAngles = new Vector3(48.1331F, 358.3615F, 4.73243F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.87653F, 2.20273F, -1.89499F),
                localAngles = new Vector3(305.9784F, 11.63596F, 346.6215F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.14702F, 0.28206F, 0.22331F),
                localAngles = new Vector3(60.55042F, 51.2736F, 61.50257F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.02351F, 0.27867F, 0.14293F),
                localAngles = new Vector3(71.33205F, 332.5947F, 340.854F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(-0.00905F, 0.33928F, 0.44833F),
                localAngles = new Vector3(24.38F, 89.76019F, 89.72921F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.07183F, 0.24613F, 0.09748F),
                localAngles = new Vector3(6.03906F, 119.3976F, 52.03307F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.00041F, 0.26526F, 0.1553F),
                localAngles = new Vector3(59.80264F, 330.3053F, 336.2152F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.06301F, 0.91131F, 3.20592F),
                localAngles = new Vector3(75.99412F, 123.6586F, 127.9823F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(0.02898F, 0.3296F, 0.91459F),
                localAngles = new Vector3(63.96839F, 3.05332F, 351.5714F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(-0.07887F, 0.26481F, 0.29013F),
                localAngles = new Vector3(343.4581F, 64.80383F, 45.3977F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(""),
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.00866F, 0.1104F, 0.20984F),
                localAngles = new Vector3(55.8921F, 64.53103F, 55.9848F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

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

            if(!NetworkServer.active || damageInfo == null || !damageInfo.attacker || (damageInfo.damageType & DamageType.DoT) != 0
                || (disableSelfDamage && damageInfo.attacker == self.gameObject)) return;

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
                if(!Util.CheckRoll(procChance, body.master?.luck ?? 0, body.master)) continue;
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