using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates;
using RoR2.Skills;

namespace ThinkInvisible.TinkersSatchel {
	public class CommandoPrimaryPulse : T2Module<CommandoPrimaryPulse> {

		////// Module Data //////

		public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferForever | AutoConfigFlags.PreventNetMismatch;



		////// Other Fields/Properties //////

		public SkillDef skillDef { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public CommandoPrimaryPulse() {
		}

		public override void RefreshPermanentLanguage() {
			permanentGenericLanguageTokens.Add("TKSAT_COMMANDO_PRIMARY_PULSE_NAME", "Pulse");
			permanentGenericLanguageTokens.Add("TKSAT_COMMANDO_PRIMARY_PULSE_DESCRIPTION", "Rapidly shoot an enemy 4 times with high recoil. <style=cIsDamage>Damage</style> per shot ramps from <style=cIsDamage>75% to 150%</style> over the course of the burst.");
			base.RefreshPermanentLanguage();
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			//load custom assets
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/CommandoPrimaryPulse.asset");

			//load vanilla assets
			targetSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Commando/CommandoBodyPrimaryFamily.asset")
				.WaitForCompletion();

			//R2API catalog reg
			skillDef.activationState = ContentAddition.AddEntityState<Fire>(out bool entStateDidSucceed);

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on CommandoPrimaryPulse! Skill will not appear nor function.");
			} else if(!ContentAddition.AddSkillDef(skillDef)) {
				TinkersSatchelPlugin._logger.LogError("SkillDef setup failed on CommandoPrimaryPulse! Skill will not appear nor function.");
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
			private void FireBullet(string targetMuzzle, int shotIndex) {
				var aim = GetAimRay();
				Util.PlaySound(EntityStates.Commando.CommandoWeapon.FirePistol2.firePistolSoundString, gameObject);
				if(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab)
					EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, targetMuzzle, false);
				var adjustedRecoil = recoilAmplitude / attackSpeedStat;
				AddRecoil(-0.7f * adjustedRecoil, -1f * recoilAmplitude, -0.1f * adjustedRecoil, 0.1f * adjustedRecoil);
				if(isAuthority) {
					new BulletAttack {
						owner = gameObject,
						weapon = gameObject,
						origin = aim.origin,
						aimVector = aim.direction,
						minSpread = 0f,
						maxSpread = characterBody.spreadBloomAngle,
						damage = Mathf.Lerp(initialDamageCoefficient, finalDamageCoefficient, (float)shotIndex/((float)burstCount -1f)) * damageStat,
						force = force,
						tracerEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab,
						muzzleName = targetMuzzle,
						hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
						isCrit = Util.CheckRoll(critStat, characterBody.master),
						radius = 0.1f,
						smartCollision = true
					}.Fire();
				}
				characterBody.AddSpreadBloom(spreadBloomValue);
			}

			public override void OnEnter() {
				base.OnEnter();
				duration = (baseDurationPerShot * burstCount + baseEndLagDuration) / attackSpeedStat;
				StartAimMode(GetAimRay(), 3f, false);
				stopwatch = 0;
				shotsFired = 0;
			}

			public override void FixedUpdate() {
				base.FixedUpdate();
				stopwatch -= Time.fixedDeltaTime;
				while(stopwatch <= 0f && shotsFired < burstCount) {
					if(shotsFired % 2 == 0) {
						PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
						FireBullet("MuzzleLeft", shotsFired);
					} else {
						PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
						FireBullet("MuzzleRight", shotsFired);
					}
					shotsFired++;
					stopwatch += baseDurationPerShot / attackSpeedStat;
				}
				if(isAuthority && fixedAge > duration && shotsFired >= burstCount) {
					outer.SetNextStateToMain();
				}
			}

			public override InterruptPriority GetMinimumInterruptPriority() {
				return InterruptPriority.Skill;
			}

			public static float initialDamageCoefficient = 0.75f;
			public static float finalDamageCoefficient = 1.5f;
			public static float force = 0f;
			public static float baseEndLagDuration = 0.32f;
			public static float baseDurationPerShot = 0.1f;
			public static float recoilAmplitude = 4f;
			public static float spreadBloomValue = 0.125f;
			public static int burstCount = 4;
			private float duration;
			private float stopwatch;
			private int shotsFired;
		}
	}
}