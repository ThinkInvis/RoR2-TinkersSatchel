using R2API;
using RoR2;
using RoR2.CharacterAI;
using System.Linq;
using TILER2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
    public class BulwarkDrone : T2Module<BulwarkDrone> {

        ////// Config //////
        


        ////// Other Fields/Properties //////

        public GameObject bulwarkDroneInteractablePrefab;
        public GameObject bulwarkDroneBodyPrefab;
        public GameObject bulwarkDroneMasterPrefab;
        public InteractableSpawnCard bulwarkDroneSpawnCard;
        public DirectorCard bulwarkDroneDirectorCard;
        public GameObject bulwarkDronePanelPrefab;
        public DirectorAPI.DirectorCardHolder bulwarkDroneDCH;



        ////// TILER2 Module Setup //////

        public override void SetupAttributes() {
            base.SetupAttributes();

            bulwarkDroneBodyPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Characters/BulwarkDrone/BulwarkDroneBody.prefab");
            ModifyBodyPrefabWithVanillaAssets();

            bulwarkDroneMasterPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Characters/BulwarkDrone/BulwarkDroneMaster.prefab");

            bulwarkDroneInteractablePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Characters/BulwarkDrone/BulwarkDroneBroken.prefab");
            ModifyInteractablePrefabWithVanillaAssets();

            ContentAddition.AddBody(bulwarkDroneBodyPrefab);
            ContentAddition.AddMaster(bulwarkDroneMasterPrefab);
            ContentAddition.AddNetworkedObject(bulwarkDroneInteractablePrefab);

            SetupSpawnCard();
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }
        public override void SetupConfig() {
            base.SetupConfig();
        }

        public override void Install() {
            base.Install();

            On.EntityStates.Drone.DeathState.OnImpactServer += DeathState_OnImpactServer;

            DirectorAPI.Helpers.AddNewInteractable(bulwarkDroneDCH);
            if(ClassicStageInfo.instance)
                DirectorAPI.Helpers.TryApplyChangesNow();
        }

        public override void Uninstall() {
            base.Uninstall();

            On.EntityStates.Drone.DeathState.OnImpactServer -= DeathState_OnImpactServer;

            DirectorAPI.Helpers.RemoveExistingInteractable(bulwarkDroneDirectorCard.spawnCard.name);
            if(ClassicStageInfo.instance)
                DirectorAPI.Helpers.TryApplyChangesNow();
        }



        ////// Private Methods //////

        void ModifyBodyPrefabWithVanillaAssets() {
            var tmpBodySetup = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/EquipmentDroneBody").InstantiateClone("TkSatTempSetupPrefab2", false);

            bulwarkDroneBodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandard.asset")
                .WaitForCompletion();
            bulwarkDroneBodyPrefab.GetComponent<BoxCollider>().material = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatItems.physicMaterial")
                .WaitForCompletion();

            foreach(var akEvent in tmpBodySetup.GetComponents<AkEvent>()) {
                var newEvent = bulwarkDroneBodyPrefab.AddComponent<AkEvent>();
                newEvent.triggerList = akEvent.triggerList.ToArray().ToList();
                newEvent.useOtherObject = akEvent.useOtherObject;
                newEvent.actionOnEventType = akEvent.actionOnEventType;
                newEvent.curveInterpolation = akEvent.curveInterpolation;
                newEvent.enableActionOnEvent = akEvent.enableActionOnEvent;
                newEvent.data = akEvent.data;
                newEvent.useCallbacks = akEvent.useCallbacks;
                newEvent.Callbacks = akEvent.Callbacks.ToArray().ToList();
                newEvent.playingId = akEvent.playingId;
                newEvent.soundEmitterObject = bulwarkDroneBodyPrefab;
                newEvent.transitionDuration = akEvent.transitionDuration;
            }

            var coreMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/TrimSheets/matTrimSheetConstructionBlue.mat")
                .WaitForCompletion();

            var mdl = bulwarkDroneBodyPrefab.transform.Find("Model Base/BulwarkDrone").GetComponent<CharacterModel>();

            #region Vanilla IDRs
            mdl.itemDisplayRuleSet.SetDisplayRuleGroup(Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/DroneWeapons/DroneWeaponsDisplay1.asset").WaitForCompletion(), new DisplayRuleGroup {
                rules = new[] {
                    new ItemDisplayRule {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/DisplayDroneWeaponLauncher.prefab").WaitForCompletion(),
                        childName = "BulwarkDroneModel",
                        localPos = new Vector3(0F, 0.78087F, 0.72665F),
                        localAngles = new Vector3(356.5202F, 0F, 0F),
                        localScale = new Vector3(1F, 1F, 1F)
                    }, new ItemDisplayRule {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/DisplayDroneWeaponRobotArm.prefab").WaitForCompletion(),
                        childName = "BulwarkDroneModel",
                        localPos = new Vector3(0.48613F, 0F, 0.41692F),
                        localAngles = new Vector3(82.44529F, 86.80688F, 0.00014F),
                        localScale = new Vector3(1.5F, 1.5F, 1.5F)
                    }
                }
            });
            mdl.itemDisplayRuleSet.SetDisplayRuleGroup(Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/DroneWeapons/DroneWeaponsDisplay2.asset").WaitForCompletion(), new DisplayRuleGroup {
                rules = new[] {
                    new ItemDisplayRule {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/DisplayDroneWeaponMinigun.prefab").WaitForCompletion(),
                        childName = "BulwarkDroneModel",
                        localPos = new Vector3(0F, 0.37601F, 0F),
                        localAngles = new Vector3(-0.00001F, 180F, 180F),
                        localScale = new Vector3(1F, 1F, 1F)
                    }, new ItemDisplayRule {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/DisplayDroneWeaponLauncher.prefab").WaitForCompletion(),
                        childName = "BulwarkDroneModel",
                        localPos = new Vector3(0F, 0.78087F, 0.72665F),
                        localAngles = new Vector3(356.5202F, 0F, 0F),
                        localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            #endregion

            mdl.baseRendererInfos[0].defaultMaterial = coreMtl;
            mdl.baseRendererInfos[0].renderer.material = coreMtl;

            var partMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLargeDirectional.mat")
                .WaitForCompletion();

            var pren = bulwarkDroneBodyPrefab.transform.Find("Model Base/BulwarkDrone/ThrusterExhaust").GetComponent<ParticleSystemRenderer>();
            pren.material = partMtl;

            GameObject.Destroy(tmpBodySetup);
        }

        void ModifyInteractablePrefabWithVanillaAssets() {
            var brokenMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Drones/matDroneBrokenGeneric.mat")
                .WaitForCompletion();
            bulwarkDroneInteractablePrefab.transform.Find("Model Base/BulwarkDrone").GetComponent<MeshRenderer>().material = brokenMtl;
        }

        void SetupSpawnCard() {
            bulwarkDroneSpawnCard = TinkersSatchelPlugin.resources.LoadAsset<InteractableSpawnCard>("Assets/TinkersSatchel/Prefabs/Characters/BulwarkDrone/iscTkSatBulwarkDrone.asset");

            bulwarkDroneDirectorCard = new DirectorCard {
                spawnCard = bulwarkDroneSpawnCard,
                minimumStageCompletions = 0,
                preventOverhead = false,
                selectionWeight = 4, //equip drone is 2, normal drones are 7
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard
            };
            bulwarkDroneDCH = new DirectorAPI.DirectorCardHolder {
                Card = bulwarkDroneDirectorCard,
                InteractableCategory = DirectorAPI.InteractableCategory.Drones,
                MonsterCategory = DirectorAPI.MonsterCategory.Invalid
            };
        }



        ////// Hooks //////

        private void DeathState_OnImpactServer(On.EntityStates.Drone.DeathState.orig_OnImpactServer orig, EntityStates.Drone.DeathState self, Vector3 contactPoint) {
            orig(self, contactPoint);
            if(self.characterBody && BodyCatalog.GetBodyPrefab(self.characterBody.bodyIndex) == bulwarkDroneBodyPrefab) {
                var broken = DirectorCore.instance.TrySpawnObject(
                    new DirectorSpawnRequest(bulwarkDroneSpawnCard, new DirectorPlacementRule {
                        placementMode = DirectorPlacementRule.PlacementMode.Direct,
                        position = contactPoint
                    }, this.rng));
                if(broken) {
                    var purch = broken.GetComponent<PurchaseInteraction>();
                    if(purch && purch.costType == CostTypeIndex.Money)
                        purch.Networkcost = Run.instance.GetDifficultyScaledCost(purch.cost);
                }
            }
        }
    }

    [RequireComponent(typeof(TeamComponent), typeof(CharacterBody))]
    public class TauntNearbyBehaviour : MonoBehaviour {
        public float range = 100f;
        public float tauntChancePerTargetPerInterval = 0.25f;
        public float scanInterval = 5f;
        public HurtBox hurtbox;
        
        TeamComponent teamcpt;
        CharacterBody body;

        float stopwatch = 0f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            teamcpt = GetComponent<TeamComponent>();
            body = GetComponent<CharacterBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                var rangeSq = range * range;
                stopwatch = scanInterval;

                var tgtsToTaunt = GameObject.FindObjectsOfType<CharacterBody>()
                    .Where(x => (x.transform.position - transform.position).sqrMagnitude < rangeSq
                        && x.master
                        && x.teamComponent.teamIndex != teamcpt.teamIndex && x.teamComponent.teamIndex != TeamIndex.Neutral)
                    .Select(x => x.master.GetComponent<BaseAI>())
                    .Where(x => x);

                var tauntChanceAdvantage = body.master ? Compat_Dronemeld.SafeGetStackCount(body.master.inventory) : 0;
                var modifiedTauntChance = 1f - Mathf.Pow(1f - tauntChancePerTargetPerInterval, tauntChanceAdvantage + 1);

                tgtsToTaunt = tgtsToTaunt
                    .OrderBy(t => BulwarkDrone.instance.rng.nextNormalizedFloat)
                    .Take(Mathf.RoundToInt(tgtsToTaunt.Count() * modifiedTauntChance));

                foreach(var ai in tgtsToTaunt)
                    TauntDebuffController.ApplyTaunt(ai, body, scanInterval);
            }
        }
    }
}
