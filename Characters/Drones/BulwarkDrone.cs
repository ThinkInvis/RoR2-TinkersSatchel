using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.UI;
using System.Linq;
using TILER2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class BulwarkDrone : T2Module<BulwarkDrone> {
        ////// Language //////

        public override void RefreshPermanentLanguage() {
            permanentGenericLanguageTokens.Add("TKSAT_BULWARKDRONE_NAME", "Bulwark Drone");
            permanentGenericLanguageTokens.Add("TKSAT_BULWARKDRONE_BODY_NAME", "Bulwark Drone");
            permanentGenericLanguageTokens.Add("TKSAT_BULWARKDRONE_CONTEXT", "Repair Bulwark Drone");
            base.RefreshPermanentLanguage();
        }

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

            DirectorAPI.Helpers.AddNewInteractable(bulwarkDroneDCH);
            if(ClassicStageInfo.instance)
                DirectorAPI.Helpers.TryApplyChangesNow();
        }

        public override void Uninstall() {
            base.Uninstall();

            DirectorAPI.Helpers.RemoveExistingInteractable(bulwarkDroneDirectorCard.spawnCard.name);
            if(ClassicStageInfo.instance)
                DirectorAPI.Helpers.TryApplyChangesNow();
        }



        ////// Private Methods //////

        void ModifyBodyPrefabWithVanillaAssets() {
            var tmpBodySetup = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/EquipmentDroneBody").InstantiateClone("TkSatTempSetupPrefab2", false);

            bulwarkDroneBodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandard.asset")
                .WaitForCompletion();
            bulwarkDroneBodyPrefab.GetComponent<CapsuleCollider>().material = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatItems.physicMaterial")
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
                newEvent.soundEmitterObject = tmpBodySetup;
                newEvent.transitionDuration = akEvent.transitionDuration;
            }

            var coreMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/TrimSheets/matTrimSheetConstructionBlue.mat")
                .WaitForCompletion();

            var mdl = bulwarkDroneBodyPrefab.transform.Find("Model Base/BulwarkDrone").GetComponent<CharacterModel>();
            mdl.baseRendererInfos[0].defaultMaterial = coreMtl;
            mdl.baseRendererInfos[0].renderer.material = coreMtl;

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
                //selectionWeight = 4, //equip drone is 2, normal drones are 7
                selectionWeight = 9999,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard
            };
            bulwarkDroneDCH = new DirectorAPI.DirectorCardHolder {
                Card = bulwarkDroneDirectorCard,
                //InteractableCategory = DirectorAPI.InteractableCategory.Drones,
                InteractableCategory = DirectorAPI.InteractableCategory.Chests,
                MonsterCategory = DirectorAPI.MonsterCategory.Invalid
            };
        }



        ////// Hooks //////

    }

    [RequireComponent(typeof(TeamComponent))]
    public class TauntNearbyBehaviour : MonoBehaviour {
        public float range = 100f;
        public float tauntChancePerTargetPerInterval = 0.5f;
        public float scanInterval = 5f;
        public HurtBox hurtbox;
        
        TeamComponent teamcpt;

        float stopwatch = 0f;

        void Awake() {
            teamcpt = GetComponent<TeamComponent>();
        }

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

                foreach(var ai in tgtsToTaunt) {
                    if(tauntChancePerTargetPerInterval > UnityEngine.Random.value) {
                        ai.currentEnemy.gameObject = this.gameObject;
                        ai.currentEnemy.bestHurtBox = hurtbox;
                        ai.enemyAttention = scanInterval;
                    }
                }
            }
        }
    }
}
