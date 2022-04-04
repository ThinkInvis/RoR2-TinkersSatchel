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
			"Projectiles may bounce and home.";
		protected override string GetDescString(string langid = null) =>
			$"All your projectile attacks have a {Pct(bounceChance, 0, 1f)} chance to bounce, <style=cIsDamage>exploding</style> one extra time and <style=cIsUtility>homing</style> towards a random enemy with <style=cIsDamage>{Pct(bounceDamageFrac)} of their original damage</style>. Can happen up to <style=cIsDamage>{baseBounces} times <style=cStack>(+{stackBounces} per stack)</style></style> per projectile.";
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
		public int stackBounces { get; private set; } = 2;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Fraction of original attack damage for bounced projectiles.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float bounceDamageFrac { get; private set; } = 0.5f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Percent chance to proc.",
			AutoConfigFlags.None, 0f, 100f)]
		public float bounceChance { get; private set; } = 15f;



		////// Other Fields/Properties //////
		
		internal PhysicMaterial bouncyPhysmat;
		internal GameObject effectPrefab;



		////// TILER2 Module Setup //////

		public Pinball() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Pinball.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/pinballIcon.png");
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			bouncyPhysmat = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatBouncy.physicMaterial")
				.WaitForCompletion();

			var tspp = new GameObject("TkSatTempSetupPrefabPrefab");
			var tsp = tspp.InstantiateClone("TkSatTempSetupPrefab", false);
			GameObject.Destroy(tspp);
			var efc = tsp.AddComponent<EffectComponent>();
			var dstroy = tsp.AddComponent<DestroyOnTimer>();
			dstroy.duration = 1f;
			tsp.AddComponent<RandomPinballSFXOnEnable>();
			var vfx = tsp.AddComponent<VFXAttributes>();
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;
			vfx.vfxIntensity = VFXAttributes.VFXIntensity.Medium;
			tsp.AddComponent<NetworkIdentity>();
			effectPrefab = tsp.InstantiateClone("TkSatPinballSFXHandler");
			GameObject.Destroy(tsp);
			ContentAddition.AddEffect(effectPrefab);
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

			if(!self.owner) return retv;

			var body = self.owner.GetComponent<CharacterBody>();
			var count = GetCount(body);

			if(count > 0) {
				var maxBounces = baseBounces + (count - 1) * stackBounces;

				var bounceEnd = endPosition
					- self.aimVector * 0.25f; //back ray off slightly for world clearance

				GameObject lastBounceTarget = null;

				var origDamage = self.damage;

				for(var i = 1; i <= maxBounces; i++) {
					if(!Util.CheckRoll(bounceChance, body.master)) return retv;
					var enemies = GatherEnemies(TeamComponent.GetObjectTeam(self.owner), TeamIndex.Neutral)
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
						self.damage = origDamage * bounceDamageFrac;
						self.InitBulletHitFromRaycastHit(ref nhi, bounceEnd, aimVec, ref nextTarget.rayHitInfo);
						self.ProcessHit(ref nhi);

						if(self.tracerEffectPrefab) {
							EffectData effectData = new EffectData {
								origin = nextTarget.obj.transform.position,
								start = bounceEnd
							};
							EffectManager.SpawnEffect(self.tracerEffectPrefab, effectData, true);
						}

						EffectData pinbEffData = new EffectData {
							origin = bounceEnd,
							start = bounceEnd
						};
						EffectManager.SpawnEffect(effectPrefab, pinbEffData, true);

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
			if(!self.owner || self.GetComponent<ProjectileStickOnImpact>()) return;
			var body = self.owner.GetComponent<CharacterBody>();
			var count = GetCount(body);
			var rb = self.GetComponent<Rigidbody>();
			if(count <= 0 || !rb || !Util.CheckRoll(bounceChance, body.master)) return;
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
			if(ps) origSpeed = ps.desiredForwardSpeed;
			else origSpeed = projectile.rigidbody.velocity.magnitude;
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
			//var oldVec = projectile.rigidbody.velocity;
			var newVec = (currTarget.transform.position - transform.position).normalized
				* origSpeed;
			//projectile.rigidbody.velocity = Vector3.RotateTowards(oldVec, newVec, HOMING_TURN_RATE * Time.fixedDeltaTime, float.MaxValue);
			projectile.rigidbody.velocity = newVec;
		}

		public void OnBounce(ProjectileImpactInfo impactInfo) {
			var hit = GetRootWithLocators(impactInfo.collider.gameObject);
			if(lastTarget != null) {
				if(hit == lastTarget)
					return;
			}
			if(currTarget)
				lastTarget = currTarget;
			else currTarget = hit;
			currentBounces++;
			if(pie)
				pie.projectileDamage.damage = origDamagePIE * Pinball.instance.bounceDamageFrac;
			if(psti)
				psti.projectileDamage.damage = origDamagePSTI * Pinball.instance.bounceDamageFrac;

			CharacterMaster ownerMaster = null;
			if(projectile && projectile.owner) {
				var ownerBody = projectile.owner.GetComponent<CharacterBody>();
				if(ownerBody)
					ownerMaster = ownerBody.master;
			}

			if(NetworkServer.active) {
				EffectData pinbEffData = new EffectData {
					origin = gameObject.transform.position,
					start = gameObject.transform.position
				};
				EffectManager.SpawnEffect(Pinball.instance.effectPrefab, pinbEffData, true);
			}

			if(currentBounces >= maxBounces || !Util.CheckRoll(Pinball.instance.bounceChance, ownerMaster)) {
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
					if(pie.impactEffect)
						pie.explosionEffect = pie.impactEffect;
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
			var enemies = GatherEnemies(projectile.teamFilter.teamIndex, TeamIndex.Neutral)
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
					* origSpeed;
			} else {
				lastTarget = null;
				currTarget = null;
			}
			projectile.rigidbody.angularVelocity = UnityEngine.Random.insideUnitSphere * 15f; //sp-- spEEN!!
		}
    }

	public class RandomPinballSFXOnEnable : MonoBehaviour {
		void OnEnable() {
			var soundSpeed = UnityEngine.Random.Range(0f, 100f);
			for(var i = 0; i < 10; i++) //sound is VERY quiet by default
				Util.PlaySound("Play_UI_obj_casinoChest_swap", gameObject, "casinoChest_swapSpeed", soundSpeed);
		}
    }
}