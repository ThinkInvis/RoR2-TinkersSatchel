using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using UnityEngine.Networking;

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



		////// TILER2 Module Setup //////

		public ExtraEquipment() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ExtraEquipment.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/extraEquipmentIcon.png");
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