using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
    public class Recombobulator : Equipment<Recombobulator> {
        //TODO: maybe also allow on enemies/bosses?
        //TODO: keep existing cost for stage

        ////// Equipment Data //////

        public override string displayName => "Quantum Recombobulator";
        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override float cooldown { get; protected set; } = 60f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Reroll an interactable once.";
        protected override string GetDescString(string langid = null) => $"<style=cIsUtility>Reroll</style> an interactable into a different one which is valid for the current stage. <style=cIsUtility>Only works once per interactable</style>.";
        protected override string GetLoreString(string langid = null) => $"";



        ////// Config //////
        
        [AutoConfigRoOString()]
        [AutoConfig("Which object names are allowed for recombobulation (comma-delimited, leading/trailing whitespace will be ignored). Items not in this list will also not be selected as the new object. WARNING: May have unintended results on some untested objects!",
            AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever)]
        public string objectNamesConfig { get; private set; } = String.Join(", ", new[] {
            "Turret1Broken",
            "Drone1Broken",
            "Drone2Broken",
            "EquipmentDroneBroken",
            "MissileDroneBroken",
            "FlameDroneBroken",
            "MegaDroneBroken",
            "Chest1",
            "Chest2",
            "GoldChest",
            "CasinoChest",
            "ShrineHealing",
            "EquipmentBarrel",
            "ShrineBlood",
            "ShrineChance",
            "ShrineCombat",
            "ShrineBoss",
            "ShrineCleanse",
            "ShrineRestack",
            "ShrineGoldshoresAccess",
            "CategoryChestDamage",
            "CategoryChestHealing",
            "CategoryChestUtility",
            "Duplicator",
            "DuplicatorLarge",
            "DuplicatorWild",
            "Scrapper",
            "MultiShopTerminal",
            "MultiShopLargeTerminal",
            "MultiShopEquipmentTerminal",
            "LunarChest",
            "ItemDroneBroken",
            "BulwarkDroneBroken"
        });



        ////// Other Fields/Properties //////

        public static HashSet<string> validObjectNames { get; private set; } = new HashSet<string>();
        WeightedSelection<DirectorCard> mostRecentDeck = null;
        internal static UnlockableDef unlockable;


        ////// TILER2 Module Setup //////

        public Recombobulator() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Recombobulator.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/recombobulatorIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
            var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/recombobulatorIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            LanguageAPI.Add(achiNameToken, "Risktaker");
            LanguageAPI.Add(achiDescToken, "Recycle a rare or boss item.");
            equipmentDef.unlockableDef = unlockable;

            if(Compat_ClassicItems.enabled) {
                LanguageAPI.Add("TKSAT_RECOMBOBULATOR_CI_EMBRYO_APPEND", "\n<style=cStack>Beating Embryo: Roll twice and choose the rarer result.</style>");
                Compat_ClassicItems.RegisterEmbryoHook(equipmentDef, "TKSAT_RECOMBOBULATOR_CI_EMBRYO_APPEND", () => "TKSAT.QuantumRecombobulator");
            }
        }

        public override void SetupConfig() {
            base.SetupConfig();
            validObjectNames.UnionWith(objectNamesConfig.Split(',')
                .Select(x => x.Trim() + "(Clone)"));
        }

        public override void Install() {
            base.Install();
            On.RoR2.SceneDirector.GenerateInteractableCardSelection += SceneDirector_GenerateInteractableCardSelection;
            On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlot_UpdateTargets;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ScrapperController.BeginScrapping += ScrapperController_BeginScrapping;
            IL.EntityStates.Drone.DeathState.OnImpactServer += DeathState_OnImpactServer;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.SceneDirector.GenerateInteractableCardSelection -= SceneDirector_GenerateInteractableCardSelection;
            On.RoR2.EquipmentSlot.UpdateTargets -= EquipmentSlot_UpdateTargets;
            On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ScrapperController.BeginScrapping -= ScrapperController_BeginScrapping;
            IL.EntityStates.Drone.DeathState.OnImpactServer -= DeathState_OnImpactServer;
        }



        ////// Private Methods //////

        private void DeathState_OnImpactServer(ILContext il) {
            var c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(
                    typeof(GameObject)
                    .GetMethod(nameof(GameObject.GetComponent), new Type[] { })
                    .MakeGenericMethod(typeof(PurchaseInteraction)))
                )) {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate<Action<GameObject>>(obj => {
                    Debug.Log($"Adding RecombobulatorFlag to {obj}");
                    obj.AddComponent<RecombobulatorFlag>();
                });
            }
        }

        private GameObject FindNearestRerollable(GameObject senderObj, Ray aim, float maxAngle, float maxDistance, bool requireLoS) {
            aim = CameraRigController.ModifyAimRayIfApplicable(aim, senderObj, out float camAdjust);
            var results = Physics.OverlapSphere(aim.origin, maxDistance + camAdjust, Physics.AllLayers, QueryTriggerInteraction.Collide);
            var minDot = Mathf.Cos(Mathf.Clamp(maxAngle, 0f, 180f) * Mathf.PI / 180f);
            return results
                .Where(x => x && x.gameObject)
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

        private void ScrapperController_BeginScrapping(On.RoR2.ScrapperController.orig_BeginScrapping orig, ScrapperController self, int intPickupIndex) {
            orig(self, intPickupIndex);
            if(self && !self.GetComponent<RecombobulatorFlag>())
                self.gameObject.AddComponent<RecombobulatorFlag>();
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator) {
            orig(self, activator);
            if(self && self.CanBeAffordedByInteractor(activator) && !self.GetComponent<RecombobulatorFlag>())
                self.gameObject.AddComponent<RecombobulatorFlag>();
        }

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
                var purch = res.GetComponent<PurchaseInteraction>();
                if(!res.GetComponent<RecombobulatorFlag>() && (!purch || purch.available)) {
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

                var oldPurch = slot.currentTarget.rootObject.GetComponent<PurchaseInteraction>();
                if(oldPurch) {
                    if(!oldPurch.available) return false;
                }

                var shopcpt = slot.currentTarget.rootObject.GetComponent<ShopTerminalBehavior>();
                if(shopcpt && shopcpt.serverMultiShopController) {
                    slot.currentTarget.rootObject = shopcpt.serverMultiShopController.transform.root.gameObject;
                    foreach(var term in shopcpt.serverMultiShopController.terminalGameObjects)
                        GameObject.Destroy(term);
                }

                GameObject.Destroy(slot.currentTarget.rootObject);

                var pos = slot.currentTarget.rootObject.transform.position;

                WeightedSelection<DirectorCard> filteredDeck = new(8);
                for(var i = 0; i < mostRecentDeck.Count; i++) {
                    var card = mostRecentDeck.GetChoice(i);
                    if(card.value != null && card.value.IsAvailable() && (validObjectNames.Contains(card.value.spawnCard.prefab.name) || validObjectNames.Contains(card.value.spawnCard.prefab.name + "(Clone)")))
                        filteredDeck.AddChoice(card);
                }
                if(filteredDeck.Count == 0)
                    return false;

                var draw = filteredDeck.Evaluate(rng.nextNormalizedFloat);

                if(Compat_ClassicItems.enabled) {
                    var rerolls = Compat_ClassicItems.CheckEmbryoProc(slot, equipmentDef);
                    for(var i = 0; i < rerolls; i++) {
                        var draw2 = filteredDeck.Evaluate(rng.nextNormalizedFloat);
                        if(draw2.selectionWeight < draw.selectionWeight)
                            draw = draw2;
                    }
                }

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

                var shopcpt2 = obj.GetComponent<MultiShopController>();
                if(shopcpt2) {
                    foreach(var term in shopcpt2.terminalGameObjects)
                        term.AddComponent<RecombobulatorFlag>();
                }

                return true;
            }
            return false;
        }
    }

    public class RecombobulatorFlag : MonoBehaviour {}

    [RegisterAchievement("TkSat_Recombobulator", "TkSat_RecombobulatorUnlockable", "")]
    public class TkSatRecombobulatorAchievement : RoR2.Achievements.BaseAchievement {
        public override void OnInstall() {
            base.OnInstall();

            IL.RoR2.EquipmentSlot.FireRecycle += EquipmentSlot_FireRecycle;
        }

        public override void OnUninstall() {
            base.OnUninstall();

            IL.RoR2.EquipmentSlot.FireRecycle -= EquipmentSlot_FireRecycle;
        }

        private void EquipmentSlot_FireRecycle(ILContext il) {
            ILCursor c = new(il);

            if(c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt<GenericPickupController>("set_NetworkpickupIndex"))) {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate<Action<PickupIndex>>(pind => {
                    var pdef = PickupCatalog.GetPickupDef(pind);
                    if(pdef != null
                    && (pdef.itemTier == ItemTier.Tier3 || pdef.itemTier == ItemTier.VoidTier3
                    || pdef.itemTier == ItemTier.Boss || pdef.itemTier == ItemTier.VoidBoss))
                        Grant();
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("TkSatRecombobulatorAchievement: failed to apply IL hook (EquipmentSlot_FireRecycle); could not find target instructions. Achievement will not trigger.");
            }
        }
    }
}