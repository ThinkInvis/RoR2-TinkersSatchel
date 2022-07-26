using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
	public class ExtraEquipment : Item<ExtraEquipment> {

		////// Item Data //////

		public override string displayName => "Scavenger's Rucksack";
		public override ItemTier itemTier => ItemTier.Boss;
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility, ItemTag.EquipmentRelated, ItemTag.WorldUnique });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) =>
			"Hold an extra Equipment. Stand still to rummage through the rucksack.";
		protected override string GetDescString(string langid = null) =>
			$"Gain 1 <style=cIsUtility>extra Equipment slot</style> <style=cStack>(+1 per stack)</style>. Standing still for more than {moveGracePeriod:N1} second{NPlur(moveGracePeriod)} causes you to cycle through Equipment slots once per {cyclePeriod:N1} second{NPlur(cyclePeriod)}.";
		protected override string GetLoreString(string langid = null) => "";



		////// Config //////

		[AutoConfigRoOSlider("{0:N1} s", 0f, 5f)]
		[AutoConfig("Time required to register a movement stop, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float moveGracePeriod { get; private set; } = 2f;

		[AutoConfigRoOSlider("{0:N1} s", 0f, 5f)]
		[AutoConfig("Cycle time of extra slots, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float cyclePeriod { get; private set; } = 1f;

		[AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
		[AutoConfig("Chance to replace a drop from a Scavenger backpack with this item.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float dropChance { get; private set; } = 0.05f;



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
            idrPrefab.transform.Find("ExtraEquipment").gameObject.GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Scav/matScavBackpack.mat").WaitForCompletion();
            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.00847F, 0.10446F, -0.27956F),
                localAngles = new Vector3(60.38528F, 48.27337F, 17.60868F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.04166F, 0.1823F, -0.19643F),
                localAngles = new Vector3(45.86486F, 221.4702F, 357.8421F),
                localScale = new Vector3(0.6F, 0.6F, 0.6F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.06297F, 0.27883F, -0.28509F),
                localAngles = new Vector3(54.64636F, 52.7043F, 22.65946F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "SpineChest3",
                localPos = new Vector3(0.38739F, 2.46798F, -0.85095F),
                localAngles = new Vector3(3.14693F, 333.734F, 145.2841F),
                localScale = new Vector3(4F, 4F, 4F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.01913F, 0.25145F, -0.43333F),
                localAngles = new Vector3(51.93349F, 44.28279F, 10.0549F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.18717F, 0.02162F, -0.18228F),
                localAngles = new Vector3(45.99033F, 343.6589F, 354.9082F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(-0.00788F, 0.04801F, -0.31238F),
                localAngles = new Vector3(44.86855F, 36.96589F, 358.149F),
                localScale = new Vector3(0.6F, 0.6F, 0.6F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.05218F, 0.14768F, -0.49051F),
                localAngles = new Vector3(58.85672F, 64.97396F, 21.87442F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.02525F, 0.03155F, -0.34501F),
                localAngles = new Vector3(43.24475F, 39.96407F, 3.55216F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.31665F, 1.52193F, -3.21407F),
                localAngles = new Vector3(319.1342F, 51.2494F, 78.68367F),
                localScale = new Vector3(4F, 4F, 4F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(0.00002F, 1.01277F, -0.13806F),
                localAngles = new Vector3(34.70179F, 75.86031F, 135.5334F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(-0.05628F, -0.01992F, -0.0987F),
                localAngles = new Vector3(55.49288F, 36.24144F, 4.64347F),
                localScale = new Vector3(0.85F, 0.85F, 0.85F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.01085F, 0.00473F, -0.31864F),
                localAngles = new Vector3(31.0059F, 44.92817F, 358.0923F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            #endregion
        }

        public override void SetupAttributes() {
			base.SetupAttributes();
		}

		public override void Install() {
			base.Install();
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
            On.RoR2.ChestBehavior.RollItem += ChestBehavior_RollItem;
		}

        public override void Uninstall() {
			base.Uninstall();
            On.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
            On.RoR2.ChestBehavior.RollItem -= ChestBehavior_RollItem;
		}



        ////// Hooks //////

        private void ChestBehavior_RollItem(On.RoR2.ChestBehavior.orig_RollItem orig, ChestBehavior self) {
			orig(self);
			if(self.gameObject.name == "ScavBackpack(Clone)" && rng.nextNormalizedFloat < dropChance) {
				self.dropPickup = pickupIndex;
			}
        }

        private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self) {
            orig(self);
            var hasItem = GetCount(self) > 0;
            var component = self.gameObject.GetComponent<ExtraEquipmentStash>();
            if(hasItem && !component)
                component = self.gameObject.AddComponent<ExtraEquipmentStash>();
            if(component)
                component.CheckCount();
        }
	}

	[RequireComponent(typeof(CharacterMaster))]
	public class ExtraEquipmentStash : MonoBehaviour {
        Queue<EquipmentState> stashedEquipment = new();

		float stationaryStopwatch = 0f;
		float shuffleStopwatch = 0f;
		bool isStopped = false;

		Vector3 prevPos;

		CharacterMaster master;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void Awake() {
			master = GetComponent<CharacterMaster>();
            if(master.hasBody)
                prevPos = master.GetBodyObject().transform.position;
            else prevPos = Vector3.zero;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void FixedUpdate() {
			if(!master || !master.hasBody || !NetworkServer.active) return;
            var currPos = master.GetBodyObject().transform.position;
			float minMove = 0.1f * Time.fixedDeltaTime;
			if((currPos - prevPos).sqrMagnitude <= minMove * minMove) {
				if(!isStopped) {
					stationaryStopwatch += Time.fixedDeltaTime;
					if(stationaryStopwatch > ExtraEquipment.instance.moveGracePeriod)
						isStopped = true;
				} else {
					shuffleStopwatch += Time.fixedDeltaTime;
					if(shuffleStopwatch >= ExtraEquipment.instance.cyclePeriod) {
						shuffleStopwatch = 0f;
                        stashedEquipment.Enqueue(master.inventory.currentEquipmentState);
                        master.inventory.SetEquipment(stashedEquipment.Dequeue(), master.inventory.activeEquipmentSlot);
					}
				}
			} else if(!isStopped) {
				isStopped = false;
				stationaryStopwatch = 0f;
				shuffleStopwatch = 0f;
			}

			prevPos = currPos;
        }

		public void CheckCount() {
			var count = ExtraEquipment.instance.GetCount(master);

            while(stashedEquipment.Count < count) {
                stashedEquipment.Enqueue(EquipmentState.empty);
            }
            while(stashedEquipment.Count > count) {
                var removedEquipment = stashedEquipment.Dequeue();
                if(removedEquipment.equipmentIndex != EquipmentIndex.None && master.hasBody) {
                    var mb = master.GetBody();
                    var ipb = mb.inputBank;
                    var obj = GameObject.Instantiate(GenericPickupController.pickupPrefab, mb.aimOrigin, Quaternion.identity);
                    var gpcComponent = obj.GetComponent<GenericPickupController>();
                    if(gpcComponent) {
                        var pi = PickupCatalog.FindPickupIndex(removedEquipment.equipmentIndex);
                        gpcComponent.NetworkpickupIndex = pi;
                    }

                    var rbdy = obj.GetComponent<Rigidbody>();
                    rbdy.velocity = (ipb ? ipb.aimDirection : mb.transform.forward) * -10f;
                    NetworkServer.Spawn(obj);
                }
            }

			if(count == 0)
				Destroy(this);
		}
    }
}