using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class Recombobulator : Equipment<Recombobulator> {
        //TODO: maybe also allow on enemies/bosses?

        ////// Equipment Data //////

        public override string displayName => "Quantum Recombobulator";
        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered => false;
        public override float cooldown { get; protected set; } = 90f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Reroll an interactable once.";
        protected override string GetDescString(string langid = null) => $"Reroll an interactable into a different one which is valid for the current stage. Only works once per interactable.";
        protected override string GetLoreString(string langid = null) => $"";



        ////// Other Fields/Properties //////

        private static readonly string[] validObjectNames = new[] {
            "Turret1Broken(Clone)",
            "Drone1Broken(Clone)",
            "Drone2Broken(Clone)",
            "GoldChest(Clone)",
            "MissileDroneBroken(Clone)",
            "FlameDroneBroken(Clone)",
            "MegaDroneBroken(Clone)",
            "Chest1(Clone)",
            "Chest2(Clone)",
            "KeyLockbox(Clone)",
            "ShrineHealing(Clone)",
            "EquipmentBarrel(Clone)",
            "ShrineBlood(Clone)",
            "ShrineChance(Clone)",
            "ShrineCombat(Clone)",
            "ShrineBoss(Clone)",
            "ShrineCleanse(Clone)",
            "ShrineRestack(Clone)",
            "ShrineGoldshoresAccess(Clone)",
            "CategoryChestDamage(Clone)",
            "CategoryChestHealing(Clone)",
            "CategoryChestUtility(Clone)",
            "Barrel1(Clone)",
            "Duplicator(Clone)",
            "DuplicatorLarge(Clone)",
            "DuplicatorWild(Clone)",
            "Scrapper(Clone)",
            "MultiShopTerminal(Clone)",
            "MultiShopLargeTerminal(Clone)",
            "MultiShopEquipmentTerminal(Clone)",
            "LunarChest(Clone)"
        };
        WeightedSelection<DirectorCard> mostRecentDeck = null;



        ////// TILER2 Module Setup //////

        public Recombobulator() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Recombobulator.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/recombobulatorIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            On.RoR2.SceneDirector.GenerateInteractableCardSelection += SceneDirector_GenerateInteractableCardSelection;
            On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlot_UpdateTargets;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.SceneDirector.GenerateInteractableCardSelection -= SceneDirector_GenerateInteractableCardSelection;
            On.RoR2.EquipmentSlot.UpdateTargets -= EquipmentSlot_UpdateTargets;
        }



        ////// Private Methods //////

        private GameObject FindNearestRerollable(GameObject senderObj, Ray aim, float maxAngle, float maxDistance, bool requireLoS) {
            aim = CameraRigController.ModifyAimRayIfApplicable(aim, senderObj, out float camAdjust);
            var results = Physics.OverlapSphere(aim.origin, maxDistance + camAdjust, Physics.AllLayers, QueryTriggerInteraction.Collide);
            var minDot = Mathf.Cos(Mathf.Clamp(maxAngle, 0f, 180f) * Mathf.PI / 180f);
            return results
                .Select(x => MiscUtil.GetRootWithLocators(x.gameObject))
                .Where(x => validObjectNames.Contains(x.name))
                .Select(x => (target: x, vdot: Vector3.Dot(aim.direction, (x.transform.position - aim.origin).normalized)))
                .Where(x => x.vdot > minDot
                    && (!requireLoS
                    || !Physics.Linecast(aim.origin, x.target.transform.position, LayerIndex.world.mask)
                    ))
                .OrderBy(x => x.vdot * Vector3.Distance(x.target.transform.position, aim.origin))
                .Select(x => x.target.gameObject)
                .FirstOrDefault();
        }



        ////// Hooks //////

        private WeightedSelection<DirectorCard> SceneDirector_GenerateInteractableCardSelection(On.RoR2.SceneDirector.orig_GenerateInteractableCardSelection orig, SceneDirector self) {
            var retv = orig(self);
            mostRecentDeck = retv;
            return retv;
        }

        private void EquipmentSlot_UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget) {
            if(targetingEquipmentIndex != catalogIndex || self.subcooldownTimer > 0f || self.stock == 0) {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
                if(targetingEquipmentIndex == catalogIndex)
                    self.targetIndicator.active = false;
                return;
            }

            var cpt = self.characterBody.GetComponent<PackBoxTracker>();
            if(!cpt) cpt = self.characterBody.gameObject.AddComponent<PackBoxTracker>();

            var res = FindNearestRerollable(self.gameObject, self.GetAimRay(), 10f, 20f, false);
            Transform tsf = null;
            if(res) tsf = res.transform;
            self.currentTarget = new EquipmentSlot.UserTargetInfo {
                transformToIndicateAt = tsf,
                pickupController = null,
                hurtBox = null,
                rootObject = res
            };

            if(self.currentTarget.rootObject != null) {
                if(!res.GetComponent<RecombobulatorFlag>()) {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator");
                } else {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerBadIndicator");
                }

                self.targetIndicator.active = true;
                self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
            } else {
                self.targetIndicator.active = false;
            }
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            if(slot.currentTarget.rootObject
                && validObjectNames.Contains(slot.currentTarget.rootObject.name)
                && !slot.currentTarget.rootObject.GetComponent<RecombobulatorFlag>()
                && mostRecentDeck != null
                && Run.instance) {

                var shopcpt = slot.currentTarget.rootObject.GetComponent<ShopTerminalBehavior>();
                if(shopcpt && shopcpt.serverMultiShopController) {
                    slot.currentTarget.rootObject = shopcpt.serverMultiShopController.transform.root.gameObject;
                    foreach(var term in shopcpt.serverMultiShopController.terminalGameObjects)
                        GameObject.Destroy(term);
                }

                GameObject.Destroy(slot.currentTarget.rootObject);

                var pos = slot.currentTarget.rootObject.transform.position;

                WeightedSelection<DirectorCard> filteredDeck = new WeightedSelection<DirectorCard>(8);
                for(var i = 0; i < mostRecentDeck.Count; i++) {
                    var card = mostRecentDeck.GetChoice(i);
                    if(card.value != null && card.value.IsAvailable())
                        filteredDeck.AddChoice(card);
                }
                if(filteredDeck.Count == 0)
                    return false;

                var draw = mostRecentDeck.Evaluate(rng.nextNormalizedFloat);

                var obj = DirectorCore.instance.TrySpawnObject(
                    new DirectorSpawnRequest(
                        draw.spawnCard,
                        new DirectorPlacementRule {
                            placementMode = DirectorPlacementRule.PlacementMode.Direct,
                            position = pos,
                            preventOverhead = false
                        },
                        this.rng
                        ));
                if(!obj) {
                    TinkersSatchelPlugin._logger.LogError("Recombobulator failed to replace interactable!");
                    return false;
                }
                var purch = obj.GetComponent<PurchaseInteraction>();
                if(purch && purch.costType == CostTypeIndex.Money) {
                    purch.Networkcost = Run.instance.GetDifficultyScaledCost(purch.cost);
                }
                obj.AddComponent<RecombobulatorFlag>();
                return true;
            }
            return false;
        }
    }

    public class RecombobulatorFlag : MonoBehaviour {}
}