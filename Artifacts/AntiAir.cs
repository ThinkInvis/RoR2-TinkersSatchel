using RoR2;
using TILER2;

namespace ThinkInvisible.TinkersSatchel {
    public class AntiAir : Artifact<AntiAir> {
        public override string displayName => "Artifact of Suppression";

        [AutoItemConfig("Incoming damage multiplier applied to airborne characters while Artifact of Suppression is active.", AutoItemConfigFlags.None, 1f, float.MaxValue)]
        public float hurtMod {get; private set;} = 5f;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangDesc(string langid = null) => "Players take heavily increased damage while airborne.";

        public AntiAir() {
            iconPathName = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/antiair_on.png";
            iconPathNameDisabled = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/antiair_off.png";
        }

        protected override void LoadBehavior() {
            On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }

        protected override void UnloadBehavior() {
            On.RoR2.HealthComponent.TakeDamage -= On_HCTakeDamage;
        }

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(IsActiveAndEnabled()
                && self.body.teamComponent.teamIndex == TeamIndex.Player
                && !self.body.characterMotor.isGrounded)
                damageInfo.damage *= hurtMod;
            orig(self, damageInfo);
        }
    }
}