using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using static TILER2.MiscUtil;
using RoR2.Skills;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
	public class CommandoSpecialPlasmaGrenade : T2Module<CommandoSpecialPlasmaGrenade> {

		////// Module Data //////

		public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferForever | AutoConfigFlags.PreventNetMismatch;



		////// Other Fields/Properties //////

		public SkillDef skillDef { get; private set; }
		public GameObject projectilePrefab { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public CommandoSpecialPlasmaGrenade() {
		}

		public override void RefreshPermanentLanguage() {
			permanentGenericLanguageTokens.Add("TKSAT_COMMANDO_SPECIAL_PLASMAGRENADE_NAME", "Plasma Grenade");
			permanentGenericLanguageTokens.Add("TKSAT_COMMANDO_SPECIAL_PLASMAGRENADE_DESCRIPTION", "<style=cIsDamage>Ignite</style>. Throw a sticky grenade with very-close-range homing that explodes for <style=cIsDamage>500% damage</style>. Can hold up to 2. <style=cStack>Watch your aim near low walls.</style>");
			base.RefreshPermanentLanguage();
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			//load custom assets
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/CommandoSpecialPlasmaGrenade.asset");
			projectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/CommandoPlasmaGrenadeProjectile.prefab");
			var ghostPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/CommandoPlasmaGrenadeGhost.prefab");

			//load vanilla assets
			targetSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Commando/CommandoBodySpecialFamily.asset")
				.WaitForCompletion();
			var detParticleMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Wisp/matWispFire.mat")
				.WaitForCompletion();
			var fuseParticleMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/ClayBoss/matClayBossLightshaft.mat")
				.WaitForCompletion();
			var explosionPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LemurianBruiser/OmniExplosionVFXLemurianBruiserFireballImpact.prefab")
				.WaitForCompletion();

			//modify
			var pRen = ghostPrefab.transform.Find("RadialGlow").gameObject.GetComponent<ParticleSystemRenderer>();
			pRen.material = detParticleMaterial;
			var expl = projectilePrefab.GetComponent<ProjectileExplosion>();
			expl.explosionEffect = explosionPrefab;
			var pRen2 = projectilePrefab.transform.Find("FuseVFX").gameObject.GetComponent<ParticleSystemRenderer>();
			pRen2.material = fuseParticleMaterial;

			//R2API catalog reg
			skillDef.activationState = ContentAddition.AddEntityState<Fire>(out bool entStateDidSucceed);

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on CommandoSpecialPlasmaGrenade! Skill will not appear nor function.");
			} else if(!ContentAddition.AddSkillDef(skillDef)) {
				TinkersSatchelPlugin._logger.LogError("SkillDef setup failed on CommandoSpecialPlasmaGrenade! Skill will not appear nor function.");
			} else {
				setupSucceeded = true;
			}

			ContentAddition.AddProjectile(projectilePrefab);
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

		public class Fire : EntityStates.Commando.CommandoWeapon.ThrowGrenade {
            public override void OnEnter() {
				projectilePrefab = CommandoSpecialPlasmaGrenade.instance.projectilePrefab;
				damageCoefficient = 5f;
				force = 700f;
				minSpread = 0;
				maxSpread = 0;
				baseDuration = 0.5f;
				recoilAmplitude = 0;
				attackSoundString = "Play_commando_M2_grenade_throw";
				projectilePitchBonus = -10f;
				baseDelayBeforeFiringProjectile = 0;
				bloom = 0;
                base.OnEnter();
            }
        }
	}

	[RequireComponent(typeof(ProjectileTargetComponent))]
	public class ProjectileGravitateTowardsTarget : MonoBehaviour {
		public float pullForce;

		public bool varyByDistance;
		public float zeroForceRadius;
		public float maxForceRadius;

		private ProjectileTargetComponent targetComponent;
		private Rigidbody rb;

		private void Start() {
			if(!NetworkServer.active) {
				enabled = false;
				return;
			}
			targetComponent = GetComponent<ProjectileTargetComponent>();
			rb = GetComponent<Rigidbody>();
		}

		private void FixedUpdate() {
			if(targetComponent.target) {
				var delta = this.targetComponent.target.transform.position - this.transform.position;
				if(varyByDistance)
					delta = delta.normalized * Remap(Mathf.Clamp(delta.magnitude, zeroForceRadius, maxForceRadius), zeroForceRadius, maxForceRadius, 0, pullForce);
				else
					delta = delta.normalized * pullForce;

				rb.AddForce(delta, ForceMode.Force);
			}
		}
	}
}