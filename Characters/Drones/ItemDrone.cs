using R2API;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TILER2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class ItemDrone : T2Module<ItemDrone> {

        ////// Config //////
        
        [AutoConfigRoOString()]
        [AutoConfig("Items to prevent giving to Item Drones, as a comma-delimited list of internal names (will be automatically trimmed).", AutoConfigFlags.PreventNetMismatch)]
        public string itemNameBlacklist { get; private set; } = "ScrapWhite, ScrapGreen, ScrapRed, ScrapYellow, ScrapWhiteSuppressed, ScrapGreenSuppressed, ScrapRedSuppressed, RegeneratingScrap, RegeneratingScrapConsumed, ExtraLifeConsumed, ExtraLifeVoidConsumed, FragileDamageBonusConsumed, HealingPotionConsumed, BeetleGland, RoboBallBuddy, MinorConstructOnKill, TitanGoldDuringTP";

        [AutoConfigRoOString()]
        [AutoConfig("Item tiers to prevent giving to Item Drones, as a comma-delimited list of internal names (will be automatically trimmed).", AutoConfigFlags.PreventNetMismatch)]
        public string itemTierNameBlacklist { get; private set; } = "LunarTierDef, VoidTier1Def, VoidTier2Def, VoidTier3Def, VoidBossDef";



        ////// Other Fields/Properties //////

        public GameObject itemDroneInteractablePrefab;
        public GameObject itemDroneBodyPrefab;
        public GameObject itemDroneMasterPrefab;
        public InteractableSpawnCard itemDroneSpawnCard;
        public DirectorCard itemDroneDirectorCard;
        public GameObject itemDronePanelPrefab;
        public DirectorAPI.DirectorCardHolder itemDroneDCH;
        public HashSet<ItemDef> blacklistedItems = new();



        ////// TILER2 Module Setup //////

        public override void SetupAttributes() {
            base.SetupAttributes();

            LoadBodyPrefab();
            ModifyBodyPrefabWithVanillaAssets();

            itemDroneMasterPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Characters/ItemDrone/ItemDroneMaster.prefab");

            CreatePanelPrefab();

            LoadInteractablePrefab();
            ModifyInteractablePrefabWithVanillaAssets();

            ContentAddition.AddBody(itemDroneBodyPrefab);
            ContentAddition.AddMaster(itemDroneMasterPrefab);
            ContentAddition.AddNetworkedObject(itemDroneInteractablePrefab);

            SetupSpawnCard();

            ItemTierCatalog.availability.CallWhenAvailable(() => ItemCatalog.availability.CallWhenAvailable(this.SetupCatalogReady));
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }

        public override void SetupConfig() {
            base.SetupConfig();
            ConfigEntryChanged += (nv, args) => {
                if(args.target.boundProperty.Name == nameof(itemTierNameBlacklist) || args.target.boundProperty.Name == nameof(itemNameBlacklist)) {
                    UpdateValidItems();
                }
            };
        }

        public void SetupCatalogReady() {
            UpdateValidItems();
        }

        public override void Install() {
            base.Install();
            On.RoR2.CharacterBody.GetDisplayName += CharacterBody_GetDisplayName;

            DirectorAPI.Helpers.AddNewInteractable(itemDroneDCH);
            if(ClassicStageInfo.instance)
                DirectorAPI.Helpers.TryApplyChangesNow();
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.CharacterBody.GetDisplayName -= CharacterBody_GetDisplayName;

            DirectorAPI.Helpers.RemoveExistingInteractable(itemDroneDirectorCard.spawnCard.name);
            if(ClassicStageInfo.instance)
                DirectorAPI.Helpers.TryApplyChangesNow();
        }



        ////// Private Methods //////

        void UpdateValidItems() {
            if(!ItemCatalog.availability.available || !ItemTierCatalog.availability.available) return;
            var names = itemNameBlacklist.Split(',').Select(x => x.Trim());
            var tierNames = itemTierNameBlacklist.Split(',').Select(x => x.Trim());
            blacklistedItems.Clear();
            blacklistedItems.UnionWith(ItemCatalog.allItemDefs.Where(idef => {
                if(names.Contains(idef.name)) return true;
                var itd = ItemTierCatalog.GetItemTierDef(idef.tier);
                if(itd && tierNames.Contains(itd.name)) return true;
                return false;
            }));
        }

        void LoadBodyPrefab() {
            ////body////
            itemDroneBodyPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Characters/ItemDrone/ItemDroneBody.prefab");
        }

        void ModifyBodyPrefabWithVanillaAssets() {
            var tmpBodySetup = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/EquipmentDroneBody").InstantiateClone("TkSatTempSetupPrefab2", false);

            itemDroneBodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandard.asset")
                .WaitForCompletion();
            itemDroneBodyPrefab.GetComponent<CapsuleCollider>().material = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatItems.physicMaterial")
                .WaitForCompletion();

            foreach(var akEvent in tmpBodySetup.GetComponents<AkEvent>()) {
                var newEvent = itemDroneBodyPrefab.AddComponent<AkEvent>();
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
            var bladesMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Drones/matDroneBrokenGeneric.mat")
                .WaitForCompletion();

            var mdl = itemDroneBodyPrefab.transform.Find("Model Base/ItemDroneArmature").GetComponent<CharacterModel>();
            mdl.baseRendererInfos[0].defaultMaterial = coreMtl;
            mdl.baseRendererInfos[0].renderer.material = coreMtl;
            for(int i = 1; i <= 3; i++) {
                mdl.baseRendererInfos[i].defaultMaterial = bladesMtl;
                mdl.baseRendererInfos[i].renderer.material = bladesMtl;
            }

            GameObject.Destroy(tmpBodySetup);

            var ren = itemDroneBodyPrefab.transform.Find("WardRangeScale/WardRangeInd").gameObject.GetComponent<MeshRenderer>();
            ren.material = ItemWard.stockIndicatorPrefab.transform.Find("IndicatorSphere").gameObject.GetComponent<MeshRenderer>().material;
            ren.material.SetTexture("_RemapTex",
                Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampDefault.png")
                .WaitForCompletion());
            ren.material.SetFloat("_AlphaBoost", 0.475f);
            ren.material.SetColor("_CutoffScroll", new Color(0.8f, 0.8f, 0.85f));
            ren.material.SetColor("_RimColor", new Color(0.8f, 0.8f, 0.85f));
        }

        void CreatePanelPrefab() {
            var tmpPanelSetup = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/ScrapperPickerPanel").InstantiateClone("TkSatTempSetupPrefab", false);
            var label = tmpPanelSetup.transform.Find("MainPanel/Juice/Label").GetComponent<HGTextMeshProUGUI>();
            label.text = "<b>Item Drone</b>\r\n<i>Pick an item to insert...</i>";
            var labelTMC = tmpPanelSetup.transform.Find("MainPanel/Juice/Label").GetComponent<LanguageTextMeshController>();
            labelTMC.token = "TKSAT_ITEMDRONE_POPUP_TEXT";

            itemDronePanelPrefab = tmpPanelSetup.InstantiateClone("TkSatItemDronePanel", false);
            GameObject.Destroy(tmpPanelSetup);
        }

        void LoadInteractablePrefab() {
            itemDroneInteractablePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Characters/ItemDrone/ItemDroneBroken.prefab");

            var ppc = itemDroneInteractablePrefab.GetComponent<PickupPickerController>();
            ppc.available = true;
            ppc.panelPrefab = itemDronePanelPrefab;
        }

        void ModifyInteractablePrefabWithVanillaAssets() {
            var brokenMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Drones/matDroneBrokenGeneric.mat")
                .WaitForCompletion();
            itemDroneInteractablePrefab.transform.Find("Model Base/mdlItemDrone/Root/ItemDrone").GetComponent<MeshRenderer>().material = brokenMtl;
            itemDroneInteractablePrefab.transform.Find("Model Base/mdlItemDrone/Root/Prop1/Prop1 1").GetComponent<MeshRenderer>().material = brokenMtl;
            itemDroneInteractablePrefab.transform.Find("Model Base/mdlItemDrone/Root/Prop2/Prop2 1").GetComponent<MeshRenderer>().material = brokenMtl;
            itemDroneInteractablePrefab.transform.Find("Model Base/mdlItemDrone/Root/Prop3/Prop3 1").GetComponent<MeshRenderer>().material = brokenMtl;
        }

        void SetupSpawnCard() {
            itemDroneSpawnCard = TinkersSatchelPlugin.resources.LoadAsset<InteractableSpawnCard>("Assets/TinkersSatchel/Prefabs/Characters/ItemDrone/iscTkSatItemDrone.asset");

            itemDroneDirectorCard = new DirectorCard {
                spawnCard = itemDroneSpawnCard,
                minimumStageCompletions = 0,
                preventOverhead = false,
                selectionWeight = 4, //equip drone is 2, normal drones are 7
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard
            };
            itemDroneDCH = new DirectorAPI.DirectorCardHolder {
                Card = itemDroneDirectorCard,
                InteractableCategory = DirectorAPI.InteractableCategory.Drones,
                MonsterCategory = DirectorAPI.MonsterCategory.Invalid
            };
        }



        ////// Hooks //////

        private string CharacterBody_GetDisplayName(On.RoR2.CharacterBody.orig_GetDisplayName orig, CharacterBody self) {
            var retv = orig(self);
            if(self.name == "ItemDroneBody(Clone)") {
                var ward = self.GetComponent<ItemWard>();
                if(!ward || ward.itemcounts.Count == 0) return retv;

                if(ward.itemcounts.Count > 1) {
                    var countByTier = ward.itemcounts
                        .Select(kvp => (itemDef: ItemCatalog.GetItemDef(kvp.Key), count: kvp.Value))
                        .Where(tDbC => tDbC.itemDef != null) //tuple itemdef by count
                        .GroupBy(tDbC => tDbC.itemDef.tier)
                        .Select(tDbC_gTier => (itemTier: tDbC_gTier.Key, count: tDbC_gTier.Sum(tDbC => tDbC.count)))
                        .Where(tTbC => tTbC.count > 0) //tuple itemtier by count
                        .Select(tTbC => $"<color=#{ColorCatalog.GetColorHexString(ItemTierCatalog.GetItemTierDef(tTbC.itemTier).colorIndex)}>{tTbC.count}</color>");

                    return $"{retv} ({string.Join(", ", countByTier)})";
                } else {
                    var idef = ItemCatalog.GetItemDef(ward.itemcounts.First().Key);
                    if(idef == null) return retv;

                    var color = "FFFFFF";
                    var itd = ItemTierCatalog.GetItemTierDef(idef.tier);
                    if(itd)
                        color = ColorCatalog.GetColorHexString(itd.colorIndex);
                    return $"{retv} (<color=#{color}>{Language.GetString(idef.nameToken)}</color>)";
                }
            }
            return retv;
        }
    }

    public class ItemDronePurchaseController : NetworkBehaviour {
        Interactor currentInteractor;

        public void SetInteractor(Interactor interactor) {
            this.currentInteractor = interactor;
        }

        public void HandlePurchase(int pind) {
            if(!NetworkServer.active) return;

            PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(pind));
            if(pickupDef == null || !currentInteractor) return;
            var body = currentInteractor.GetComponent<CharacterBody>();
            if(!body || !body.inventory)
                return;
            var count = body.inventory.GetItemCount(pickupDef.itemIndex);
            if(count <= 0) return;

            int remCount = 1;
            if(pickupDef.itemTier == ItemTier.Tier2 || pickupDef.itemTier == ItemTier.VoidTier2)
                remCount = 3;
            if(pickupDef.itemTier == ItemTier.Tier1 || pickupDef.itemTier == ItemTier.VoidTier1)
                remCount = 5;

            remCount = Mathf.Min(count, remCount);

            CharacterMaster extantMaster;
            GameObject effectTarget;
            if(Compat_Dronemeld.enabled && (extantMaster = Compat_Dronemeld.TryApply(body.master, "ItemDroneMaster")) != null) {
                var persist = extantMaster.GetComponent<ItemDroneWardPersist>();
                if(!persist) return;
                persist.AddItems(pickupDef.itemIndex, remCount);
                effectTarget = extantMaster.GetBodyObject();
            } else {
                var summon = GetComponent<SummonMasterBehavior>();
                var cm = summon.OpenSummonReturnMaster(currentInteractor);
                var persist = cm.GetComponent<ItemDroneWardPersist>();
                if(!summon || !persist)
                    return;
                body.inventory.RemoveItem(pickupDef.itemIndex, remCount);
                persist.AddItems(pickupDef.itemIndex, remCount);
                effectTarget = cm.GetBodyObject();
            }

            if(effectTarget) {
                for(var i = 0; i < remCount; i++) {
                    var effectData = new EffectData {
                        origin = body.corePosition,
                        genericFloat = Mathf.Lerp(1.5f, 2.5f, (float)i / (float)remCount),
                        genericUInt = (uint)(pickupDef.itemIndex + 1)
                    };
                    effectData.SetNetworkedObjectReference(effectTarget);
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/ItemTakenOrbEffect"), effectData, true);
                }
            }

            GameObject.Destroy(this.gameObject);
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class ItemDroneDropOnDeath : MonoBehaviour, IOnKilledServerReceiver {
        CharacterBody body;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        public void OnKilledServer(DamageReport damageReport) {
            if(!body || !body.master || !body.master.IsDeadAndOutOfLivesServer()) return;
            var idwp = body.master.GetComponent<ItemDroneWardPersist>();
            if(!idwp) return;

            var totalItems = idwp.stacks.Sum();
            if(totalItems <= 0) return;

            var thetaStep = 360f / (float)totalItems;
            var thetaCurr = 0f;
            for(int i = 0; i < idwp.stacks.Length; i++) {
                for(int j = 0; j < idwp.stacks[i]; j++) {
                    var vel = Quaternion.AngleAxis((float)i * thetaCurr, Vector3.up) * new Vector3(5f, 5f);
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex((ItemIndex)i), transform.position, vel);
                    thetaCurr += thetaStep;
                }
            }
            MonoBehaviour.Destroy(this);
        }
    }

    [RequireComponent(typeof(CharacterMaster))]
    public class ItemDroneWardPersist : MonoBehaviour {
        public int[] stacks { get; private set; } = null;
        CharacterMaster master;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            master = GetComponent<CharacterMaster>();
            stacks = ItemCatalog.RequestItemStackArray();
            if(master)
                master.onBodyStart += Master_onBodyStart;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void OnDestroy() {
            ItemCatalog.ReturnItemStackArray(stacks);
            stacks = null;
            if(master)
                master.onBodyStart -= Master_onBodyStart;
        }

        void Master_onBodyStart(CharacterBody obj) {
            var body = master.GetBodyObject();
            if(!body) return;
            var ward = body.GetComponent<ItemWard>();
            if(!ward) return;
            if(ward.radius != 100f)
                ward.radius = 100f;
            for(var i = 0; i < stacks.Length; i++)
                CheckItemCount((ItemIndex)i);
        }

        public void AddItems(ItemIndex ind, int count) {
            stacks[(int)ind] += count;
            CheckItemCount(ind);
        }

        void CheckItemCount(ItemIndex ind) {
            if(!master.hasBody) return;
            var ward = master.GetBodyObject().GetComponent<ItemWard>();
            ward.itemcounts.TryGetValue(ind, out int oldCount);
            var newCount = stacks[(int)ind];
            var countToChange = Mathf.Abs(oldCount - newCount);
            if(oldCount < newCount) {
                for(var i = 0; i < countToChange; i++)
                    ward.ServerAddItem(ind);
            } else if(oldCount > newCount) {
                for(var i = 0; i < countToChange; i++)
                    ward.ServerRemoveItem(ind);
            }
        }
    }

    [RequireComponent(typeof(PickupPickerController))]
    public class PickupPickerControllerFilteredSelector : MonoBehaviour {
        [Obsolete()]
        public HashSet<ItemDef> blacklistedItems = new();
        PickupPickerController ppc;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        void Awake() {
            ppc = GetComponent<PickupPickerController>();
        }

        public void SetOptionsFromInteractor(Interactor activator) {
            if(!activator) return;
            var body = activator.GetComponent<CharacterBody>();
            if(!body || !body.inventory) return;
            var opts = new List<PickupPickerController.Option>();
            for(int i = 0; i < body.inventory.itemAcquisitionOrder.Count; i++) {
                var iind = body.inventory.itemAcquisitionOrder[i];
                var idef = ItemCatalog.GetItemDef(iind);
                var pind = PickupCatalog.FindPickupIndex(iind);
                if(idef && pind != PickupIndex.none && idef.canRemove && !idef.hidden && idef.tier != ItemTier.NoTier && !ItemDrone.instance.blacklistedItems.Contains(idef)) {
                    opts.Add(new PickupPickerController.Option {
                        available = true,
                        pickupIndex = pind
                    });
                }
            }
            ppc.SetOptionsServer(opts.ToArray());
        }
    }
}
