using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using System.Linq;
using System.Collections.Generic;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

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

            R2API.Networking.NetworkingAPI.RegisterMessageType<MsgPackboxPack>();
            R2API.Networking.NetworkingAPI.RegisterMessageType<MsgPackboxPlace>();
        }

        public override void Install() {
            base.Install();

            On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlot_UpdateTargets;
            On.RoR2.UI.EquipmentIcon.Update += EquipmentIcon_Update; //SetDisplayData hook is bugged and uses System.ValueType instead of DisplayData, can't easily make manual hook because it's not static
            On.RoR2.UI.AllyCardController.UpdateInfo += AllyCardController_UpdateInfo;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.EquipmentSlot.UpdateTargets -= EquipmentSlot_UpdateTargets;
            On.RoR2.UI.EquipmentIcon.Update -= EquipmentIcon_Update;
            On.RoR2.UI.AllyCardController.UpdateInfo -= AllyCardController_UpdateInfo;
        }



        ////// Hooks //////

        private void AllyCardController_UpdateInfo(On.RoR2.UI.AllyCardController.orig_UpdateInfo orig, RoR2.UI.AllyCardController self) {
            orig(self);
            if(self != null && self.sourceMaster) {
                var bodyObj = self.sourceMaster.GetBodyObject();
                if(!bodyObj) return;
                var packFlag = bodyObj.GetComponent<PackBoxHandler>();
                if(packFlag && packFlag.isBoxed) {
                    self.portraitIconImage.texture = secondaryIconResource.texture;
                    self.portraitIconImage.enabled = true;
                }
            }
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

            GameObject indPrefab;

            if(cpt.packedObject) {
                indPrefab = placeIndicatorPrefab;
                bool didPlace = TryGetBoxablePlacePos(self.GetAimRay(), out Vector3 loc, out bool didHitGround);
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
                if(validObjectNames.Contains(slot.currentTarget.rootObject?.name)) {
                    var shopcpt = slot.currentTarget.rootObject.GetComponent<ShopTerminalBehavior>();
                    if(shopcpt && shopcpt.serverMultiShopController)
                        slot.currentTarget.rootObject = shopcpt.serverMultiShopController.transform.root.gameObject;

                    var pbh = slot.currentTarget.rootObject.GetComponent<PackBoxHandler>();
                    if(!pbh)
                        pbh = slot.currentTarget.rootObject.AddComponent<PackBoxHandler>();

                    pbh.TryPack(cpt, null);

                    return false;
                }
            } else {
                var pbh = cpt.packedObject.GetComponent<PackBoxHandler>();
                if(!pbh) {
                    TinkersSatchelPlugin._logger.LogError("PackBoxTracker contains GameObject with no PackBoxHandler");
                    return false;
                }
                if(TryGetBoxablePlacePos(slot.GetAimRay(), out Vector3 placeLoc, out _)) {
                    return pbh.TryPlace(cpt, placeLoc);
                } else return false;
            }

            return false;
        }

        bool TryGetBoxablePlacePos(Ray aim, out Vector3 loc, out bool didHitGround) {
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

        public struct MsgPackboxPack : INetMessage {
            GameObject _target;
            GameObject[] _aux;
            GameObject _owner;

            public void Deserialize(NetworkReader reader) {
                _target = reader.ReadGameObject();
                _owner = reader.ReadGameObject();
                _aux = new GameObject[reader.ReadInt32()];
                for(var i = 0; i < _aux.Length; i++)
                    _aux[i] = reader.ReadGameObject();
            }

            public void Serialize(NetworkWriter writer) {
                writer.Write(_target);
                writer.Write(_owner);
                writer.Write(_aux.Length);
                for(var i = 0; i < _aux.Length; i++)
                    writer.Write(_aux[i]);
            }

            public void OnReceived() {
                if(!NetworkClient.active) {
                    TinkersSatchelPlugin._logger.LogError($"Client-targeted MsgPackboxPack received by server-only game instance");
                    return;
                }
                if(!_target) {
                    TinkersSatchelPlugin._logger.LogError($"Received MsgPackboxPack for null GameObject");
                    return;
                }
                if(!_owner) {
                    TinkersSatchelPlugin._logger.LogError($"Received MsgPackboxPack for null GameObject");
                    return;
                }

                var pbh = _target.GetComponent<PackBoxHandler>();
                if(!pbh) pbh = _target.AddComponent<PackBoxHandler>();

                var pbt = _owner.GetComponent<PackBoxTracker>();
                if(!pbt) pbt = _owner.AddComponent<PackBoxTracker>();

                pbh.TryPack(pbt, _aux);
            }

            public MsgPackboxPack(GameObject target, PackBoxTracker owner, GameObject[] aux) {
                _target = target;
                _owner = owner.gameObject;
                _aux = aux;
            }
        }

        public struct MsgPackboxPlace : INetMessage {
            GameObject _target;
            GameObject _owner;
            Vector3 _pos;

            public void Deserialize(NetworkReader reader) {
                _target = reader.ReadGameObject();
                _owner = reader.ReadGameObject();
                _pos = reader.ReadVector3();
            }

            public void Serialize(NetworkWriter writer) {
                writer.Write(_target);
                writer.Write(_owner);
                writer.Write(_pos);
            }

            public void OnReceived() {
                if(!NetworkClient.active) {
                    TinkersSatchelPlugin._logger.LogError($"Client-targeted MsgPackboxPlace received by server-only game instance");
                    return;
                }
                if(!_target) {
                    TinkersSatchelPlugin._logger.LogError($"Received MsgPackboxUnpack for null GameObject");
                    return;
                }
                if(!_owner) {
                    TinkersSatchelPlugin._logger.LogError($"Received MsgPackboxPack for null GameObject");
                    return;
                }

                var pbh = _target.GetComponent<PackBoxHandler>();
                var pbt = _owner.GetComponent<PackBoxTracker>();

                if(!pbh || !pbt) {
                    TinkersSatchelPlugin._logger.LogError($"MsgPackboxPack has an invalid GameObject (names: {_target.name} {_owner.name})");
                    return;
                }

                pbh.TryPlace(pbt, _pos);
            }

            public MsgPackboxPlace(GameObject target, PackBoxTracker owner, Vector3 pos) {
                _target = target;
                _owner = owner.gameObject;
                _pos = pos;
            }
        }
    }

    public class PackBoxTracker : MonoBehaviour {
        public GameObject packedObject;
        public Transform groundTarget;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        void Awake() {
            groundTarget = new GameObject().transform;
        }
    }

    public class PackBoxHandler : MonoBehaviour {
        public bool isBoxed = false;
        public bool queuedDeactivate = false;
        public Dictionary<GameObject, Vector3> auxiliaryPackedObjects = new Dictionary<GameObject, Vector3>();

        public void CollectAuxiliary(GameObject[] auxOverride) {
            auxiliaryPackedObjects.Clear();

            if(auxOverride != null && auxOverride.Length > 0) {
                foreach(var obj in auxOverride) {
                    if(!obj) continue;
                    auxiliaryPackedObjects.Add(obj, obj.transform.position - transform.position);
                }
            } else {
                var shopcpt = gameObject.GetComponent<MultiShopController>();
                if(shopcpt && shopcpt._terminalGameObjects != null) {
                    foreach(var terminal in shopcpt._terminalGameObjects) {
                        if(!terminal) continue;
                        auxiliaryPackedObjects.Add(terminal, terminal.transform.position - transform.position);
                    }
                }
                var healcpt = gameObject.GetComponent<ShrineHealingBehavior>();
                if(healcpt && healcpt.wardInstance != null) {
                    auxiliaryPackedObjects.Add(healcpt.wardInstance, healcpt.wardInstance.transform.position - transform.position);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        void LateUpdate() {
            if(queuedDeactivate) {
                queuedDeactivate = false;
                isBoxed = true;
                var loc = gameObject.GetComponentInChildren<ModelLocator>();
                if(loc)
                    loc.modelTransform.gameObject.SetActive(false);
                foreach(var obj in auxiliaryPackedObjects) {
                    obj.Key.SetActive(false);
                    loc = obj.Key.GetComponentInChildren<ModelLocator>();
                    if(loc)
                        loc.modelTransform.gameObject.SetActive(false);
                }
                gameObject.SetActive(false);
            }
        }

        public bool TryPlace(PackBoxTracker from, Vector3 pos) {
            if(!from || from.packedObject != gameObject) {
                TinkersSatchelPlugin._logger.LogError("PackBoxHandler.TryPlace called on null PackBoxTracker, or this PackBoxHandler was not contained in it");
                return false;
            }

            PlaceGlobal(from, pos);

            if(NetworkServer.active && !NetworkClient.active)
                new PackBox.MsgPackboxPlace(gameObject, from, pos).Send(R2API.Networking.NetworkDestination.Clients);
            if(NetworkClient.active)
                PlaceClient(pos);

            return true;
        }

        public void PlaceClient(Vector3 pos) {
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeleportOutBoom"), new EffectData {
                origin = pos,
                rotation = transform.rotation
            }, true);
        }

        public void PlaceGlobal(PackBoxTracker from, Vector3 pos) {
            var body = GetComponent<CharacterBody>();
            if(body && body.master)
                transform.position =
                    body.master.CalculateSafeGroundPosition(pos, body)
                    + (body.corePosition - body.footPosition);
            else transform.position = pos;
            gameObject.SetActive(true);
            var singleLoc = gameObject.GetComponentInChildren<ModelLocator>();
            if(singleLoc)
                singleLoc.modelTransform.gameObject.SetActive(true);
            foreach(var aux in auxiliaryPackedObjects) {
                aux.Key.transform.position = pos + aux.Value;
                aux.Key.SetActive(true);
                var locs = aux.Key.gameObject.GetComponentsInChildren<ModelLocator>();
                foreach(var loc in locs) {
                    loc.modelTransform.gameObject.SetActive(true);
                }
            }
            from.packedObject = null;
            isBoxed = false;
        }
        
        public bool TryPack(PackBoxTracker into, GameObject[] auxOverride) {
            if(!into) {
                TinkersSatchelPlugin._logger.LogError("PackBoxHandler.TryPack called on null PackBoxTracker");
                return false;
            }

            PackGlobal(into, auxOverride);
            if(NetworkServer.active && !NetworkClient.active)
                new PackBox.MsgPackboxPack(gameObject, into, auxiliaryPackedObjects.Select(x => x.Key).ToArray()).Send(R2API.Networking.NetworkDestination.Clients);
            if(NetworkClient.active)
                PackClient();

            return true;
        }

        public void PackClient() {
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeleportOutBoom"), new EffectData {
                origin = transform.position,
                rotation = transform.rotation
            }, true);
        }

        public void PackGlobal(PackBoxTracker into, GameObject[] auxOverride) {
            DirectorCore.instance.RemoveAllOccupiedNodes(gameObject);
            into.packedObject = gameObject;
            queuedDeactivate = true;
            CollectAuxiliary(auxOverride);
        }
    }
}