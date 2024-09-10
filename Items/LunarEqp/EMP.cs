using RoR2;
using UnityEngine;
using TILER2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class EMP : Equipment<EMP> {

        ////// Equipment Data //////

        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 60f;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            duration.ToString("N0"), range.ToString("N0")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:N0} m", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Range of all equipment effects.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float range { get; private set; } = 100f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 60f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of skill disable.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float duration { get; private set; } = 10f;



        ////// Other Fields/Properties /////

        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public EMP() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/EMP.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/EMPIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/EMP.prefab");
        }

        public override void SetupModifyEquipmentDef() {
            base.SetupModifyEquipmentDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.1694F, 0.00347F, 0.13829F),
                localAngles = new Vector3(6.65467F, 133.3017F, 75.57108F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.38728F, 0.00965F, -0.06446F),
                localAngles = new Vector3(333.5912F, 202.3656F, 70.59932F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.20068F, 0.08115F, -0.06248F),
                localAngles = new Vector3(344.8479F, 220.4551F, 79.18226F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-1.51252F, -0.99778F, 1.23513F),
                localAngles = new Vector3(32.2198F, 58.24542F, 114.4344F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.24835F, 0.13692F, 0.12219F),
                localAngles = new Vector3(341.0877F, 311.8262F, 263.4802F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.16964F, 0.13347F, 0.09027F),
                localAngles = new Vector3(351.428F, 162.9317F, 78.83822F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.2218F, -0.18057F, -0.12922F),
                localAngles = new Vector3(356.4394F, 210.3498F, 85.3334F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.17444F, -0.09371F, -0.00565F),
                localAngles = new Vector3(7.12427F, 353.5861F, 242.4066F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.19328F, 0.01618F, -0.01016F),
                localAngles = new Vector3(22.64412F, 356.1034F, 258.6612F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(2.02937F, 0.31588F, 0.7402F),
                localAngles = new Vector3(314.2263F, 190.9837F, 83.7415F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(0.75783F, 0.18496F, -0.42217F),
                localAngles = new Vector3(351.9894F, 202.5631F, 104.2369F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.16242F, 0.09494F, 0.06865F),
                localAngles = new Vector3(341.4641F, 166.4982F, 79.28785F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Center",
                localPos = new Vector3(0.13884F, -0.01109F, 0.05046F),
                localAngles = new Vector3(328.3857F, 173.8581F, 67.26126F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }
        


        ////// Hooks //////

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            float sqrad = range * range;
            foreach(var cb in CharacterBody.readOnlyInstancesList) {
                var deltaPos = cb.transform.position - slot.transform.position;
                if(deltaPos.sqrMagnitude <= sqrad) {
                    bool isSurvivor = cb.teamComponent.teamIndex == TeamIndex.Player;
                    if(cb.skillLocator) {
                        var stsd = cb.gameObject.GetComponent<ServerTimedSkillDisable>();
                        if(!stsd) stsd = cb.gameObject.AddComponent<ServerTimedSkillDisable>();
                        if(!isSurvivor) {
                            stsd.ServerApply(duration, SkillSlot.Primary);
                        }
                        stsd.ServerApply(duration, SkillSlot.Secondary);
                        stsd.ServerApply(duration, SkillSlot.Utility);
                        stsd.ServerApply(duration, SkillSlot.Special);
                    }
                }
            }

            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FusionCellExplosion"), new EffectData {
                origin = slot.characterBody ? slot.characterBody.corePosition : slot.transform.position,
                scale = range,
                color = Color.cyan
            }, true);
            
            var myTeam = slot.teamComponent.teamIndex;
            var toDelete = new List<ProjectileController>();
            foreach(var projectile in InstanceTracker.GetInstancesList<ProjectileController>()) {
                if(!projectile.cannotBeDeleted
                    && projectile.teamFilter.teamIndex != myTeam
                    && (projectile.transform.position - (slot.characterBody ? slot.characterBody.corePosition : slot.transform.position)).sqrMagnitude < sqrad)
                    toDelete.Add(projectile);
            }
            for(int i = toDelete.Count - 1; i >= 0; i--)
                GameObject.Destroy(toDelete[i].gameObject);

            return true;
        }
    }
}