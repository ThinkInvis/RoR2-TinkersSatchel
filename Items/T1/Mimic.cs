using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class Mimic : Item<Mimic> {

        ////// Item Data //////

        public override string displayName => "Mostly-Tame Mimic";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Utility});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Mimics your other items at random.";
        protected override string GetDescString(string langid = null) => "Picks one of your other items to <style=cIsUtility>mimic</style> <style=cStack>(each stack is tracked separately)</style>. <style=cIsUtility>Every " + decayRate.ToString("N0") + " seconds</style>, the mimic has a <style=cIsUtility>" + Pct(decayChance, 1) + " chance to switch to a new item</style>.";
        protected override string GetLoreString(string langid = null) => "This is getting out of hand.\n\nA couple weeks ago, Jameson figured out that the mimics can be tamed... but only enough to keep them from chomping your fingers off. That was good enough for most of the crew. A few of them also became of the opinion that they're kind of cute -- the ones that aren't trying to kill us planetside, anyway.\n\nSo we've started taking them as pets. Didn't account for one thing until it was too late, though: their mimickry and hiding instincts? <i>No</i> getting rid of those. So... now our ship's full of things which aren't sure exactly which things they should be.\n\nI swear, if my favorite mug starts running away because my coffee's too hot for it <i>one</i> more time....";



        ////// Config //////
        #region Config
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time between batches of re-mimics.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float decayRate { get; private set; } = 3f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Chance for each individual Mimic to re-mimic per proc.", AutoConfigFlags.None, 0f, 1f)]
        public float decayChance { get; private set; } = 0.15f;

        [AutoConfig("No more than this many Mimics (per player) will change at the same time. Used to be for performance reasons which appear to no longer be in play; now kept for posterity.", AutoConfigFlags.None, 0, int.MaxValue)]
        public int lagLimit { get; private set; } = 50;

        [AutoConfig("Relative weight for a Mimic to prefer Tier 1 items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableT1 { get; private set; } = 0.8f;

        [AutoConfig("Relative weight for a Mimic to prefer Tier 2 items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableT2 { get; private set; } = 0.2f;

        [AutoConfig("Relative weight for a Mimic to prefer Tier 3 items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableT3 { get; private set; } = 0.1f;

        [AutoConfig("Relative weight for a Mimic to prefer Boss Tier items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableTB { get; private set; } = 0.05f;

        [AutoConfig("Relative weight for a Mimic to prefer Lunar Tier items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableTL { get; private set; } = 0.05f;

        [AutoConfig("Relative weight for a Mimic to prefer all other items.", AutoConfigFlags.None, 0f, 1f)]
        public float chanceTableTX { get; private set; } = 0f;

        //[AutoItemConfig("If true, will not be added to drop tables; will instead have a chance to replace normal items on pickup. NYI!")]
        //public bool isCamo {get; private set;} = false;
        #endregion



        ////// Other Fields/Properties //////

        public readonly ItemIndex mimicRecalcDummy;

        //todo: expose this, blacklist scrap, key
        internal HashSet<ItemDef> mimicBlacklist = new HashSet<ItemDef>();



        ////// TILER2 Module Setup //////

        public Mimic() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Mimic.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/mimicIcon.png");
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
            if(count <= 0) return;
            var minv = self.gameObject.GetComponent<MimicInventory>();
            if(!minv) minv = self.gameObject.AddComponent<MimicInventory>();
            minv.totalMimics = minv.fakeInv.GetRealItemCount(catalogIndex);
        }

        private void On_InvRemoveItemByIndex(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count) {
            orig(self, itemIndex, count);
            if(count <= 0) return;
            var minv = self.gameObject.GetComponent<MimicInventory>();
            if(!minv) minv = self.gameObject.AddComponent<MimicInventory>();
            if(itemIndex != catalogIndex) {
                if(minv.fakeInv.GetRealItemCount(itemIndex) == 0)
                    minv.Redistribute(itemIndex);
            } else {
                minv.totalMimics = minv.fakeInv.GetRealItemCount(catalogIndex);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void Awake() {
            if(!NetworkServer.active) return;
            inventory = GetComponent<Inventory>();
            fakeInv = GetComponent<FakeInventory>();
            if(!fakeInv) fakeInv = gameObject.AddComponent<FakeInventory>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void FixedUpdate() {
            if(!NetworkServer.active) return;
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                stopwatch = Mimic.instance.decayRate;
                Shuffle();
            }
        }

        private void Shuffle() {
            var iarrSel = GetSelection();
            if(iarrSel.Count <= 1) return;
            //int countToShuffle = Mathf.Min(count, Mathf.FloorToInt(Mimic.instance.mimicRng.nextNormalizedFloat * count * Mimic.instance.decayChance * 2f));)
            int totalToChange = 0;
            for(int i = 0; i < totalMimics; i++) {
                if(Mimic.instance.rng.nextNormalizedFloat > Mimic.instance.decayChance) continue;
                totalToChange++;
                if(totalToChange >= Mimic.instance.lagLimit) break;
            }

            RemoveMimics(totalToChange, true);
            AddMimics(totalToChange, true);
            RebuildDict();
        }

        private WeightedSelection<ItemIndex[]> GetSelection(params ItemIndex[] ignore) {
            var stacks = inventory.itemStacks.Select((val, ind) =>
                new KeyValuePair<ItemDef, int>(ItemCatalog.GetItemDef((ItemIndex)ind), fakeInv.GetRealItemCount((ItemIndex)ind)))
                .Where((x) => {
                    return x.Value > 0
                        && !x.Key.hidden
                        && x.Key.tier != ItemTier.NoTier
                        && !ignore.Contains(x.Key.itemIndex)
                        && !FakeInventory.blacklist.Contains(x.Key)
                        && !Mimic.instance.mimicBlacklist.Contains(x.Key);
                }).Select(x => x.Key).GroupBy(x => x.tier)
                .ToDictionary(x => x.Key, x => x.Select(y => y.itemIndex).ToArray());

            var retv = new WeightedSelection<ItemIndex[]>();
            foreach(var kvp in stacks) {
                if(kvp.Value.Length == 0) continue;

                var weight = Mimic.instance.chanceTableTX;
                if(kvp.Key == ItemTier.Tier1 || kvp.Key == ItemTier.VoidTier1)
                    weight = Mimic.instance.chanceTableT1;
                if(kvp.Key == ItemTier.Tier2 || kvp.Key == ItemTier.VoidTier2)
                    weight = Mimic.instance.chanceTableT2;
                if(kvp.Key == ItemTier.Tier3 || kvp.Key == ItemTier.VoidTier3)
                    weight = Mimic.instance.chanceTableT3;
                if(kvp.Key == ItemTier.Boss || kvp.Key == ItemTier.VoidBoss)
                    weight = Mimic.instance.chanceTableTB;
                if(kvp.Key == ItemTier.Lunar)
                    weight = Mimic.instance.chanceTableTL;

                retv.AddChoice(kvp.Value, weight);
            }
            return retv;
        }

        internal void AddMimics(int count, bool suppressRebuild = false, params ItemIndex[] ignore) {
            var iarrSel = GetSelection(ignore);
            if(iarrSel.Count < 1) return;
            for(int i = 0; i < count; i++) {
                var toAdd = Mimic.instance.rng.NextElementUniform(iarrSel.Evaluate(Mimic.instance.rng.nextNormalizedFloat));
                _mimics.Add(toAdd);

                fakeInv.GiveItem(toAdd);
            }

            if(!suppressRebuild)
                RebuildDict();
        }

        internal void RemoveMimics(int count, bool suppressRebuild = false) {
            var iarrSel = GetSelection();
            var totalRemovals = Mathf.Min(count, totalMimics);
            for(int i = 0; i < totalRemovals; i++) {
                var toRemove = Mimic.instance.rng.NextElementUniform(_mimics);
                _mimics.Remove(toRemove);

                fakeInv.RemoveItem(toRemove);
            }

            if(!suppressRebuild)
                RebuildDict();
        }

        internal void Redistribute(ItemIndex ind) {
            var moved = _mimics.Count(x => x == ind);
            if(moved == 0) return;
            fakeInv.RemoveItem(ind, fakeInv.GetItemCount(ind));
            _mimics.RemoveAll(x => x == ind);
            AddMimics(moved, false, ind);
        }

        private void RebuildDict() {
            _mimickedCounts.Clear();
            foreach(var kvp in _mimics.GroupBy(x => x))
                _mimickedCounts.Add(kvp.Key, kvp.Count());
        }
    }
}