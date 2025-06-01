using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;
using RoR2.Navigation;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

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
        }



        ////// Private Methods //////

        void UpdateDroneMasterPrefabNames() {
            droneMasterPrefabNames.Clear();
            droneMasterPrefabNames.UnionWith(masterNamesConfig.Split(',')
                .Select(x => x.Trim()));
        }



        ////// Hooks //////

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            var candidates = CharacterMaster.readOnlyInstancesList.Where(x => x.IsDeadAndOutOfLivesServer() && x.teamIndex == TeamIndex.Player);

            GameObject obj;
            GameObject podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/RoboCratePod");

            //Try to find a free node to call down at; default to EquipmentSlot's own position otherwise
            var nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
            var nodeInd = nodeGraph.FindClosestNodeWithFlagConditions(slot.transform.position, HullClassification.Human, NodeFlags.None, NodeFlags.None, false);
            Vector3 nodePos = slot.transform.position;
            Quaternion nodeRot = Quaternion.identity;
            if(nodeGraph.GetNodePosition(nodeInd, out nodePos)) {
                var targPos = slot.transform.position;
                targPos.y = nodePos.y;
                nodeRot = Util.QuaternionSafeLookRotation(nodePos - targPos);
            }

            //Create object...
            if(candidates.Count() > 0) { //Has dead ally, revive
                var which = rng.NextElementUniform(candidates.ToArray());
                var newBody = which.Respawn(nodePos, nodeRot);
                if(!newBody) return false;
                obj = newBody.gameObject;
                string rezTargetName = Language.GetString(newBody.baseNameToken);
                var newBodyUser = Util.LookUpBodyNetworkUser(newBody);
                if(newBodyUser)
                    rezTargetName = newBodyUser.userName;
                var rezzerName = slot.characterBody ? Language.GetString(slot.characterBody.baseNameToken) : Language.GetString("TINKERSSATCHEL_REVIVEONCE_MSG_REVIVE_VAGUE");
                var rezzerUser = Util.LookUpBodyNetworkUser(slot.characterBody);
                if(rezzerUser)
                    rezzerName = rezzerUser.userName;
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage {
                    paramTokens = new[] { rezzerName, rezTargetName },
                    baseToken = "TINKERSSATCHEL_REVIVEONCE_MSG_REVIVE"
                });
            } else { //No dead ally, summon drone
                //Prepare name list and select one
                var validNames = new HashSet<string>(droneMasterPrefabNames);
                if(!ItemDrone.instance.enabled)
                    validNames.Remove("ItemDroneMaster");
                if(!BulwarkDrone.instance.enabled)
                    validNames.Remove("BulwarkDroneMaster");
                var whichName = rng.NextElementUniform(validNames.ToArray());
                var whichIndex = MasterCatalog.FindMasterIndex(whichName);
                var which = MasterCatalog.GetMasterPrefab(whichIndex);
                if(!which) return false;

                //Create drone object
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

                //Additional setup...
                if(obj.name == "EquipmentDroneBody(Clone)" && obj.TryGetComponent<CharacterBody>(out var droneBody) && droneBody.master) {
                    //Give random equipment to equipment drones
                    var validEqp = Run.instance.availableEquipmentDropList.Where(
                        pind => CatalogUtil.TryGetEquipmentDef(pind, out var edef) && edef.canBeRandomlyTriggered
                        ).ToArray();
                    var randomEqp = PickupCatalog.GetPickupDef(rng.NextElementUniform(validEqp)).equipmentIndex;
                    droneBody.master.inventory.SetEquipment(new EquipmentState(randomEqp, Run.FixedTimeStamp.negativeInfinity, 1), 0);
                } else if(which == ItemDrone.instance.itemDroneMasterPrefab) {
                    //Give random items to item drones
                    var wardPersist = summon.GetComponent<ItemDroneWardPersist>();

                    var drops = LegacyResourcesAPI.Load<BasicPickupDropTable>("DropTables/dtSmallChest");
                    var drop = drops.GenerateDrop(rng);
                    if(wardPersist && CatalogUtil.TryGetItemDef(drop, out var idef)) {
                        int remCount = 1;
                        if(idef.tier == ItemTier.Tier2 || idef.tier == ItemTier.VoidTier2)
                            remCount = 3;
                        if(idef.tier == ItemTier.Tier1 || idef.tier == ItemTier.VoidTier1)
                            remCount = 5;
                        wardPersist.AddItems(idef.itemIndex, remCount);
                    }
                }
            }

            if(!obj) return false;
            var objBody = obj.GetComponent<CharacterBody>();

            //Put object in drop pod
            var podObj = GameObject.Instantiate(podPrefab, nodePos, nodeRot);
            var podSeat = podObj.GetComponent<VehicleSeat>();
            if(podSeat) {
                podSeat.AssignPassenger(obj);
            } else {
                TinkersSatchelPlugin._logger.LogError($"Pod {podObj} spawned for revived prefab {obj} has no seat!");
            }
            NetworkServer.Spawn(podObj);
            objBody.SetBodyStateToPreferredInitialState();

            //Consume equipment, adjust cooldown timer of next equipment picked up to use cooldown for this equipment
            if(slot.equipmentIndex == this.equipmentDef.equipmentIndex) {
                slot.inventory.SetEquipment(new EquipmentState(EquipmentIndex.None, Run.FixedTimeStamp.now + cooldown * slot.inventory.CalculateEquipmentCooldownScale(), 0), (uint)slot.inventory.activeEquipmentSlot);
            }
            return true;
        }
    }

    [RegisterAchievement("TkSat_ReviveOnce", "TkSat_ReviveOnceUnlockable", "", 1u)]
    public class TkSatReviveOnceAchievement : RoR2.Achievements.BaseAchievement {
        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
            On.RoR2.GlobalEventManager.OnCharacterDeath -= GlobalEventManager_OnCharacterDeath;
        }


        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) {
            orig(self, damageReport);
            if(!damageReport.attackerBody || damageReport.attackerBody != localUser.cachedBody) return;
            if((damageReport.damageInfo.damageType.damageSource & DamageSource.Primary) != 0) {
                if(!damageReport.attackerBody.TryGetComponent<TkSatReviveOnceAchievementStreakTracker>(out var tracker))
                    tracker = damageReport.attackerBody.gameObject.AddComponent<TkSatReviveOnceAchievementStreakTracker>();
                tracker.kills++;
                if(tracker.kills > 20)
                    Grant();
            }
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill) {
            orig(self, skill);
            if(!NetworkServer.active) return;
            if(self && self.skillLocator
                && self.skillLocator.FindSkillSlot(skill) != SkillSlot.Primary
                && self.TryGetComponent<TkSatReviveOnceAchievementStreakTracker>(out var tracker)) {
                tracker.kills = 0;
            }
        }
    }

    public class TkSatReviveOnceAchievementStreakTracker : MonoBehaviour {
        public int kills = 0;
    }
}