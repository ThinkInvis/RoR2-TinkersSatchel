using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class ItemDrone : T2Module<ItemDrone> {
        public GameObject itemDroneInteractablePrefab;
        public GameObject itemDroneBodyPrefab;
        public GameObject itemDroneMasterPrefab;
        public InteractableSpawnCard itemDroneSpawnCard;
        public DirectorCard itemDroneDirectorCard;
        public GameObject itemDronePanelPrefab;
        public DirectorAPI.DirectorCardHolder itemDroneDCH;

        public override void RefreshPermanentLanguage() {
            permanentGenericLanguageTokens.Add("TKSAT_ITEMDRONE_NAME", "Item Drone");
            permanentGenericLanguageTokens.Add("TKSAT_ITEMDRONE_BODY_NAME", "Item Drone");
            permanentGenericLanguageTokens.Add("TKSAT_ITEMDRONE_POPUP_TEXT", "<b>Item Drone</b>\r\n<i>Pick an item to insert...</i>");
            permanentGenericLanguageTokens.Add("TKSAT_ITEMDRONE_CONTEXT", "Give Item");
            base.RefreshPermanentLanguage();
        }
        public override void SetupAttributes() {
            base.SetupAttributes();

            ////body////
            var tmpBodySetup = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/EquipmentDroneBody").InstantiateClone("TkSatTempSetupPrefab", false);
            var body = tmpBodySetup.GetComponent<CharacterBody>();
            body.baseNameToken = "TKSAT_ITEMDRONE_BODY_NAME";
            body.baseMoveSpeed *= 2.5f;
            var ward = tmpBodySetup.AddComponent<ItemWard>();
            ward.radius = 60f; // doesn't work at all, stays at default 10. TODO: how and why??
            ward.displayRadiusFracH = 0.015f;
            ward.displayRadiusFracV = 0f;
            ward.displayRadiusOffset = Vector3.down * 2f;
            ward.displayIndivScale *= 0.35f;
            var model = tmpBodySetup.GetComponent<ModelLocator>();
            model.modelTransform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
            var wardInd = GameObject.Instantiate(ItemWard.stockIndicatorPrefab, model.modelBaseTransform);
            wardInd.transform.localPosition = Vector3.zero;
            var ren = wardInd.transform.Find("IndicatorSphere").GetComponent<MeshRenderer>();
            ren.material.SetFloat("_AlphaBoost", 0.45f);
            ward.rangeIndicator = wardInd.transform;
            tmpBodySetup.AddComponent<ItemDroneDropOnDeath>();

            itemDroneBodyPrefab = tmpBodySetup.InstantiateClone("TkSatItemDroneBody");
            GameObject.Destroy(tmpBodySetup);

            ////master////
            var tmpMasterSetup = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/EquipmentDroneMaster").InstantiateClone("TkSatTempSetupPrefab", false);
            var persist = tmpMasterSetup.AddComponent<ItemDroneWardPersist>();
            var master = tmpMasterSetup.GetComponent<CharacterMaster>();
            master.bodyPrefab = itemDroneBodyPrefab;
            foreach(var skilldriver in master.GetComponents<AISkillDriver>()) {
                if(skilldriver.customName == "IdleNearLeaderWhenNoEnemies") {
                    skilldriver.maxDistance = 15f;
                } else if(skilldriver.customName == "ChaseDownRandomEnemiesIfLeaderIsDead") {
                    skilldriver.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
                    skilldriver.skillSlot = SkillSlot.None;
                } else if(skilldriver.customName == "ReturnToLeaderWhenNoEnemies" || skilldriver.customName == "HardLeashToLeader" || skilldriver.customName == "SoftLeashToLeader") {
                } else {
                    skilldriver.enabled = false;
                }
            }

            itemDroneMasterPrefab = tmpMasterSetup.InstantiateClone("TkSatItemDroneMaster");
            GameObject.Destroy(tmpMasterSetup);

            ////scrapper panel////
            var tmpPanelSetup = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/ScrapperPickerPanel").InstantiateClone("TkSatTempSetupPrefab", false);
            var label = tmpPanelSetup.transform.Find("MainPanel/Juice/Label").GetComponent<HGTextMeshProUGUI>();
            label.text = "<b>Item Drone</b>\r\n<i>Pick an item to insert...</i>";
            var labelTMC = tmpPanelSetup.transform.Find("MainPanel/Juice/Label").GetComponent<LanguageTextMeshController>();
            labelTMC.token = "TKSAT_ITEMDRONE_POPUP_TEXT";

            itemDronePanelPrefab = tmpPanelSetup.InstantiateClone("TkSatItemDronePanel", false);
            GameObject.Destroy(tmpPanelSetup);

            ////interactable////
            var tmpInteractableSetup = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BrokenDrones/EquipmentDroneBroken").InstantiateClone("TkSatTempSetupPrefab", false);
            //MonoBehaviour.Destroy(tmpInteractableSetup.GetComponent<PurchaseInteraction>());
            var purch = tmpInteractableSetup.GetComponent<PurchaseInteraction>();
            purch.contextToken = "TKSAT_ITEMDRONE_CONTEXT";
            purch.costType = CostTypeIndex.None;
            purch.cost = 0;
            var smb = tmpInteractableSetup.GetComponent<SummonMasterBehavior>();
            smb.masterPrefab = itemDroneMasterPrefab;
            smb.callOnEquipmentSpentOnPurchase = false;
            var nwui = tmpInteractableSetup.AddComponent<NetworkUIPromptController>();
            var gdnp = tmpInteractableSetup.GetComponent<GenericDisplayNameProvider>();
            gdnp.displayToken = "TKSAT_ITEMDRONE_NAME";
            var ppc = tmpInteractableSetup.AddComponent<PickupPickerController>();
            ppc.panelPrefab = itemDronePanelPrefab;
            ppc.cutoffDistance = 10f;
            ppc.contextString = "TKSAT_ITEMDRONE_CONTEXT";
            ppc.available = true;
            tmpInteractableSetup.AddComponent<ItemDronePurchaseController>();
            var model2 = tmpInteractableSetup.GetComponent<ModelLocator>();
            model2.modelTransform.localScale = new Vector3(0.2f, 0.2f, 0.4f);

            itemDroneInteractablePrefab = tmpInteractableSetup.InstantiateClone("TkSatItemDroneBroken");
            GameObject.Destroy(tmpInteractableSetup);

            ////spawncard////

            itemDroneSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            itemDroneSpawnCard.prefab = itemDroneInteractablePrefab;
            itemDroneSpawnCard.sendOverNetwork = true;
            itemDroneSpawnCard.hullSize = HullClassification.Human;
            itemDroneSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            itemDroneSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            itemDroneSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoChestSpawn;
            itemDroneSpawnCard.directorCreditCost = 15;
            itemDroneSpawnCard.occupyPosition = true;
            itemDroneSpawnCard.eliteRules = SpawnCard.EliteRules.Default;
            itemDroneSpawnCard.orientToFloor = true;
            itemDroneSpawnCard.slightlyRandomizeOrientation = true;
            itemDroneSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

            ContentAddition.AddBody(itemDroneBodyPrefab);
            ContentAddition.AddMaster(itemDroneMasterPrefab);

            itemDroneDirectorCard = new DirectorCard {
                spawnCard = itemDroneSpawnCard,
                minimumStageCompletions = 0,
                preventOverhead = false,
                selectionWeight = 0,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard
            };
            itemDroneDCH = new DirectorAPI.DirectorCardHolder {
                Card = itemDroneDirectorCard,
                InteractableCategory = DirectorAPI.InteractableCategory.Drones,
                MonsterCategory = DirectorAPI.MonsterCategory.Invalid
            };

            DirectorAPI.Helpers.AddNewInteractable(itemDroneDCH);
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }
        public override void SetupConfig() {
            base.SetupConfig();
        }
        public override void Install() {
            base.Install();
            On.RoR2.PickupPickerController.HandlePickupSelected += PickupPickerController_HandlePickupSelected;
            On.RoR2.PickupPickerController.OnInteractionBegin += PickupPickerController_OnInteractionBegin;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.CharacterBody.GetDisplayName += CharacterBody_GetDisplayName;
            itemDroneDirectorCard.selectionWeight = 4;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.PickupPickerController.HandlePickupSelected -= PickupPickerController_HandlePickupSelected;
            On.RoR2.PickupPickerController.OnInteractionBegin -= PickupPickerController_OnInteractionBegin;
            On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteraction_OnInteractionBegin;
            On.RoR2.CharacterBody.GetDisplayName -= CharacterBody_GetDisplayName;
            DirectorAPI.Helpers.RemoveExistingInteractable(itemDroneDCH.Card.spawnCard.name);
            itemDroneDirectorCard.selectionWeight = 0;
        }


        private string CharacterBody_GetDisplayName(On.RoR2.CharacterBody.orig_GetDisplayName orig, CharacterBody self) {
            var retv = orig(self);
            if(self.name == "TkSatItemDroneBody(Clone)") {
                var ward = self.GetComponent<ItemWard>();
                if(!ward || ward.itemcounts.Count == 0) return retv;
                var idef = ItemCatalog.GetItemDef(ward.itemcounts.First().Key);
                if(idef == null) return retv;
                
                var color = "FFFFFF";
                var itd = ItemTierCatalog.GetItemTierDef(idef.tier);
                if(itd)
                    color = ColorCatalog.GetColorHexString(itd.colorIndex);
                return $"{retv} (<color=#{color}>{Language.GetString(idef.nameToken)}</color>)";
            }
            return retv;
        }

        private void PickupPickerController_HandlePickupSelected(On.RoR2.PickupPickerController.orig_HandlePickupSelected orig, PickupPickerController self, int choiceIndex) {
            orig(self, choiceIndex);

            if(!NetworkServer.active) return;

            var idpc = self.GetComponent<ItemDronePurchaseController>();
            if(!idpc) return;
            self.networkUIPromptController.ClearParticipant();
            if((ulong)choiceIndex >= (ulong)((long)self.options.Length))
                return;
            ref var pickupRef = ref self.options[choiceIndex];
            if(!pickupRef.available) return;
            PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(pickupRef.pickupIndex.value));
            if(pickupDef == null || !idpc.currentInteractor) return;
            var body = idpc.currentInteractor.GetComponent<CharacterBody>();
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

            var summon = self.GetComponent<SummonMasterBehavior>();
            var cm = summon.OpenSummonReturnMaster(idpc.currentInteractor);
            var cmBody = cm.GetBodyObject();
            var persist = cm.GetComponent<ItemDroneWardPersist>();
            if(!cmBody || !persist)
                return;
            var cmWard = cmBody.GetComponent<ItemWard>();
            //cmWard.radius = 50f;
            body.inventory.RemoveItem(pickupDef.itemIndex, remCount);
            //cmWard.ServerAddItem(pickupDef.itemIndex);
            persist.index = pickupDef.itemIndex;
            persist.count = remCount;

            for(var i = 0; i < remCount; i++) {
                var effectData = new EffectData {
                    origin = body.corePosition,
                    genericFloat = Mathf.Lerp(1.5f, 2.5f, (float)i/(float)remCount),
                    genericUInt = (uint)(pickupDef.itemIndex + 1)
                };
                effectData.SetNetworkedObjectReference(cmBody);
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/ItemTakenOrbEffect"), effectData, true);
            }

            GameObject.Destroy(self.gameObject);
        }

        private void PickupPickerController_OnInteractionBegin(On.RoR2.PickupPickerController.orig_OnInteractionBegin orig, PickupPickerController self, Interactor activator) {
            if(self && NetworkServer.active) {
                var idpc = self.GetComponent<ItemDronePurchaseController>();

                if(idpc) {
                    idpc.currentInteractor = activator;
                    self.SetOptionsFromInteractor(activator);
                }
            }
            orig(self, activator);
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator) {
            if(self && NetworkServer.active) {
                var idpc = self.GetComponent<ItemDronePurchaseController>();

                if(idpc) {
                    self.GetComponent<PickupPickerController>().OnInteractionBegin(activator);
                    return;
                }
            }
            orig(self, activator);
        }
    }

    public class ItemDronePurchaseController : MonoBehaviour {
        public Interactor currentInteractor;
    }

    [RequireComponent(typeof(CharacterBody))]
    public class ItemDroneDropOnDeath : MonoBehaviour, IOnKilledServerReceiver {
        CharacterBody body;

        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        public void OnKilledServer(DamageReport damageReport) {
            if(!body || !body.master || !body.master.IsDeadAndOutOfLivesServer()) return;
            var idwp = body.master.GetComponent<ItemDroneWardPersist>();
            if(!idwp || idwp.count <= 0) return;

            var theta = 360f / (float)idwp.count;
            for(int i = 0; i < idwp.count; i++) {
                var vel = Quaternion.AngleAxis((float)i * theta, Vector3.up) * new Vector3(5f, 5f);
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(idwp.index), transform.position, vel);
            }
            MonoBehaviour.Destroy(this);
        }
    }

    [RequireComponent(typeof(CharacterMaster))]
    public class ItemDroneWardPersist : MonoBehaviour {
        public ItemIndex index = ItemIndex.None;
        public int count = 0;
        CharacterMaster master;
        void Awake() {
            master = GetComponent<CharacterMaster>();
        }
        void FixedUpdate() {
            if(!NetworkServer.active || !master || index == ItemIndex.None) return;
            var body = master.GetBodyObject();
            if(!body) return;
            var ward = body.GetComponent<ItemWard>();
            if(!ward) return;
            if(ward.radius != 60f)
                ward.radius = 60f;
            int oldCount = 0;
            ward.itemcounts.TryGetValue(index, out oldCount);
            var ctc = Mathf.Abs(oldCount - count);
            if(oldCount < count) {
                for(var i = 0; i < ctc; i++)
                    ward.ServerAddItem(index);
            } else if(oldCount > count) {
                for(var i = 0; i < ctc; i++)
                    ward.ServerRemoveItem(index);
            }
        }
    }
}
