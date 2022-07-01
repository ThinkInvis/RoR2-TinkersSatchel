using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using RoR2.ExpansionManagement;

namespace ThinkInvisible.TinkersSatchel {
	public class GupRay : Item<GupRay> {

		////// Item Data //////

		public override string displayName => "Gup Ray";
		public override ItemTier itemTier => ItemTier.VoidTier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) => "Split enemies into two much weaker copies. <style=cIsVoid>Corrupts all Shrink Rays</style>.";
		protected override string GetDescString(string langid = null) => $"Once every {icd:N1} seconds, hitting a non-final-boss enemy <style=cIsUtility>splits</style> them into 2 copies with {Pct(statMult)} <style=cIsHealth>health</style> and <style=cIsDamage>damage</style>. <style=cStack>Enemies can be split 1 time per stack.</style> <style=cIsVoid>Corrupts all Shrink Rays</style>.";
		protected override string GetLoreString(string langid = null) => "";



		////// Config //////

		[AutoConfigRoOIntSlider("{0:N0}", 2, 20)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Number of copies to spawn per split.",
			AutoConfigFlags.None, 2, int.MaxValue)]
		public int splitCount { get; private set; } = 2;

		[AutoConfigRoOSlider("{0:P0}", float.Epsilon, 1f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
		[AutoConfig("Damage/health multiplier per stack (exponential).",
			AutoConfigFlags.None, float.Epsilon, 1f)]
		public float statMult { get; private set; } = 0.25f;

		[AutoConfigRoOSlider("{0:P0}", float.Epsilon, 1f)]
		[AutoConfig("Visual scale multiplier per stack (exponential).",
			AutoConfigFlags.DeferUntilNextStage, float.Epsilon, 1f)]
		public float scaleMult { get; private set; } = 0.5f;

		[AutoConfigRoOSlider("{0:N1} s", 0f, 30f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Internal cooldown of applying the effect. Does not stack.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float icd { get; private set; } = 5f;



		////// Other Fields/Properties //////

		public ItemDef gupDebuff { get; private set; }
		public bool currentSplitIsGupRay { get; private set; }
		public GameObject idrPrefab { get; private set; }



		////// TILER2 Module Setup //////

		public GupRay() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/GupRay.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/gupRayIcon.png");
			idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/GupRay.prefab");
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
				localPos = new Vector3(-0.32652F, -0.0434F, 0.04198F),
				localAngles = new Vector3(299.9375F, 302.1016F, 90.4572F),
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

			gupDebuff = ScriptableObject.CreateInstance<ItemDef>();
			gupDebuff.deprecatedTier = ItemTier.NoTier;
			gupDebuff.canRemove = false;
			gupDebuff.hidden = true;
			gupDebuff.nameToken = "TKSAT_INTERNAL_GUPRAY_COUNTER";
			gupDebuff.loreToken = "";
			gupDebuff.descriptionToken = "";
			gupDebuff.pickupToken = "";
			gupDebuff.name = "TkSatInternalGupRayCounter";
			gupDebuff.tags = new ItemTag[] { };
			ContentAddition.AddItemDef(gupDebuff);

			itemDef.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset")
				.WaitForCompletion();

			On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
				var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
				isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
				isp.relationships = new[] {new ItemDef.Pair {
					itemDef1 = ShrinkRay.instance.itemDef,
					itemDef2 = itemDef
				}};
				orig(providers.Concat(new[] { isp }).ToArray());
			};
		}

		public override void Install() {
			base.Install();

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            IL.RoR2.BodySplitter.PerformInternal += BodySplitter_PerformInternal;
		}

        public override void Uninstall() {
			base.Uninstall();

			On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
			IL.RoR2.BodySplitter.PerformInternal -= BodySplitter_PerformInternal;
		}



		////// Hooks //////

		private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
			orig(self, damageInfo, victim);
			if(NetworkServer.active && damageInfo != null && damageInfo.attacker) {
				var count = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
				var victimBody = victim.GetComponent<CharacterBody>();
				if(count > 0 && damageInfo.attacker != victim && victimBody
					&& !victimBody.gameObject.name.Contains("Brother")
					&& !victimBody.gameObject.name.Contains("VoidRaidCrab")
					&& !victimBody.gameObject.name.Contains("ScavLunar")
					&& victimBody.master && victimBody.master.inventory && victimBody.master.inventory.GetItemCount(gupDebuff) < count) {
					var sricd = damageInfo.attacker.GetComponent<ShrinkRayICDComponent>();
					if(!sricd)
						sricd = damageInfo.attacker.AddComponent<ShrinkRayICDComponent>();
					if(Time.fixedTime - sricd.lastHit > icd) {
						sricd.lastHit = Time.fixedTime;

						victimBody.master.inventory.GiveItem(gupDebuff);

						EffectManager.SpawnEffect(EntityStates.Gup.BaseSplitDeath.deathEffectPrefab, new EffectData {
							origin = victim.GetComponent<CharacterBody>().corePosition,
							scale = 10f
						}, true);
						var masterPrefab = MasterCatalog.GetMasterPrefab(victimBody.master.masterIndex);
						currentSplitIsGupRay = true;
						new BodySplitter {
							body = victimBody,
							masterSummon = {
								masterPrefab = masterPrefab
							},
							count = splitCount,
							splinterInitialVelocityLocal = new Vector3(0f, 20f, 10f),
							minSpawnCircleRadius = 3f,
							moneyMultiplier = 1f / (float)splitCount
						}.Perform();
						currentSplitIsGupRay = false;
						if(victimBody.master)
							victimBody.master.TrueKill();
						GameObject.Destroy(victimBody.gameObject);
					}
				}
			}
		}

		private void BodySplitter_PerformInternal(ILContext il) {
			ILCursor c = new ILCursor(il);

			if(c.TryGotoNext(MoveType.Before,
				x => x.MatchLdloc(out _),
				x => x.MatchLdloc(out _),
				x => x.MatchCallOrCallvirt<BodySplitter>(nameof(BodySplitter.AddBodyVelocity)))) {
				c.Index++;
				c.Emit(OpCodes.Dup);
				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Action<CharacterBody, BodySplitter>>((newBody, oldSplitter) => {
					if(!currentSplitIsGupRay || !oldSplitter.body) return;
					var splitCount = newBody.master.inventory.GetItemCount(gupDebuff);
					newBody.modelLocator.transform.localScale *= Mathf.Pow(GupRay.instance.scaleMult, splitCount);
					var statsFac = Mathf.Pow(statMult, splitCount);
					newBody.baseMaxHealth *= statsFac;
					newBody.levelMaxHealth *= statsFac;
					newBody.baseDamage *= statsFac;
					newBody.levelDamage *= statsFac;
					newBody.healthComponent.Networkhealth = oldSplitter.body.healthComponent.health * statsFac;
					newBody.MarkAllStatsDirty();
					
					if(TeleporterInteraction.instance && TeleporterInteraction.instance.bossGroup && TeleporterInteraction.instance.bossGroup.combatSquad && TeleporterInteraction.instance.bossGroup.combatSquad.ContainsMember(oldSplitter.body.master))
						TeleporterInteraction.instance.bossGroup.combatSquad.AddMember(newBody.master);
				});
            } else {
				TinkersSatchelPlugin._logger.LogError("GupRay: failed to apply IL hook (BodySplitter_PerformInternal), could not find target instructions. Item may not work correctly.");
            }
		}
	}

	public class GupRayICDComponent : MonoBehaviour {
		public float lastHit = 0f;
    }
}