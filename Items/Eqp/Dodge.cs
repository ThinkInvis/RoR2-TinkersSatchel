using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;

namespace ThinkInvisible.TinkersSatchel {
    public class Dodge : Equipment<Dodge> {

        ////// Equipment Data //////

        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered { get; protected set; } = true;
        public override float cooldown { get; protected set; } = 10f;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            evadeBurstSpeed.ToString("N0"), evadeBurstSpeedGrounded.ToString("N0"), invulnTime.ToString("N1")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:N1} m/s", 0f, 50f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Instant speed to add on equipment activation.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float evadeBurstSpeed { get; private set; } = 45f;

        [AutoConfigRoOSlider("{0:N1} m/s", 0f, 50f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Instant speed to add on equipment activation.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float evadeBurstSpeedGrounded { get; private set; } = 70f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfig("Duration of the invulnerability effect.", AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever, 0f, float.MaxValue)]
        public float invulnTime { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfig("Minimum cooldown between equipment activations.", AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever, 0f, float.MaxValue)]
        public float icd { get; private set; } = 0.5f;



        ////// TILER2 Module Setup //////

        public Dodge() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Dodge.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/dodgeIcon.png");
        }

        public override void SetupModifyEquipmentDef() {
            base.SetupModifyEquipmentDef();
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            On.RoR2.Inventory.GetEquipmentSlotMaxCharges += Inventory_GetEquipmentSlotMaxCharges;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.Inventory.GetEquipmentSlotMaxCharges -= Inventory_GetEquipmentSlotMaxCharges;
        }



        ////// Hooks //////

        private int Inventory_GetEquipmentSlotMaxCharges(On.RoR2.Inventory.orig_GetEquipmentSlotMaxCharges orig, Inventory self, byte slot) {
            //note: normally unused, UpdateEquipment has the same calculation hardcoded. Tweaks/ModdableEquipmentMaxCharges module fixes this.
            return Math.Min(orig(self, slot) * ((self.GetEquipment(slot).equipmentDef == equipmentDef) ? 3 : 1), 255);
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            if(!slot.characterBody) return false;

            if(slot.characterBody.characterMotor && slot.characterBody.characterDirection) {
                var boostVec = slot.characterBody.characterDirection.forward;
                if(slot.inputBank && slot.inputBank.moveVector != Vector3.zero)
                    boostVec = slot.inputBank.moveVector;
                boostVec.y = 0f;
                boostVec = boostVec.normalized;
                slot.characterBody.characterMotor.velocity = boostVec * (slot.characterBody.characterMotor.isGrounded ? evadeBurstSpeedGrounded : evadeBurstSpeed);
            }

            slot.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            slot.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, invulnTime);

            return true;
        }
    }
}