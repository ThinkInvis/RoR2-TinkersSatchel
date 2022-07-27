using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class Mimic : Item<Mimic> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Utility});

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            (1f - chanceTableSpikiness).ToString("P0"), scrambleRate.ToString("N0")
        };


        ////// Config //////
        #region Config
        [AutoConfigRoOSlider("{0:N1} s", 0f, 60f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time between batches of re-mimics.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float scrambleRate { get; private set; } = 15f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfig("Linear scalar on chosen item types per tier. At 1, only one item per tier will ever be chosen; at 0, all items will remain valid.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableSpikiness { get; private set; } = 0.8f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfig("Relative weight for a Mimic to prefer Tier 1 items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableT1 { get; private set; } = 0.8f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfig("Relative weight for a Mimic to prefer Tier 2 items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableT2 { get; private set; } = 0.2f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfig("Relative weight for a Mimic to prefer Tier 3 items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableT3 { get; private set; } = 0.1f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfig("Relative weight for a Mimic to prefer Boss Tier items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableTB { get; private set; } = 0.05f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfig("Relative weight for a Mimic to prefer Lunar Tier items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableTL { get; private set; } = 0.05f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfig("Relative weight for a Mimic to prefer all other items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableTX { get; private set; } = 0f;

        //[AutoItemConfig("If true, will not be added to drop tables; will instead have a chance to replace normal items on pickup. NYI!")]
        //public bool isCamo {get; private set;} = false;
        #endregion



        ////// Other Fields/Properties //////

        public readonly ItemIndex mimicRecalcDummy;

        //todo: expose this, blacklist scrap, key
        internal HashSet<ItemDef> mimicBlacklist = new();



        ////// TILER2 Module Setup //////

        public Mimic() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Mimic.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/mimicIcon.png");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();

            FakeInventory.blacklist.Add(itemDef);
            //scrap
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ScrapWhite"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ScrapGreen"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ScrapRed"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ScrapYellow"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ScrapRedSuppressed"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ScrapGreenSuppressed"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ScrapWhiteSuppressed"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/RegeneratingScrap"));
            //consumable items
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/RegeneratingScrap"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ExtraLife"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ExtraLifeVoid"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/FragileDamageBonus"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/HealingPotion"));
            //consumed items
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/RegeneratingScrapConsumed"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ExtraLifeConsumed"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ExtraLifeVoidConsumed"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/FragileDamageBonusConsumed"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/HealingPotionConsumed"));
            //AI summons (bugged)
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/BeetleGland"));
            mimicBlacklist.Add(LegacyResourcesAPI.Load<ItemDef>("ItemDefs/RoboBallBuddy"));
        }

        public override void Install() {
            base.Install();

            On.RoR2.Inventory.GiveItem_ItemIndex_int += On_InvGiveItemByIndex;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int += On_InvRemoveItemByIndex;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.Inventory.GiveItem_ItemIndex_int -= On_InvGiveItemByIndex;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int -= On_InvRemoveItemByIndex;
        }



        ////// Public API //////
        
        public void BlacklistItem(ItemDef def) {
            mimicBlacklist.Add(def);
        }



        ////// Hooks //////

        private void On_InvGiveItemByIndex(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count) {
            orig(self, itemIndex, count);
            if(count <= 0 || itemIndex != catalogIndex || !self || !NetworkServer.active) return;
            var minv = self.gameObject.GetComponent<MimicInventory>();
            if(!minv) {
                minv = self.gameObject.AddComponent<MimicInventory>();
                if(!minv) return; //happens if object is being destroyed this frame
                minv.LocateOrCreateComponentsServer();
            }
            minv.totalMimics = minv.fakeInv.GetRealItemCount(catalogIndex);
        }

        private void On_InvRemoveItemByIndex(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count) {
            orig(self, itemIndex, count);
            if(count <= 0 || !self || !NetworkServer.active) return;
            var minv = self.gameObject.GetComponent<MimicInventory>();
            if(minv) {
                if(itemIndex != catalogIndex) {
                    if(minv.fakeInv.GetRealItemCount(itemIndex) == 0)
                        minv.Shuffle();
                } else {
                    var newCount = minv.fakeInv.GetRealItemCount(catalogIndex);
                    minv.totalMimics = newCount;
                    if(newCount == 0) //lost last stack
                        GameObject.Destroy(minv);
                }
            }
        }
	}

    [RequireComponent(typeof(Inventory))]
    public class MimicInventory : NetworkBehaviour {
        private float stopwatch = 0f;
        private readonly Dictionary<ItemIndex, int> _mimickedCounts;
        public readonly ReadOnlyDictionary<ItemIndex, int> mimickedCounts;

        private readonly List<ItemIndex> _mimics;
        
        private Inventory inventory;
        internal FakeInventory fakeInv;
        WeightedSelection<ItemIndex[]> currentSelection = null;

        public int totalMimics {get => _mimics.Count; internal set {
                if(value < 0) {
                    TinkersSatchelPlugin._logger.LogError($"MimicInventory.totalMimics_set: cannot set total mimics to a negative value ({value}); no changes were made to mimic count.");
                    return;
                }
                if(value > _mimics.Count)
                    AddMimics(value - _mimics.Count);
                else if(value < _mimics.Count)
                    RemoveMimics(_mimics.Count - value);
            }}

        public MimicInventory() {
            _mimickedCounts = new Dictionary<ItemIndex, int>();
            mimickedCounts = new ReadOnlyDictionary<ItemIndex, int>(_mimickedCounts);
            _mimics = new List<ItemIndex>();
        }

        public void LocateOrCreateComponentsServer() {
            if(!NetworkServer.active) return;
            inventory = GetComponent<Inventory>();
            if(!inventory) {
                TinkersSatchelPlugin._logger.LogError($"MimicInventory added to incompatible GameObject \"{this.gameObject.name}\" with no Inventory");
            }
            fakeInv = GetComponent<FakeInventory>();
            if(!fakeInv) fakeInv = gameObject.AddComponent<FakeInventory>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void Awake() {
            LocateOrCreateComponentsServer();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void FixedUpdate() {
            if(!NetworkServer.active) return;
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                stopwatch = Mimic.instance.scrambleRate;
                Shuffle();
            }
        }

        internal void Shuffle() {
            BuildSelection();
            totalMimics = 0;
            totalMimics = fakeInv.GetRealItemCount(Mimic.instance.catalogIndex);
        }

        private void BuildSelection() {
            var stacks = inventory.itemStacks.Select((val, ind) =>
                new KeyValuePair<ItemDef, int>(ItemCatalog.GetItemDef((ItemIndex)ind), fakeInv.GetRealItemCount((ItemIndex)ind)))
                .Where((x) => {
                    return x.Value > 0
                        && !x.Key.hidden
                        && x.Key.tier != ItemTier.NoTier
                        && !FakeInventory.blacklist.Contains(x.Key)
                        && !Mimic.instance.mimicBlacklist.Contains(x.Key);
                })
                .Select(x => x.Key)
                .GroupBy(x => x.tier)
                .ToDictionary(x => x.Key, x => x
                    .OrderBy(n => Mimic.instance.rng.nextNormalizedFloat)
                    .Take(Mathf.Max(Mathf.CeilToInt(x.Count() * (1f - Mimic.instance.chanceTableSpikiness)),1))
                    .Select(y => y.itemIndex)
                    .ToArray());
            var retv = new WeightedSelection<ItemIndex[]>();
            foreach(var kvp in stacks) {
                if(kvp.Value.Length == 0) continue;
                var weight = kvp.Key switch {
                    ItemTier.Tier1 or ItemTier.VoidTier1 => Mimic.instance.chanceTableT1,
                    ItemTier.Tier2 or ItemTier.VoidTier2 => Mimic.instance.chanceTableT2,
                    ItemTier.Tier3 or ItemTier.VoidTier3 => Mimic.instance.chanceTableT3,
                    ItemTier.Boss or ItemTier.VoidBoss => Mimic.instance.chanceTableTB,
                    ItemTier.Lunar => Mimic.instance.chanceTableTL,
                    _ => Mimic.instance.chanceTableTX
                };
                retv.AddChoice(kvp.Value, weight);
            }
            currentSelection = retv;
        }

        internal void AddMimics(int count) {
            if(currentSelection == null) BuildSelection();
            if(currentSelection.Count == 0) return;
            for(int i = 0; i < count; i++) {
                var toAdd = Mimic.instance.rng.NextElementUniform(currentSelection.Evaluate(Mimic.instance.rng.nextNormalizedFloat));
                _mimics.Add(toAdd);
                fakeInv.GiveItem(toAdd);
            }
            RebuildDict();
        }

        internal void RemoveMimics(int count) {
            var totalRemovals = Mathf.Min(count, totalMimics);
            for(int i = 0; i < totalRemovals; i++) {
                var toRemove = Mimic.instance.rng.NextElementUniform(_mimics);
                _mimics.Remove(toRemove);
                fakeInv.RemoveItem(toRemove);
            }
            RebuildDict();
        }

        private void RebuildDict() {
            _mimickedCounts.Clear();
            foreach(var kvp in _mimics.GroupBy(x => x))
                _mimickedCounts.Add(kvp.Key, kvp.Count());
        }
    }
}