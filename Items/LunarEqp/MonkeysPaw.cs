using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
    public class MonkeysPaw : Equipment<MonkeysPaw> {

        ////// Equipment Data //////

        public override string displayName => "Lemurian's Claw";
        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 120f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Clone an item to every ally... <style=cDeath>BUT living enemies also receive one per ally.</style>";
        protected override string GetDescString(string langid = null) =>
            $"Use on an item drop to consume it and give <style=cDeath>ALL characters</style> a copy of it; living enemies gain multiple equal to ally count.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, this equipment will not work on Lunar items.",
            AutoConfigFlags.PreventNetMismatch)]
        public bool noLunars { get; private set; } = true;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, this equipment will not work on Void items.",
            AutoConfigFlags.PreventNetMismatch)]
        public bool noVoid { get; private set; } = true;



        ////// Other Fields/Properties //////

        public GameObject idrPrefab { get; private set; }



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

        public override void RefreshPermanentLanguage() {
            permanentGenericLanguageTokens["TKSAT_MONKEYSPAW_ACTIVATED"] = "<style=cEvent>{0}'s <color=#307FFF>Lemurian's Claw</color> curls...</style>";
            permanentGenericLanguageTokens["TKSAT_MONKEYSPAW_ACTIVATED_2P"] = "<style=cEvent>Your <color=#307FFF>Lemurian's Claw</color> curls...</style>";
            permanentGenericLanguageTokens["TKSAT_MONKEYSPAW_ITEMGRANT"] = "<color=#FFFF00>EVERYONE</color> <style=cEvent>picked up {1}. Enemies received {2}.</style>";

            base.RefreshPermanentLanguage();
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

        bool IsPickupValid(GenericPickupController ctrl) {
            if(!ctrl) return false;
            var pdef = PickupCatalog.GetPickupDef(ctrl.pickupIndex);
            if(pdef == null || pdef.itemIndex == ItemIndex.None) return false;
            var idef = ItemCatalog.GetItemDef(pdef.itemIndex);
            if(!idef) return false;
            if(noLunars && idef.tier == ItemTier.Lunar) return false;
            if(noVoid &&
                (idef.tier == ItemTier.VoidTier1 || idef.tier == ItemTier.VoidTier2
                || idef.tier == ItemTier.VoidTier3 || idef.tier == ItemTier.VoidBoss))
                return false;

            return true;
        }



        ////// Hooks //////

        private void EquipmentSlot_UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget) {
            if(targetingEquipmentIndex != catalogIndex)
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);

            self.currentTarget = new EquipmentSlot.UserTargetInfo(self.FindPickupController(self.GetAimRay(), 10f, 30f, true, false));
            if(self.currentTarget.transformToIndicateAt && IsPickupValid(self.currentTarget.pickupController)) {
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
            if(!IsPickupValid(slot.currentTarget.pickupController)) return false;

            var pickup = PickupCatalog.GetPickupDef(slot.currentTarget.pickupController.pickupIndex);;

            Chat.SendBroadcastChat(new SubjectChatMessage {
                baseToken = "TKSAT_MONKEYSPAW_ACTIVATED",
                subjectAsCharacterBody = slot.characterBody
            });

            var allyCount = CharacterMaster.readOnlyInstancesList.Count(cm => cm.teamIndex == slot.teamComponent.teamIndex);
            var idef = ItemCatalog.GetItemDef(pickup.itemIndex);
            var tdef = ItemTierCatalog.GetItemTierDef(idef.tier);

            Chat.SendBroadcastChat(new ColoredTokenChatMessage {
                baseToken = "TKSAT_MONKEYSPAW_ITEMGRANT",
                paramTokens = new[] { Language.GetString(idef.nameToken), "x" + allyCount.ToString() },
                paramColors = new[] { ColorCatalog.GetColor(tdef.colorIndex), new Color32(255, 255, 255, 255) }
            });

            foreach(var cm in CharacterMaster.readOnlyInstancesList) {
                if(cm.teamIndex != slot.teamComponent.teamIndex)
                    cm.inventory.GiveItem(pickup.itemIndex, allyCount);
                else
                    cm.inventory.GiveItem(pickup.itemIndex);
            }

            GameObject.Destroy(slot.currentTarget.rootObject);
            slot.InvalidateCurrentTarget();
            return true;
        }
    }
}