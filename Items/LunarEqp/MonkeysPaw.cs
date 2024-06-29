﻿using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;
using UnityEngine.AddressableAssets;
using System;
using System.Collections.Generic;
using RoR2.Stats;

namespace ThinkInvisible.TinkersSatchel {
    public class MonkeysPaw : Equipment<MonkeysPaw> {

        ////// Equipment Data //////

        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 120f;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            refundFrac.ToString("0%")
        };


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
            "CategoryChestUtility",
            "CategoryChest2Damage Variant",
            "CategoryChest2Healing Variant",
            "CategoryChest2Utility Variant"
        });

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Cost reduction (0 = no reduction).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float refundFrac { get; private set; } = 0.5f;



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

        bool IsInteractableValid(GameObject obj, out ChestBehavior cb, out PurchaseInteraction purch) {
            cb = null;
            purch = null;
            if(!obj) return false;
            return obj.TryGetComponent(out cb)
                && (!obj.TryGetComponent<PurchaseInteraction>(out purch)
                    || (purch.available && purch.costType is CostTypeIndex.Money or CostTypeIndex.PercentHealth or CostTypeIndex.LunarCoin))
                && cb.dropTable is BasicPickupDropTable bpdt && bpdt.equipmentWeight == 0 && bpdt.lunarEquipmentWeight == 0;
        }



        ////// Hooks //////

        private void EquipmentSlot_UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget) {
            if(targetingEquipmentIndex != catalogIndex) {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
                return;
            }

            //clear vanilla targeting info, in case we're swapping from another equipment
            self.currentTarget = default(EquipmentSlot.UserTargetInfo);
            self.targetIndicator.targetTransform = null;

            var res = CommonCode.FindNearestInteractable(self.gameObject, validObjectNames, self.GetAimRay(), 10f, 20f, false);

            if(res) {
                self.targetIndicator.targetTransform = res.transform;
                if(IsInteractableValid(res, out _, out _)) {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
                } else {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerBadIndicator");
                }
                self.targetIndicator.active = true;
            } else self.targetIndicator.active = false;
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            slot.UpdateTargets(catalogIndex, false);
            var targetObj = slot.targetIndicator.targetTransform.gameObject;
            if(!IsInteractableValid(targetObj, out ChestBehavior cb, out PurchaseInteraction purch)) return false;

            if(purch && refundFrac != 1f) {
                var origCost = purch.cost;
                purch.cost = Mathf.CeilToInt(purch.cost * (1f - refundFrac));
                if(!slot.characterBody.TryGetComponent<Interactor>(out var iac) || !purch.CanBeAffordedByInteractor(iac)) {
                    purch.cost = origCost;
                    return false;
                }
                var payCostResults = CostTypeCatalog.GetCostTypeDef(purch.costType)
                    .PayCost(purch.cost, iac, targetObj, rng, ItemIndex.None); //paying items currently unsupported
                if(slot.characterBody)
                    StatManager.OnPurchase(slot.characterBody, purch.costType, purch.purchaseStatNames.Select(new Func<string, StatDef>(StatDef.Find)));
            }

            cb.dropCount++;
            cb.Open();

            Chat.SendBroadcastChat(new SubjectChatMessage {
                baseToken = "TKSAT_MONKEYSPAW_ACTIVATED",
                subjectAsCharacterBody = slot.characterBody
            });

            if(purch)
                purch.SetAvailable(false);

            var aiSafePind = CommonCode.GenerateAISafePickup(this.rng, cb.dropTable, Run.instance.smallChestDropTierSelector);
            CatalogUtil.TryGetItemDef(aiSafePind, out var aiSafeIdef);
            var aiSafeTdef = ItemTierCatalog.GetItemTierDef(aiSafeIdef.tier);

            var grantCount = aiSafeIdef.tier switch {
                ItemTier.Tier2 or ItemTier.VoidTier2 => 3,
                ItemTier.Tier3 or ItemTier.VoidTier3 or ItemTier.Boss or ItemTier.VoidBoss => 1,
                _ => 5
            };

            Chat.SendBroadcastChat(new ColoredTokenChatMessage {
                baseToken = "TKSAT_MONKEYSPAW_ITEMGRANT",
                paramTokens = new[] { Language.GetString(aiSafeIdef.nameToken), grantCount.ToString() },
                paramColors = new[] { ColorCatalog.GetColor(aiSafeTdef.colorIndex), new Color32(255, 255, 255, 255) }
            });

            var enemies = MiscUtil.GatherEnemies(TeamIndex.Player, TeamIndex.Neutral, TeamIndex.None)
                .Where(e => e.body && e.body.inventory);

            foreach(var enemy in enemies)
                RoR2.Orbs.ItemTransferOrb.DispatchItemTransferOrb(targetObj.transform.position, enemy.body.inventory, aiSafeIdef.itemIndex, grantCount);

            slot.InvalidateCurrentTarget();
            return true;
        }
    }
}