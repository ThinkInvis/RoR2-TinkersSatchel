using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using System.Linq;
using System.Collections.Generic;
using MonoMod.RuntimeDetour;
using R2API.Utils;

namespace ThinkInvisible.TinkersSatchel {
    public class PackBox : Equipment<PackBox> {

        ////// Equipment Data //////

        public override string displayName => "Cardboard Box";
        public override bool isLunar => false;
        public override float cooldown {get; protected set;} = 180f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Pack up and move.";
        protected override string GetDescString(string langid = null) =>
            "Use once to <style=cIsUtility>pack up</style> a <style=cIsDamage>turret</style>, <style=cIsHealing>healing shrine</style>, or <style=cIsUtility>most other interactables</style>. Use again to <style=cIsUtility>place</style> the packed object and put the Cardboard Box on cooldown.";
        protected override string GetLoreString(string langid = null) => "";




        ////// Other Fields/Properties //////

        private static readonly string[] validObjectNames = new[] {
            "Turret1Body(Clone)",
            "Turret1Broken(Clone)",
            "SquidTurretBody(Clone)",
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
            "FusionCellDestructibleBody(Clone)",
            "LunarChest(Clone)",
            "LunarShopTerminal(Clone)" //todo: disallow or kick out of bazaar, achievement
        };
        readonly Sprite secondaryIconResource;
        GameObject packIndicatorPrefab;
        GameObject placeIndicatorPrefab;
        GameObject placeIndicatorBadPrefab;



        ////// TILER2 Module Setup //////

        public PackBox() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/PackBox.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/packBoxIconOpen.png");
            secondaryIconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/packBoxIconClosed.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var nSpriteD = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/packBoxArrowDown.png");
            var nSpriteU = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/packBoxArrowUp.png");

            var recy = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator").InstantiateClone("temporary setup prefab", false);
            foreach(var spr in recy.GetComponentsInChildren<SpriteRenderer>()) {
                if(spr.sprite.name != "texRecyclerArrow") continue;
                spr.sprite = nSpriteD;
            }
            placeIndicatorPrefab = recy.InstantiateClone("TkSatPackBoxPlaceIndicator", false);
            GameObject.Destroy(recy);

            recy = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator").InstantiateClone("temporary setup prefab", false);
            foreach(var spr in recy.GetComponentsInChildren<SpriteRenderer>()) {
                if(spr.sprite.name != "texRecyclerArrow") continue;
                spr.sprite = nSpriteU;
            }
            packIndicatorPrefab = recy.InstantiateClone("TkSatPackBoxPackIndicator", false);
            GameObject.Destroy(recy);

            recy = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerBadIndicator").InstantiateClone("temporary setup prefab", false);
            foreach(var spr in recy.GetComponentsInChildren<SpriteRenderer>()) {
                if(spr.sprite.name != "texRecyclerArrow") continue;
                spr.sprite = nSpriteD;
            }
            placeIndicatorBadPrefab = recy.InstantiateClone("TkSatPackBoxPlaceBadIndicator", false);
            GameObject.Destroy(recy);
        }

        public override void Install() {
            base.Install();

            On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlot_UpdateTargets;
            On.RoR2.UI.EquipmentIcon.Update += EquipmentIcon_Update; //SetDisplayData hook is bugged and uses System.ValueType instead of DisplayData, can't easily make manual hook because it's not static

        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.EquipmentSlot.UpdateTargets -= EquipmentSlot_UpdateTargets;
            On.RoR2.UI.EquipmentIcon.Update -= EquipmentIcon_Update;
        }



        ////// Hooks //////

        private void EquipmentSlot_UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget) {
            if(targetingEquipmentIndex != catalogIndex || self.subcooldownTimer > 0f || self.stock == 0) {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
                if(targetingEquipmentIndex == catalogIndex)
                    self.targetIndicator.active = false;
                return;
            }

            var cpt = self.characterBody.GetComponent<PackBoxTracker>();
            if(!cpt) cpt = self.characterBody.gameObject.AddComponent<PackBoxTracker>();

            GameObject indPrefab;

            if(cpt.packedObject) {
                indPrefab = placeIndicatorPrefab;
                bool didPlace = TryPlaceBoxable(self.GetAimRay(), out Vector3 loc, out bool didHitGround);
                if(didPlace || didHitGround) {
                    if(!didPlace) indPrefab = placeIndicatorBadPrefab;
                    cpt.groundTarget.transform.position = loc;
                    self.currentTarget = new EquipmentSlot.UserTargetInfo {
                        transformToIndicateAt = cpt.groundTarget,
                        pickupController = null,
                        hurtBox = null,
                        rootObject = cpt.groundTarget.gameObject
                    };
                    //todo: on-ground indicator like engi blueprints, will need to track separately?
                } else self.currentTarget = new EquipmentSlot.UserTargetInfo {
                        transformToIndicateAt = null,
                        pickupController = null,
                        hurtBox = null,
                        rootObject = null
                    };
            } else {
                indPrefab = packIndicatorPrefab;
                var res = FindNearestBoxable(self.gameObject, self.GetAimRay(), 10f, 20f, false);
                self.currentTarget = new EquipmentSlot.UserTargetInfo {
                    transformToIndicateAt = res?.transform,
                    pickupController = null,
                    hurtBox = null,
                    rootObject = res
                };
            }

            if(self.currentTarget.rootObject != null) {
                self.targetIndicator.visualizerPrefab = indPrefab;
                self.targetIndicator.active = true;
                self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
            } else {
                self.targetIndicator.active = false;
            }
        }


        private void EquipmentIcon_Update(On.RoR2.UI.EquipmentIcon.orig_Update orig, RoR2.UI.EquipmentIcon self) {
            orig(self);
            if(self.iconImage && self.currentDisplayData.equipmentDef == instance.equipmentDef) {
                var cpt = self.targetEquipmentSlot?.characterBody?.GetComponent<PackBoxTracker>();
                if(cpt && cpt.packedObject)
                    self.iconImage.texture = instance.secondaryIconResource.texture;
            }
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            var cpt = slot.characterBody.GetComponent<PackBoxTracker>();
            if(!cpt) cpt = slot.characterBody.gameObject.AddComponent<PackBoxTracker>();

            if(cpt.packedObject == null) {
                if(validObjectNames.Contains(slot.currentTarget.rootObject.name)) {
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeleportOutBoom"), new EffectData {
                        origin = slot.currentTarget.rootObject.transform.position,
                        rotation = slot.currentTarget.rootObject.transform.rotation
                    }, true);

                    var shopcpt = slot.currentTarget.rootObject.GetComponent<ShopTerminalBehavior>();
                    if(shopcpt && shopcpt.serverMultiShopController) {
                        slot.currentTarget.rootObject = shopcpt.serverMultiShopController.transform.root.gameObject;
                        foreach(var terminal in shopcpt.serverMultiShopController._terminalGameObjects) {
                            cpt.auxiliaryPackedObjects.Add(terminal, terminal.transform.position - slot.currentTarget.rootObject.transform.position);
                        }
                    }

                    cpt.packedObject = slot.currentTarget.rootObject;
                    cpt.queuedDeactivate = true;
                    return false;
                }
            } else {
                if(TryPlaceBoxable(slot.GetAimRay(), out Vector3 placeLoc, out _)) {
                    var body = cpt.packedObject.GetComponent<CharacterBody>();
                    if(body && body.master)
                        cpt.packedObject.transform.position =
                            body.master.CalculateSafeGroundPosition(placeLoc, body)
                            + (body.corePosition - body.footPosition);
                    else cpt.packedObject.transform.position = placeLoc;
                    cpt.packedObject.SetActive(true);
                    var singleLoc = cpt.packedObject.GetComponentInChildren<ModelLocator>();
                    if(singleLoc)
                        singleLoc.modelTransform.gameObject.SetActive(true);
                    foreach(var aux in cpt.auxiliaryPackedObjects) {
                        aux.Key.transform.position = placeLoc + aux.Value;
                        aux.Key.SetActive(true);
                        var locs = aux.Key.gameObject.GetComponentsInChildren<ModelLocator>();
                        foreach(var loc in locs) {
                            loc.modelTransform.gameObject.SetActive(true);
                        }
                    }
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeleportOutBoom"), new EffectData {
                        origin = placeLoc,
                        rotation = cpt.packedObject.transform.rotation
                    }, true);
                    cpt.packedObject = null;
                    cpt.auxiliaryPackedObjects.Clear();

                    return true;
                } else return false;
            }

            return false;
        }

        bool TryPlaceBoxable(Ray aim, out Vector3 loc, out bool didHitGround) {
            var dir = aim.direction;
            dir.y = 0;
            dir.Normalize();
            aim.direction = dir;
            didHitGround = false;
            var groundAim = new Ray(aim.GetPoint(6f) + Vector3.up * 3f, Vector3.down);
            loc = Vector3.zero;
            if(Physics.SphereCast(groundAim, 0.5f, out RaycastHit hit, 8f, LayerIndex.world.mask) && hit.normal.y > 0.5f) {
                loc = hit.point;
                didHitGround = true;
                if(!Physics.CheckCapsule(
                    loc + Vector3.up * 1.32f,
                    loc + Vector3.up * 0.5f,
                    0.45f,
                    LayerIndex.world.mask | LayerIndex.defaultLayer.mask)) {
                    return true;
                }
            }
            return false;
        }

        private GameObject FindNearestBoxable(GameObject senderObj, Ray aim, float maxAngle, float maxDistance, bool requireLoS) {
            aim = CameraRigController.ModifyAimRayIfApplicable(aim, senderObj, out float camAdjust);
            var results = Physics.OverlapSphere(aim.origin, maxDistance + camAdjust, Physics.AllLayers, QueryTriggerInteraction.Collide);
            var minDot = Mathf.Cos(Mathf.Clamp(maxAngle, 0f, 180f) * Mathf.PI/180f);
            return results
                .Select(x => GetRootWithLocators(x.gameObject))
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

        private GameObject GetRootWithLocators(GameObject target, int maxSearch = 5) {
            if(!target) return null;
            GameObject scan = target;
            for(int i = 0; i < maxSearch; i++) {
                var cpt = scan.GetComponent<EntityLocator>();

                if(cpt) {
                    scan = cpt.entity;
                    continue;
                }

                var next = scan.transform.root;
                if(next && next.gameObject != scan)
                    scan = next.gameObject;
                else
                    return scan;
            }
            return scan;
        }
    }

    public class PackBoxTracker : MonoBehaviour {
        public GameObject packedObject;
        public Dictionary<GameObject, Vector3> auxiliaryPackedObjects = new Dictionary<GameObject, Vector3>();
        public bool queuedDeactivate = false;
        public Transform groundTarget;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        void Awake() {
            groundTarget = new GameObject().transform;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        void LateUpdate() {
            if(queuedDeactivate) {
                queuedDeactivate = false;
                packedObject.SetActive(false);
                var loc = packedObject.GetComponentInChildren<ModelLocator>();
                if(loc)
                    loc.modelTransform.gameObject.SetActive(false);
                foreach(var obj in auxiliaryPackedObjects) {
                    obj.Key.SetActive(false);
                    loc = obj.Key.GetComponentInChildren<ModelLocator>();
                    if(loc)
                        loc.modelTransform.gameObject.SetActive(false);
                }
            }
        }
    }
}