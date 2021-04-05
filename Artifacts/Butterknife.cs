using R2API.Utils;
using RoR2;
using TILER2;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public class Butterknife : Artifact_V2<Butterknife> {
        public override string displayName => "Artifact of Haste";

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetDescString(string langid = null) => "All combatants attack 10x faster and deal 1/20x damage.";

        private System.Reflection.MethodInfo cbDamageSetter;
        private System.Reflection.MethodInfo cbAttackSetter;

        public Butterknife() {
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/butterknife_on.png");
            iconResourceDisabled = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/butterknife_off.png");
            cbDamageSetter = typeof(CharacterBody).GetPropertyCached("damage").GetSetMethod(true);
            cbAttackSetter = typeof(CharacterBody).GetPropertyCached("attackSpeed").GetSetMethod(true);
        }

        public override void Install() {
            base.Install();
            On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.CharacterBody.RecalculateStats -= On_CBRecalcStats;
        }

        private void On_CBRecalcStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
            orig(self);
            if(IsActiveAndEnabled()) {
                cbDamageSetter.Invoke(self, new object[] {self.damage / 20f});
                cbAttackSetter.Invoke(self, new object[] {self.attackSpeed * 10f});
            }
        }
    }
}