using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using R2API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using EntityStates;
using RoR2.Projectile;
using RoR2.Skills;

namespace ThinkInvisible.TinkersSatchel {
    public class EngiSecondaryChaff : T2Module<EngiSecondaryChaff> {

        ////// Module Data //////

        public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferForever | AutoConfigFlags.PreventNetMismatch;



        ////// Other Fields/Properties //////
        
		public static SkillDef skillDef { get; private set; }



		////// TILER2 Module Setup //////

		public EngiSecondaryChaff() {
        }

        public override void RefreshPermanentLanguage() {
            permanentGenericLanguageTokens.Add("TKSAT_ENGI_SECONDARY_CHAFF_NAME", "Chaff");
            permanentGenericLanguageTokens.Add("TKSAT_ENGI_SECONDARY_CHAFF_DESCRIPTION", "Deal <style=cIsDamage>200% damage</style> and <style=cIsUtility>clear enemy projectiles</style> in a frontal cone. Any struck enemies within line of sight of any of your turrets will be <style=cIsUtility>Taunt</style>ed by a turret for 4 seconds.");
            base.RefreshPermanentLanguage();
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

			//load custom assets
			var muzzleEffectPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/EngiChaffFlareEffect.prefab");
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/EngiSecondaryChaff.asset");

			//load vanilla assets
			var targetSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Engi/EngiBodySecondaryFamily.asset")
				.WaitForCompletion();
			var tracerMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matTracerBrightTransparent.mat")
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
				targetSkillFamily.AddVariant(skillDef);
            }
		}

        public override void Install() {
            base.Install();
        }

        public override void Uninstall() {
            base.Uninstall();
		}



		////// Hooks //////



		////// Skill States //////

		public class Fire : BaseSkillState {
			public override void OnEnter() {
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				var aim = GetAimRay();
				StartAimMode(aim, 2f, false);
				PlayCrossfade("Gesture Left Cannon, Additive", "FireGrenadeLeft", 0.5f);
				PlayCrossfade("Gesture Left Cannon, Additive", "FireGrenadeRight", 0.5f);
				if(effectPrefab) {
					EffectManager.SimpleMuzzleFlash(effectPrefab, gameObject, "MuzzleLeft", false);
					EffectManager.SimpleMuzzleFlash(effectPrefab, gameObject, "MuzzleRight", false);
				}
				characterBody.AddSpreadBloom(spreadBloomValue);
				AddRecoil(-1f * recoilAmplitude, -2f * recoilAmplitude, -1f * recoilAmplitude, 1f * recoilAmplitude);
				Util.PlaySound(EntityStates.Engi.EngiWeapon.FireGrenades.attackSoundString, base.gameObject);
				FireCone(aim);
			}

			public void FireCone(Ray aim) {
				var hits = Physics.OverlapSphere(aim.origin, coneRange, LayerIndex.entityPrecise.mask)
					.Where(x => Vector3.Angle(aim.direction, x.ClosestPoint(aim.origin) - aim.origin) < coneHalfAngleDegrees
						&& TeamComponent.GetObjectTeam(x.gameObject) != teamComponent.teamIndex);
				foreach(var hit in hits) {
					if(hit.TryGetComponent<HurtBox>(out var hb) && hb.healthComponent) {
						var di = new DamageInfo {
							attacker = this.characterBody.gameObject,
							canRejectForce = true,
							crit = this.characterBody.RollCrit(),
							damage = this.characterBody.damage * damageCoeff,
							damageColorIndex = DamageColorIndex.Default,
							damageType = DamageType.AOE,
							force = Vector3.zero,
							inflictor = null,
							procChainMask = default,
							procCoefficient = 1f,
							position = hb.transform.position
						};
						hb.healthComponent.TakeDamage(di);
						if(!di.rejected && this.characterBody.master && hb.healthComponent.body && hb.healthComponent.body.master) {
							var aic = hb.healthComponent.body.master.aiComponents.FirstOrDefault(x => x.isActiveAndEnabled);
							if(aic != default) {
								var orderedDepls = this.characterBody.master.deployablesList
									.Where(x => x.deployable && x.slot == DeployableSlot.EngiTurret)
									.OrderBy(x => (x.deployable.transform.position - hb.healthComponent.body.aimOrigin).sqrMagnitude);
								foreach(var depl in orderedDepls) {
									var rayfrom = hb.healthComponent.body.aimOrigin;
									var rayto = depl.deployable.transform.position;
									var deplb = depl.deployable.GetComponent<CharacterBody>();
									if(deplb && deplb.mainHurtBox)
										rayto = deplb.mainHurtBox.transform.position;
									var hasNoLoS = Physics.Linecast(rayfrom, rayto, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
									if(!hasNoLoS) {
										TauntDebuffController.ApplyTaunt(aic, this.characterBody, 4f);
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
				if(isAuthority && fixedAge >= duration)
					outer.SetNextStateToMain();
			}

            public override InterruptPriority GetMinimumInterruptPriority() {
				return InterruptPriority.PrioritySkill;
            }

            public static GameObject effectPrefab;
			public static float recoilAmplitude = 1f;
			public static float spreadBloomValue = 0.3f;
			public static float baseDuration = 2f;
			public static float coneRange = 20f;
			public static float coneHalfAngleDegrees = 60f;
			public static float damageCoeff = 2f;

			float duration = 0f;
		}
    }
}