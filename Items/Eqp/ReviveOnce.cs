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

        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override float cooldown { get; protected set; } = 10f;



        ////// Config //////

        [AutoConfigRoOString()]
        [AutoConfig("Which master prefab names to spawn if there are no allies to be revived. WARNING: May have unintended results on some untested objects!",
            AutoConfigFlags.PreventNetMismatch)]
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

        public readonly HashSet<string> droneMasterPrefabNames = new();
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public ReviveOnce() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ReviveOnce.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/reviveOnceIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/ReviveOnce.prefab");
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
                childName = "SpineChest3",
                localPos = new Vector3(-0.08684F, 0.67153F, -1.08192F),
                localAngles = new Vector3(44.63957F, 264.7216F, 107.5511F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.01093F, 0.05395F, -0.36182F),
                localAngles = new Vector3(314.4274F, 93.80039F, 295.7014F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.15785F, 0.13082F, -0.11723F),
                localAngles = new Vector3(322.6137F, 19.11888F, 332.5494F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.0131F, -0.01474F, -0.22271F),
                localAngles = new Vector3(328.3462F, 59.25051F, 349.7125F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.00568F, 0.22235F, -0.35905F),
                localAngles = new Vector3(334.1837F, 59.43953F, 3.43586F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00072F, 0.34057F, -0.30971F),
                localAngles = new Vector3(345.0132F, 50.15996F, 12.58943F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.1298F, 1.52182F, -2.26367F),
                localAngles = new Vector3(320.2272F, 71.04354F, 329.5704F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(1.01798F, 0.30345F, -0.23827F),
                localAngles = new Vector3(320.4994F, 321.4309F, 4.27371F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.12059F, 0.06472F, -0.16892F),
                localAngles = new Vector3(321.9678F, 54.92611F, 353.519F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00089F, 0.05696F, -0.30533F),
                localAngles = new Vector3(318.8723F, 56.99937F, 349.1709F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            #endregion
        }

        public override void SetupConfig() {
            base.SetupConfig();

            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(masterNamesConfig))
                    UpdateDroneMasterPrefabNames();
            };
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            UpdateDroneMasterPrefabNames();

            if(Compat_ClassicItems.enabled) {
                Compat_ClassicItems.RegisterEmbryoHook(equipmentDef, "TKSAT_REVIVEONCE_CI_EMBRYO_APPEND", () => "TKSAT.CommandTerminal");
            }
        }

        public override void Install() {
            base.Install();
        }

        public override void Uninstall() {
            base.Uninstall();
        }



        ////// Private Methods //////

        void UpdateDroneMasterPrefabNames() {
            droneMasterPrefabNames.Clear();
            droneMasterPrefabNames.UnionWith(masterNamesConfig.Split(',')
                .Select(x => x.Trim()));
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
                if(obj.name == "EquipmentDroneBody(Clone)" && obj.TryGetComponent<CharacterBody>(out var droneBody) && droneBody.master) {
                    var randomEqp = PickupCatalog.GetPickupDef(rng.NextElementUniform(Run.instance.availableEquipmentDropList)).equipmentIndex;
                    droneBody.master.inventory.SetEquipment(new EquipmentState(randomEqp, Run.FixedTimeStamp.negativeInfinity, 1), 0);
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
                slot.inventory.SetEquipment(new EquipmentState(EquipmentIndex.None, Run.FixedTimeStamp.now + cooldown * slot.inventory.CalculateEquipmentCooldownScale(), 0), (uint)slot.inventory.activeEquipmentSlot);
            }
            return true;
        }
    }
}