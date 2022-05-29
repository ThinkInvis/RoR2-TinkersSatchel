using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;
using RoR2.Navigation;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    public class ReviveOnce : Equipment<ReviveOnce> {

        ////// Equipment Data //////

        public override string displayName => "Command Terminal";
        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override float cooldown { get; protected set; } = 10f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Revive an ally or summon a drone. Consumed on use.";
        protected override string GetDescString(string langid = null) => $"<style=cIsHealing>Revives</style> one survivor at random, calling them down in a drop pod. If no survivors are dead, the drop pod will contain a <style=cIsUtility>random drone</style> instead. <color=#FF7F7F>Will be consumed on use</style>.";
        protected override string GetLoreString(string langid = null) => $"";



        ////// Config //////

        [AutoConfigRoOString()]
        [AutoConfig("Which master prefab names to spawn if there are no allies to be revived. WARNING: May have unintended results on some untested objects!",
            AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever)]
        public string masterNamesConfig { get; private set; } = String.Join(", ", new[] {
            "EquipmentDroneMaster",
            "Drone1Master",
            "Drone2Master",
            "FlameDroneMaster",
            "DroneMissileMaster",
            "ItemDroneMaster",
            "BulwarkDroneMaster"
        });



        ////// Other Fields/Properties //////

        public readonly HashSet<string> droneMasterPrefabNames = new HashSet<string>();



        ////// TILER2 Module Setup //////

        public ReviveOnce() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ReviveOnce.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/reviveOnceIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            droneMasterPrefabNames.UnionWith(masterNamesConfig.Split(',')
                .Select(x => x.Trim()));

            if(Compat_ClassicItems.enabled) {
                LanguageAPI.Add("TKSAT_REVIVEONCE_CI_EMBRYO_APPEND", "\n<style=cStack>Beating Embryo: Activates twice simultaneously.</style>");
                Compat_ClassicItems.RegisterEmbryoHook(equipmentDef, "TKSAT_REVIVEONCE_CI_EMBRYO_APPEND", () => "TKSAT.CommandTerminal");
            }
        }

        public override void Install() {
            base.Install();
        }

        public override void Uninstall() {
            base.Uninstall();
        }



        ////// Hooks //////

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            var retv = PerformEquipmentActionInternal(slot);
            if(Compat_ClassicItems.enabled) {
                var count = Compat_ClassicItems.CheckEmbryoProc(slot, equipmentDef);
                for(var i = 0; i < count; i++)
                    retv |= PerformEquipmentActionInternal(slot);
            }
            return retv;
        }

        private bool PerformEquipmentActionInternal(EquipmentSlot slot) {
            var candidates = CharacterMaster.readOnlyInstancesList.Where(x => x.IsDeadAndOutOfLivesServer() && x.teamIndex == TeamIndex.Player);

            GameObject obj;
            GameObject podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/RoboCratePod");

            var nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
            var nodeInd = nodeGraph.FindClosestNodeWithFlagConditions(slot.transform.position, HullClassification.Human, NodeFlags.None, NodeFlags.None, false);
            Vector3 nodePos = slot.transform.position;
            Quaternion nodeRot = Quaternion.identity;
            if(nodeGraph.GetNodePosition(nodeInd, out nodePos)) {
                var targPos = slot.transform.position;
                targPos.y = nodePos.y;
                nodeRot = Util.QuaternionSafeLookRotation(nodePos - targPos);
            }

            if(candidates.Count() > 0) {
                var which = rng.NextElementUniform(candidates.ToArray());
                var newBody = which.Respawn(nodePos, nodeRot);
                if(!newBody) return false;
                obj = newBody.gameObject;
                string rezTargetName = Language.GetString(newBody.baseNameToken);
                var newBodyUser = Util.LookUpBodyNetworkUser(newBody);
                if(newBodyUser)
                    rezTargetName = newBodyUser.userName;
                var rezzerName = slot.characterBody ? Language.GetString(slot.characterBody.baseNameToken) : "Someone";
                var rezzerUser = Util.LookUpBodyNetworkUser(slot.characterBody);
                if(rezzerUser)
                    rezzerName = rezzerUser.userName;
                NetUtil.ServerSendGlobalChatMsg($"{rezzerName} called down a clone of {rezTargetName}!");
            } else {
                var whichName = rng.NextElementUniform(droneMasterPrefabNames.ToArray());
                var whichIndex = MasterCatalog.FindMasterIndex(whichName);
                var which = MasterCatalog.GetMasterPrefab(whichIndex);
                if(!which) return false;
                var summon = new MasterSummon {
                    masterPrefab = which,
                    position = nodePos,
                    rotation = nodeRot,
                    summonerBodyObject = slot.characterBody ? slot.characterBody.gameObject : null,
                    ignoreTeamMemberLimit = true,
                    useAmbientLevel = new bool?(true)
                }.Perform();
                if(!summon) return false;
                obj = summon.GetBodyObject();
                if(!obj) return false;
                if(obj.name == "EquipmentDroneBody(Clone)") {
                    var droneInv = obj.GetComponent<Inventory>();
                    if(droneInv) {
                        var randomEqp = rng.NextElementUniform(RoR2.Artifacts.EnigmaArtifactManager.validEquipment); 
                        droneInv.SetEquipment(new EquipmentState(randomEqp, Run.FixedTimeStamp.negativeInfinity, 1), 0);
                    }
                } else if(which == ItemDrone.instance.itemDroneMasterPrefab) {
                    var wardPersist = summon.GetComponent<ItemDroneWardPersist>();

                    var drops = LegacyResourcesAPI.Load<BasicPickupDropTable>("DropTables/dtSmallChest");
                    var drop = drops.GenerateDrop(rng);
                    var pdef = PickupCatalog.GetPickupDef(drop);
                    if(wardPersist && pdef != null && pdef.itemIndex != ItemIndex.None) {
                        int remCount = 1;
                        if(pdef.itemTier == ItemTier.Tier2 || pdef.itemTier == ItemTier.VoidTier2)
                            remCount = 3;
                        if(pdef.itemTier == ItemTier.Tier1 || pdef.itemTier == ItemTier.VoidTier1)
                            remCount = 5;
                        wardPersist.index = pdef.itemIndex;
                        wardPersist.count = remCount;
                    }
                }
            }

            if(!obj) return false;
            var objBody = obj.GetComponent<CharacterBody>();

            var podObj = GameObject.Instantiate(podPrefab, nodePos, nodeRot);
            var podSeat = podObj.GetComponent<VehicleSeat>();
            if(podSeat) {
                podSeat.AssignPassenger(obj);
            } else {
                TinkersSatchelPlugin._logger.LogError($"Pod {podObj} spawned for revived prefab {obj} has no seat!");
            }
            NetworkServer.Spawn(podObj);
            objBody.SetBodyStateToPreferredInitialState();

            if(slot.equipmentIndex == this.equipmentDef.equipmentIndex) {
                slot.inventory.SetEquipment(new EquipmentState(EquipmentIndex.None, Run.FixedTimeStamp.now + cooldown, 0), (uint)slot.inventory.activeEquipmentSlot);
            }
            return true;
        }
    }
}