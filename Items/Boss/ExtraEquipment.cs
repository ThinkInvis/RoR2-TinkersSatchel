using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
	public class ExtraEquipment : Item<ExtraEquipment> {

		public const int MAX_STACKS = 200; //total equipment slots are limited to byte.maxvalue (255)

		////// Item Data //////

		public override string displayName => "Scavenger's Rucksack";
		public override ItemTier itemTier => ItemTier.Boss;
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility, ItemTag.EquipmentRelated, ItemTag.WorldUnique });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) =>
			"Hold an extra Equipment. Stand still to rummage through the rucksack.";
		protected override string GetDescString(string langid = null) =>
			$"Gain 1 extra Equipment slot <style=cStack>(+1 per stack)</style>. Standing still for more than {moveGracePeriod:N1} second{NPlur(moveGracePeriod)} causes you to cycle through Equipment slots once per {cyclePeriod:N1} second{NPlur(cyclePeriod)}.";
		protected override string GetLoreString(string langid = null) => "";



		////// Config //////

		[AutoConfigRoOSlider("{0:N1} s", 0f, 5f)]
		[AutoConfig("Time required to register a movement stop, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float moveGracePeriod { get; private set; } = 1f;

		[AutoConfigRoOSlider("{0:N1} s", 0f, 5f)]
		[AutoConfig("Cycle time of extra slots, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float cyclePeriod { get; private set; } = 1f;

		[AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
		[AutoConfig("Chance to replace a drop from a Scavenger backpack with this item.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float dropChance { get; private set; } = 0.03f;



        ////// Other Fields/Properties //////

        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public ExtraEquipment() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ExtraEquipment.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/extraEquipmentIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/ExtraEquipment.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            modelResource.transform.Find("ExtraEquipment").gameObject.GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Scav/matScavBackpack.mat").WaitForCompletion();
            modelResource.transform.Find("ExtraEquipment").gameObject.GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Scav/matScavBackpack.mat").WaitForCompletion();
            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.04417F, 0.19067F, -0.24033F),
                localAngles = new Vector3(337.4471F, 55.56866F, 354.1383F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00054F, 0.27487F, -0.29389F),
                localAngles = new Vector3(320.018F, 64.74491F, 342.704F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.01826F, 0.41296F, -0.21866F),
                localAngles = new Vector3(6.28242F, 43.10916F, 36.10896F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "SpineChest3",
                localPos = new Vector3(-0.08684F, 0.67153F, -1.08192F),
                localAngles = new Vector3(44.63957F, 264.7216F, 107.5511F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.01093F, 0.05395F, -0.36182F),
                localAngles = new Vector3(314.4274F, 93.80039F, 295.7014F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.15785F, 0.13082F, -0.11723F),
                localAngles = new Vector3(322.6137F, 19.11888F, 332.5494F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.0131F, -0.01474F, -0.22271F),
                localAngles = new Vector3(328.3462F, 59.25051F, 349.7125F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.00568F, 0.22235F, -0.35905F),
                localAngles = new Vector3(334.1837F, 59.43953F, 3.43586F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00072F, 0.34057F, -0.30971F),
                localAngles = new Vector3(345.0132F, 50.15996F, 12.58943F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.1298F, 1.52182F, -2.26367F),
                localAngles = new Vector3(320.2272F, 71.04354F, 329.5704F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(1.01798F, 0.30345F, -0.23827F),
                localAngles = new Vector3(320.4994F, 321.4309F, 4.27371F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.12059F, 0.06472F, -0.16892F),
                localAngles = new Vector3(321.9678F, 54.92611F, 353.519F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.00089F, 0.05696F, -0.30533F),
                localAngles = new Vector3(318.8723F, 56.99937F, 349.1709F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            #endregion
        }

        public override void SetupAttributes() {
			base.SetupAttributes();
		}

		public override void Install() {
			base.Install();
			CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.ChestBehavior.RollItem += ChestBehavior_RollItem;
		}

        public override void Uninstall() {
			base.Uninstall();
			CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
			On.RoR2.ChestBehavior.RollItem -= ChestBehavior_RollItem;
		}



		////// Hooks //////

		private void ChestBehavior_RollItem(On.RoR2.ChestBehavior.orig_RollItem orig, ChestBehavior self) {
			orig(self);
			if(self.gameObject.name == "ScavBackpack(Clone)" && rng.nextNormalizedFloat < dropChance) {
				self.dropPickup = pickupIndex;
			}
		}

		private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
			var hasItem = GetCount(body) > 0;
			var component = body.GetComponent<ExtraEquipmentTracker>();
			if(hasItem && !component)
				component = body.gameObject.AddComponent<ExtraEquipmentTracker>();
			if(component)
				component.CheckCount();
		}
	}

	[RequireComponent(typeof(CharacterBody))]
	public class ExtraEquipmentTracker : MonoBehaviour {
		int trackedExtraSlotCount = 0;

		float stationaryStopwatch = 0f;
		float shuffleStopwatch = 0f;
		bool isStopped = false;

		Vector3 prevPos;

		CharacterBody body;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void Awake() {
			body = GetComponent<CharacterBody>();
			prevPos = body.transform.position;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void FixedUpdate() {
			if(!body || !NetworkServer.active) return;
			float minMove = 0.1f * Time.fixedDeltaTime;
			if((body.transform.position - prevPos).sqrMagnitude <= minMove * minMove) {
				if(!isStopped) {
					stationaryStopwatch += Time.fixedDeltaTime;
					if(stationaryStopwatch > ExtraEquipment.instance.moveGracePeriod)
						isStopped = true;
				} else {
					shuffleStopwatch += Time.fixedDeltaTime;
					if(shuffleStopwatch >= ExtraEquipment.instance.cyclePeriod) {
						shuffleStopwatch = 0f;
						body.inventory.SetActiveEquipmentSlot((byte)((body.inventory.activeEquipmentSlot + 1) % Mathf.Min(256, body.inventory.GetEquipmentSlotCount())));
					}
				}
			} else if(!isStopped) {
				isStopped = false;
				stationaryStopwatch = 0f;
				shuffleStopwatch = 0f;
			}

			prevPos = body.transform.position;
        }

		public void CheckCount() {
			var count = Mathf.Min(ExtraEquipment.instance.GetCount(body), ExtraEquipment.MAX_STACKS);

			if(count == 0) {
				HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.inventory.equipmentStateSlots, body.inventory.GetEquipmentSlotCount() - trackedExtraSlotCount, trackedExtraSlotCount);
				Destroy(this);
			} else if(count > trackedExtraSlotCount) {
				while(trackedExtraSlotCount != count) {
					HG.ArrayUtils.ArrayAppend(ref body.inventory.equipmentStateSlots, new EquipmentState(EquipmentIndex.None, Run.FixedTimeStamp.now, 0));
					trackedExtraSlotCount++;
				}
			} else if(count < trackedExtraSlotCount) {
				while(trackedExtraSlotCount != count) {
					HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.inventory.equipmentStateSlots, body.inventory.GetEquipmentSlotCount() - 1);
					trackedExtraSlotCount--;
				}
			}
		}
    }
}