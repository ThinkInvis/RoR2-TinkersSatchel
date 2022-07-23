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
            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
        }

        public override void Uninstall() {
            base.Uninstall();
            Run.onPlayerFirstCreatedServer -= Run_onPlayerFirstCreatedServer;
            On.RoR2.EquipmentSlot.PerformEquipmentAction -= EquipmentSlot_PerformEquipmentAction;
            On.RoR2.SceneDirector.PopulateScene -= SceneDirector_PopulateScene;
        }



        ////// Hooks //////

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self) {
            orig(self);
            if(!Run.instance || !IsActiveAndEnabled()) return;
            var dpr = new DirectorPlacementRule { placementMode = DirectorPlacementRule.PlacementMode.Random };
            var sc1 = LegacyResourcesAPI.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscEquipmentBarrel");
            var sc2 = LegacyResourcesAPI.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscTripleShopEquipment");
            var participants = Run.instance.participatingPlayerCount;
            for(var count = 0; count < participants * 2; count++) {
                for(var retries = 0; retries < 10; retries++) {
                    var obj = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(
                        (self.rng.nextNormalizedFloat < 0.8f) ? sc1 : sc2,
                        dpr,
                        self.rng
                        ));
                    if(obj) {
                        if(obj.TryGetComponent<PurchaseInteraction>(out var purch) && purch.costType == CostTypeIndex.Money)
                            purch.Networkcost = Run.instance.GetDifficultyScaledCost(purch.cost);
                        break;
                    }
                }
            }
        }

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