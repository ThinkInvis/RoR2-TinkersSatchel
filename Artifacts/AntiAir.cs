using RoR2;
using TILER2;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public class AntiAir : Artifact<AntiAir> {

        ////// Config //////

        [AutoConfig("Incoming damage multiplier applied to airborne characters while Artifact of Suppression is active.", AutoConfigFlags.None, 1f, float.MaxValue)]
        public float hurtMod { get; private set; } = 5f;



        ////// TILER2 Module Setup //////

        public AntiAir() {
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/antiair_on.png");
            iconResourceDisabled = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/antiair_off.png");
        }

        public override void Install() {
            base.Install();
            On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.HealthComponent.TakeDamage -= On_HCTakeDamage;
        }



        ////// Hooks //////

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(IsActiveAndEnabled()
                && self.body != null
                && self.body.teamComponent != null
                && self.body.teamComponent.teamIndex == TeamIndex.Player
                && self.body.characterMotor != null
                && !self.body.characterMotor.isGrounded)
                damageInfo.damage *= hurtMod;
            orig(self, damageInfo);
        }
    }
}