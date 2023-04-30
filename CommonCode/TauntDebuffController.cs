using RoR2;
using UnityEngine;
using System.Collections.Generic;
using RoR2.CharacterAI;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
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
}