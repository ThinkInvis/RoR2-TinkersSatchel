using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    //todo: aim assist with tracking (may need to build velocity table for turret projectiles?), make drones follow aim
    public class Wrangler : Item<Wrangler> {

        ////// Item Data //////
        
        public override string displayName => "RC Controller";
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });
        public override bool itemIsAIBlacklisted { get; protected set; } = true;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Nearby turrets and drones attack with you... <color=#FF7F7F>BUT no longer attack automatically.</color>";
        protected override string GetDescString(string langid = null) => $"All <style=cIsDamage>turrets and drones</style> under your ownership <style=cIsUtility>within {wrange} meters</style> will <style=cIsUtility>no longer auto-target, auto-attack, or chase enemies</style>. Order drones to fire by holding your <style=cIsUtility>Primary skill</style> keybind. Affected <style=cIsDamage>turrets and drones</style> gain <style=cIsDamage>+{Pct(baseExtraSpeed)} attack speed <style=cStack>(+{Pct(stackExtraSpeed)} per stack)</style></style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Extra fire rate applied at 1 stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseExtraSpeed { get; private set; } = 0.4f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Extra fire rate applied per additional stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float stackExtraSpeed { get; private set; } = 0.4f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Range (m) to search for AI to override.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float wrange { get; private set; } = 150f;



        ////// Other Fields/Properties //////

        private readonly string[] validBodyNames = new[] {
            "Drone1Body(Clone)",
            "BackupDroneBody(Clone)",
            "FlameDroneBody(Clone)",
            "MegaDroneBody(Clone)",
            "MissileDroneBody(Clone)",
            "Turret1Body(Clone)",
            "EngiTurretBody(Clone)",
            "SquidTurretBody(Clone)",
            "RoboBallGreenBuddyBody(Clone)",
            "RoboBallRedBuddyBody(Clone)"
        };
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public Wrangler() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Wrangler.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/wranglerIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Wrangler.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MainWeapon",
                localPos = new Vector3(-0.04371F, 0.46627F, -0.05887F),
                localAngles = new Vector3(20.04837F, 269.312F, 28.16736F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MuzzleGun",
                localPos = new Vector3(0.00204F, 0.06189F, -0.02282F),
                localAngles = new Vector3(8.42397F, 251.936F, 293.6512F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "GunR",
                localPos = new Vector3(-0.1934F, 0.13084F, 0.05881F),
                localAngles = new Vector3(289.4016F, 118.0818F, 4.18462F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "LowerArmL",
                localPos = new Vector3(-0.88421F, 3.87637F, -1.00819F),
                localAngles = new Vector3(26.97832F, 314.7427F, 35.86337F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "CannonHeadR",
                localPos = new Vector3(0.02736F, 0.32489F, 0.26456F),
                localAngles = new Vector3(309.8985F, 241.7234F, 327.4531F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "BowHinge2R",
                localPos = new Vector3(-0.04738F, 0.28482F, -0.06068F),
                localAngles = new Vector3(274.4703F, 36.94548F, 353.5578F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechHandL",
                localPos = new Vector3(-0.12094F, 0.15992F, 0.10351F),
                localAngles = new Vector3(21.41251F, 49.257F, 45.67459F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(-0.01649F, 0.01343F, 0.119F),
                localAngles = new Vector3(21.10289F, 92.18459F, 25.15307F),
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

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();

            On.RoR2.CharacterAI.BaseAI.EvaluateSkillDrivers += BaseAI_EvaluateSkillDrivers;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyAim += BaseAI_UpdateBodyAim;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyInputs += BaseAI_UpdateBodyInputs;
            On.RoR2.GenericSkill.RunRecharge += GenericSkill_RunRecharge;

            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.CharacterAI.BaseAI.EvaluateSkillDrivers -= BaseAI_EvaluateSkillDrivers;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyAim -= BaseAI_UpdateBodyAim;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyInputs -= BaseAI_UpdateBodyInputs;
            On.RoR2.GenericSkill.RunRecharge -= GenericSkill_RunRecharge;

            GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }



        ////// Hooks //////
        #region Hooks
        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            if(!sender) return;
            var cpt = sender.GetComponent<WranglerReceiverComponent>();
            if(cpt && cpt.cachedWranglerCount > 0)
                args.attackSpeedMultAdd += baseExtraSpeed + stackExtraSpeed * (cpt.cachedWranglerCount - 1);
        }

        private void GenericSkill_RunRecharge(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt) {
            if(!self.characterBody) {
                orig(self, dt);
                return;
            }
            var cpt = self.characterBody.GetComponent<WranglerReceiverComponent>();
            if(cpt && cpt.cachedWranglerCount > 0) {
                dt *= baseExtraSpeed + stackExtraSpeed * (cpt.cachedWranglerCount - 1);
            }
            orig(self, dt);
        }

        private void BaseAI_UpdateBodyInputs(On.RoR2.CharacterAI.BaseAI.orig_UpdateBodyInputs orig, RoR2.CharacterAI.BaseAI self) {
            orig(self);
            if(!self.body || self.leader == null) return;
            var cpt = self.body.GetComponent<WranglerReceiverComponent>();
            if(cpt && cpt.isWrangled && self.leader.characterBody) {
                var fireEverything = self.leader.characterBody.inputBank.skill1.down;
                self.bodyInputBank.skill1.PushState(fireEverything);
                self.bodyInputBank.skill2.PushState(fireEverything);
                self.bodyInputBank.skill3.PushState(fireEverything);
                self.bodyInputBank.skill4.PushState(fireEverything);
                self.bodyInputBank.activateEquipment.PushState(fireEverything);
            }
        }

        private RoR2.CharacterAI.BaseAI.SkillDriverEvaluation BaseAI_EvaluateSkillDrivers(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSkillDrivers orig, RoR2.CharacterAI.BaseAI self) {
            var retv = orig(self);

            if(!self.body) return retv;

            var cpt = self.body.GetComponent<WranglerReceiverComponent>();
            if(!cpt) {
                if(validBodyNames.Contains(self.body.name))
                    cpt = self.body.gameObject.AddComponent<WranglerReceiverComponent>();
            }
            if(!cpt) return retv;

            if(self.leader == null || !self.leader.characterBody
                || Vector3.Distance(
                    self.body.corePosition,
                    self.leader.characterBody.corePosition
                    ) > wrange)
                cpt.SetWranglerCount(0);
            else
                cpt.SetWranglerCount(GetCount(self.leader.characterBody));

            if(cpt.isWrangled) {
                //force drones to fly to leader
                float health = 1f;
                if(self.bodyHealthComponent)
                    health = self.bodyHealthComponent.combinedHealthFraction;
                var f = System.Array.Find(self.skillDrivers, x => x.customName == "HardLeashToLeader");
                if(f) {
                    return self.EvaluateSingleSkillDriver(in retv, f, health) ?? retv;
                }
            }
            return retv;
        }

        private void BaseAI_UpdateBodyAim(On.RoR2.CharacterAI.BaseAI.orig_UpdateBodyAim orig, RoR2.CharacterAI.BaseAI self, float deltaTime) {
            if(!self.body || self.leader == null) {
                orig(self, deltaTime);
                return;
            }
            var cpt = self.body.GetComponent<WranglerReceiverComponent>();
            if(cpt && cpt.isWrangled && self.leader.characterBody) {
                var scpt = self.leader.characterBody.GetComponent<WranglerSenderComponent>();
                if(!scpt)
                    scpt = self.leader.gameObject.AddComponent<WranglerSenderComponent>();
                self.bodyInputs.desiredAimDirection = (scpt.cachedAimPosition - self.bodyInputBank.aimOrigin).normalized;
            }
            orig(self, deltaTime);
        }
        #endregion
    }

    [RequireComponent(typeof(CharacterBody))]
    public class WranglerReceiverComponent : MonoBehaviour {
        GameObject laserObj;
        LineRenderer laser;
        CharacterBody body;
        static readonly Color LASER_COLOR = Color.cyan;
        const float SEEK_RANGE = 200f;
        float lasWidth = 0f;

        public int cachedWranglerCount { get; private set; } = 0;
        public bool isWrangled { get; private set; } = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
            laserObj = Instantiate(EntityStates.GolemMonster.ChargeLaser.laserPrefab, transform.position, transform.rotation);
            laserObj.transform.parent = transform;
            laser = laserObj.GetComponent<LineRenderer>();
            laser.startWidth = 0f;
            laser.endWidth = 0f;
            laser.startColor = LASER_COLOR;
            laser.endColor = LASER_COLOR;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Update() {
            laser.startWidth = lasWidth;
            laser.endWidth = lasWidth;
            if(isWrangled) {
                var aim = body.inputBank.GetAimRay();
                var p1 = body.aimOrigin;
                var p2 = aim.GetPoint(SEEK_RANGE);
                if(Physics.Raycast(aim, out RaycastHit raycastHit, SEEK_RANGE, LayerIndex.world.mask | LayerIndex.entityPrecise.mask))
                    p2 = raycastHit.point;
                laser.SetPosition(0, p1);
                laser.SetPosition(1, p2);
            }
        }

        public void SetWranglerCount(int count) {
            if(cachedWranglerCount != count)
                body.MarkAllStatsDirty();
            cachedWranglerCount = count;
            isWrangled = count > 0;
            lasWidth = isWrangled ? 0.05f : 0f;
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class WranglerSenderComponent : MonoBehaviour {
        public Vector3 cachedAimPosition { get; private set; }
        CharacterBody body;
        const float SEEK_RANGE = 500f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(!body) body = GetComponent<CharacterBody>();
            var aim = new Ray(body.inputBank.aimOrigin, body.inputBank.aimDirection);
            if(Util.CharacterRaycast(body.gameObject, aim, out RaycastHit hit, SEEK_RANGE, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore)) {
                cachedAimPosition = hit.point;
            } else cachedAimPosition = aim.GetPoint(SEEK_RANGE);
        }
    }
}