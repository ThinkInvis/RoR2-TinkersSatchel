using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using TILER2;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public class DisposableEquip : Artifact<DisposableEquip> {

        ////// Artifact Data //////

        public override string displayName => "Artifact of Flexibility";

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetDescString(string langid = null) => "Start with 2 Scavenger's Rucksacks. All equipments have no cooldown and are consumed on use. Spawns extra equipment barrels/multishops.";



        ////// Config //////



        ////// TILER2 Module Setup //////

        public DisposableEquip() {
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/DisposableEquip_on.png");
            iconResourceDisabled = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/DisposableEquip_off.png");
        }

        public override void Install() {
            base.Install();
            Run.onPlayerFirstCreatedServer += Run_onPlayerFirstCreatedServer;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        public override void Uninstall() {
            base.Uninstall();
            Run.onPlayerFirstCreatedServer -= Run_onPlayerFirstCreatedServer;
            On.RoR2.EquipmentSlot.PerformEquipmentAction -= EquipmentSlot_PerformEquipmentAction;
        }



        ////// Hooks //////

        private void Run_onPlayerFirstCreatedServer(Run run, PlayerCharacterMasterController pcmc) {
            if(IsActiveAndEnabled() && pcmc && pcmc.master)
                pcmc.master.inventory.GiveItem(ExtraEquipment.instance.catalogIndex, 2);
        }

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef) {
            var retv = orig(self, equipmentDef);
            if(retv && IsActiveAndEnabled())
                self.inventory.SetEquipment(new EquipmentState(EquipmentIndex.None, Run.FixedTimeStamp.now, 0), (uint)self.inventory.activeEquipmentSlot);
            return retv;
        }
    }
}