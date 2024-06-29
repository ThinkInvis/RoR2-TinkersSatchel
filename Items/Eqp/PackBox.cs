using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using System.Linq;
using System.Collections.Generic;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class PackBox : Equipment<PackBox> {

        ////// Equipment Data //////

        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 60f;



        ////// Config //////

        [AutoConfigRoOString()]
        [AutoConfig("Which object names are allowed for packing (comma-delimited, leading/trailing whitespace will be ignored). WARNING: May have unintended results on some untested objects!",
            AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever)]
        public string objectNamesConfig { get; private set; } = String.Join(", ", new[] {
            "Turret1Body",
            "Turret1Broken",
            "SquidTurretBody",
            "Drone1Broken",
            "Drone2Broken",
            "GoldChest",
            "CasinoChest",
            "MissileDroneBroken",
            "FlameDroneBroken",
            "MegaDroneBroken",
            "EquipmentDroneBroken",
            "Chest1",
            "Chest2",
            "KeyLockbox",
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
            "CategoryChest2Damage Variant",
            "CategoryChest2Healing Variant",
            "CategoryChest2Utility Variant",
            "Barrel1",
            "Duplicator",
            "DuplicatorLarge",
            "DuplicatorWild",
            "Scrapper",
            "MultiShopTerminal",
            "MultiShopLargeTerminal",
            "MultiShopEquipmentTerminal",
            "FusionCellDestructibleBody",
            "ExplosivePotDestructibleBody",
            "WarbannerWard",
            "LunarChest",
            "LunarShopTerminal", //todo: disallow or kick out of bazaar, achievement
            "ItemDroneBroken",
            "BulwarkDroneBroken"
        });



        ////// Other Fields/Properties //////

        public static HashSet<string> validObjectNames { get; private set; } = new HashSet<string>();
        readonly Sprite secondaryIconResource;
        GameObject packIndicatorPrefab;
        GameObject placeIndicatorPrefab;
        GameObject placeIndicatorBadPrefab;
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public PackBox() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/PackBox.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/packBoxIconOpen.png");
            secondaryIconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/packBoxIconClosed.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/PackBox.prefab");
        }


        public override void SetupModifyEquipmentDef() {
            base.SetupModifyEquipmentDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.01173F, 0.24063F, -0.23785F),
                localAngles = new Vector3(348.217F, 174.1781F, 6.23518F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.0031F, 0.17327F, -0.32618F),
                localAngles = new Vector3(356.7084F, 180.0849F, 3.30317F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00163F, 0.46855F, -0.24893F),
                localAngles = new Vector3(329.3575F, 179.1878F, 180.2223F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "SpineStomach1",
                localPos = new Vector3(0.01083F, 2.70808F, 0.04819F),
                localAngles = new Vector3(273.0301F, 79.39784F, 11.40969F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00255F, 0.20853F, -0.38892F),
                localAngles = new Vector3(358.1821F, 180.0955F, 0.83747F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.23517F, 0.11433F, -0.12078F),
                localAngles = new Vector3(0.69698F, 129.7389F, 0.83228F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.00871F, 0.21141F, -0.19674F),
                localAngles = new Vector3(0.17244F, 181.0088F, 0.16927F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.03863F, 0.17957F, -0.39445F),
                localAngles = new Vector3(348.7206F, 184.4746F, 269.3459F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.01334F, 0.34869F, -0.33658F),
                localAngles = new Vector3(341.0104F, 181.0101F, 355.4333F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.07441F, 0.89015F, -2.46648F),
                localAngles = new Vector3(1.49764F, 182.5504F, 178.3599F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(-0.37323F, 0.55907F, -1.13146F),
                localAngles = new Vector3(336.5591F, 192.258F, 175.5002F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.08664F, -0.19885F, -0.23894F),
                localAngles = new Vector3(354.6045F, 182.65F, 353.025F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00391F, 0.01788F, -0.33649F),
                localAngles = new Vector3(9.35437F, 175.9276F, 348.3685F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            #endregion
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

            if(Compat_ClassicItems.enabled) {
                Compat_ClassicItems.RegisterEmbryoHook(equipmentDef, "TKSAT_PACKBOX_CI_EMBRYO_APPEND", () => "TKSAT.CardboardBox");
            }
        }

        public override void SetupConfig() {
            base.SetupConfig();
            validObjectNames.UnionWith(objectNamesConfig.Split(',')
                .Select(x => x.Trim() + "(Clone)"));
        }

        public override void Install() {
            base.Install();

            On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlot_UpdateTargets;
            On.RoR2.UI.EquipmentIcon.Update += EquipmentIcon_Update; //SetDisplayData hook is bugged and uses System.ValueType instead of DisplayData, can't easily make manual hook because it's not static
            On.RoR2.UI.AllyCardController.UpdateInfo += AllyCardController_UpdateInfo;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.EquipmentSlot.UpdateTargets -= EquipmentSlot_UpdateTargets;
            On.RoR2.UI.EquipmentIcon.Update -= EquipmentIcon_Update;
            On.RoR2.UI.AllyCardController.UpdateInfo -= AllyCardController_UpdateInfo;
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
        }



        ////// Hooks //////

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(!body) return;
            var cpt = body.GetComponent<PackBoxTracker>();
            if(cpt && cpt.packedObject && EquipmentCatalog.GetEquipmentDef(body.inventory.currentEquipmentIndex) != equipmentDef) {
                var pbh = cpt.packedObject.GetComponent<PackBoxHandler>();
                if(!pbh) {
                    TinkersSatchelPlugin._logger.LogError("PackBoxTracker contained a packed object with no PackBoxHandler (during equipment change autodrop)");
                    return;
                }
                pbh.TryPlaceServer(cpt, pbh.transform.position);
            }
        }

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

            //clear vanilla targeting info, in case we're swapping from another equipment
            self.currentTarget = default(EquipmentSlot.UserTargetInfo);
            self.targetIndicator.targetTransform = null;
            
            var cpt = self.characterBody.GetComponent<PackBoxTracker>();
            if(!cpt) cpt = self.characterBody.gameObject.AddComponent<PackBoxTracker>();

            if(cpt.packedObject) {
                bool didPlace = TryGetBoxablePlacePos(self.GetAimRay(), out Vector3 loc, out bool didHitGround);
                if(didPlace || didHitGround) {
                    if(!didPlace) self.targetIndicator.visualizerPrefab = placeIndicatorBadPrefab;
                    else self.targetIndicator.visualizerPrefab = placeIndicatorPrefab;
                    cpt.groundTarget.transform.position = loc;
                    self.targetIndicator.targetTransform = cpt.groundTarget;
                    //todo: on-ground indicator like engi blueprints, will need to track separately?
                }
            } else {
                self.targetIndicator.visualizerPrefab = packIndicatorPrefab;
                var res = FindNearestBoxable(self.gameObject, self.GetAimRay(), 10f, 20f, false);
                self.targetIndicator.targetTransform = res ? res.transform : null;
                cpt.targetObject = res;
            }
            self.targetIndicator.active = self.targetIndicator.targetTransform != null;
        }


        private void EquipmentIcon_Update(On.RoR2.UI.EquipmentIcon.orig_Update orig, RoR2.UI.EquipmentIcon self) {
            orig(self);
            if(self && self.iconImage && self.currentDisplayData.equipmentDef == instance.equipmentDef && self.targetEquipmentSlot && self.targetEquipmentSlot.characterBody) {
                var cpt = self.targetEquipmentSlot.characterBody.GetComponent<PackBoxTracker>();
                if(cpt && cpt.packedObject)
                    self.iconImage.texture = instance.secondaryIconResource.texture;
            }
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            var cpt = slot.characterBody.GetComponent<PackBoxTracker>();
            if(!cpt) cpt = slot.characterBody.gameObject.AddComponent<PackBoxTracker>();

            if(cpt.packedObject == null) {
                if(cpt.targetObject && validObjectNames.Contains(cpt.targetObject.name)) {
                    var shopcpt = slot.currentTarget.rootObject.GetComponent<ShopTerminalBehavior>();
                    if(shopcpt && shopcpt.serverMultiShopController)
                        cpt.targetObject = shopcpt.serverMultiShopController.transform.root.gameObject;

                    var pbh = cpt.targetObject.GetComponent<PackBoxHandler>();
                    if(!pbh)
                        pbh = cpt.targetObject.AddComponent<PackBoxHandler>();

                    pbh.TryPackServer(cpt);

                    return false;
                }
            } else {
                var pbh = cpt.packedObject.GetComponent<PackBoxHandler>();
                if(!pbh) {
                    TinkersSatchelPlugin._logger.LogError("PackBoxTracker contains GameObject with no PackBoxHandler");
                    return false;
                }
                if(TryGetBoxablePlacePos(slot.GetAimRay(), out Vector3 placeLoc, out _)) {
                    var didPlace = pbh.TryPlaceServer(cpt, placeLoc);
                    if(Compat_ClassicItems.enabled) {
                        if(didPlace && Util.CheckRoll(Mathf.Pow(0.5f, Compat_ClassicItems.CheckEmbryoProc(slot, equipmentDef)) * 100f))
                            return false;
                    }
                    return didPlace;
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
                .Where(x => x && x.gameObject)
                .Select(x => MiscUtil.GetRootWithLocators(x.gameObject))
                .Concat( //OverlapSphere doesn't hit Warbanners
                    GameObject.FindObjectsOfType<BuffWard>()
                    .Select(x => x.gameObject)
                    .Where(x => x.name == "WarbannerWard(Clone)" && Vector3.Distance(x.transform.position, aim.origin) < maxDistance + camAdjust)
                    )
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

                pbh.PackGlobal(pbt, _aux);
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
                if(!_target) {
                    TinkersSatchelPlugin._logger.LogError($"Received MsgPackboxPlace for null GameObject");
                    return;
                }
                if(!_owner) {
                    TinkersSatchelPlugin._logger.LogError($"Received MsgPackboxPlace for null GameObject");
                    return;
                }

                var pbh = _target.GetComponent<PackBoxHandler>();
                var pbt = _owner.GetComponent<PackBoxTracker>();

                if(!pbh || !pbt) {
                    TinkersSatchelPlugin._logger.LogError($"MsgPackboxPlace has an invalid GameObject (names: {_target.name} {_owner.name})");
                    return;
                }

                pbh.PlaceGlobal(pbt, _pos);
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
        public GameObject targetObject;
        public Transform groundTarget;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        void Awake() {
            groundTarget = new GameObject().transform;
        }
    }

    public class PackBoxHandler : MonoBehaviour {
        public bool isBoxed = false;
        public bool queuedDeactivate = false;
        public Dictionary<GameObject, Vector3> auxiliaryPackedObjects = new();

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

        public bool TryPlaceServer(PackBoxTracker from, Vector3 pos) {
            if(!NetworkServer.active) {
                TinkersSatchelPlugin._logger.LogError("PackBoxHandler.TryPlaceServer called on client");
                return false;
            }
            if(!from || from.packedObject != gameObject) {
                TinkersSatchelPlugin._logger.LogError("PackBoxHandler.TryPlaceServer called on null PackBoxTracker, or this PackBoxHandler was not contained in it");
                return false;
            }

            new PackBox.MsgPackboxPlace(gameObject, from, pos).Send(R2API.Networking.NetworkDestination.Clients);

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
            if(NetworkClient.active)
                PlaceClient(pos);
        }
        
        public bool TryPackServer(PackBoxTracker into) {
            if(!NetworkServer.active) {
                TinkersSatchelPlugin._logger.LogError("PackBoxHandler.TryPackServer called on client");
                return false;
            }
            if(!into) {
                TinkersSatchelPlugin._logger.LogError("PackBoxHandler.TryPackServer called on null PackBoxTracker");
                return false;
            }

            new PackBox.MsgPackboxPack(gameObject, into, auxiliaryPackedObjects.Select(x => x.Key).ToArray()).Send(R2API.Networking.NetworkDestination.Clients);
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
            if(NetworkClient.active)
                PackClient();
        }
    }
}