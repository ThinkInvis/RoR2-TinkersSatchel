using RoR2;
using UnityEngine;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using System.Collections.Generic;
using TILER2;
using RoR2.Skills;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
	public class TimedSkillDisableModule : T2Module<TimedSkillDisableModule> {
		public override bool managedEnable => false;

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
					sk.SetSkillOverride(_target.gameObject, TimedSkillDisableModule.disabledSkillDef, GenericSkill.SkillOverridePriority.Network);
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
					sk.UnsetSkillOverride(_target.gameObject, TimedSkillDisableModule.disabledSkillDef, GenericSkill.SkillOverridePriority.Network);
					sk.rechargeStopwatch = _cooldown;
					sk.stock = _stock;
				}
			}
		}
	}
}