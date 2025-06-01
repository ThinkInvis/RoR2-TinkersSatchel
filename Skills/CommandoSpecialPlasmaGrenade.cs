using UnityEngine;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using RoR2.Skills;
using RoR2.Projectile;

namespace ThinkInvisible.TinkersSatchel {
	public class CommandoSpecialPlasmaGrenade : T2Module<CommandoSpecialPlasmaGrenade> {

		////// Module Data //////

		public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferUntilEndGame | AutoConfigFlags.PreventNetMismatch;



		////// Other Fields/Properties //////

		public SkillDef skillDef { get; private set; }
		public SkillDef scepterSkillDef { get; private set; }
		public GameObject projectilePrefab { get; private set; }
		public GameObject scepterProjectilePrefab { get; private set; }
		bool setupSucceeded = false;
		bool scepSetupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public CommandoSpecialPlasmaGrenade() {
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			//load custom assets
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/CommandoSpecialPlasmaGrenade.asset");
			scepterSkillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/CIScepter/CommandoSpecialPlasmaGrenadeScep.asset");
			projectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/CommandoPlasmaGrenadeProjectile.prefab");
			var ghostPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/Ghosts/CommandoPlasmaGrenadeGhost.prefab");
			scepterProjectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/CommandoPlasmaGrenadeProjectileScep.prefab");
			var scepterGhostPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/Ghosts/CommandoPlasmaGrenadeGhostScep.prefab");

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

			var scepterExplosionPrefabTmp = explosionPrefab.InstantiateClone("TkSatTempSetupPrefab", false);

			scepterExplosionPrefabTmp.transform.localScale *= 2f;

			var scepterExplosionPrefab = scepterExplosionPrefabTmp.InstantiateClone("TkSatPlasmaGrenadeScepterExplosion", false);

			pRen = scepterGhostPrefab.transform.Find("RadialGlow").gameObject.GetComponent<ParticleSystemRenderer>();
			pRen.material = detParticleMaterial;
			expl = scepterProjectilePrefab.GetComponent<ProjectileExplosion>();
			expl.explosionEffect = scepterExplosionPrefab;
			pRen2 = scepterProjectilePrefab.transform.Find("FuseVFX").gameObject.GetComponent<ParticleSystemRenderer>();
			pRen2.material = fuseParticleMaterial;

			//R2API catalog reg
			var astate = ContentAddition.AddEntityState<Fire>(out bool entStateDidSucceed);
			skillDef.activationState = astate;
			scepterSkillDef.activationState = astate;

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on CommandoSpecialPlasmaGrenade! Skill will not appear nor function.");
			} else if(!ContentAddition.AddSkillDef(skillDef)) {
				TinkersSatchelPlugin._logger.LogError("SkillDef setup failed on CommandoSpecialPlasmaGrenade! Skill will not appear nor function.");
			} else {
				setupSucceeded = true;
			}

			ContentAddition.AddEffect(scepterExplosionPrefab);
			ContentAddition.AddProjectile(projectilePrefab);
			ContentAddition.AddProjectile(scepterProjectilePrefab);

			if(Compat_AncientScepter.enabled) {
				if(!ContentAddition.AddSkillDef(scepterSkillDef)) {
					scepSetupSucceeded = false;
				} else {
					scepSetupSucceeded = Compat_AncientScepter.RegisterScepterSkill(scepterSkillDef, "CommandoBody", skillDef);
				}
				if(!scepSetupSucceeded)
					TinkersSatchelPlugin._logger.LogError("Ancient Scepter support failed for CommandoSpecialPlasmaGrenade! Ancient Scepter will not work on this skill.");
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

		public class Fire : EntityStates.Commando.CommandoWeapon.ThrowGrenade {
            public override void OnEnter() {
				bool isScepter = skillLocator && skillLocator.FindSkillByDef(CommandoSpecialPlasmaGrenade.instance.scepterSkillDef) != null;
				if(isScepter) {
					projectilePrefab = CommandoSpecialPlasmaGrenade.instance.scepterProjectilePrefab;
					damageCoefficient = 10f;
					force = 1200f;
				} else {
					projectilePrefab = CommandoSpecialPlasmaGrenade.instance.projectilePrefab;
					damageCoefficient = 5f;
					force = 700f;
				}
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
	public class ProjectileSteerTowardsTargetRB : MonoBehaviour {
		public float targetSpeed;
		public float rotationSpeed;
		public float speedSpeed;

		private ProjectileTargetComponent targetComponent;
		private Rigidbody rb;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		private void Start() {
			targetComponent = GetComponent<ProjectileTargetComponent>();
			rb = GetComponent<Rigidbody>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		private void FixedUpdate() {
			if(targetComponent.target)
				rb.velocity = Vector3.RotateTowards(rb.velocity,
					(targetComponent.target.transform.position - transform.position).normalized * targetSpeed,
					rotationSpeed * Mathf.PI / 180f * Time.fixedDeltaTime, speedSpeed * Time.fixedDeltaTime);
		}
	}
}