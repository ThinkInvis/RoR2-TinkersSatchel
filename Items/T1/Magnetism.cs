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
            rangedAmountBase.ToString("N2"), rangedAmountMax.ToString("N2"), rangedAmountLambda.ToString("N0"), meleeAmount.ToString("N1"), critAmount.ToString("N1")
        };



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:N0}", 0f, 5f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Projectile magnetism angle (deg) at first stack.", AutoConfigFlags.PreventNetMismatch, 0f, 180f)]
        public float rangedAmountBase { get; private set; } = 1f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 180f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Projectile magnetism angle (deg) cap.", AutoConfigFlags.PreventNetMismatch, 0f, 180f)]
        public float rangedAmountMax { get; private set; } = 30f;

        [AutoConfigRoOIntSlider("{0:N0}", 1, 200)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Number of item stacks over which projectile magnetism angle approaches its cap by half.", AutoConfigFlags.PreventNetMismatch, 1, int.MaxValue)]
        public int rangedAmountLambda { get; private set; } = 50;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Melee draw-in range (m) per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float meleeAmount { get; private set; } = 2f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 100f)]
        [AutoConfig("Maximum melee draw-in pull speed (m/s).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float meleeForce { get; private set; } = 30f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 10f)]
        [AutoConfig("Maximum melee draw-in pull angular speed (rotations/sec).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float meleeTurnForce { get; private set; } = 4f;

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
            if(!self.hitBoxGroup || !self.attacker || !self.attacker.TryGetComponent<CharacterBody>(out var ownerBody) || !ownerBody.characterMotor || !ownerBody.characterDirection) return retv; //missing important data, abort
            Vector3 averageHitboxCentroid = Vector3.zero;
            foreach(var hb in self.hitBoxGroup.hitBoxes) {
                averageHitboxCentroid += hb.transform.position;
            }
            averageHitboxCentroid /= self.hitBoxGroup.hitBoxes.Length;
            if((ownerBody.coreTransform.position - averageHitboxCentroid).sqrMagnitude >= 25f) return retv; //attack is too far from owner, probably not melee, abort
            var targets = MiscUtil.GatherEnemies(ownerBody.teamComponent.teamIndex, TeamIndex.Neutral);
            var maxRange = meleeAmount * GetCount(ownerBody);
            foreach(var t in targets) {
                if(!t.body || (!t.body.characterMotor && !t.body.rigidbody) || (t.body.healthComponent && !t.body.healthComponent.alive)) continue;
                var towardsVec = averageHitboxCentroid - t.body.corePosition;
                if(towardsVec.magnitude < maxRange) {
                    var falloffFactor = (towardsVec.magnitude / maxRange);
                    var targetSpeed = falloffFactor * meleeForce;
                    Vector3 currentVelocity;
                    if(t.body.characterMotor)
                        currentVelocity = t.body.characterMotor.velocity;
                    else
                        currentVelocity = t.body.rigidbody.velocity;
                    var currentSpeed = currentVelocity.magnitude;
                    var angularAccel = meleeTurnForce * Mathf.PI * 2 * falloffFactor;
                    if(currentSpeed < targetSpeed) {
                        currentVelocity *= targetSpeed / currentSpeed;
                    }
                    var newVelocity = Vector3.RotateTowards(currentVelocity, towardsVec.normalized * targetSpeed, angularAccel * Time.fixedDeltaTime, 0f);
                    if(t.body.characterMotor)
                        t.body.characterMotor.velocity = newVelocity;
                    else
                        t.body.rigidbody.velocity = newVelocity;
                }
            }
            return retv;
        }

        private void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self) {
            if(self.owner && self.owner.TryGetComponent<CharacterBody>(out var ownerBody)) {
                var count = GetCount(ownerBody);
                if(count > 0) {
                    var bs = new BullseyeSearch {
                        maxAngleFilter = CalculateMagnetismAngle(count),
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

            if(!fireProjectileInfo.owner || !fireProjectileInfo.owner.TryGetComponent<CharacterBody>(out var cb)
                || !fireProjectileInfo.projectilePrefab || fireProjectileInfo.projectilePrefab.GetComponent<Deployable>()
                || projectileController.TryGetComponent<RoR2.Projectile.MissileController>(out _)) return;
            var count = GetCount(cb);
            if(count == 0) return;

            var bs = new BullseyeSearch {
                maxAngleFilter = CalculateMagnetismAngle(count),
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



        ////// Private API //////

        float CalculateMagnetismAngle(int stacks) {
            return Mathf.Lerp(rangedAmountBase, rangedAmountMax, 1f - Mathf.Pow(2, -(stacks - 1f) / rangedAmountLambda));
        }
    }

    [RequireComponent(typeof(RoR2.Projectile.ProjectileController))]
    [RequireComponent(typeof(RoR2.Projectile.ProjectileTargetComponent))]
    public class ProjectileForceTowardsTarget : MonoBehaviour {
        RoR2.Projectile.ProjectileController controller;
        RoR2.Projectile.ProjectileTargetComponent target;

        public float minSpeed = 30f;
        public float angularAccel = 360f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            controller = GetComponent<RoR2.Projectile.ProjectileController>();
            target = GetComponent<RoR2.Projectile.ProjectileTargetComponent>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(target.target) {
                var currentSpeed = controller.rigidbody.velocity.magnitude;
                if(currentSpeed < minSpeed) {
                    controller.rigidbody.velocity *= minSpeed / currentSpeed;
                }
                controller.rigidbody.velocity = Vector3.RotateTowards(controller.rigidbody.velocity, (target.target.position - controller.rigidbody.position).normalized * minSpeed, angularAccel * Mathf.PI / 180f, 0f);
            }
        }
    }
}