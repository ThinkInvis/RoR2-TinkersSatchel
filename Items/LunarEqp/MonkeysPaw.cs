using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;
using UnityEngine.AddressableAssets;
using System;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
    public class MonkeysPaw : Equipment<MonkeysPaw> {

        ////// Equipment Data //////

        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 120f;



        ////// Config //////
        
        [AutoConfigRoOString()]
        [AutoConfig("Which object names are allowed for activation (comma-delimited, leading/trailing whitespace will be ignored). Target objects must also incorporate a ChestBehavior component. WARNING: May have unintended results on some untested objects!",
            AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever)]
        public string objectNamesConfig { get; private set; } = String.Join(", ", new[] {
            "Chest1",
            "Chest2",
            "GoldChest",
            "CategoryChestDamage",
            "CategoryChestHealing",
            "CategoryChestUtility"
        });



        ////// Other Fields/Properties //////

        public GameObject idrPrefab { get; private set; }
        public static HashSet<string> validObjectNames { get; private set; } = new HashSet<string>();



        ////// TILER2 Module Setup //////

        public MonkeysPaw() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/MonkeysPaw.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/monkeysPawIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/MonkeysPaw.prefab");
        }

        public override void SetupModifyEquipmentDef() {
            base.SetupModifyEquipmentDef();

            modelResource.transform.Find("MonkeysPaw").gameObject.GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Lemurian/matLemurian.mat").WaitForCompletion();
            idrPrefab.transform.Find("MonkeysPaw").gameObject.GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Lemurian/matLemurian.mat").WaitForCompletion();
            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.22045F, -0.06626F, 0.11193F),
                localAngles = new Vector3(359.0299F, 357.3219F, 25.2928F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.38728F, 0.00965F, -0.06446F),
                localAngles = new Vector3(31.87035F, 332.9695F, 3.18838F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.23353F, -0.00868F, -0.08696F),
                localAngles = new Vector3(27.00084F, 326.5775F, 4.93487F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
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

        public override void SetupConfig() {
            base.SetupConfig();
            validObjectNames.UnionWith(objectNamesConfig.Split(',')
                .Select(x => x.Trim()));
        }

        public override void Install() {
            base.Install();
            On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlot_UpdateTargets;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.EquipmentSlot.UpdateTargets -= EquipmentSlot_UpdateTargets;
        }


        ////// Private Methods //////

        bool IsInteractableValid(GameObject obj, out ChestBehavior cb) {
            cb = null;
            if(!obj) return false;
            return obj.TryGetComponent(out cb) && obj.TryGetComponent<PurchaseInteraction>(out var purch) && purch.available && cb.dropTable is BasicPickupDropTable bpdt && bpdt.equipmentWeight == 0 && bpdt.lunarEquipmentWeight == 0;
        }



        ////// Hooks //////

        private void EquipmentSlot_UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget) {
            if(targetingEquipmentIndex != catalogIndex) {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
                return;
            }

            var res = CommonCode.FindNearestInteractable(self.gameObject, validObjectNames, self.GetAimRay(), 10f, 20f, false);
            Transform tsf = null;
            if(res) tsf = res.transform;
            self.currentTarget = new EquipmentSlot.UserTargetInfo {
                transformToIndicateAt = tsf,
                pickupController = null,
                hurtBox = null,
                rootObject = res
            };

            if(self.currentTarget.rootObject != null) {
                var purch = res.GetComponent<PurchaseInteraction>();
                var ch = res.GetComponent<ChestBehavior>();
                if(!purch || purch.available) {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
                } else {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerBadIndicator");
                }

                self.targetIndicator.active = true;
                self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
            } else {
                self.targetIndicator.active = false;
            }

            if(self.currentTarget.transformToIndicateAt && IsInteractableValid(res, out _)) {
                self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
                self.targetIndicator.active = true;
                self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
            } else {
                self.targetIndicator.active = false;
                self.targetIndicator.targetTransform = null;
            }
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            slot.UpdateTargets(catalogIndex, false);
            if(!IsInteractableValid(slot.currentTarget.rootObject, out ChestBehavior cb)) return false;

            cb.dropCount++;
            cb.Open();

            Chat.SendBroadcastChat(new SubjectChatMessage {
                baseToken = "TKSAT_MONKEYSPAW_ACTIVATED",
                subjectAsCharacterBody = slot.characterBody
            });

            cb.Roll();

            var pdef = PickupCatalog.GetPickupDef(cb.dropPickup);
            var idef = ItemCatalog.GetItemDef(pdef.itemIndex);
            var tdef = ItemTierCatalog.GetItemTierDef(idef.tier);

            var grantCount = idef.tier switch {
                ItemTier.Tier2 or ItemTier.VoidTier2 => 3,
                ItemTier.Tier3 or ItemTier.VoidTier3 or ItemTier.Boss or ItemTier.VoidBoss => 1,
                _ => 5
            };

            Chat.SendBroadcastChat(new ColoredTokenChatMessage {
                baseToken = "TKSAT_MONKEYSPAW_ITEMGRANT",
                paramTokens = new[] { Language.GetString(idef.nameToken), "x" + grantCount.ToString() },
                paramColors = new[] { ColorCatalog.GetColor(tdef.colorIndex), new Color32(255, 255, 255, 255) }
            });

            var enemies = MiscUtil.GatherEnemies(TeamIndex.Player, TeamIndex.Neutral, TeamIndex.None)
                .Where(e => e.body && e.body.inventory);

            foreach(var enemy in enemies)
                RoR2.Orbs.ItemTransferOrb.DispatchItemTransferOrb(TeleporterInteraction.instance.holdoutZoneController.transform.position, enemy.body.inventory, pdef.itemIndex, grantCount);

            slot.InvalidateCurrentTarget();
            return true;
        }
    }
}