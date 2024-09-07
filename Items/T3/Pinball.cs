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
		///
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

		protected override string[] GetDescStringArgs(string langID = null) => new[] {
			(bounceChance/100f).ToString("0%"), bounceDamageFrac.ToString("0%"), baseBounces.ToString("N0"), stackBounces.ToString("N0"), meleeProjectileDamage.ToString("0%")
		};



		////// Config //////

		[AutoConfigRoOIntSlider("{0:N0}", 0, 20)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Number of extra projectile bounces at first stack.",
			AutoConfigFlags.None, 0, int.MaxValue)]
		public int baseBounces { get; private set; } = 3;

		[AutoConfigRoOIntSlider("{0:N0}", 0, 20)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Number of extra projectile bounces per additional stack.",
			AutoConfigFlags.None, 0, int.MaxValue)]
		public int stackBounces { get; private set; } = 2;

		[AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Fraction of original attack damage for bounced projectiles.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float bounceDamageFrac { get; private set; } = 0.5f;

		[AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Percent chance to proc.",
			AutoConfigFlags.None, 0f, 100f)]
		public float bounceChance { get; private set; } = 15f;

		[AutoConfigRoOString()]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Extra projectile names to blacklist (comma-delimited, leading/trailing whitespace will be ignored).",
			AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever)]
		public string blacklistedProjectiles { get; private set; } = "TreebotFlower1, TreebotFlower2, TreebotFlowerSeed";

		[AutoConfigRoOSlider("{0:P0}", 0f, 2f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Proportion of melee attack damage on fired projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float meleeProjectileDamage { get; private set; } = 0.5f;

		[AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Proc coefficient for melee projectiles.", AutoConfigFlags.DeferForever, 0f, 1f)]
		public float procCoefficient { get; private set; } = 0.5f;



		////// Other Fields/Properties //////

		internal PhysicMaterial bouncyPhysmat;
		internal GameObject effectPrefab;
		internal static UnlockableDef unlockable;
		public HashSet<string> projectileNameBlacklist { get; private set; } = new HashSet<string>();
		public GameObject idrPrefab { get; private set; }
		public GameObject projectilePrefab { get; private set; }
		readonly HashSet<System.WeakReference<OverlapAttack>> firedAttacks = new();
		const float GC_INTERVAL = 2f;
		float _gcStopwatch;



		////// TILER2 Module Setup //////

		public Pinball() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Pinball.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/pinballIcon.png");
			idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Pinball.prefab");
		}

		public override void SetupModifyItemDef() {
			base.SetupModifyItemDef();

			CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

			#region ItemDisplayRule Definitions

			/// Survivors ///
			displayRules.Add("Bandit2Body", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(-0.06567F, 0.12138F, -0.20501F),
				localAngles = new Vector3(280.6782F, 297.5041F, 122.7027F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("CaptainBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.00143F, 0.00143F, -0.22823F),
				localAngles = new Vector3(281.4762F, 124.1527F, 312.8369F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("CommandoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.07875F, 0.19793F, -0.14871F),
				localAngles = new Vector3(346.105F, 246.2088F, 198.6338F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("CrocoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.02502F, -0.37262F, 3.06713F),
				localAngles = new Vector3(304.7492F, 11.28959F, 235.7366F),
				localScale = new Vector3(8F, 8F, 8F)
			});
			displayRules.Add("EngiBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(-0.01673F, -0.18265F, 0.35104F),
				localAngles = new Vector3(13.1134F, 274.4481F, 9.74112F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("HuntressBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.15394F, 0.07526F, -0.05401F),
				localAngles = new Vector3(303.8185F, 51.77041F, 340.8576F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("LoaderBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "MechBase",
				localPos = new Vector3(0.12752F, 0.14517F, -0.18434F),
				localAngles = new Vector3(279.9368F, 94.9445F, 337.786F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("MageBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.18761F, 0.06765F, -0.29962F),
				localAngles = new Vector3(273.3469F, 10.00318F, 44.01978F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("MercBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.01144F, 0.07258F, -0.16841F),
				localAngles = new Vector3(353.4411F, 267.4354F, 203.5448F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("ToolbotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(1.58694F, 1.38868F, -1.89818F),
				localAngles = new Vector3(286.5735F, 115.4369F, 302.4759F),
				localScale = new Vector3(10F, 10F, 10F)
			});
			displayRules.Add("TreebotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "PlatformBase",
				localPos = new Vector3(0.12912F, 0.64686F, -0.97308F),
				localAngles = new Vector3(331.9042F, 81.83606F, 9.11342F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("RailgunnerBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Backpack",
				localPos = new Vector3(0.2635F, -0.18186F, 0.20648F),
				localAngles = new Vector3(278.9552F, 56.63836F, 307.4236F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.04817F, -0.09084F, -0.19568F),
				localAngles = new Vector3(322.1917F, 215.9342F, 229.2181F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			#endregion
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			projectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/PinballProjectile.prefab");
			projectilePrefab.GetComponent<ProjectileImpactExplosion>().blastProcCoefficient = procCoefficient;
			ContentAddition.AddProjectile(projectilePrefab);

			bouncyPhysmat = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatBouncy.physicMaterial")
				.WaitForCompletion();

			var tspp = new GameObject("TkSatTempSetupPrefabPrefab");
			var tsp = tspp.InstantiateClone("TkSatTempSetupPrefab", false);
			GameObject.Destroy(tspp);
			tsp.AddComponent<EffectComponent>();
			var dstroy = tsp.AddComponent<DestroyOnTimer>();
			dstroy.duration = 1f;
			tsp.AddComponent<RandomPinballSFXOnEnable>();
			var vfx = tsp.AddComponent<VFXAttributes>();
			vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;
			vfx.vfxIntensity = VFXAttributes.VFXIntensity.Medium;
			tsp.AddComponent<NetworkIdentity>();
			effectPrefab = tsp.InstantiateClone("TkSatPinballSFXHandler", false);
			GameObject.Destroy(tsp);
			ContentAddition.AddEffect(effectPrefab);

			unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
			unlockable.cachedName = $"TkSat_{name}Unlockable";
			unlockable.sortScore = 200;
			unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/pinballIcon.png");
			ContentAddition.AddUnlockableDef(unlockable);
			itemDef.unlockableDef = unlockable;
		}

        public override void SetupConfig() {
            base.SetupConfig();
			projectileNameBlacklist.UnionWith(blacklistedProjectiles.Split(',')
				.Select(x => x.Trim() + "(Clone)"));
		}

        public override void Install() {
			base.Install();
            On.RoR2.Projectile.ProjectileController.Start += ProjectileController_Start;
            On.RoR2.Projectile.ProjectileImpactExplosion.OnProjectileImpact += ProjectileImpactExplosion_OnProjectileImpact;
			On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += ProjectileSingleTargetImpact_OnProjectileImpact;
            On.RoR2.Projectile.ProjectileExplosion.Detonate += ProjectileExplosion_Detonate;
            On.RoR2.BulletAttack.ProcessHitList += BulletAttack_ProcessHitList;
            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;
            On.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
		}

        public override void Uninstall() {
			base.Uninstall();
			On.RoR2.Projectile.ProjectileController.Start -= ProjectileController_Start;
			On.RoR2.Projectile.ProjectileImpactExplosion.OnProjectileImpact -= ProjectileImpactExplosion_OnProjectileImpact;
			On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact -= ProjectileSingleTargetImpact_OnProjectileImpact;
			On.RoR2.Projectile.ProjectileExplosion.Detonate -= ProjectileExplosion_Detonate;
			On.RoR2.BulletAttack.ProcessHitList -= BulletAttack_ProcessHitList;
			On.RoR2.Run.FixedUpdate -= Run_FixedUpdate;
			On.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;
		}



		////// Hooks //////

		private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self) {
			orig(self);
			_gcStopwatch -= Time.fixedDeltaTime;
			if(_gcStopwatch <= 0f) {
				firedAttacks.RemoveWhere(r => !r.TryGetTarget(out _));
				_gcStopwatch = GC_INTERVAL;
			}
		}

		private bool OverlapAttack_Fire(On.RoR2.OverlapAttack.orig_Fire orig, OverlapAttack self, List<HurtBox> hitResults) {
			var retv = orig(self, hitResults);
			if(self.attacker && self.attacker.TryGetComponent<CharacterBody>(out var attackerBody)
				&& !firedAttacks.Any(x => x.TryGetTarget(out var t) && t == self)) {
				var count = GetCount(attackerBody);
				if(count > 0) {
					if(Util.CheckRoll(bounceChance, attackerBody.master)) {
						ProjectileManager.instance.FireProjectile(new FireProjectileInfo {
							crit = attackerBody.RollCrit(),
							damage = self.damage * meleeProjectileDamage,
							damageColorIndex = DamageColorIndex.Item,
							force = 0f,
							owner = self.attacker,
							rotation = Quaternion.Euler(rng.nextNormalizedFloat * 360f, rng.nextNormalizedFloat * 360f, rng.nextNormalizedFloat * 360f),
							position = attackerBody.corePosition,
							procChainMask = self.procChainMask,
							projectilePrefab = projectilePrefab
						});
					}

					firedAttacks.Add(new(self));
				}
			}
			return retv;
		}

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
						var (obj, rayHitInfo) = Pinball.instance.rng.NextElementUniform(enemies.ToArray());
						var aimVec = (obj.transform.position - bounceEnd).normalized;
						var nhi = default(BulletAttack.BulletHit);
						self.damage = origDamage * bounceDamageFrac;
						self.InitBulletHitFromRaycastHit(ref nhi, bounceEnd, aimVec, ref rayHitInfo);
						self.ProcessHit(ref nhi);

						if(self.tracerEffectPrefab) {
							EffectData effectData = new() {
								origin = obj.transform.position,
								start = bounceEnd
							};
							EffectManager.SpawnEffect(self.tracerEffectPrefab, effectData, true);
						}

						EffectData pinbEffData = new() {
							origin = bounceEnd,
							start = bounceEnd
						};
						EffectManager.SpawnEffect(effectPrefab, pinbEffData, true);

						bounceEnd = obj.transform.position;
						lastBounceTarget = obj;
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
			if(!self || !self.owner || self.GetComponent<ProjectileStickOnImpact>() || self.GetComponent<Deployable>() || projectileNameBlacklist.Contains(self.gameObject.name)) return;
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

		bool origDestroyOnWorldPIE;
		bool origDestroyOnEnemyPIE;
		bool origDestroyOnWorldPSTI;
		bool origDestroyOnEnemyPSTI;
		float origDamagePIE;
		float origDamagePSTI;
		float origSpeed;
		readonly List<PhysicMaterial> origPhysmats = new();

		GameObject lastTarget = null;
		GameObject currTarget = null;

		ProjectileController projectile;
		ProjectileImpactExplosion pie = null;
		ProjectileSingleTargetImpact psti = null;
		ProjectileSimple ps = null;
		ProjectileExplosion pe = null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
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
			var newVec = (currTarget.transform.position - transform.position).normalized
				* origSpeed;
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
				EffectData pinbEffData = new() {
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void OnEnable() {
			var soundSpeed = UnityEngine.Random.Range(0f, 100f);
			for(var i = 0; i < 10; i++) //sound is VERY quiet by default
				Util.PlaySound("Play_UI_obj_casinoChest_swap", gameObject, "casinoChest_swapSpeed", soundSpeed);
		}
    }

	[RegisterAchievement("TkSat_Pinball", "TkSat_PinballUnlockable", "", 3u)]
	public class TkSatPinballAchievement : RoR2.Achievements.BaseAchievement {
		public override void OnInstall() {
			base.OnInstall();
			On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
		}

		public override void OnUninstall() {
			base.OnUninstall();
			On.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
		}

		private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self) {
			orig(self);
			if(localUser.cachedMaster != self) return;
			int matches = 0;
			if(self.inventory.GetItemCount(RoR2Content.Items.ExplodeOnDeath) > 0) matches++;
			if(self.inventory.GetItemCount(RoR2Content.Items.IgniteOnKill) > 0) matches++;
			if(self.inventory.GetItemCount(RoR2Content.Items.Dagger) > 0) matches++;
			if(self.inventory.GetItemCount(RoR2Content.Items.Icicle) > 0) matches++;
			if(self.inventory.GetItemCount(RoR2Content.Items.LaserTurbine) > 0) matches++;
			if(self.inventory.GetItemCount(RoR2Content.Items.BleedOnHitAndExplode) > 0) matches++;
			if(matches >= 3)
				Grant();
		}
	}
}