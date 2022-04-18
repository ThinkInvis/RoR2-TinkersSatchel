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

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Number of copies to spawn per split.",
			AutoConfigFlags.None, 2, int.MaxValue)]
		public int splitCount { get; private set; } = 2;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
		[AutoConfig("Damage/health multiplier per stack (exponential).",
			AutoConfigFlags.None, float.Epsilon, 1f)]
		public float statMult { get; private set; } = 0.25f;

		[AutoConfig("Visual scale multiplier per stack (exponential).",
			AutoConfigFlags.DeferUntilNextStage, float.Epsilon, 1f)]
		public float scaleMult { get; private set; } = 0.5f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Internal cooldown of applying the effect. Does not stack.",
			AutoConfigFlags.None, 0f, float.MaxValue)]
		public float icd { get; private set; } = 5f;



		////// Other Fields/Properties //////

		public ItemDef gupDebuff { get; private set; }
		public ItemDef wasTelebossTracker { get; private set; }
		public bool currentSplitIsGupRay { get; private set; }



		////// TILER2 Module Setup //////

		public GupRay() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/GupRay.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/gupRayIcon.png");
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			gupDebuff = ScriptableObject.CreateInstance<ItemDef>();
			gupDebuff.tier = ItemTier.NoTier;
			gupDebuff.canRemove = false;
			gupDebuff.hidden = true;
			gupDebuff.nameToken = "TKSAT_INTERNAL_GUPRAY_COUNTER";
			gupDebuff.name = "TkSatInternalGupRayCounter";
			gupDebuff.tags = new ItemTag[] { };
			ContentAddition.AddItemDef(gupDebuff);

			wasTelebossTracker = ScriptableObject.CreateInstance<ItemDef>();
			wasTelebossTracker.tier = ItemTier.NoTier;
			wasTelebossTracker.canRemove = false;
			wasTelebossTracker.hidden = true;
			wasTelebossTracker.nameToken = "TKSAT_INTERNAL_GUPRAY_WASBOSS";
			wasTelebossTracker.name = "TkSatInternalGupRayWasBoss";
			wasTelebossTracker.tags = new ItemTag[] { };
			ContentAddition.AddItemDef(wasTelebossTracker);

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
				if(count > 0 && damageInfo.attacker != victim && victimBody && !victimBody.master.isBoss && victimBody.master && victimBody.master.inventory && victimBody.master.inventory.GetItemCount(gupDebuff) < count) {
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

			int locSpawnedBody;
			if(c.TryGotoNext(MoveType.Before,
				x => x.MatchLdloc(out locSpawnedBody),
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