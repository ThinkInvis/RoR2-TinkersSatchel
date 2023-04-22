using R2API;
using RoR2;
using RoR2.Skills;
using TILER2;
using UnityEngine;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using System.Collections.Generic;
using RoR2.CharacterAI;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
	public class CommonCode : T2Module<CommonCode> {
        public override bool managedEnable => false;

		public static SkillDef disabledSkillDef;
		public static BuffDef tauntDebuff;

		static GameObject _worldSpaceWeaponDummy = null;
		public static GameObject worldSpaceWeaponDummy {
			get {
				if(!_worldSpaceWeaponDummy) _worldSpaceWeaponDummy = new GameObject("Workaround for an Inconvenient Quirk of BulletAttack");
				return _worldSpaceWeaponDummy;
			}
		}

        public override void SetupAttributes() {
            base.SetupAttributes();

			var captainSD = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/CaptainBody/CaptainSkillUsedUp");

			disabledSkillDef = SkillUtil.CloneSkillDef(captainSD);
			disabledSkillDef.skillNameToken = "TKSAT_DISABLED_SKILL_NAME";
			disabledSkillDef.skillDescriptionToken = "TKSAT_DISABLED_SKILL_DESCRIPTION";
			disabledSkillDef.dontAllowPastMaxStocks = false;
			disabledSkillDef.beginSkillCooldownOnSkillEnd = true;

			ContentAddition.AddSkillDef(disabledSkillDef);

			tauntDebuff = ScriptableObject.CreateInstance<BuffDef>();
			tauntDebuff.buffColor = Color.white;
			tauntDebuff.canStack = false;
			tauntDebuff.isDebuff = true;
			tauntDebuff.name = "TKSATTaunt";
			tauntDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texAttackIcon.png")
				.WaitForCompletion();
			ContentAddition.AddBuffDef(tauntDebuff);

			R2API.Networking.NetworkingAPI.RegisterMessageType<ServerTimedSkillDisable.MsgApply>();
			R2API.Networking.NetworkingAPI.RegisterMessageType<ServerTimedSkillDisable.MsgRemove>();
		}

        public override void SetupBehavior() {
            base.SetupBehavior();

            On.RoR2.Util.CleanseBody += Util_CleanseBody;
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += BaseAI_FindEnemyHurtBox;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.BulletAttack.FireSingle += BulletAttack_FireSingle;
        }

        private void BulletAttack_FireSingle(On.RoR2.BulletAttack.orig_FireSingle orig, BulletAttack self, Vector3 normal, int muzzleIndex) {
			if(self.weapon == worldSpaceWeaponDummy)
				self.weapon = null; //force tracer effect to happen in worldspace. BulletAttack.Fire sets weapon to owner if null, even if you set it to null on purpose >:(
			orig(self, normal, muzzleIndex);
		}

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
			if(self && self.body && damageInfo != null && damageInfo.attacker && damageInfo.attacker.TryGetComponent<CharacterBody>(out var atkb) && atkb.master) {
				bool shouldApplyTauntPenalty = true;
				bool foundAnyActiveTaunts = false;
				foreach(var aic in atkb.master.aiComponents) {
					if(!aic || !aic.isActiveAndEnabled || !aic.TryGetComponent<TauntDebuffController>(out var tdc)) continue;
					if(tdc.isTaunted) {
						foundAnyActiveTaunts = true;
						shouldApplyTauntPenalty &= tdc.ShouldApplyTauntPenalty(self.body);
					}
                }
				shouldApplyTauntPenalty &= foundAnyActiveTaunts;
				if(shouldApplyTauntPenalty)
					damageInfo.damage *= 0.5f;
            }
			orig(self, damageInfo);
        }

        private void Util_CleanseBody(On.RoR2.Util.orig_CleanseBody orig, CharacterBody characterBody, bool removeDebuffs, bool removeBuffs, bool removeCooldownBuffs, bool removeDots, bool removeStun, bool removeNearbyProjectiles) {
			orig(characterBody, removeDebuffs, removeBuffs, removeCooldownBuffs, removeDots, removeStun, removeNearbyProjectiles);
			if(removeDebuffs && characterBody) {
				if(characterBody.TryGetComponent<ServerTimedSkillDisable>(out var stsd))
					stsd.ServerCleanse();
				if(characterBody.master) {
					foreach(var aic in characterBody.master.aiComponents) {
						if(!aic || !aic.isActiveAndEnabled || !aic.TryGetComponent<TauntDebuffController>(out var tdc)) continue;
						tdc.Cleanse();
					}
				}
            }
		}

		private HurtBox BaseAI_FindEnemyHurtBox(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS) {
			var retv = orig(self, maxDistance, full360Vision, filterByLoS);
			if(self && self.TryGetComponent<TauntDebuffController>(out var tdc) && tdc.isTaunted) {
				var taunters = tdc.GetTaunters();
				var priority = self.enemySearch.GetResults().Where(x => x && x.healthComponent && taunters.Contains(x.healthComponent.body)).FirstOrDefault();
				if(priority == default(HurtBox))
					return retv;
				else return priority;
			}
			return retv;
		}

		internal static void RetrieveDefaultMaterials(ItemDisplay disp) {
			for(var i = 0; i < disp.rendererInfos.Length; i++) {
				var ri = disp.rendererInfos[i];
				ri.defaultMaterial = ri.renderer.material;
				disp.rendererInfos[i] = ri;
			}
		}

		internal static GameObject FindNearestInteractable(GameObject senderObj, HashSet<string> validObjectNames, Ray aim, float maxAngle, float maxDistance, bool requireLoS) {
			aim = CameraRigController.ModifyAimRayIfApplicable(aim, senderObj, out float camAdjust);
			var results = Physics.OverlapSphere(aim.origin, maxDistance + camAdjust, Physics.AllLayers, QueryTriggerInteraction.Collide);
			var minDot = Mathf.Cos(Mathf.Clamp(maxAngle, 0f, 180f) * Mathf.PI / 180f);
			GameObject retv = null;
			var lowestC = float.MaxValue;
			foreach(var obj in results) {
				if(!obj || !obj.gameObject) continue;
				var root = MiscUtil.GetRootWithLocators(obj.gameObject);
				if(!validObjectNames.Contains(root.name.Replace("(Clone)", ""))) continue;
				var vdot = Vector3.Dot(aim.direction, (root.transform.position - aim.origin).normalized);
				if(vdot < minDot) continue;
				if(requireLoS && !Physics.Linecast(aim.origin, root.transform.position, LayerIndex.world.mask))
					continue;
				var c = vdot * Vector3.Distance(root.transform.position, aim.origin);
				if(c < lowestC) {
					lowestC = c;
					retv = root;
				}
			}
			return retv;
		}
	}

	[RequireComponent(typeof(BaseAI))]
	public class TauntDebuffController : MonoBehaviour {
		BaseAI ai;
		readonly Dictionary<CharacterBody, float> tauntTimers = new();
		public bool isTaunted => tauntTimers.Count > 0;
		public HashSet<CharacterBody> GetTaunters() { return new HashSet<CharacterBody>(tauntTimers.Keys); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void Awake() {
			ai = GetComponent<BaseAI>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void FixedUpdate() {
			if(!ai) return;
			var keys = tauntTimers.Keys.ToList();
			bool wasTaunted = isTaunted;
			foreach(var key in keys) {
				if(!key) { //remove destroyed bodies
					tauntTimers.Remove(key);
					continue;
                }
				tauntTimers[key] -= Time.fixedDeltaTime;
				if(tauntTimers[key] <= 0f) { //remove on timeout
					tauntTimers.Remove(key);
					continue;
                }
			}
			if(isTaunted) {
				if(ai.currentEnemy.gameObject && !tauntTimers.ContainsKey(ai.currentEnemy.gameObject.GetComponent<CharacterBody>())) {
					ai.currentEnemy.Reset();
				}
			} else {
				if(wasTaunted) {
					ai.currentEnemy.Reset();
					if(ai.body)
						ai.body.SetBuffCount(CommonCode.tauntDebuff.buffIndex, 0);
				}
            }
		}
		public static void ApplyTaunt(BaseAI to, CharacterBody from, float duration) {
			if(!to) return;
			if(!to.TryGetComponent<TauntDebuffController>(out var tdc))
				tdc = to.gameObject.AddComponent<TauntDebuffController>();
			tdc.ApplyTaunt(from, duration);
        }
		public void ApplyTaunt(CharacterBody from, float duration) {
			if(!tauntTimers.ContainsKey(from) || tauntTimers[from] < duration)
				tauntTimers[from] = duration;
			if(ai.body && isTaunted)
				ai.body.SetBuffCount(CommonCode.tauntDebuff.buffIndex, 1);
		}
		public void Cleanse() {
			bool wasTaunted = isTaunted;
			tauntTimers.Clear();
			if(wasTaunted) {
				ai.currentEnemy.Reset();
				if(ai.body)
					ai.body.SetBuffCount(CommonCode.tauntDebuff.buffIndex, 0);
			}
		}
		public bool ShouldApplyTauntPenalty(CharacterBody target) {
			return tauntTimers.Count > 0 && !tauntTimers.ContainsKey(target);
        }
    }

	[RequireComponent(typeof(CharacterBody))]
	public class ServerTimedSkillDisable : MonoBehaviour {
		readonly List<float> primaryDisablers = new();
		readonly List<float> secondaryDisablers = new();
		readonly List<float> utilityDisablers = new();
		readonly List<float> specialDisablers = new();
		float cachedPrimaryCooldown;
		float cachedSecondaryCooldown;
		float cachedUtilityCooldown;
		float cachedSpecialCooldown;
		int cachedPrimaryStock;
		int cachedSecondaryStock;
		int cachedUtilityStock;
		int cachedSpecialStock;

		CharacterBody body;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void Awake() {
			body = GetComponent<CharacterBody>();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void OnDestroy() {
			ServerCleanse();
		}

		public void ServerApply(float time, SkillSlot slot) {
			if(!NetworkServer.active) return;
			switch(slot) {
				case SkillSlot.Primary:
					if(primaryDisablers.Count == 0 && body.skillLocator && body.skillLocator.primary) {
						cachedPrimaryCooldown = body.skillLocator.primary.rechargeStopwatch;
						cachedPrimaryStock = body.skillLocator.primary.stock;
					}
					primaryDisablers.Add(time);
					break;
				case SkillSlot.Secondary:
					if(secondaryDisablers.Count == 0 && body.skillLocator && body.skillLocator.secondary) {
						cachedSecondaryCooldown = body.skillLocator.secondary.rechargeStopwatch;
						cachedSecondaryStock = body.skillLocator.secondary.stock;
					}
					secondaryDisablers.Add(time);
					break;
				case SkillSlot.Utility:
					if(utilityDisablers.Count == 0 && body.skillLocator && body.skillLocator.utility) {
						cachedUtilityCooldown = body.skillLocator.utility.rechargeStopwatch;
						cachedUtilityStock = body.skillLocator.utility.stock;
					}
					utilityDisablers.Add(time);
					break;
				case SkillSlot.Special:
					if(specialDisablers.Count == 0 && body.skillLocator && body.skillLocator.special) {
						cachedSpecialCooldown = body.skillLocator.special.rechargeStopwatch;
						cachedSpecialStock = body.skillLocator.special.stock;
					}
					specialDisablers.Add(time);
					break;
				default:
					return;
			}
			new MsgApply(slot, body).Send(R2API.Networking.NetworkDestination.Clients);
		}

		public void ServerCleanse() {
			if(primaryDisablers.Count > 0)
				new MsgRemove(SkillSlot.Primary, body, cachedPrimaryCooldown, cachedPrimaryStock).Send(R2API.Networking.NetworkDestination.Clients);
			if(secondaryDisablers.Count > 0)
				new MsgRemove(SkillSlot.Secondary, body, cachedSecondaryCooldown, cachedSecondaryStock).Send(R2API.Networking.NetworkDestination.Clients);
			if(utilityDisablers.Count > 0)
				new MsgRemove(SkillSlot.Utility, body, cachedUtilityCooldown, cachedUtilityStock).Send(R2API.Networking.NetworkDestination.Clients);
			if(specialDisablers.Count > 0)
				new MsgRemove(SkillSlot.Special, body, cachedSpecialCooldown, cachedSpecialStock).Send(R2API.Networking.NetworkDestination.Clients);
			primaryDisablers.Clear();
			secondaryDisablers.Clear();
			utilityDisablers.Clear();
			specialDisablers.Clear();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void FixedUpdate() {
			if(!NetworkServer.active) return;

			bool hadPrimary = primaryDisablers.Count > 0;
			bool hadSecondary = secondaryDisablers.Count > 0;
			bool hadUtility = utilityDisablers.Count > 0;
			bool hadSpecial = specialDisablers.Count > 0;
			for(var i = 0; i < primaryDisablers.Count; i++) {
				primaryDisablers[i] -= Time.fixedDeltaTime;
            }
			for(var i = 0; i < secondaryDisablers.Count; i++) {
				secondaryDisablers[i] -= Time.fixedDeltaTime;
			}
			for(var i = 0; i < utilityDisablers.Count; i++) {
				utilityDisablers[i] -= Time.fixedDeltaTime;
			}
			for(var i = 0; i < specialDisablers.Count; i++) {
				specialDisablers[i] -= Time.fixedDeltaTime;
			}
			primaryDisablers.RemoveAll(x => x <= 0f);
			secondaryDisablers.RemoveAll(x => x <= 0f);
			utilityDisablers.RemoveAll(x => x <= 0f);
			specialDisablers.RemoveAll(x => x <= 0f);
			bool hasPrimaryNow = primaryDisablers.Count > 0;
			bool hasSecondaryNow = secondaryDisablers.Count > 0;
			bool hasUtilityNow = utilityDisablers.Count > 0;
			bool hasSpecialNow = specialDisablers.Count > 0;
			if(hadPrimary && !hasPrimaryNow)
				new MsgRemove(SkillSlot.Primary, body, cachedPrimaryCooldown, cachedPrimaryStock).Send(R2API.Networking.NetworkDestination.Clients);
			if(hadSecondary && !hasSecondaryNow)
				new MsgRemove(SkillSlot.Secondary, body, cachedSecondaryCooldown, cachedSecondaryStock).Send(R2API.Networking.NetworkDestination.Clients);
			if(hadUtility && !hasUtilityNow)
				new MsgRemove(SkillSlot.Utility, body, cachedUtilityCooldown, cachedUtilityStock).Send(R2API.Networking.NetworkDestination.Clients);
			if(hadSpecial && !hasSpecialNow)
				new MsgRemove(SkillSlot.Special, body, cachedSpecialCooldown, cachedSpecialStock).Send(R2API.Networking.NetworkDestination.Clients);
		}

        public struct MsgApply : INetMessage {
			SkillSlot _slot;
			CharacterBody _target;

			public MsgApply(SkillSlot slot, CharacterBody target) {
				_slot = slot;
				_target = target;
            }

            public void Deserialize(NetworkReader reader) {
				_slot = (SkillSlot)reader.ReadSByte();
				var tgto = reader.ReadGameObject();
				if(tgto)
					_target = tgto.GetComponent<CharacterBody>();
            }

            public void Serialize(NetworkWriter writer) {
				writer.Write((sbyte)_slot);
				writer.Write(_target.gameObject);
			}

			public void OnReceived() {
				if(!_target) {
					TinkersSatchelPlugin._logger.LogWarning("Received ServerTimedSkillDisable.MsgApply for nonexistent game object on client");
					return;
                }
				var sk = _target.skillLocator.GetSkill(_slot);
				if(sk) {
					sk.SetSkillOverride(_target.gameObject, CommonCode.disabledSkillDef, GenericSkill.SkillOverridePriority.Network);
					if(sk.stateMachine) { //stop ongoing skills
						sk.stateMachine.SetInterruptState(new EntityStates.Idle(), EntityStates.InterruptPriority.Any);
						sk.stateMachine.SetNextStateToMain();
					}
				}
			}
		}

		public struct MsgRemove : INetMessage {
			SkillSlot _slot;
			CharacterBody _target;
			float _cooldown;
			int _stock;

			public MsgRemove(SkillSlot slot, CharacterBody target, float cooldown, int stock) {
				_slot = slot;
				_target = target;
				_cooldown = cooldown;
				_stock = stock;
			}

			public void Deserialize(NetworkReader reader) {
				_slot = (SkillSlot)reader.ReadSByte();
				var tgto = reader.ReadGameObject();
				if(tgto)
					_target = tgto.GetComponent<CharacterBody>();
				_cooldown = reader.ReadSingle();
				_stock = reader.ReadInt32();
			}

			public void Serialize(NetworkWriter writer) {
				writer.Write((sbyte)_slot);
				writer.Write(_target.gameObject);
				writer.Write(_cooldown);
				writer.Write(_stock);
			}

			public void OnReceived() {
				if(!_target) {
					TinkersSatchelPlugin._logger.LogWarning("Received ServerTimedSkillDisable.MsgRemove for nonexistent game object on client");
					return;
				}
				var sk = _target.skillLocator.GetSkill(_slot);
				if(sk) {
					sk.UnsetSkillOverride(_target.gameObject, CommonCode.disabledSkillDef, GenericSkill.SkillOverridePriority.Network);
					sk.rechargeStopwatch = _cooldown;
					sk.stock = _stock;
				}
			}
		}
	}

	public class WobbleSpinnerAnim : MonoBehaviour {
		public float rotateSpeed = 0.5f;
		public float wobbleBaseMin = -0.2f;
		public float wobbleBaseMax = 0.2f;
		public float wobbleEccentricity = 0.15f;
		public float wobbleStability = 2f;
		public Vector3 rotateAxis;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
		void Update() {
			this.gameObject.transform.Rotate(rotateAxis, rotateSpeed);
			Vector3 rb = Vector3.one * Random.Range(wobbleBaseMin, wobbleBaseMax);
			Vector3 rx = new(
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2),
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2),
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2)
				);
			this.gameObject.transform.localScale = (rb + rx) * (1f / wobbleStability) + Vector3.one;
		}
	}
}