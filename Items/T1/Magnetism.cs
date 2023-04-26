using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class Magnetism : Item<Magnetism> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            rangedAmount.ToString("0%"), meleeAmount.ToString("N1"), critAmount.ToString("P0")
        };



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:P0}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Projectile magnetism angle (deg) per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float rangedAmount { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Melee lunge speed (m/s) per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float meleeAmount { get; private set; } = 2f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Global critical chance increase (percentage 0-100) per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float critAmount { get; private set; } = 2.5f;



        ////// Other Fields/Properties //////



        ////// TILER2 Module Setup //////

        public Magnetism() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Magnetism.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/magnetismIcon.png");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
            On.RoR2.Projectile.ProjectileController.Awake += ProjectileController_Awake;
        }

        public override void Uninstall() {
            base.Uninstall();
            R2API.RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;
            On.RoR2.Projectile.ProjectileController.Awake -= ProjectileController_Awake;
        }



        ////// Hooks //////

        private bool OverlapAttack_Fire(On.RoR2.OverlapAttack.orig_Fire orig, OverlapAttack self, System.Collections.Generic.List<HurtBox> hitResults) {
            var retv = orig(self, hitResults);
            Vector3 averageHitboxCentroid = Vector3.zero;
            foreach(var hb in self.hitBoxGroup.hitBoxes) {
                averageHitboxCentroid += hb.transform.position;
            }
            averageHitboxCentroid /= self.hitBoxGroup.hitBoxes.Length;
            if(self.attacker && self.attacker.TryGetComponent<CharacterBody>(out var ownerBody) && (ownerBody.coreTransform.position - averageHitboxCentroid).sqrMagnitude < 25f && ownerBody.characterMotor && ownerBody.characterDirection) {
                var boostVec = ownerBody.characterDirection.forward;
                boostVec.z = 0f;
                boostVec = boostVec.normalized;
                ownerBody.characterMotor.velocity += boostVec * GetCount(ownerBody) * meleeAmount;
            }
            return retv;
        }

        private void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self) {
            if(self.owner && self.owner.TryGetComponent<CharacterBody>(out var ownerBody)) {
                var count = GetCount(ownerBody);
                if(count > 0) {
                    var bs = new BullseyeSearch {
                        maxAngleFilter = count * rangedAmount,
                        maxDistanceFilter = self.maxDistance,
                        teamMaskFilter = TeamMask.allButNeutral,
                        filterByLoS = true,
                        searchOrigin = self.origin,
                        searchDirection = self.aimVector,
                        sortMode = BullseyeSearch.SortMode.Angle
                    };
                    bs.teamMaskFilter.RemoveTeam(ownerBody.teamComponent.teamIndex);
                    bs.RefreshCandidates();
                    var res = bs.GetResults();
                    if(res.Any()) {
                        var magTarget = res.First();
                        self.aimVector = (magTarget.transform.position - self.origin).normalized;
                    }
                }
            }
            orig(self);
        }

        private void ProjectileController_Awake(On.RoR2.Projectile.ProjectileController.orig_Awake orig, RoR2.Projectile.ProjectileController self) {
            orig(self);
            if(!self.owner || !self.owner.TryGetComponent<CharacterBody>(out var cb) || !self.TryGetComponent<TeamFilter>(out _) || self.TryGetComponent<RoR2.Projectile.MissileController>(out _)) return;
            var count = GetCount(cb);
            if(count == 0) return;

            if(!self.TryGetComponent<RoR2.Projectile.ProjectileTargetComponent>(out _))
                self.gameObject.AddComponent<RoR2.Projectile.ProjectileTargetComponent>();

            if(!self.TryGetComponent<RoR2.Projectile.ProjectileDirectionalTargetFinder>(out var pdtf)) {
                pdtf = self.gameObject.AddComponent<RoR2.Projectile.ProjectileDirectionalTargetFinder>();
                pdtf.lookRange = 500f;
                pdtf.lookCone = 0f;
                pdtf.onlySearchIfNoTarget = false;
                pdtf.allowTargetLoss = true;
                pdtf.testLoS = true;
                pdtf.ignoreAir = false;
            }
            pdtf.lookCone += count * rangedAmount;

            if(!self.TryGetComponent<RoR2.Projectile.ProjectileSteerTowardTarget>(out var pstt)) {
                pstt = self.gameObject.AddComponent<RoR2.Projectile.ProjectileSteerTowardTarget>();
                pstt.rotationSpeed = 90f;
                pstt.yAxisOnly = false;
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args) {
            if(!sender) return;
            args.critAdd += GetCount(sender) * critAmount;
        }
    }
}