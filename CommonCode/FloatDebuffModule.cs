using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
	public class FloatDebuffModule : T2Module<FloatDebuffModule> {
		public override bool managedEnable => false;

		public static BuffDef floatDebuff;

		public override void SetupAttributes() {
			base.SetupAttributes();

			floatDebuff = ScriptableObject.CreateInstance<BuffDef>();
			floatDebuff.buffColor = Color.white;
			floatDebuff.canStack = false;
			floatDebuff.isDebuff = true;
			floatDebuff.name = "TKSATFloat";
            floatDebuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/floatDebuffIcon.png");
            floatDebuff.ignoreGrowthNectar = true;
            ContentAddition.AddBuffDef(floatDebuff);
		}

		public override void SetupBehavior() {
			base.SetupBehavior();

            On.RoR2.Util.CleanseBody += Util_CleanseBody;
        }

        private void Util_CleanseBody(On.RoR2.Util.orig_CleanseBody orig, CharacterBody characterBody, bool removeDebuffs, bool removeBuffs, bool removeCooldownBuffs, bool removeDots, bool removeStun, bool removeNearbyProjectiles) {
            orig(characterBody, removeDebuffs, removeBuffs, removeCooldownBuffs, removeDots, removeStun, removeNearbyProjectiles);
            if(removeDebuffs && characterBody) {
                if(characterBody.TryGetComponent<FloatDebuffController>(out var fdc))
                    GameObject.Destroy(fdc);
            }
        }

        public static void Inflict(HealthComponent target, DamageInfo damageInfo, FloatDebuffController.FloatDebuffParams debuffParams) {
            var fdc = target.gameObject.GetComponent<FloatDebuffController>();
            if(!fdc) fdc = target.gameObject.AddComponent<FloatDebuffController>();
            fdc.Inflict(damageInfo, debuffParams);
        }
    }

    [RequireComponent(typeof(HealthComponent))]
    public class FloatDebuffController : MonoBehaviour {
        HealthComponent healthComponent;
        IPhysMotor motor;
        bool started = false;
        public Vector3 targetHoldPos;
        public float holdStopwatch;
        public float wobbleSeed;
        public DamageInfo deferredDamageInfo;
        public FloatDebuffParams debuffParams;

        public struct FloatDebuffParams {
            public float duration;
            public float height;
            public float wobbleRadius;
            public float wobbleSpeed;
            public float wobbleForce;
            public float slamForce;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            healthComponent = GetComponent<HealthComponent>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(!started) return;
            holdStopwatch -= Time.fixedDeltaTime;
            var wobbleParam = (holdStopwatch + wobbleSeed) * Mathf.PI * debuffParams.wobbleSpeed;
            var targetPos = targetHoldPos + new Vector3(Mathf.Cos(wobbleParam), Mathf.Cos(wobbleParam * 2), Mathf.Cos(wobbleParam * 3)) * debuffParams.wobbleRadius;
            var velVec = (targetPos - healthComponent.body.transform.position);
            var vStr = Mathf.Min(debuffParams.wobbleForce, velVec.magnitude);
            velVec = velVec.normalized * Mathf.Pow(vStr/debuffParams.wobbleForce, 0.5f) * debuffParams.wobbleForce;
            motor.ApplyForceImpulse(new PhysForceInfo {
                force = (velVec - motor.velocity) * motor.mass,
                ignoreGroundStick = true,
                disableAirControlUntilCollision = false
            });
            if(holdStopwatch <= 0f) {
                motor.ApplyForceImpulse(new PhysForceInfo {
                    force = new Vector3(0, -debuffParams.slamForce, 0) * motor.mass,
                    ignoreGroundStick = true,
                    disableAirControlUntilCollision = false
                });
                healthComponent.TakeDamage(deferredDamageInfo);
                Destroy(this);
            }
        }

        public void Inflict(DamageInfo damageInfo, FloatDebuffParams debuffParams) {
            if(!healthComponent.alive) return;
            if(deferredDamageInfo != null)
                healthComponent.TakeDamage(deferredDamageInfo);
            deferredDamageInfo = damageInfo;
            this.debuffParams = debuffParams;
            if(!healthComponent.TryGetComponent<SetStateOnHurt>(out var ssoh) || !healthComponent.TryGetComponent<IPhysMotor>(out motor) || !ssoh.canBeStunned) {
                healthComponent.TakeDamage(deferredDamageInfo);
                Destroy(this);
            } else {
                if(!started)
                    targetHoldPos = healthComponent.transform.position + new Vector3(0, debuffParams.height, 0);
                wobbleSeed = Time.fixedTime % 1f;
                if(holdStopwatch < debuffParams.duration)
                    holdStopwatch = debuffParams.duration;
                started = true;
            }
        }
    }
}