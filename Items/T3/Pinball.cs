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



		////// Other Fields/Properties //////

		internal PhysicMaterial bouncyPhysmat;
		internal GameObject effectPrefab;
		internal static UnlockableDef unlockable;
		public HashSet<string> projectileNameBlacklist { get; private set; } = new HashSet<string>();
		public GameObject idrPrefab { get; private set; }



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
				childName = "Stomach",
				localPos = new Vector3(0.22045F, -0.06626F, 0.11193F),
				localAngles = new Vector3(359.0299F, 357.3219F, 25.2928F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CaptainBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.38728F, 0.00965F, -0.06446F),
				localAngles = new Vector3(31.87035F, 332.9695F, 3.18838F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CommandoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.23353F, -0.00868F, -0.08696F),
				localAngles = new Vector3(27.00084F, 326.5775F, 4.93487F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CrocoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(-0.6739F, -1.47899F, 1.63122F),
				localAngles = new Vector3(354.4511F, 7.12517F, 355.0916F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("EngiBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(0.24835F, 0.13692F, 0.12219F),
				localAngles = new Vector3(19.74273F, 338.7649F, 343.2596F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("HuntressBody", new ItemDisplayRule {
				childName = "Stomach",
				localPos = new Vector3(0.17437F, -0.01902F, 0.11239F),
				localAngles = new Vector3(14.62809F, 338.0782F, 18.2589F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F),
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab
			});
			displayRules.Add("LoaderBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "MechBase",
				localPos = new Vector3(0.28481F, -0.22564F, -0.12889F),
				localAngles = new Vector3(0.98176F, 51.91312F, 23.00177F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("MageBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(0.16876F, -0.10376F, 0.02998F),
				localAngles = new Vector3(357.5521F, 355.006F, 105.9485F),
				localScale = new Vector3(0.25F, 0.25F, 0.25F)
			});
			displayRules.Add("MercBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "ThighR",
				localPos = new Vector3(-0.08794F, 0.03176F, -0.06409F),
				localAngles = new Vector3(350.6662F, 317.2625F, 21.97947F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("ToolbotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(2.33895F, -0.34548F, 0.80107F),
				localAngles = new Vector3(311.4177F, 7.89006F, 354.1869F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("TreebotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "PlatformBase",
				localPos = new Vector3(0.75783F, -0.10773F, 0.00385F),
				localAngles = new Vector3(308.2326F, 10.8672F, 329.0782F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("RailgunnerBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Backpack",
				localPos = new Vector3(0.28636F, -0.3815F, -0.06912F),
				localAngles = new Vector3(352.4358F, 63.85439F, 6.83272F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.17554F, -0.13447F, -0.0436F),
				localAngles = new Vector3(15.08189F, 9.51543F, 15.89409F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			#endregion
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

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
			effectPrefab = tsp.InstantiateClone("TkSatPinballSFXHandler");
			GameObject.Destroy(tsp);
			ContentAddition.AddEffect(effectPrefab);

			var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
			var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
			unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
			unlockable.cachedName = $"TkSat_{name}Unlockable";
			unlockable.sortScore = 200;
			unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/pinballIcon.png");
			ContentAddition.AddUnlockableDef(unlockable);
			LanguageAPI.Add(achiNameToken, "Woe, Explosions Be Upon Ye");
			LanguageAPI.Add(achiDescToken, "Item Set: Damage-on-kill. Have 3 or more (of 6) at once.");
			itemDef.unlockableDef = unlockable;
		}

        public override void SetupConfig() {
            base.SetupConfig();
			projectileNameBlacklist.UnionWith(blacklistedProjectiles.Split(',')
				.Select(x => x.Trim() + "(Clone)"));
		}

        //todo: fix commando piercing shot (does not cause effect)

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
						var (obj, rayHitInfo) = Pinball.instance.rng.NextElementUniform(enemies.ToArray());
						var aimVec = (obj.transform.position - bounceEnd).normalized;
						var nhi = default(BulletAttack.BulletHit);
						self.damage = origDamage * bounceDamageFrac;
						self.InitBulletHitFromRaycastHit(ref nhi, bounceEnd, aimVec, ref rayHitInfo);
						self.ProcessHit(ref nhi);

						if(self.tracerEffectPrefab) {
							EffectData effectData = new EffectData {
								origin = obj.transform.position,
								start = bounceEnd
							};
							EffectManager.SpawnEffect(self.tracerEffectPrefab, effectData, true);
						}

						EffectData pinbEffData = new EffectData {
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

		//const float HOMING_TURN_RATE = 360f * Mathf.PI / 180f; //degs/sec

		bool origDestroyOnWorldPIE;
		bool origDestroyOnEnemyPIE;
		bool origDestroyOnWorldPSTI;
		bool origDestroyOnEnemyPSTI;
		float origDamagePIE;
		float origDamagePSTI;
		float origSpeed;
		readonly List<PhysicMaterial> origPhysmats = new List<PhysicMaterial>();

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void OnEnable() {
			var soundSpeed = UnityEngine.Random.Range(0f, 100f);
			for(var i = 0; i < 10; i++) //sound is VERY quiet by default
				Util.PlaySound("Play_UI_obj_casinoChest_swap", gameObject, "casinoChest_swapSpeed", soundSpeed);
		}
    }

	[RegisterAchievement("TkSat_Pinball", "TkSat_PinballUnlockable", "")]
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