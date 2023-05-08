using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates;
using RoR2.Skills;
using RoR2.Projectile;

namespace ThinkInvisible.TinkersSatchel {
	public class HuntressPrimaryBombArrow : T2Module<HuntressPrimaryBombArrow> {

		////// Module Data //////

		public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferUntilEndGame | AutoConfigFlags.PreventNetMismatch;



		////// Config //////



		////// Other Fields/Properties //////

		public SkillDef skillDef { get; private set; }
		public GameObject muzzleFlashPrefab { get; private set; }
		public GameObject projectilePrefab { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public HuntressPrimaryBombArrow() {
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			//load custom assets
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/HuntressPrimaryBombArrow.asset");
			projectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/HuntressBombArrowProjectile.prefab");
			var projectilePrefabGhost = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/Ghosts/HuntressBombArrowGhost.prefab");

			//load vanilla assets
			muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/MuzzleflashHuntress.prefab")
				.WaitForCompletion();
			targetSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Huntress/HuntressBodyPrimaryFamily.asset")
				.WaitForCompletion();
			var tracerMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Firework/matFireworkSparkle.mat")
				.WaitForCompletion();
			var explosionPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab")
				.WaitForCompletion();

			//modify
			projectilePrefabGhost.transform.Find("Sparks").gameObject.GetComponent<ParticleSystemRenderer>().material = tracerMtl;
			projectilePrefab.GetComponent<ProjectileController>().ghostPrefab = projectilePrefabGhost;
			projectilePrefab.GetComponent<ProjectileExplosion>().explosionEffect = explosionPrefab;

			//R2API catalog reg
			skillDef.activationState = ContentAddition.AddEntityState<Fire>(out bool entStateDidSucceed);
			ContentAddition.AddProjectile(projectilePrefab);

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on HuntressPrimaryBombArrow! Skill will not appear nor function.");
			} else if(!ContentAddition.AddSkillDef(skillDef)) {
				TinkersSatchelPlugin._logger.LogError("SkillDef setup failed on HuntressPrimaryBombArrow! Skill will not appear nor function.");
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
			public static float baseDuration = 0.6f;
			public const string ATTACK_SOUND_STRING = "Play_huntress_m1_ready";
			public const string MUZZLE_STRING = "Muzzle";

			float duration;

			private void FireProjectile() {
				EffectManager.SimpleMuzzleFlash(HuntressPrimaryBombArrow.instance.muzzleFlashPrefab, gameObject, MUZZLE_STRING, true);
				var aim = GetAimRay();
				var modelTransform = GetModelTransform();
				if(modelTransform && modelTransform.TryGetComponent<ChildLocator>(out var childLoc)) {
					var child = childLoc.FindChild(MUZZLE_STRING);
					if(child)
						aim.origin = child.position;
				}
				if(isAuthority) {
					ProjectileManager.instance.FireProjectile(
						HuntressPrimaryBombArrow.instance.projectilePrefab, aim.origin,
						Util.QuaternionSafeLookRotation(aim.direction),
						gameObject, damageStat, 0f, characterBody.RollCrit());
				}
			}

			public override void OnEnter() {
				base.OnEnter();
				Util.PlayAttackSpeedSound(ATTACK_SOUND_STRING, gameObject, attackSpeedStat);
				duration = baseDuration / attackSpeedStat;
				if(characterBody)
					StartAimMode(GetAimRay(), duration + 1f, false); //characterBody.SetAimTimer(duration + 1f);

				PlayCrossfade("Gesture, Override", "FireSeekingShot", "FireSeekingShot.playbackRate", duration, duration * 0.2f / attackSpeedStat);
				PlayCrossfade("Gesture, Additive", "FireSeekingShot", "FireSeekingShot.playbackRate", duration, duration * 0.2f / attackSpeedStat);
			}

			public override void OnExit() {
				base.OnExit();
				FireProjectile();
			}

			public override void FixedUpdate() {
				base.FixedUpdate();
				if(fixedAge > duration && isAuthority) {
					outer.SetNextStateToMain();
					return;
				}
			}

			public override InterruptPriority GetMinimumInterruptPriority() {
				return InterruptPriority.Skill;
			}
		}
	}
}