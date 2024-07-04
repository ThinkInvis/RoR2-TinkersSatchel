using R2API;
using RoR2;
using RoR2.Skills;
using TILER2;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using RoR2.EntitlementManagement;
using System;

namespace ThinkInvisible.TinkersSatchel {
	public class CommonCode : T2Module<CommonCode> {
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

        private void BulletAttack_FireSingle(On.RoR2.BulletAttack.orig_FireSingle orig, BulletAttack self, Vector3 normal, int muzzleIndex) {
			if(self.weapon == worldSpaceWeaponDummy)
				self.weapon = null; //force tracer effect to happen in worldspace. BulletAttack.Fire sets weapon to owner if null, even if you set it to null on purpose >:(
			orig(self, normal, muzzleIndex);
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
			var aiSafeSelector = new WeightedSelection<PickupIndex>();

			if(dropTable is BasicPickupDropTable bpdt) {
				foreach(var ch in bpdt.selector.choices.Where(c => PickupIndexIsAISafe(c.value)))
					aiSafeSelector.AddChoice(ch);
			} else if(dropTable is ExplicitPickupDropTable epdt) {
				foreach(var ch in epdt.weightedSelection.choices.Where(c => PickupIndexIsAISafe(c.value)))
					aiSafeSelector.AddChoice(ch);
			} 
			
			if(aiSafeSelector.choices.Length == 0) {
				foreach(var tier in fallback.choices) {
					foreach(var pind in tier.value) {
						if(!PickupIndexIsAISafe(pind)) continue;
						aiSafeSelector.AddChoice(pind, tier.weight);
					}
				}
			}

			if(aiSafeSelector.choices.Length == 0) {
				TinkersSatchelPlugin._logger.LogError("GenerateAISafePickup (droptable and weighted fallback): both normal and fallback selections contained 0 valid items");
				return PickupIndex.none;
			}

			return aiSafeSelector.Evaluate(rng.nextNormalizedFloat);
		}

		public static PickupIndex GenerateAISafePickup(Xoroshiro128Plus rng, PickupDropTable dropTable, List<PickupIndex> fallback) {
			var aiSafeSelector = new WeightedSelection<PickupIndex>();

			if(dropTable is BasicPickupDropTable bpdt) {
				foreach(var ch in bpdt.selector.choices.Where(c => PickupIndexIsAISafe(c.value)))
					aiSafeSelector.AddChoice(ch);
			} else if(dropTable is ExplicitPickupDropTable epdt) {
				foreach(var ch in epdt.weightedSelection.choices.Where(c => PickupIndexIsAISafe(c.value)))
					aiSafeSelector.AddChoice(ch);
			}

			if(aiSafeSelector.choices.Length == 0) {
				foreach(var pind in fallback) {
					if(!PickupIndexIsAISafe(pind)) continue;
					aiSafeSelector.AddChoice(pind, 1f);
				}
			}

			if(aiSafeSelector.choices.Length == 0) {
				TinkersSatchelPlugin._logger.LogError("GenerateAISafePickup (droptable and uniform fallback): both normal and fallback selections contained 0 valid items");
				return PickupIndex.none;
			}

			return aiSafeSelector.Evaluate(rng.nextNormalizedFloat);
		}
	}
}