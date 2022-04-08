using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
    public class EMP : Equipment<EMP> {

        ////// Equipment Data //////

        public override string displayName => "EMP Device";
        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered => false;
        public override float cooldown {get; protected set;} = 60f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Disable skills on enemies... <style=cDeath>BUT disable non-primary skills on survivors.</style>";
        protected override string GetDescString(string langid = null) =>
            $"For 10 seconds, <style=cIsUtility>all skills</style> on enemies and <style=cIsUtility>non-primary skills</style> on survivors within 100 m will be <color=#FF7F7F>disabled</style>.";
        protected override string GetLoreString(string langid = null) => "";



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
            float sqrad = 100f * 100f;
            foreach(var tcpt in GameObject.FindObjectsOfType<TeamComponent>()) {
                var deltaPos = tcpt.transform.position - slot.transform.position;
                if(deltaPos.sqrMagnitude <= sqrad) {
                    bool isSurvivor = tcpt.teamIndex == TeamIndex.Player;
                    if(tcpt.body && tcpt.body.skillLocator) {
                        var stsd = tcpt.body.gameObject.GetComponent<ServerTimedSkillDisable>();
                        if(!stsd) stsd = tcpt.body.gameObject.AddComponent<ServerTimedSkillDisable>();
                        if(!isSurvivor) {
                            stsd.ServerApply(10f, SkillSlot.Primary);
                        }
                        stsd.ServerApply(10f, SkillSlot.Secondary);
                        stsd.ServerApply(10f, SkillSlot.Utility);
                        stsd.ServerApply(10f, SkillSlot.Special);
                    }
                }
            }

            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/FusionCellExplosion"), new EffectData {
                origin = slot.characterBody ? slot.characterBody.corePosition : slot.transform.position,
                scale = 100f,
                color = Color.cyan
            }, true);

            return true;
        }
    }
}