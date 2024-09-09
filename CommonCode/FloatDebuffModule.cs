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
			floatDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texBuffSlow50Icon.png")
				.WaitForCompletion();
			ContentAddition.AddBuffDef(floatDebuff);
		}

		public override void SetupBehavior() {
			base.SetupBehavior();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            healthComponent = GetComponent<HealthComponent>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(!started) return;
            holdStopwatch -= Time.fixedDeltaTime;
            var wobbleParam = (holdStopwatch + wobbleSeed) * Mathf.PI * 0.5f;
            var targetPos = targetHoldPos + new Vector3(Mathf.Cos(wobbleParam), Mathf.Cos(wobbleParam * 2), Mathf.Cos(wobbleParam * 3)) * KleinBottle.instance.pullWobble;
            var velVec = (targetPos - healthComponent.body.transform.position);
            motor.ApplyForceImpulse(new PhysForceInfo {
                force = (velVec.normalized * 8f - motor.velocity) * motor.mass,
                ignoreGroundStick = true,
                disableAirControlUntilCollision = false
            });
            if(holdStopwatch <= 0f) {
                motor.ApplyForceImpulse(new PhysForceInfo {
                    force = new Vector3(0, -25f, 0) * motor.mass,
                    ignoreGroundStick = true,
                    disableAirControlUntilCollision = false
                });
                InflictDamage();
            }
        }

        public void RenewFloat() {
            healthComponent.TakeDamage(deferredDamageInfo);
            holdStopwatch = KleinBottle.instance.pullTime;
            healthComponent.GetComponent<SetStateOnHurt>().SetStun(KleinBottle.instance.pullTime);
        }

        public void InflictFloat() {
            if(!healthComponent.TryGetComponent<SetStateOnHurt>(out var ssoh) || !healthComponent.TryGetComponent<IPhysMotor>(out motor) || !ssoh.canBeStunned) {
                InflictDamage();
            } else {
                targetHoldPos = healthComponent.transform.position + new Vector3(0, KleinBottle.instance.pullHeight, 0);
                wobbleSeed = KleinBottle.instance.rng.nextNormalizedFloat;
                holdStopwatch = KleinBottle.instance.pullTime;
                ssoh.SetStun(KleinBottle.instance.pullTime);
            }
            started = true;
        }

        public void InflictDamage() {
            healthComponent.TakeDamage(deferredDamageInfo);
            Destroy(this);
        }
    }
}