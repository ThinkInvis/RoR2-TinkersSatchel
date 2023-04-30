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
            rangedAmount.ToString("N2"), meleeAmount.ToString("N1"), critAmount.ToString("N1")
        };



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:N0}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Projectile magnetism angle (deg) per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float rangedAmount { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Melee draw-in range (m) per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float meleeAmount { get; private set; } = 4f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 100f)]
        [AutoConfig("Maximum melee draw-in pull speed (m/s).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float meleeForce { get; private set; } = 30f;

        [AutoConfigRoOSlider("{0:N0}%", 0f, 10f)]
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
            On.RoR2.Projectile.ProjectileManager.InitializeProjectile += ProjectileManager_InitializeProjectile;
        }

        public override void Uninstall() {
            base.Uninstall();
            R2API.RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;
            On.RoR2.Projectile.ProjectileManager.InitializeProjectile -= ProjectileManager_InitializeProjectile;
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
                var targets = MiscUtil.GatherEnemies(ownerBody.teamComponent.teamIndex, TeamIndex.Neutral);
                var maxRange = meleeAmount * GetCount(ownerBody);
                foreach(var t in targets) {
                    if(!t.body || !t.body.characterMotor) continue;
                    var towardsVec = t.body.corePosition - averageHitboxCentroid;
                    if(towardsVec.magnitude < maxRange && t.body) {
                        t.body.characterMotor.velocity += meleeForce * towardsVec.normalized * (towardsVec.magnitude / maxRange);
                    }
                }
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

        private void ProjectileManager_InitializeProjectile(On.RoR2.Projectile.ProjectileManager.orig_InitializeProjectile orig, RoR2.Projectile.ProjectileController projectileController, RoR2.Projectile.FireProjectileInfo fireProjectileInfo) {
            orig(projectileController, fireProjectileInfo);

            if(!fireProjectileInfo.owner || !fireProjectileInfo.owner.TryGetComponent<CharacterBody>(out var cb) || projectileController.TryGetComponent<RoR2.Projectile.MissileController>(out _)) return;
            var count = GetCount(cb);
            if(count == 0) return;

            var bs = new BullseyeSearch {
                maxAngleFilter = count * rangedAmount,
                maxDistanceFilter = 500f,
                teamMaskFilter = TeamMask.allButNeutral,
                filterByLoS = true,
                searchOrigin = fireProjectileInfo.position,
                searchDirection = fireProjectileInfo.rotation * Vector3.forward,
                sortMode = BullseyeSearch.SortMode.Angle
            };
            bs.teamMaskFilter.RemoveTeam(cb.teamComponent.teamIndex);
            bs.RefreshCandidates();
            var res = bs.GetResults();
            if(!res.Any()) return;
            var magTarget = res.First();

            if(!projectileController.TryGetComponent<TeamFilter>(out _)) {
                var tf = projectileController.gameObject.AddComponent<TeamFilter>();
                tf.teamIndex = cb.teamComponent.teamIndex;
            }

            if(!projectileController.TryGetComponent<RoR2.Projectile.ProjectileTargetComponent>(out var ptc))
                ptc = projectileController.gameObject.AddComponent<RoR2.Projectile.ProjectileTargetComponent>();
            ptc.target = magTarget.transform;

            projectileController.gameObject.AddComponent<ProjectileForceTowardsTarget>();
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args) {
            if(!sender) return;
            args.critAdd += GetCount(sender) * critAmount;
        }
    }

    [RequireComponent(typeof(RoR2.Projectile.ProjectileController))]
    [RequireComponent(typeof(RoR2.Projectile.ProjectileTargetComponent))]
    public class ProjectileForceTowardsTarget : MonoBehaviour {
        RoR2.Projectile.ProjectileController controller;
        RoR2.Projectile.ProjectileTargetComponent target;

        public float minSpeed = 30f;
        public float angularAccel = 360f;

        void Awake() {
            controller = GetComponent<RoR2.Projectile.ProjectileController>();
            target = GetComponent<RoR2.Projectile.ProjectileTargetComponent>();
        }

        void FixedUpdate() {
            if(target.target) {
                var currentSpeed = controller.rigidbody.velocity.magnitude;
                if(currentSpeed < minSpeed) {
                    controller.rigidbody.velocity *= currentSpeed / minSpeed;
                }
                controller.rigidbody.velocity = Vector3.RotateTowards(controller.rigidbody.velocity, (target.target.position - controller.rigidbody.position).normalized * minSpeed, angularAccel * Mathf.PI / 180f, 0f);
            }
        }
    }
}