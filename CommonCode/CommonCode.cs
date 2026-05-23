using R2API;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using RoR2.Navigation;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
	public class CommonCode : Module<CommonCode> {
        public override bool managedEnable => false;

		[Obsolete("Replaced by TimedSkillDisableModule.disabledSkillDef.")]
		public static SkillDef disabledSkillDef => TimedSkillDisableModule.disabledSkillDef;
		[Obsolete("Replaced by TauntDebuffModule.tauntDebuff.")]
		public static BuffDef tauntDebuff => TauntDebuffModule.tauntDebuff;

		public static ExpansionDef expansionDef;
		public static ExpansionDef voidExpansionDef;

		public static DirectorCardCategorySelection globalInteractablesDccs;

		static GameObject _worldSpaceWeaponDummy = null;
		public static GameObject worldSpaceWeaponDummy {
			get {
				if(!_worldSpaceWeaponDummy) _worldSpaceWeaponDummy = new GameObject("Workaround for an Inconvenient Quirk of BulletAttack");
				return _worldSpaceWeaponDummy;
			}
		}

		void _SetupExpansions() {
			expansionDef = TinkersSatchelPlugin.resources.LoadAsset<ExpansionDef>("Assets/TinkersSatchel/TinkersSatchelExpansion.asset");
			voidExpansionDef = TinkersSatchelPlugin.resources.LoadAsset<ExpansionDef>("Assets/TinkersSatchel/TinkersSatchelVoidExpansion.asset");

			var disabIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();
			expansionDef.disabledIconSprite = disabIcon;
			voidExpansionDef.disabledIconSprite = disabIcon;
			voidExpansionDef.requiredEntitlement = Addressables.LoadAssetAsync<EntitlementDef>("RoR2/DLC1/Common/entitlementDLC1.asset").WaitForCompletion();

			ContentAddition.AddExpansionDef(expansionDef);
			ContentAddition.AddExpansionDef(voidExpansionDef);
		}

		void _SetupInteractablesCategory() {
			globalInteractablesDccs = TinkersSatchelPlugin.resources.LoadAsset<DirectorCardCategorySelection>("Assets/TinkersSatchel/dccsTkSatGlobalInteractables.asset");
            DirectorAPI.InteractableActions += DirectorAPI_InteractableActions;
		}

		internal class ConditionalDirectorCardHolder {
			public DirectorAPI.DirectorCardHolder directorCardHolder;
			public ExpansionDef[] requiredExpansions;
			public ConditionalDirectorCardHolder(DirectorAPI.DirectorCardHolder dch, params ExpansionDef[] exps) {
				directorCardHolder = dch;
				requiredExpansions = exps;
			}
		}
		internal static HashSet<ConditionalDirectorCardHolder> dchList = new();

		private void DirectorAPI_InteractableActions(DccsPool arg1, DirectorAPI.StageInfo arg2) {
			var toAdd = dchList.Where(dch => dch.requiredExpansions.All(ed => Run.instance.IsExpansionEnabled(ed)));
			foreach(var cat in arg1.poolCategories) {
				foreach(var pool in cat.alwaysIncluded) {
					foreach(var dch in toAdd) {
						pool.dccs.AddCard(dch.directorCardHolder);
					}
				}
				foreach(var pool in cat.includedIfConditionsMet) {
					foreach(var dch in toAdd) {
						pool.dccs.AddCard(dch.directorCardHolder);
					}
				}
				foreach(var pool in cat.includedIfNoConditionsMet) {
					foreach(var dch in toAdd) {
						pool.dccs.AddCard(dch.directorCardHolder);
					}
				}
			}
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

			_SetupExpansions();
			_SetupInteractablesCategory();
		}

        public override void SetupBehavior() {
            base.SetupBehavior();

            On.RoR2.BulletAttack.FireSingle += BulletAttack_FireSingle;
        }

        private void BulletAttack_FireSingle(On.RoR2.BulletAttack.orig_FireSingle orig, BulletAttack self, BulletAttack.FireSingleArgs args) {
			if(self.weapon == worldSpaceWeaponDummy)
				self.weapon = null; //force tracer effect to happen in worldspace. BulletAttack.Fire sets weapon to owner if null, even if you set it to null on purpose >:(
			orig(self, args);
		}

		internal static void RetrieveDefaultMaterials(ItemDisplay disp) {
			for(var i = 0; i < disp.rendererInfos.Length; i++) {
				var ri = disp.rendererInfos[i];
				ri.defaultMaterial = ri.renderer.material;
				disp.rendererInfos[i] = ri;
			}
		}

        /// <summary>
        /// Iterates towards the root of a GameObject, including jumping through EntityLocators.
        /// </summary>
        /// <param name="target">The GameObject to search for the 'true' root of.</param>
        /// <param name="maxSearch">The maximum amount of recursion to go through.</param>
        /// <returns>Null if the given object was null; the most top-level object with the given constraints otherwise.</returns>
        public static GameObject GetRootWithLocators(GameObject target, int maxSearch = 5) {
            if(!target) return null;
            GameObject scan = target;
            for(int i = 0; i < maxSearch; i++) {
                if(scan.TryGetComponent<EntityLocator>(out var eloc) && eloc.entity) {
                    scan = eloc.entity;
                    continue;
                }

                var next = scan.transform.root;
                if(next && next.gameObject != scan)
                    scan = next.gameObject;
                else
                    return scan;
            }
            return scan;
        }

        internal static GameObject FindNearestInteractable(GameObject senderObj, HashSet<string> validObjectNames, Ray aim, float maxAngle, float maxDistance, bool requireLoS) {
			aim = CameraRigController.ModifyAimRayIfApplicable(aim, senderObj, out float camAdjust);
			var results = Physics.OverlapSphere(aim.origin, maxDistance + camAdjust, Physics.AllLayers, QueryTriggerInteraction.Collide);
			var minDot = Mathf.Cos(Mathf.Clamp(maxAngle, 0f, 180f) * Mathf.PI / 180f);
			GameObject retv = null;
			var lowestC = float.MaxValue;
			foreach(var obj in results) {
				if(!obj || !obj.gameObject) continue;
				var root = GetRootWithLocators(obj.gameObject);
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

		public static bool PickupIndexIsAISafe(PickupIndex pind) {
			return CatalogUtil.TryGetItemDef(pind, out var idef) && !idef.ContainsTag(ItemTag.AIBlacklist);
		}

		public static PickupIndex GenerateAISafePickup(Xoroshiro128Plus rng, List<PickupIndex> selection) {
			var aiSafeSelector = selection.Where(pind => PickupIndexIsAISafe(pind));

			if(aiSafeSelector.Count() == 0) {
				TinkersSatchelPlugin._logger.LogError("GenerateAISafePickup (single uniform list): selection contained 0 valid items");
				return PickupIndex.none;
			}

			return rng.NextElementUniform(aiSafeSelector.ToArray());
		}

		public static PickupIndex GenerateAISafePickup(Xoroshiro128Plus rng, WeightedSelection<List<PickupIndex>> selection) {
			var aiSafeSelector = new WeightedSelection<PickupIndex>();

			foreach(var tier in selection.choices) {
				foreach(var pind in tier.value) {
					if(!PickupIndexIsAISafe(pind)) continue;
					aiSafeSelector.AddChoice(pind, tier.weight);
				}
			}

			if(aiSafeSelector.choices.Length == 0) {
				TinkersSatchelPlugin._logger.LogError("GenerateAISafePickup (single selection): selection contained 0 valid items");
				return PickupIndex.none;
			}

			return aiSafeSelector.Evaluate(rng.nextNormalizedFloat);
		}

		public static PickupIndex GenerateAISafePickup(Xoroshiro128Plus rng, PickupDropTable dropTable, WeightedSelection<List<PickupIndex>> fallback) {
			var aiSafeSelector = new WeightedSelection<UniquePickup>();

			if(dropTable is BasicPickupDropTable bpdt) {
				foreach(var ch in bpdt.selector.choices.Where(c => PickupIndexIsAISafe(c.value.pickupIndex)))
					aiSafeSelector.AddChoice(ch);
			} else if(dropTable is ExplicitPickupDropTable epdt) {
				foreach(var ch in epdt.weightedSelection.choices.Where(c => PickupIndexIsAISafe(c.value.pickupIndex)))
					aiSafeSelector.AddChoice(ch);
			} 
			
			if(aiSafeSelector.choices.Length == 0) {
				foreach(var tier in fallback.choices) {
					foreach(var pind in tier.value) {
						if(!PickupIndexIsAISafe(pind)) continue;
						aiSafeSelector.AddChoice(new UniquePickup(pind), tier.weight);
					}
				}
			}

			if(aiSafeSelector.choices.Length == 0) {
				TinkersSatchelPlugin._logger.LogError("GenerateAISafePickup (droptable and weighted fallback): both normal and fallback selections contained 0 valid items");
				return PickupIndex.none;
			}

			return aiSafeSelector.Evaluate(rng.nextNormalizedFloat).pickupIndex;
		}

		public static PickupIndex GenerateAISafePickup(Xoroshiro128Plus rng, PickupDropTable dropTable, List<PickupIndex> fallback) {
			var aiSafeSelector = new WeightedSelection<UniquePickup>();

			if(dropTable is BasicPickupDropTable bpdt) {
				foreach(var ch in bpdt.selector.choices.Where(c => PickupIndexIsAISafe(c.value.pickupIndex)))
					aiSafeSelector.AddChoice(ch);
			} else if(dropTable is ExplicitPickupDropTable epdt) {
				foreach(var ch in epdt.weightedSelection.choices.Where(c => PickupIndexIsAISafe(c.value.pickupIndex)))
					aiSafeSelector.AddChoice(ch);
			}

			if(aiSafeSelector.choices.Length == 0) {
				foreach(var pind in fallback) {
					if(!PickupIndexIsAISafe(pind)) continue;
					aiSafeSelector.AddChoice(new UniquePickup(pind), 1f);
				}
			}

			if(aiSafeSelector.choices.Length == 0) {
				TinkersSatchelPlugin._logger.LogError("GenerateAISafePickup (droptable and uniform fallback): both normal and fallback selections contained 0 valid items");
				return PickupIndex.none;
			}

			return aiSafeSelector.Evaluate(rng.nextNormalizedFloat).pickupIndex;
        }

        /// <summary>
        /// Returns a list of enemy TeamComponents given an ally team (to ignore while friendly fire is off) and a list of ignored teams (to ignore under all circumstances).
        /// </summary>
        /// <param name="allyIndex">The team to ignore if friendly fire is off.</param>
        /// <param name="ignore">Additional teams to always ignore.</param>
        /// <returns>A list of all TeamComponents that match the provided team constraints.</returns>
        public static List<TeamComponent> GatherEnemies(TeamIndex allyIndex, params TeamIndex[] ignore) {
            var retv = new List<TeamComponent>();
            bool isFF = FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off;
            var scan = ((TeamIndex[])Enum.GetValues(typeof(TeamIndex))).Except(ignore);
            foreach(var ind in scan) {
                if(isFF || allyIndex != ind)
                    retv.AddRange(TeamComponent.GetTeamMembers(ind));
            }
            return retv;
        }
        public static Language GetBestLanguage(string langID) => ((langID == null) ? null : Language.FindLanguageByName(langID)) ?? Language.currentLanguage ?? Language.english;
    }
	public static class CommonCodeExtensions {
		internal static Quaternion ApplyRandomSpread(this Xoroshiro128Plus rng, Quaternion targetRotation, float coneHalfAngleDegr) {
			var phi = rng.nextNormalizedFloat * Mathf.PI * 2f;
			var z = Mathf.Lerp(Mathf.Cos(coneHalfAngleDegr * Mathf.PI / 180f), 1f, rng.nextNormalizedFloat);
			var zf = Mathf.Sqrt(1f - z * z);
			var rDir = new Vector3(zf * Mathf.Cos(phi), zf * Mathf.Sin(phi), z);
			return targetRotation * Quaternion.LookRotation(rDir);
		}

        /// <summary>
        /// Uses reflection to subscribe an event handler to an EventInfo.
        /// </summary>
        /// <param name="evt">The EventInfo to subscribe to.</param>
        /// <param name="o">The object instance to apply this subscription to.</param>
        /// <param name="lam">The method to subscribe with.</param>
        public static void ReflAddEventHandler(this EventInfo evt, object o, Action<object, EventArgs> lam) {
            var pArr = evt.EventHandlerType.GetMethod("Invoke").GetParameters().Select(p => Expression.Parameter(p.ParameterType)).ToArray();
            var h = Expression.Lambda(evt.EventHandlerType, Expression.Call(Expression.Constant(lam), lam.GetType().GetMethod("Invoke"), pArr[0], pArr[1]), pArr).Compile();
            evt.AddEventHandler(o, h);
        }
    }
}