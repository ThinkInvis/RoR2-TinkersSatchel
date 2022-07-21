using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class MonkeysPaw : Equipment<MonkeysPaw> {

        ////// Equipment Data //////

        public override string displayName => "Lemurian's Claw";
        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 120f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Clone an item to every ally... <style=cDeath>BUT enemies also receive one per ally.</style>";
        protected override string GetDescString(string langid = null) =>
            $"Use on an item drop to consume it and give <style=cDeath>ALL characters</style> a copy of it; enemies gain multiple equal to ally count.";
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



        ////// TILER2 Module Setup //////

        public MonkeysPaw() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/MonkeysPaw.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/monkeysPawIcon.png");
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