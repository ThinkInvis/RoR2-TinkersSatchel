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
using RoR2.ExpansionManagement;
using RoR2.EntitlementManagement;

namespace ThinkInvisible.TinkersSatchel {
	public class CommonCode : T2Module<CommonCode> {
        public override bool managedEnable => false;

		public static SkillDef disabledSkillDef;
		public static BuffDef tauntDebuff;

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

			ContentAddition.AddExpansionDef(expansionDef);
			ContentAddition.AddExpansionDef(voidExpansionDef);

			voidExpansionDef.requiredEntitlement = Addressables.LoadAssetAsync<EntitlementDef>("RoR2/DLC1/Common/entitlementDLC1.asset").WaitForCompletion();
		}

		void _SetupDisabledSkill() {
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

		void _SetupTauntDebuff() {
			tauntDebuff = ScriptableObject.CreateInstance<BuffDef>();
			tauntDebuff.buffColor = Color.white;
			tauntDebuff.canStack = false;
			tauntDebuff.isDebuff = true;
			tauntDebuff.name = "TKSATTaunt";
			tauntDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texAttackIcon.png")
				.WaitForCompletion();
			ContentAddition.AddBuffDef(tauntDebuff);
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
			_SetupDisabledSkill();
			_SetupTauntDebuff();
			_SetupInteractablesCategory();
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
}