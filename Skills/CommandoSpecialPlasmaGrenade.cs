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

		public override void RefreshPermanentLanguage() {
			permanentGenericLanguageTokens["TKSAT_COMMANDO_SPECIAL_PLASMAGRENADE_NAME"] = "Plasma Grenade";
			permanentGenericLanguageTokens["TKSAT_COMMANDO_SPECIAL_PLASMAGRENADE_DESCRIPTION"] = "<style=cIsDamage>Ignite</style>. Throw a sticky grenade with very-close-range homing that explodes for <style=cIsDamage>500% damage</style>. Hold up to 2. <style=cStack>Watch your aim near low walls.</style>";
			permanentGenericLanguageTokens["TKSAT_COMMANDO_SPECIAL_PLASMAGRENADE_NAME_SCEP"] = "Big F【??』ing Grenade";
			permanentGenericLanguageTokens["TKSAT_COMMANDO_SPECIAL_PLASMAGRENADE_DESCRIPTION_SCEP"] = "<style=cIsDamage>Ignite</style>. Throw a sticky grenade with very-close-range homing that explodes for <style=cIsDamage>500% damage</style>. Hold up to 2. <style=cStack>Watch your aim near low walls.</style>\n<color=#d299ff>SCEPTER: Double blast radius and damage.</color>";
			base.RefreshPermanentLanguage();
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			//load custom assets
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/CommandoSpecialPlasmaGrenade.asset");
			scepterSkillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/CIScepter/CommandoSpecialPlasmaGrenadeScep.asset");
			projectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/CommandoPlasmaGrenadeProjectile.prefab");
			var ghostPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/CommandoPlasmaGrenadeGhost.prefab");
			scepterProjectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/CommandoPlasmaGrenadeProjectileScep.prefab");
			var scepterGhostPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/CommandoPlasmaGrenadeGhostScep.prefab");

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

			if(Compat_ClassicItems.enabled) {
				if(!ContentAddition.AddSkillDef(scepterSkillDef)) {
					scepSetupSucceeded = false;
				} else {
					scepSetupSucceeded = Compat_ClassicItems.RegisterScepterSkill(scepterSkillDef, "CommandoBody", SkillSlot.Special, skillDef);
				}
				if(!scepSetupSucceeded)
					TinkersSatchelPlugin._logger.LogError("ClassicItems Scepter support failed for CommandoSpecialPlasmaGrenade! Ancient Scepter will not work on this skill.");
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
			if(!NetworkServer.active) {
				enabled = false;
				return;
			}
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