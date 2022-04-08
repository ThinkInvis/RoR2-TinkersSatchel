using R2API;
using RoR2;
using RoR2.Skills;
using TILER2;
using UnityEngine;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
	public class CommonCode : T2Module<CommonCode> {
        public override bool managedEnable => false;

        public override void RefreshPermanentLanguage() {
			permanentGenericLanguageTokens.Add("TKSAT_DISABLED_SKILL_NAME", "Suppressed Skill");
			permanentGenericLanguageTokens.Add("TKSAT_DISABLED_SKILL_DESCRIPTION", "Something has temporarily prevented you from using this skill!");

			base.RefreshPermanentLanguage();
		}

		public static SkillDef disabledSkillDef;

        public override void SetupAttributes() {
            base.SetupAttributes();

			var captainSD = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/CaptainBody/CaptainSkillUsedUp");

			disabledSkillDef = SkillUtil.CloneSkillDef(captainSD);
			disabledSkillDef.skillNameToken = "TKSAT_DISABLED_SKILL_NAME";
			disabledSkillDef.skillDescriptionToken = "TKSAT_DISABLED_SKILL_DESCRIPTION";
			disabledSkillDef.dontAllowPastMaxStocks = false;
			disabledSkillDef.beginSkillCooldownOnSkillEnd = true;

			ContentAddition.AddSkillDef(disabledSkillDef);

			R2API.Networking.NetworkingAPI.RegisterMessageType<ServerTimedSkillDisable.MsgApply>();
			R2API.Networking.NetworkingAPI.RegisterMessageType<ServerTimedSkillDisable.MsgRemove>();
		}

        public override void SetupBehavior() {
            base.SetupBehavior();

            On.RoR2.Util.CleanseBody += Util_CleanseBody;
        }

        private void Util_CleanseBody(On.RoR2.Util.orig_CleanseBody orig, CharacterBody characterBody, bool removeDebuffs, bool removeBuffs, bool removeCooldownBuffs, bool removeDots, bool removeStun, bool removeNearbyProjectiles) {
			orig(characterBody, removeDebuffs, removeBuffs, removeCooldownBuffs, removeDots, removeStun, removeNearbyProjectiles);
			if(removeDebuffs && characterBody && characterBody.TryGetComponent<ServerTimedSkillDisable>(out var stsd)) {
				stsd.ServerCleanse();
            }
        }
    }

	[RequireComponent(typeof(CharacterBody))]
	public class ServerTimedSkillDisable : MonoBehaviour {
		List<float> primaryDisablers = new List<float>();
		List<float> secondaryDisablers = new List<float>();
		List<float> utilityDisablers = new List<float>();
		List<float> specialDisablers = new List<float>();
		float cachedPrimaryCooldown;
		float cachedSecondaryCooldown;
		float cachedUtilityCooldown;
		float cachedSpecialCooldown;
		int cachedPrimaryStock;
		int cachedSecondaryStock;
		int cachedUtilityStock;
		int cachedSpecialStock;

		CharacterBody body;

		void Awake() {
			body = GetComponent<CharacterBody>();
        }

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

	public class TargetSpinnerAnim : MonoBehaviour {
		public float rotateTime = 0.5f;
		public float delayTime = 1f;
		public Vector3 rotateAxis;

		private float targPos = -1f;
		private float currVel;
		private float currPos;
		private float stopwatch;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void Update() {
			stopwatch -= Time.deltaTime;
			if(stopwatch < 0f) {
				targPos = Random.value * Mathf.PI * 2;
				stopwatch = rotateTime + delayTime;
			}
			currPos = Mathf.SmoothDampAngle(currPos, targPos, ref currVel, rotateTime);
			this.gameObject.transform.Rotate(rotateAxis, currVel);
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
			Vector3 rx = new Vector3(
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2),
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2),
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2)
				);
			this.gameObject.transform.localScale = (rb + rx) * (1f / wobbleStability) + Vector3.one;
		}
	}

	public class PixieFuseFlicker : MonoBehaviour {
		[SerializeField]
		public GameObject[] targets;
		public float switchIntervalMin;
		public float switchIntervalMax;
		public float flickerInterval;
		public int flickersMin;
		public int flickersMax;

		private float stopwatch = 0f;
		private int flickerCount = 0;
		private int currIndex = 0;

		void Update() {
			stopwatch -= Time.deltaTime;
			if(stopwatch < 0f) {
				if(flickerCount <= 0) {
					currIndex = Random.Range(0, targets.Length);
					flickerCount = Random.Range(flickersMin, flickersMax + 1) * 2;
					for(var i = 0; i < targets.Length; i++) {
						targets[i].SetActive(currIndex == i);
					}
				}

				stopwatch = (flickerCount > 1 ? Random.Range(0.02f, 0.1f) : Random.Range(switchIntervalMin, switchIntervalMax));
				flickerCount--;
				targets[currIndex].SetActive(!targets[currIndex].activeSelf);
			}
		}
	}
}