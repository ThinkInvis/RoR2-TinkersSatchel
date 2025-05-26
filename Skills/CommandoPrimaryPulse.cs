using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates;
using RoR2.Skills;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
	public class CommandoPrimaryPulse : T2Module<CommandoPrimaryPulse> {

		////// Module Data //////

		public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferUntilEndGame | AutoConfigFlags.PreventNetMismatch;



		////// Config //////
		
		[AutoConfigRoOCheckbox()]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("If true, skill will have no recoil and use an alternate mechanic: high-end damage will start at +0 compared to low-end, and ramp up while attacking the same enemy without missing.", AutoConfigFlags.PreventNetMismatch)]
		public bool altModeFocusFire { get; private set; } = false;

		////// Other Fields/Properties //////

		public SkillDef skillDef { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public CommandoPrimaryPulse() {
		}

		public override void RefreshPermanentLanguage() {
			permanentGenericLanguageTokens["TKSAT_COMMANDO_PRIMARY_PULSE_DESCRIPTION"] = Language.GetString(altModeFocusFire ? "TKSAT_COMMANDO_PRIMARY_PULSE_DESCRIPTION_ALT" : "TKSAT_COMMANDO_PRIMARY_PULSE_DESCRIPTION_MAIN");

			foreach(var kvp in Language.languagesByName) {
				if(!permanentSpecificLanguageTokens.ContainsKey(kvp.Key)) permanentSpecificLanguageTokens.Add(kvp.Key, new Dictionary<string, string>());
				var specLang = permanentSpecificLanguageTokens[kvp.Key];
				specLang["TKSAT_COMMANDO_PRIMARY_PULSE_DESCRIPTION"] = kvp.Value.GetLocalizedStringByToken(altModeFocusFire ? "TKSAT_COMMANDO_PRIMARY_PULSE_DESCRIPTION_ALT" : "TKSAT_COMMANDO_PRIMARY_PULSE_DESCRIPTION_MAIN");
			}

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
			private void FireBulletAltMode(string targetMuzzle, int shotIndex) {
				var aim = GetAimRay();
				Util.PlaySound(EntityStates.Commando.CommandoWeapon.FirePistol2.firePistolSoundString, gameObject);
				if(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab)
					EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, targetMuzzle, false);
				if(isAuthority) {
					if(!gameObject.TryGetComponent<CommandoPrimaryPulseAltModeTracker>(out var tracker)) tracker = gameObject.AddComponent<CommandoPrimaryPulseAltModeTracker>();
					float adjustedFinalDamageCoefficient = Mathf.Lerp(initialDamageCoefficient, finalDamageCoefficient, (float)tracker.consecutiveHitCount / 20f);
					new BulletAttack {
						owner = gameObject,
						weapon = gameObject,
						origin = aim.origin,
						aimVector = aim.direction,
						minSpread = 0f,
						maxSpread = characterBody.spreadBloomAngle,
						damage = Mathf.Lerp(initialDamageCoefficient, adjustedFinalDamageCoefficient, (float)shotIndex / ((float)burstCount - 1f)) * damageStat,
						force = force,
						tracerEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab,
						muzzleName = targetMuzzle,
						hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
						isCrit = Util.CheckRoll(critStat, characterBody.master),
						radius = 0.1f,
						smartCollision = true,
						hitCallback = AltModeHitCallback,
                        damageType = DamageTypeCombo.GenericPrimary
                    }.Fire();
				}
			}

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
						smartCollision = true,
						damageType = DamageTypeCombo.GenericPrimary
					}.Fire();
				}
				characterBody.AddSpreadBloom(spreadBloomValue);
			}

			public static bool AltModeHitCallback(BulletAttack atk, ref BulletAttack.BulletHit hitInfo) {
				var retv = BulletAttack.DefaultHitCallbackImplementation(atk, ref hitInfo);
				if(!atk.owner) return false;
				if(!atk.owner.TryGetComponent<CommandoPrimaryPulseAltModeTracker>(out var tracker)) tracker = atk.owner.AddComponent<CommandoPrimaryPulseAltModeTracker>();
				if(hitInfo.hitHurtBox && hitInfo.hitHurtBox.healthComponent) {
					tracker.OnHit(hitInfo.hitHurtBox.healthComponent.gameObject);
                } else {
					tracker.OnHit(null);
                }
				return retv;
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
						if(CommandoPrimaryPulse.instance.altModeFocusFire)
							FireBulletAltMode("MuzzleLeft", shotsFired);
						else
							FireBullet("MuzzleLeft", shotsFired);
					} else {
						PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
						if(CommandoPrimaryPulse.instance.altModeFocusFire)
							FireBulletAltMode("MuzzleRight", shotsFired);
						else
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

	public class CommandoPrimaryPulseAltModeTracker : MonoBehaviour {
		GameObject lastHitTarget = null;
		public int consecutiveHitCount { get; private set; } = 1;

		public void OnHit(GameObject hit) {
			if(lastHitTarget == hit)
				consecutiveHitCount++;
			else
				consecutiveHitCount = 1;
			lastHitTarget = hit;
        }
    }
}