using RoR2;
using UnityEngine;
using TILER2;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ThinkInvisible.TinkersSatchel {
	public class MiscObjectTrackerModule : T2Module<MiscObjectTrackerModule> {
		public override bool managedEnable => false;

		private static List<GameObject> warbanners = new();
		public static readonly ReadOnlyCollection<GameObject> readOnlyWarbanners = new(warbanners);
		private static List<GameObject> randomDamageZones = new();
		public static readonly ReadOnlyCollection<GameObject> readOnlyRandomDamageZones = new(randomDamageZones);
		private static List<GameObject> crippleWards = new();
		public static readonly ReadOnlyCollection<GameObject> readOnlyCrippleWards = new(crippleWards);
		internal static List<GameObject> deskplants = new();
		public static readonly ReadOnlyCollection<GameObject> readOnlyDeskplants = new(deskplants);

		public override void SetupAttributes() {
			base.SetupAttributes();
		}

		public override void SetupBehavior() {
			base.SetupBehavior();

            On.RoR2.BuffWard.OnEnable += BuffWard_OnEnable;
            On.RoR2.BuffWard.OnDisable += BuffWard_OnDisable;
            On.RoR2.HealingWard.Awake += HealingWard_Awake;
        }

        private void HealingWard_Awake(On.RoR2.HealingWard.orig_Awake orig, HealingWard self) {
			orig(self);
			self.gameObject.AddComponent<HealingWardInstanceTracker>();
        }

        private void BuffWard_OnEnable(On.RoR2.BuffWard.orig_OnEnable orig, BuffWard self) {
			orig(self);
			switch(self.gameObject.name) {
				case "WarbannerWard(Clone)":
					warbanners.Add(self.gameObject);
					break;
				case "DamageZoneWard(Clone)":
					randomDamageZones.Add(self.gameObject);
					break;
				case "CrippleWard(Clone)":
					crippleWards.Add(self.gameObject);
					break;
			}
		}

		private void BuffWard_OnDisable(On.RoR2.BuffWard.orig_OnDisable orig, BuffWard self) {
			orig(self);
			switch(self.gameObject.name) {
				case "WarbannerWard(Clone)":
					warbanners.Remove(self.gameObject);
					break;
				case "DamageZoneWard(Clone)":
					randomDamageZones.Remove(self.gameObject);
					break;
				case "CrippleWard(Clone)":
					crippleWards.Remove(self.gameObject);
					break;
			}
		}
    }

	public class HealingWardInstanceTracker : MonoBehaviour {
		void OnEnable() {
			switch(gameObject.name) {
				case "DeskplantWard(Clone)":
					MiscObjectTrackerModule.deskplants.Add(gameObject);
					break;
			}
		}

		void OnDisable() {
			switch(gameObject.name) {
				case "DeskplantWard(Clone)":
					MiscObjectTrackerModule.deskplants.Remove(gameObject);
					break;
			}
		}
    }
}