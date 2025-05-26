﻿using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using EntityStates;
using RoR2.Skills;
using RoR2.Projectile;

namespace ThinkInvisible.TinkersSatchel {
    public class EngiSecondaryChaff : T2Module<EngiSecondaryChaff> {

        ////// Module Data //////

        public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferUntilEndGame | AutoConfigFlags.PreventNetMismatch;



        ////// Other Fields/Properties //////
        
		public SkillDef skillDef { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public EngiSecondaryChaff() {
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

			//load custom assets
			var muzzleEffectPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/EngiChaffFlareEffect.prefab");
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/EngiSecondaryChaff.asset");

			//load vanilla assets
			targetSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Engi/EngiBodySecondaryFamily.asset")
				.WaitForCompletion();
			var tracerMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Firework/matFireworkSparkle.mat")
				.WaitForCompletion();

			//modify
			muzzleEffectPrefab.GetComponent<ParticleSystemRenderer>().material = tracerMtl;
			Fire.effectPrefab = muzzleEffectPrefab;

			//R2API catalog reg
			ContentAddition.AddEffect(muzzleEffectPrefab);

			skillDef.activationState = ContentAddition.AddEntityState<Fire>(out bool entStateDidSucceed);

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on EngiSecondaryChaff! Skill will not appear nor function.");
			} else if(!ContentAddition.AddSkillDef(skillDef)) {
				TinkersSatchelPlugin._logger.LogError("SkillDef setup failed on EngiSecondaryChaff! Skill will not appear nor function.");
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

		public class Fire : BaseSkillState {
			public override void OnEnter() {
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				initialAimRay = GetAimRay();
				StartAimMode(initialAimRay, 2f, false);
			}

			public void FireFX() {
				if(firedCount % 2 == 0)
					PlayCrossfade("Gesture Left Cannon, Additive", "FireGrenadeLeft", 0.1f);
				else
					PlayCrossfade("Gesture Right Cannon, Additive", "FireGrenadeRight", 0.1f);
				if(effectPrefab) {
					EffectManager.SpawnEffect(effectPrefab, new EffectData {
						origin = initialAimRay.origin,
						rotation = Util.QuaternionSafeLookRotation(initialAimRay.direction)
					}, true);
				}
				Util.PlaySound("Play_item_proc_firework_explo", base.gameObject);
			}

			public void CleanseProjectiles(Ray aim) {
				var sqrad = coneRange * coneRange;
				var myTeam = teamComponent.teamIndex;
				var toDelete = new List<ProjectileController>();
				foreach(var projectile in InstanceTracker.GetInstancesList<ProjectileController>()) {
					var del = projectile.transform.position - aim.origin;
					if(!projectile.cannotBeDeleted
						&& projectile.teamFilter.teamIndex != myTeam
						&& del.sqrMagnitude < sqrad
						&& Vector3.Angle(aim.direction, del) < coneHalfAngleDegrees)
						toDelete.Add(projectile);
				}
				for(int i = toDelete.Count - 1; i >= 0; i--)
					GameObject.Destroy(toDelete[i].gameObject);
			}

			public void FireCone(Ray aim) {
				var hits = Physics.OverlapSphere(aim.origin, coneRange, LayerIndex.entityPrecise.mask)
					.Where(x => Vector3.Angle(aim.direction, x.ClosestPoint(aim.origin) - aim.origin) < coneHalfAngleDegrees);
				HashSet<HealthComponent> hitHCs = new();
				foreach(var hit in hits) {
					if(hit && hit.TryGetComponent<HurtBox>(out var hb) && hb.healthComponent && !hitHCs.Contains(hb.healthComponent)) {
						hitHCs.Add(hb.healthComponent);
						if(this.healthComponent == hb.healthComponent || !FriendlyFireManager.ShouldSplashHitProceed(hb.healthComponent, this.teamComponent.teamIndex)) continue;
						var di = new DamageInfo {
							attacker = this.characterBody.gameObject,
							canRejectForce = true,
							crit = this.characterBody.RollCrit(),
							damage = this.characterBody.damage * damageCoeff,
							damageColorIndex = DamageColorIndex.Default,
							damageType = new DamageTypeCombo(DamageType.AOE, DamageTypeExtended.Generic, DamageSource.Secondary),
							force = Vector3.zero,
							inflictor = null,
							procChainMask = default,
							procCoefficient = 0.5f,
							position = hb.transform.position
						};
						hb.healthComponent.TakeDamage(di);
						if(!di.rejected && this.characterBody.master && this.characterBody.master.deployablesList != null && hb.healthComponent.body && hb.healthComponent.body.master) {
							var aic = hb.healthComponent.body.master.aiComponents.FirstOrDefault(x => x.isActiveAndEnabled);
							if(aic != default) {
								var orderedDepls = this.characterBody.master.deployablesList
									.Where(x => x.deployable && x.slot == DeployableSlot.EngiTurret)
									.OrderBy(x => (x.deployable.transform.position - hb.healthComponent.body.aimOrigin).sqrMagnitude);
								foreach(var depl in orderedDepls) {
									var rayfrom = hb.healthComponent.body.aimOrigin;
									var rayto = depl.deployable.transform.position;
									var deplm = depl.deployable.gameObject.GetComponent<CharacterMaster>();
									if(deplm) {
										var deplb = deplm.GetBody();
										if(deplb) {
											if(deplb.mainHurtBox)
												rayto = deplb.mainHurtBox.transform.position;
											var hasNoLoS = Physics.Linecast(rayfrom, rayto, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
											if(!hasNoLoS) {
												TauntDebuffController.ApplyTaunt(aic, deplb, 6f);
											}
										}
									}
								}
							}
                        }
                    }
                }
			}

			public override void OnExit() {
				base.OnExit();
			}

			public override void FixedUpdate() {
				base.FixedUpdate();
				stopwatch -= Time.fixedDeltaTime;
				while(stopwatch <= 0f && firedCount < iterations) {
					firedCount++;
					stopwatch += duration / (float)iterations;
					characterBody.AddSpreadBloom(spreadBloomValue);
					AddRecoil(-1f * recoilAmplitude, -2f * recoilAmplitude, -1f * recoilAmplitude, 1f * recoilAmplitude);
					FireFX();
					CleanseProjectiles(initialAimRay);
					FireCone(initialAimRay);
				}
				if(isAuthority && firedCount >= iterations)
					outer.SetNextStateToMain();
			}

            public override InterruptPriority GetMinimumInterruptPriority() {
				return InterruptPriority.PrioritySkill;
            }

            public static GameObject effectPrefab;
			public static float recoilAmplitude = 0.5f;
			public static float spreadBloomValue = 0.15f;
			public static float baseDuration = 0.6f;
			public static float coneRange = 20f;
			public static float coneHalfAngleDegrees = 60f;
			public static float damageCoeff = 0.75f;
			public static int iterations = 4;

			float duration = 0f;
			float stopwatch = 0f;
			Ray initialAimRay;
			int firedCount = 0;
		}
    }
}