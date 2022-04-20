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
            $"For {duration:N0} seconds, <style=cIsUtility>all skills</style> on enemies and <style=cIsUtility>non-primary skills</style> on survivors within {range:N0} m will be <color=#FF7F7F>disabled</style>. Also clears <style=cIsDamage>enemy projectiles</style> when used.";
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



        ////// TILER2 Module Setup //////

        public EMP() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/EMP.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/EMPIcon.png");
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