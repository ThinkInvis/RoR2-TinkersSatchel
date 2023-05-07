using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using EntityStates;
using RoR2.Projectile;
using RoR2.Skills;

namespace ThinkInvisible.TinkersSatchel {
    public class EngiPrimaryFlak : T2Module<EngiPrimaryFlak> {

        ////// Module Data //////

        public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferUntilEndGame | AutoConfigFlags.PreventNetMismatch;



        ////// Other Fields/Properties //////
        
		public GameObject projectilePrefab { get; private set; }
		public GameObject projectileGhost { get; private set; }
		public GameObject subProjectilePrefab { get; private set; }
		public GameObject subProjectileGhost { get; private set; }
		public SteppedSkillDef skillDef { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public EngiPrimaryFlak() {
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

			//load custom assets
			projectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/EngiFlakProjectile.prefab");
			projectileGhost = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/Ghosts/EngiFlakGhost.prefab");
			subProjectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/EngiFlakSubProjectile.prefab");
			subProjectileGhost = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/Ghosts/EngiFlakSubGhost.prefab");
			skillDef = ScriptableObject.CreateInstance<SteppedSkillDef>();
			skillDef.activationStateMachineName = "Weapon";
			skillDef.skillNameToken = "TKSAT_ENGI_PRIMARY_FLAK_NAME";
			skillDef.skillDescriptionToken = "TKSAT_ENGI_PRIMARY_FLAK_DESCRIPTION";
			skillDef.interruptPriority = InterruptPriority.Any;
			skillDef.icon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/SkillIcons/EngiPrimaryFlak.png");
			skillDef.baseRechargeInterval = 0;
			skillDef.baseMaxStock = 1;
			skillDef.rechargeStock = 1;
			skillDef.requiredStock = 1;
			skillDef.stockToConsume = 1;
			skillDef.resetCooldownTimerOnUse = true;
			skillDef.fullRestockOnAssign = true;
			skillDef.dontAllowPastMaxStocks = false;
			skillDef.beginSkillCooldownOnSkillEnd = true;
			skillDef.cancelSprintingOnActivation = true;
			skillDef.forceSprintDuringState = false;
			skillDef.canceledFromSprinting = false;
			skillDef.isCombatSkill = true;
			skillDef.mustKeyPress = false;
			skillDef.stepCount = 2;
			skillDef.stepGraceDuration = 0.1f;

			//load vanilla assets
			var mainMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Engi/matEngiTurret.mat")
				.WaitForCompletion();
			var tracerMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matTracerBrightTransparent.mat")
				.WaitForCompletion();
			var muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab")
				.WaitForCompletion();
			var explosionEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiGrenadeExplosion.prefab")
				.WaitForCompletion();
			var shrapnelHitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommando.prefab")
				.WaitForCompletion();
			targetSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Engi/EngiBodyPrimaryFamily.asset")
				.WaitForCompletion();

			//modify
			projectilePrefab.GetComponent<ProjectileSimple>().lifetimeExpiredEffect = explosionEffectPrefab;
			projectilePrefab.GetComponent<ProjectileOverlapAttack>().impactEffect = shrapnelHitEffectPrefab;
			subProjectilePrefab.GetComponent<ProjectileOverlapAttack>().impactEffect = shrapnelHitEffectPrefab;

			projectileGhost.GetComponent<MeshRenderer>().material = mainMtl;
			subProjectileGhost.transform.Find("SpikeModel").GetComponent<MeshRenderer>().material = mainMtl;
			subProjectileGhost.transform.Find("Trail").GetComponent<TrailRenderer>().material = tracerMtl;

			FireContinuous.projectilePrefab = projectilePrefab;
			FireContinuous.effectPrefab = muzzleEffectPrefab;

			//R2API catalog reg
			skillDef.activationState = ContentAddition.AddEntityState<FireContinuous>(out bool entStateDidSucceed);
			ContentAddition.AddProjectile(projectilePrefab);
			ContentAddition.AddProjectile(subProjectilePrefab);

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on EngiPrimaryFlak! Skill will not appear nor function.");
			} else if(!ContentAddition.AddSkillDef(skillDef)) {
				TinkersSatchelPlugin._logger.LogError("SkillDef setup failed on EngiPrimaryFlak! Skill will not appear nor function.");
			} else {
				setupSucceeded = true;
            }
		}

        public override void Install() {
            base.Install();
			if(setupSucceeded) {
				targetSkillFamily.AddVariant(skillDef);
			}
        }

        public override void Uninstall() {
            base.Uninstall();
			if(setupSucceeded) {
				targetSkillFamily.RemoveVariant(skillDef);
			}
		}



        ////// Hooks //////


        ////// Skill States //////
        
        public class FireContinuous : BaseSkillState, SteppedSkillDef.IStepSetter {
			private void FireGrenade(string targetMuzzle) {
				Util.PlaySound(EntityStates.Engi.EngiWeapon.FireGrenades.attackSoundString, base.gameObject);
				var aim = GetAimRay();
				var modelTransform = GetModelTransform();
				if(modelTransform && modelTransform.TryGetComponent<ChildLocator>(out var childLoc)) {
					var child = childLoc.FindChild(targetMuzzle);
					if(child)
						aim.origin = child.position;
				}
				AddRecoil(-1f * recoilAmplitude, -2f * recoilAmplitude, -1f * recoilAmplitude, 1f * recoilAmplitude);
				if(effectPrefab)
					EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, targetMuzzle, false);
				if(isAuthority) {
					ProjectileManager.instance.FireProjectile(
						projectilePrefab, aim.origin,
						Util.QuaternionSafeLookRotation(
							Util.ApplySpread(
								aim.direction,
								0f, characterBody.spreadBloomAngle,
								1f, 1f, 0f, 0f)
							),
						gameObject, damageStat, 0f, characterBody.RollCrit());
				}
				characterBody.AddSpreadBloom(spreadBloomValue);
			}

			public override void OnEnter() {
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				StartAimMode(GetAimRay(), 3f, false);
				if(whichMuzzle == 0) {
					FireGrenade("MuzzleLeft");
					PlayCrossfade("Gesture Left Cannon, Additive", "FireGrenadeLeft", 0.1f);
				} else {
					FireGrenade("MuzzleRight");
					PlayCrossfade("Gesture Right Cannon, Additive", "FireGrenadeRight", 0.1f);
				}
				Util.PlaySound(EntityStates.Engi.EngiWeapon.ChargeGrenades.chargeLoopStartSoundString, gameObject);
			}

			public override void OnExit() {
				base.OnExit();
				Util.PlaySound(EntityStates.Engi.EngiWeapon.ChargeGrenades.chargeLoopStopSoundString, gameObject);
			}

			public override void FixedUpdate() {
				base.FixedUpdate();
				if(isAuthority && fixedAge >= duration)
					outer.SetNextStateToMain();
			}
			public override InterruptPriority GetMinimumInterruptPriority() {
				return InterruptPriority.Skill;
			}

            public void SetStep(int i) {
				whichMuzzle = i % 2;
            }

            public static GameObject effectPrefab;
			public static GameObject projectilePrefab;
			public static float recoilAmplitude = 1f;
			public static float spreadBloomValue = 0.3f;
			public static float baseDuration = 0.55f;

			int whichMuzzle = 0;
			float duration = 0f;
		}
    }

	public class EngiFlakShrapnelSpawner : MonoBehaviour {
		public Transform[] childPoints;
		public ProjectileController projc;
		public ProjectileDamage projd;
		public TeamFilter filt;
		public float range;
		public GameObject projectilePrefab;
		const float MAX_TWEAK_ANGLE = 75f;
		public void OnDetonate() {
			var hits = Physics.OverlapSphere(transform.position, range, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore)
				.Where(x => x.TryGetComponent<HurtBox>(out var hb) && hb.healthComponent && hb.healthComponent.TryGetComponent<TeamComponent>(out var tc) && tc.teamIndex != filt.teamIndex);
			foreach(var point in childPoints) {
				FireSmartAimShrapnel(point, hits);
            }
        }
		public void FireSmartAimShrapnel(Transform originalPoint, IEnumerable<Collider> hits) {
			var ray = new Ray(originalPoint.position, originalPoint.forward);
			var filteredHits = hits
				.Select(x => {
					var vectorTo = (x.ClosestPoint(ray.origin) - ray.origin).normalized;
					return (collider: x, direction: vectorTo, angle: Vector3.Angle(vectorTo, ray.direction));
				})
				.Where(x => x.angle < MAX_TWEAK_ANGLE);

			if(filteredHits.Count() > 0)
				ray.direction = filteredHits.OrderBy(x => x.angle).First().direction;

			ProjectileManager.instance.FireProjectile(new FireProjectileInfo {
				damage = projd.damage * 0.25f,
				crit = projd.crit,
				damageColorIndex = DamageColorIndex.Default,
				force = 0,
				owner = projc.owner,
				position = originalPoint.position,
				procChainMask = projc.procChainMask,
				projectilePrefab = EngiPrimaryFlak.instance.subProjectilePrefab,
				rotation = Util.QuaternionSafeLookRotation(ray.direction)
			});
		}
    }
}