using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;
using RoR2.Projectile;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
	public class Pinball : Item<Pinball> {

		////// Item Data //////

		public override string displayName => "Pinball Wizard";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) =>
			"Projectiles may bounce, gaining damage and homing.";
		protected override string GetDescString(string langid = null) =>
			$"All your projectile attacks have a {Pct(homeChance)} chance to bounce <style=cStack>(not affected by luck)</style>, homing towards a random enemy and gaining +{Pct(bounceDamage)} of their original damage. Can happen up to {baseBounces} times (+{stackBounces} per stack).";
		protected override string GetLoreString(string langid = null) =>
			"Ding! Ding! Ding! Ding!";



		////// Config //////

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Number of extra projectile bounces at first stack.",
			AutoConfigFlags.None, 0, int.MaxValue)]
		public int baseBounces { get; private set; } = 3;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Number of extra projectile bounces per additional stack.",
			AutoConfigFlags.None, 0, int.MaxValue)]
		public int stackBounces { get; private set; } = 1;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Additional multiplier to original attack damage per bounce.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float bounceDamage { get; private set; } = 0.25f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Percent chance to proc.",
			AutoConfigFlags.None, 0f, 100f)]
		public float homeChance { get; private set; } = 50f;



		////// Other Fields/Properties //////
		
		internal PhysicMaterial bouncyPhysmat;



		////// TILER2 Module Setup //////

		public Pinball() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Pinball.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/pinballIcon.png");
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			bouncyPhysmat = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatBouncy.physicMaterial")
				.WaitForCompletion();
		}

		//todo: fix commando piercing shot, maybe blacklist rex ult

		public override void Install() {
			base.Install();
            On.RoR2.Projectile.ProjectileController.Start += ProjectileController_Start;
            On.RoR2.Projectile.ProjectileImpactExplosion.OnProjectileImpact += ProjectileImpactExplosion_OnProjectileImpact;
			On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += ProjectileSingleTargetImpact_OnProjectileImpact;
            On.RoR2.Projectile.ProjectileExplosion.Detonate += ProjectileExplosion_Detonate;
            On.RoR2.BulletAttack.ProcessHitList += BulletAttack_ProcessHitList;
		}

        public override void Uninstall() {
			base.Uninstall();
			On.RoR2.Projectile.ProjectileController.Start -= ProjectileController_Start;
			On.RoR2.Projectile.ProjectileImpactExplosion.OnProjectileImpact -= ProjectileImpactExplosion_OnProjectileImpact;
			On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact -= ProjectileSingleTargetImpact_OnProjectileImpact;
			On.RoR2.Projectile.ProjectileExplosion.Detonate -= ProjectileExplosion_Detonate;
			On.RoR2.BulletAttack.ProcessHitList -= BulletAttack_ProcessHitList;
		}



		////// Hooks //////

		private GameObject BulletAttack_ProcessHitList(On.RoR2.BulletAttack.orig_ProcessHitList orig, BulletAttack self, List<BulletAttack.BulletHit> hits, ref Vector3 endPosition, List<GameObject> ignoreList) {
			var retv = orig(self, hits, ref endPosition, ignoreList);

			var count = GetCount(self.owner?.GetComponent<CharacterBody>());

			if(count > 0) {
				var maxBounces = baseBounces + (count - 1) * stackBounces;

				var bounceEnd = endPosition
					- self.aimVector * 0.25f; //back ray off slightly for world clearance

				GameObject lastBounceTarget = null;

				var origDamage = self.damage;

				for(var i = 1; i <= maxBounces; i++) {
					if(!Util.CheckRoll(homeChance)) return retv;
					var enemies = GatherEnemies(TeamComponent.GetObjectTeam(self.owner))
						.Select(x => MiscUtil.GetRootWithLocators(x.gameObject))
						.Select(obj => {
							if(obj == lastBounceTarget) return (null, default);
							var hc = obj.GetComponent<HealthComponent>();
							if(!hc || !hc.alive) return (null, default);
							var dvec = (obj.transform.position - bounceEnd);
							var ddist = dvec.magnitude;
							if(ddist > self.maxDistance) return (null, default);
							var ray = new Ray(bounceEnd, dvec.normalized);
							var worldcastDidHit = Physics.Raycast(ray, ddist, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
							if(worldcastDidHit)
								return (null, default);
							var rayDidHit = Physics.Raycast(ray, out var rayHitInfo, ddist, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore);
							if(!rayDidHit)
								return (null, default);
							return (obj, rayHitInfo);
						}).Where(kvp => kvp.obj != null);

					if(enemies.Count() > 0) {
						var nextTarget = Pinball.instance.rng.NextElementUniform(enemies.ToArray());
						var aimVec = (nextTarget.obj.transform.position - bounceEnd).normalized;
						var nhi = default(BulletAttack.BulletHit);
						self.damage = origDamage * (1f + (float)i * bounceDamage);
						self.InitBulletHitFromRaycastHit(ref nhi, bounceEnd, aimVec, ref nextTarget.rayHitInfo);
						self.ProcessHit(ref nhi);

						if(self.tracerEffectPrefab) {
							EffectData effectData = new EffectData {
								origin = nextTarget.obj.transform.position,
								start = bounceEnd
							};
							//effectData.SetChildLocatorTransformReference(this.weapon, muzzleIndex);
							EffectManager.SpawnEffect(self.tracerEffectPrefab, effectData, true);
						}

						bounceEnd = nextTarget.obj.transform.position;
						lastBounceTarget = nextTarget.obj;
					} else break;
				}

				self.damage = origDamage;
			}

			return retv;
		}

		private void ProjectileExplosion_Detonate(On.RoR2.Projectile.ProjectileExplosion.orig_Detonate orig, ProjectileExplosion self) {
			var ppc = self.gameObject.GetComponent<PinballProjectileController>();
			if(ppc && ppc.isBouncy) {
				return;
			}
			orig(self);
		}

		private void ProjectileController_Start(On.RoR2.Projectile.ProjectileController.orig_Start orig, ProjectileController self) {
			orig(self);
			var count = GetCount(self.owner?.GetComponent<CharacterBody>());
			if(count <= 0 || !Util.CheckRoll(homeChance)) return;
			var ppc = self.gameObject.AddComponent<PinballProjectileController>();
			ppc.maxBounces = baseBounces + stackBounces * (count - 1);
		}

		private void ProjectileImpactExplosion_OnProjectileImpact(On.RoR2.Projectile.ProjectileImpactExplosion.orig_OnProjectileImpact orig, ProjectileImpactExplosion self, ProjectileImpactInfo impactInfo) {
			orig(self, impactInfo);
			var ppc = self.gameObject.GetComponent<PinballProjectileController>();
			if(ppc) {
				ppc.OnBounce(impactInfo);
            }
		}

		private void ProjectileSingleTargetImpact_OnProjectileImpact(On.RoR2.Projectile.ProjectileSingleTargetImpact.orig_OnProjectileImpact orig, ProjectileSingleTargetImpact self, ProjectileImpactInfo impactInfo) {
			orig(self, impactInfo);
			var ppc = self.gameObject.GetComponent<PinballProjectileController>();
			if(ppc) {
				ppc.OnBounce(impactInfo);
			}
		}
	}

	[RequireComponent(typeof(ProjectileController))]
	public class PinballProjectileController : MonoBehaviour {
		public int maxBounces = 0;
		public int currentBounces = 0;
		public bool isBouncy { get; private set; }

		//const float HOMING_TURN_RATE = 360f * Mathf.PI / 180f; //degs/sec

		bool origDestroyOnWorldPIE;
		bool origDestroyOnEnemyPIE;
		bool origDestroyOnWorldPSTI;
		bool origDestroyOnEnemyPSTI;
		float origDamagePIE;
		float origDamagePSTI;
		float origSpeed;
		List<PhysicMaterial> origPhysmats = new List<PhysicMaterial>();

		GameObject lastTarget = null;
		GameObject currTarget = null;

		ProjectileController projectile;
		ProjectileImpactExplosion pie = null;
		ProjectileSingleTargetImpact psti = null;
		ProjectileSimple ps = null;
		ProjectileExplosion pe = null;

		void Awake() {
			isBouncy = true;
			projectile = GetComponent<ProjectileController>();
			pie = GetComponent<ProjectileImpactExplosion>();
			psti = GetComponent<ProjectileSingleTargetImpact>();
			ps = GetComponent<ProjectileSimple>();
			pe = GetComponent<ProjectileExplosion>();
			if(pe == pie) pe = null;
			var colliders = GetComponentsInChildren<Collider>();
			for(var i = 0; i < colliders.Length; i++) {
				origPhysmats.Add(colliders[i].material);
				colliders[i].material = Pinball.instance.bouncyPhysmat;
            }
			if(pie) {
				origDestroyOnWorldPIE = pie.destroyOnWorld;
				origDestroyOnEnemyPIE = pie.destroyOnEnemy;
				pie.destroyOnWorld = false;
				pie.destroyOnEnemy = false;
				origDamagePIE = pie.projectileDamage.damage;
			}
			if(psti) {
				origDestroyOnWorldPSTI = psti.destroyOnWorld;
				origDestroyOnEnemyPSTI = psti.destroyWhenNotAlive;
				psti.destroyOnWorld = false;
				psti.destroyWhenNotAlive = false;
				origDamagePSTI = psti.projectileDamage.damage;
			}
			origSpeed = ps?.desiredForwardSpeed ?? projectile.rigidbody.velocity.magnitude;
		}

		void FixedUpdate() {
			if(isBouncy && (
				(pie && pie.stopwatch > pie.lifetime)
				|| (ps && ps.stopwatch > ps.lifetime))) {
				isBouncy = false;
			}
			if(!currTarget) return;
			var cpt = currTarget.GetComponent<HealthComponent>();
			if(!cpt || !cpt.alive) {
				currTarget = null;
				return;
			}
			//simplistic homing; todo: curveballs, predict velocity
			var damageFac = 1f + currentBounces * Pinball.instance.bounceDamage;
			//var oldVec = projectile.rigidbody.velocity;
			var newVec = (currTarget.transform.position - transform.position).normalized
				* origSpeed * damageFac;
			//projectile.rigidbody.velocity = Vector3.RotateTowards(oldVec, newVec, HOMING_TURN_RATE * Time.fixedDeltaTime, float.MaxValue);
			projectile.rigidbody.velocity = newVec;
		}

		public void OnBounce(ProjectileImpactInfo impactInfo) {
			var hit = GetRootWithLocators(impactInfo.collider.gameObject);
			if(lastTarget != null) {
				if(hit == lastTarget)
					return;
			}
			lastTarget = currTarget ?? hit;
			currentBounces++;
			var damageFac = 1f + currentBounces * Pinball.instance.bounceDamage;
			if(pie)
				pie.projectileDamage.damage = origDamagePIE * damageFac;
			if(psti)
				psti.projectileDamage.damage = origDamagePSTI * damageFac;
			if(currentBounces >= maxBounces || !Util.CheckRoll(Pinball.instance.homeChance)) {
				isBouncy = false;
				currentBounces = maxBounces;
				var colliders = GetComponentsInChildren<Collider>();
				for(var i = 0; i < colliders.Length; i++) {
					colliders[i].material = origPhysmats[i];
				}
				if(pie) {
					pie.destroyOnWorld = origDestroyOnWorldPIE;
					pie.destroyOnEnemy = origDestroyOnEnemyPIE;
				}
				if(psti) {
					psti.destroyOnWorld = origDestroyOnWorldPSTI;
					psti.destroyWhenNotAlive = origDestroyOnEnemyPSTI;
				}
			} else {
				if(psti) {
					psti.alive = true;
				}
				if(pie) {
					pie.hasImpact = false;
					pie.stopwatch = 0f;
					pie.stopwatchAfterImpact = 0f;
					pie.explosionEffect = (pie.impactEffect ?? pie.explosionEffect);
					pie.DetonateServer();
					pie.alive = true;
				}
				if(pe) {
					pe.DetonateServer();
                }
				if(ps) {
					ps.stopwatch = 0f;
				}
			}
			var enemies = GatherEnemies(projectile.teamFilter.teamIndex)
				.Select(x => MiscUtil.GetRootWithLocators(x.gameObject))
				.Where(obj => {
					if(obj == lastTarget) return false;
					var hc = obj.GetComponent<HealthComponent>();
					if(!hc || !hc.alive) return false;
					var dvec = (obj.transform.position - transform.position);
					var ddist = dvec.magnitude;
					var ray = new Ray(transform.position, dvec.normalized);
					if(Physics.Raycast(ray, ddist, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
						return false;
					return true;
				});
			GameObject nextTarget = null;
			if(enemies.Count() > 0)
				nextTarget = Pinball.instance.rng.NextElementUniform(enemies.ToArray());
			if(nextTarget) {
				currTarget = nextTarget;
				projectile.rigidbody.velocity =
					(nextTarget.transform.position - transform.position).normalized
					* origSpeed * damageFac;
			} else {
				lastTarget = null;
				currTarget = null;
			}
			projectile.rigidbody.angularVelocity = UnityEngine.Random.insideUnitSphere * 15f; //sp-- spEEN!!
		}
    }
}