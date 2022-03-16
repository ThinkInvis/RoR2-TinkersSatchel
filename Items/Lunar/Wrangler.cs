using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using R2API;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class Wrangler : Item<Wrangler> {

        ////// Item Data //////
        
        public override string displayName => "RC Controller";
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });
        public override bool itemIsAIBlacklisted { get; protected set; } = true;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Nearby turrets and drones attack with you... <color=#FF7F7F>BUT no longer attack automatically.</color>";
        protected override string GetDescString(string langid = null) => $"All <style=cIsDamage>turrets and drones</style> under your ownership <style=cIsUtility>within {wrange} meters</style> will <style=cIsUtility>no longer auto-target, auto-attack, or chase enemies</style>. Order drones to fire by holding your <style=cIsUtility>Primary skill</style>. Affected <style=cIsDamage>turrets and drones</style> gain <style=cIsDamage>{Pct(baseExtraSpeed + 1)} attack speed <style=cStack>(+{Pct(stackExtraSpeed)} per stack)</style></style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////
        
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra fire rate applied at 1 stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseExtraSpeed { get; private set; } = 1f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra fire rate applied per additional stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float stackExtraSpeed { get; private set; } = 0.25f;

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
            "EngiTurretBody(Clone)"
        };



        ////// TILER2 Module Setup //////

        public Wrangler() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Wrangler.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/wranglerIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();

            On.RoR2.CharacterAI.BaseAI.EvaluateSkillDrivers += BaseAI_EvaluateSkillDrivers;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyAim += BaseAI_UpdateBodyAim;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyInputs += BaseAI_UpdateBodyInputs;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.GenericSkill.RunRecharge += GenericSkill_RunRecharge;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.CharacterAI.BaseAI.EvaluateSkillDrivers -= BaseAI_EvaluateSkillDrivers;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyAim -= BaseAI_UpdateBodyAim;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyInputs -= BaseAI_UpdateBodyInputs;
            On.RoR2.CharacterBody.OnInventoryChanged -= CharacterBody_OnInventoryChanged;
            On.RoR2.GenericSkill.RunRecharge -= GenericSkill_RunRecharge;

            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }



        ////// Hooks //////
        #region Hooks
        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            foreach(var m in CharacterMaster.instancesList) {
                if(IsValidWrangleTarget(m) && m.aiComponents[0].leader.characterBody == self) {
                    m.GetBody().MarkAllStatsDirty();
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            if(IsValidWrangleTarget(sender.master, false)) {
                var count = GetCount(sender.master.aiComponents[0].leader.characterBody);
                var cpt = sender.GetComponent<WranglerReceiverComponent>();
                if(count <= 0) {
                    if(cpt)
                        cpt.HideLaser();
                    return;
                } else {
                    if(!cpt) cpt = sender.gameObject.AddComponent<WranglerReceiverComponent>();
                    cpt.ShowLaser();
                    args.attackSpeedMultAdd += baseExtraSpeed + stackExtraSpeed * (count - 1);
                }
            }
        }

        private void GenericSkill_RunRecharge(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt) {
            if(self.characterBody && IsValidWrangleTarget(self.characterBody.master)) {
                var count = GetCount(self.characterBody.master.aiComponents[0].leader.characterBody);
                if(count > 0) {
                    dt *= baseExtraSpeed + stackExtraSpeed * (count - 1);
                }
            }
            orig(self, dt);
        }

        private void BaseAI_UpdateBodyInputs(On.RoR2.CharacterAI.BaseAI.orig_UpdateBodyInputs orig, RoR2.CharacterAI.BaseAI self) {
            orig(self);
            if(IsValidWrangleTarget(self)) {
                self.bodyInputBank.skill1.PushState(self.leader.characterBody.inputBank.skill1.down);
            }
        }

        private RoR2.CharacterAI.BaseAI.SkillDriverEvaluation BaseAI_EvaluateSkillDrivers(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSkillDrivers orig, RoR2.CharacterAI.BaseAI self) {
            var retv = orig(self);
            if(!IsValidWrangleTarget(self)) return retv;

            var health = self.bodyHealthComponent?.combinedHealthFraction ?? 1f;
            var f = System.Array.Find(self.skillDrivers, x => x.customName == "HardLeashToLeader");
            if(f) {
                return self.EvaluateSingleSkillDriver(in retv, f, health) ?? retv;
            }
            return retv;
        }

        private void BaseAI_UpdateBodyAim(On.RoR2.CharacterAI.BaseAI.orig_UpdateBodyAim orig, RoR2.CharacterAI.BaseAI self, float deltaTime) {
            if(IsValidWrangleTarget(self)) {
                var wcpt = self.leader.characterBody.GetComponent<WranglerSenderComponent>();
                if(!wcpt)
                    wcpt = self.leader.gameObject.AddComponent<WranglerSenderComponent>();
                self.bodyInputs.desiredAimDirection = (wcpt.cachedAimPosition - self.bodyInputBank.aimOrigin).normalized;
            }
            orig(self, deltaTime);
        }
        #endregion



        ////// Non-Public Methods //////

        bool IsValidWrangleTarget(CharacterMaster wrangleTarget, bool includeCount = true) {
            return wrangleTarget && wrangleTarget.hasBody && validBodyNames.Contains(wrangleTarget.GetBody().name)
                && wrangleTarget.aiComponents.Length > 0
                && (!includeCount || GetCount(wrangleTarget.aiComponents[0].leader?.characterBody) > 0)
                && Vector3.Distance(
                    wrangleTarget.GetBody().corePosition,
                    wrangleTarget.aiComponents[0].leader.characterBody.corePosition
                    ) < wrange;
        }

        bool IsValidWrangleTarget(RoR2.CharacterAI.BaseAI wrangleTarget) {
            return GetCount(wrangleTarget.leader?.characterBody) > 0 && validBodyNames.Contains(wrangleTarget.body?.name)
                & Vector3.Distance(
                    wrangleTarget.body.corePosition,
                    wrangleTarget.leader.characterBody.corePosition
                    ) < wrange;
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class WranglerReceiverComponent : MonoBehaviour {
        GameObject laserObj;
        LineRenderer laser;
        CharacterBody body;
        static readonly Color LASER_COLOR = Color.cyan;
        const float SEEK_RANGE = 200f;
        float lasWidth = 0f;

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
            if(lasWidth > 0f) {
                var aim = body.inputBank.GetAimRay();
                var p1 = body.aimOrigin;
                var p2 = aim.GetPoint(SEEK_RANGE);
                if(Physics.Raycast(aim, out RaycastHit raycastHit, SEEK_RANGE, LayerIndex.world.mask | LayerIndex.entityPrecise.mask))
                    p2 = raycastHit.point;
                laser.SetPosition(0, p1);
                laser.SetPosition(1, p2);
            }
        }

        public void ShowLaser() {
            lasWidth = 0.1f;
        }

        public void HideLaser() {
            lasWidth = 0f;
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