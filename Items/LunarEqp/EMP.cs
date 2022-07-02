using RoR2;
using UnityEngine;
using TILER2;
using RoR2.Projectile;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
    public class EMP : Equipment<EMP> {

        ////// Equipment Data //////

        public override string displayName => "EMP Device";
        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 60f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Disable skills on enemies... <style=cDeath>BUT disable non-primary skills on survivors.</style>";
        protected override string GetDescString(string langid = null) =>
            $"For {duration:N0} seconds, <style=cIsUtility>all skills</style> on enemies and <style=cIsUtility>non-primary skills</style> on survivors within {range:N0} m will be <color=#FF7F7F>disabled</color>. Also clears <style=cIsDamage>enemy projectiles</style> when used.";
        protected override string GetLoreString(string langid = null) => "";



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
                localPos = new Vector3(0.23353F, -0.00868F, -0.08696F),
                localAngles = new Vector3(27.00084F, 326.5775F, 4.93487F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.6739F, -1.47899F, 1.63122F),
                localAngles = new Vector3(354.4511F, 7.12517F, 355.0916F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.24835F, 0.13692F, 0.12219F),
                localAngles = new Vector3(19.74273F, 338.7649F, 343.2596F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
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
        


        ////// Hooks //////

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            float sqrad = range * range;
            foreach(var tcpt in GameObject.FindObjectsOfType<TeamComponent>()) {
                var deltaPos = tcpt.transform.position - slot.transform.position;
                if(deltaPos.sqrMagnitude <= sqrad) {
                    bool isSurvivor = tcpt.teamIndex == TeamIndex.Player;
                    if(tcpt.body && tcpt.body.skillLocator) {
                        var stsd = tcpt.body.gameObject.GetComponent<ServerTimedSkillDisable>();
                        if(!stsd) stsd = tcpt.body.gameObject.AddComponent<ServerTimedSkillDisable>();
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