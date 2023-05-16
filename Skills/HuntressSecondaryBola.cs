using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates;
using RoR2.Skills;
using RoR2.Projectile;
using UnityEngine.Networking;
using RoR2.Orbs;
using R2API.Networking.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
	public class HuntressSecondaryBola : T2Module<HuntressSecondaryBola> {

		////// Module Data //////

		public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferUntilEndGame | AutoConfigFlags.PreventNetMismatch;



		////// Config //////



		////// Other Fields/Properties //////

		public SkillDef skillDef { get; private set; }
		public GameObject muzzleFlashPrefab { get; private set; }
		public GameObject orbEffectPrefab { get; private set; }
		public GameObject tetherPrefab { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public HuntressSecondaryBola() {
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			//load custom assets
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/HuntressSecondaryBola.asset");
			tetherPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/BolaTether.prefab");

			//load vanilla assets
			muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/MuzzleflashHuntress.prefab")
				.WaitForCompletion();
			targetSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Huntress/HuntressBodySecondaryFamily.asset")
				.WaitForCompletion();
			orbEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressGlaiveOrbEffect.prefab")
				.WaitForCompletion();

			//R2API catalog reg
			skillDef.activationState = ContentAddition.AddEntityState<ThrowBola>(out bool entStateDidSucceed);

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on HuntressSecondaryBola! Skill will not appear nor function.");
			} else if(!ContentAddition.AddSkillDef(skillDef)) {
				TinkersSatchelPlugin._logger.LogError("SkillDef setup failed on HuntressSecondaryBola! Skill will not appear nor function.");
			} else {
				setupSucceeded = true;
			}
		}

		public override void Install() {
			base.Install();
			if(setupSucceeded) {
				targetSkillFamily.AddVariant(skillDef);
                On.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.FireOrbGlaive += ThrowGlaive_FireOrbGlaive;
			}
		}

        public override void Uninstall() {
			base.Uninstall();
			if(setupSucceeded) {
				targetSkillFamily.RemoveVariant(skillDef);
				On.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.FireOrbGlaive -= ThrowGlaive_FireOrbGlaive;
			}
		}



		////// Hooks //////

		private void ThrowGlaive_FireOrbGlaive(On.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.orig_FireOrbGlaive orig, EntityStates.Huntress.HuntressWeapon.ThrowGlaive self) {
			if(self is ThrowBola) {
				if(!NetworkServer.active || self.hasTriedToThrowGlaive) return;
				self.hasTriedToThrowGlaive = true;
				if(!self.initialOrbTarget) return;
				self.hasSuccessfullyThrownGlaive = true;
				var muzzle = self.childLocator.FindChild("HandR");
				EffectManager.SimpleMuzzleFlash(EntityStates.Huntress.HuntressWeapon.ThrowGlaive.muzzleFlashPrefab, self.gameObject, "HandR", true);
				var orb = new BolaOrb {
					lightningType = LightningOrb.LightningType.HuntressGlaive,
					damageValue = 0f,
					teamIndex = TeamComponent.GetObjectTeam(self.gameObject),
					attacker = self.gameObject,
					bouncesRemaining = 0,
					speed = 60f,
					bouncedObjects = new(),
					range = 100f,
					origin = muzzle.position,
					target = self.initialOrbTarget
				};
				OrbManager.instance.AddOrb(orb);
			} else
				orig(self);
		}



		////// Skill States //////

		public class ThrowBola : EntityStates.Huntress.HuntressWeapon.ThrowGlaive {
        }
	}

	public class BolaOrb : LightningOrb {
		public override void Begin() {
			lightningType = LightningType.Count; //invalid type
			duration = 0.5f;
			var effectData = new EffectData {
				origin = origin,
				genericFloat = duration
			};
			effectData.SetHurtBoxReference(target);
			EffectManager.SpawnEffect(HuntressSecondaryBola.instance.orbEffectPrefab, effectData, true);
		}

        public override void OnArrival() {
            base.OnArrival();
			if(target.healthComponent.body && attacker.TryGetComponent<CharacterBody>(out var attackerBody))
				BolaTetherController.Inflict(target.healthComponent.body, attackerBody);
        }
    }

	[RequireComponent(typeof(BezierCurveLine))]
	public class BolaTetherController : NetworkBehaviour {
		BezierCurveLine bezierCurveLine;
		GameObject targetRoot; //sync
		GameObject primaryTargetRoot; //sync
		CharacterBody attackerBody; //sync
		const float REEL_SPEED = 10f;
		const float TICK_INTERVAL = 0.5f;
		const float ATTACH_TIME = 0.5f;
		const int TICK_COUNT = 8;
		const float RADIUS = 25f;
		const float DAMAGE_PER_TICK = 0.375f;
		float tickStopwatch = 0f;
		int ticksRemaining = TICK_COUNT;
		float fixedAge = 0f;
		float age = 0f;
		bool beginSiphon = false;
		HealthComponent targetHealthComponent;
		HealthComponent primaryTargetHealthComponent;

		public static void Inflict(CharacterBody victim, CharacterBody attacker) {
			if(!NetworkServer.active) return;
			var sourcePos = victim.corePosition;
			var search = new BullseyeSearch {
				searchOrigin = sourcePos,
				maxDistanceFilter = RADIUS,
				teamMaskFilter = TeamMask.GetEnemyTeams(attacker.teamComponent.teamIndex),
				sortMode = BullseyeSearch.SortMode.Distance,
				filterByLoS = true,
				searchDirection = Vector3.up
			};
			search.RefreshCandidates();
			var results = search.GetResults().Select(e => e.healthComponent).Where(h => h);
			foreach(var healthComponent in results) {
				var tetherObj = UnityEngine.Object.Instantiate<GameObject>(HuntressSecondaryBola.instance.tetherPrefab, sourcePos, Quaternion.identity);
				var tetherController = tetherObj.GetComponent<BolaTetherController>();
				new MsgSetTargets(tetherController, victim.gameObject, healthComponent.gameObject, attacker.gameObject).Send(R2API.Networking.NetworkDestination.Server);
				NetworkServer.Spawn(tetherObj);
				healthComponent.body.AddTimedBuff(RoR2Content.Buffs.Slow50, ATTACH_TIME + TICK_INTERVAL * (TICK_COUNT - 1));
			}
		}

		private void Awake() {
			bezierCurveLine = GetComponent<BezierCurveLine>();
		}

		private void DoDamageTick() {
			if(!targetHealthComponent) targetHealthComponent = targetRoot.GetComponent<HealthComponent>();
			if(!primaryTargetHealthComponent) primaryTargetHealthComponent = primaryTargetRoot.GetComponent<HealthComponent>();
			if(primaryTargetRoot) {
				DamageInfo damageInfo = new DamageInfo {
					position = targetRoot.transform.position,
					attacker = attackerBody.gameObject,
					inflictor = null,
					damage = DAMAGE_PER_TICK * attackerBody.damage,
					damageColorIndex = DamageColorIndex.Default,
					damageType = DamageType.Generic,
					crit = attackerBody.RollCrit(),
					force = Vector3.zero,
					procChainMask = default,
					procCoefficient = 0.25f
				};
				targetHealthComponent.TakeDamage(damageInfo);
				if(!targetHealthComponent.alive && NetworkServer.active) {
					targetRoot = null;
					new MsgSetTargets(this, primaryTargetRoot, null, attackerBody.gameObject).Send(R2API.Networking.NetworkDestination.Clients);
				}
			}
		}

		private Vector3 GetTargetRootPosition() {
			if(targetHealthComponent) return targetHealthComponent.body.corePosition;
			else if(targetRoot) return targetRoot.transform.position;
			else return transform.position;
		}

		private Vector3 GetPrimaryTargetRootPosition() {
			if(primaryTargetHealthComponent) return primaryTargetHealthComponent.body.corePosition;
			else if(primaryTargetRoot) return primaryTargetRoot.transform.position;
			else return transform.position;
		}

		private void Update() {
			age += Time.deltaTime;
			var primaryPos = GetPrimaryTargetRootPosition();
			var targetPos = GetTargetRootPosition();
			bezierCurveLine.transform.position = primaryPos;
			bezierCurveLine.endTransform.position = beginSiphon ? targetPos : Vector3.Lerp(primaryPos, targetPos, age / ATTACH_TIME);
		}

		private void FixedUpdate() {
			fixedAge += Time.fixedDeltaTime;
			if(!targetRoot || targetHealthComponent && !targetHealthComponent.alive) {
				if(NetworkServer.active)
					UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if(!beginSiphon && fixedAge >= ATTACH_TIME) {
				beginSiphon = true;
				return;
			}
			if(primaryTargetRoot && Util.HasEffectiveAuthority(targetRoot) && targetRoot != primaryTargetRoot) {
				var deltaP = primaryTargetRoot.transform.position - GetTargetRootPosition();
				var deltaV = deltaP.normalized * REEL_SPEED * Time.fixedDeltaTime;
				if(targetRoot.TryGetComponent<CharacterMotor>(out var targetMotor)) {
					targetMotor.rootMotion += deltaV;
				} else if(targetRoot.TryGetComponent<Rigidbody>(out var targetRigidbody)) {
					targetRigidbody.velocity += deltaV;
				}
			}
			if(NetworkServer.active) {
				tickStopwatch -= Time.fixedDeltaTime;
				if(tickStopwatch <= 0f) {
					tickStopwatch += TICK_INTERVAL;
					DoDamageTick();
					ticksRemaining--;
					if(ticksRemaining == 0)
						UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}

		public struct MsgSetTargets : INetMessage {
			BolaTetherController sender;
			GameObject primaryTargetRoot;
			GameObject targetRoot;
			GameObject attackerRoot;

			public MsgSetTargets(BolaTetherController sender, GameObject primaryTargetRoot, GameObject targetRoot, GameObject attackerRoot) {
				this.sender = sender;
				this.primaryTargetRoot = primaryTargetRoot;
				this.targetRoot = targetRoot;
				this.attackerRoot = attackerRoot;
			}

            public void Deserialize(NetworkReader reader) {
				sender = reader.ReadGameObject().GetComponent<BolaTetherController>();
				primaryTargetRoot = reader.ReadGameObject();
				targetRoot = reader.ReadGameObject();
				attackerRoot = reader.ReadGameObject();
            }

            public void Serialize(NetworkWriter writer) {
				writer.Write(sender.gameObject);
				writer.Write(primaryTargetRoot);
				writer.Write(targetRoot);
				writer.Write(attackerRoot);
			}

			public void OnReceived() {
				sender.primaryTargetRoot = primaryTargetRoot;
				sender.targetRoot = targetRoot;
				sender.attackerBody = attackerRoot.GetComponent<CharacterBody>();
			}
		}
    }
}