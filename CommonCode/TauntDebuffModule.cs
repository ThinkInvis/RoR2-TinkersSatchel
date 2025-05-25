using RoR2;
using UnityEngine;
using System.Collections.Generic;
using RoR2.CharacterAI;
using System.Linq;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
	public class TauntDebuffModule : T2Module<TauntDebuffModule> {
		public override bool managedEnable => false;

		public static BuffDef tauntDebuff;

		public override void SetupAttributes() {
			base.SetupAttributes();

			tauntDebuff = ScriptableObject.CreateInstance<BuffDef>();
			tauntDebuff.buffColor = Color.white;
			tauntDebuff.canStack = false;
			tauntDebuff.isDebuff = true;
			tauntDebuff.name = "TKSATTaunt";
			tauntDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texAttackIcon.png")
				.WaitForCompletion();
            tauntDebuff.ignoreGrowthNectar = true;
            tauntDebuff.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
            ContentAddition.AddBuffDef(tauntDebuff);
		}

		public override void SetupBehavior() {
			base.SetupBehavior();

			On.RoR2.Util.CleanseBody += Util_CleanseBody;
			On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += BaseAI_FindEnemyHurtBox;
			On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
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
						ai.body.SetBuffCount(TauntDebuffModule.tauntDebuff.buffIndex, 0);
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
				ai.body.SetBuffCount(TauntDebuffModule.tauntDebuff.buffIndex, 1);
		}
		public void Cleanse() {
			bool wasTaunted = isTaunted;
			tauntTimers.Clear();
			if(wasTaunted) {
				ai.currentEnemy.Reset();
				if(ai.body)
					ai.body.SetBuffCount(TauntDebuffModule.tauntDebuff.buffIndex, 0);
			}
		}
		public bool ShouldApplyTauntPenalty(CharacterBody target) {
			return tauntTimers.Count > 0 && !tauntTimers.ContainsKey(target);
        }
    }
}