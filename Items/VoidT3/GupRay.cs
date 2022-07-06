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
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility });

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
				localPos = new Vector3(-0.22324F, 0.30146F, 0.02418F),
				localAngles = new Vector3(21.22066F, 290.6745F, 15.75656F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CommandoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(-0.21217F, -0.00958F, -0.12372F),
				localAngles = new Vector3(332.5844F, 235.295F, 76.58227F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("CrocoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(2.14873F, -0.78192F, 0.65279F),
				localAngles = new Vector3(10.63145F, 93.94849F, 111.4574F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("EngiBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(-0.25678F, 0.13689F, 0.07907F),
				localAngles = new Vector3(350.8508F, 285.7603F, 250.8249F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("HuntressBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(-0.16329F, 0.00278F, -0.14153F),
				localAngles = new Vector3(19.33466F, 47.14599F, 79.93434F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("LoaderBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "MechBase",
				localPos = new Vector3(-0.26597F, -0.01956F, -0.01343F),
				localAngles = new Vector3(11.71471F, 72.83887F, 344.3481F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("MageBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(-0.18668F, -0.07374F, 0.02444F),
				localAngles = new Vector3(10.10351F, 251.6842F, 230.3821F),
				localScale = new Vector3(0.25F, 0.25F, 0.25F)
			});
			displayRules.Add("MercBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "ThighL",
				localPos = new Vector3(0.1533F, 0.1287F, 0.04594F),
				localAngles = new Vector3(357.9319F, 262.4632F, 264.5586F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("ToolbotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(-2.50783F, 0.0843F, -1.1181F),
				localAngles = new Vector3(325.9112F, 266.2314F, 72.65425F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("TreebotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "WeaponPlatformEnd",
				localPos = new Vector3(-0.29042F, -0.40263F, 0.16839F),
				localAngles = new Vector3(358.2487F, 265.0534F, 270.1194F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("RailgunnerBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "BottomRail",
				localPos = new Vector3(-0.0009F, 0.73048F, -0.0816F),
				localAngles = new Vector3(355.8767F, 134.6801F, 283.2966F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Center",
				localPos = new Vector3(-0.1592F, -0.00442F, -0.06078F),
				localAngles = new Vector3(6.98974F, 268.1853F, 85.401F),
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
			ILCursor c = new(il);

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