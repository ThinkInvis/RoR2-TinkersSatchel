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
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public ReviveOnce() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ReviveOnce.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/reviveOnceIcon.png"); idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/ReviveOnce.prefab");
        }

        public override void SetupModifyEquipmentDef() {
            base.SetupModifyEquipmentDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.04417F, 0.19067F, -0.24033F),
                localAngles = new Vector3(337.4471F, 55.56866F, 354.1383F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00054F, 0.27487F, -0.29389F),
                localAngles = new Vector3(320.018F, 64.74491F, 342.704F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.01826F, 0.41296F, -0.21866F),
                localAngles = new Vector3(6.28242F, 43.10916F, 36.10896F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
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
                var validNames = new HashSet<string>(droneMasterPrefabNames);
                if(!ItemDrone.instance.enabled)
                    validNames.Remove("ItemDroneMaster");
                if(!BulwarkDrone.instance.enabled)
                    validNames.Remove("BulwarkDroneMaster");
                var whichName = rng.NextElementUniform(validNames.ToArray());
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