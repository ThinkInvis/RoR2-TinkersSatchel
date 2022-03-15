using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API.Utils;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class Mimic : Item<Mimic> {
        public override string displayName => "Mostly-Tame Mimic";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Utility});

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time between batches of re-mimics.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float decayRate {get; private set;} = 3f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Chance for each individual Mimic to re-mimic per proc.", AutoConfigFlags.None, 0f, 1f)]
        public float decayChance {get; private set;} = 0.15f;

        [AutoConfig("No more than this many Mimics (per player) will change at the same time. Recommended to keep at a low number (below 100) for performance reasons.", AutoConfigFlags.None, 0, int.MaxValue)]
        public int lagLimit {get; private set;} = 50;

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

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Mimics your other items at random.";
        protected override string GetDescString(string langid = null) => "Picks one of your other items to <style=cIsUtility>mimic</style> <style=cStack>(each stack is tracked separately)</style>. <style=cIsUtility>Every " + decayRate.ToString("N0") + " seconds</style>, the mimic has a <style=cIsUtility>" + Pct(decayChance, 1) + " chance to switch to a new item</style>.";
        protected override string GetLoreString(string langid = null) => "This is getting out of hand.\n\nA couple weeks ago, Jameson figured out that the mimics can be tamed... but only enough to keep them from chomping your fingers off. That was good enough for most of the crew. A few of them also became of the opinion that they're kind of cute -- the ones that aren't trying to kill us planetside, anyway.\n\nSo we've started taking them as pets. Didn't account for one thing until it was too late, though: their mimickry and hiding instincts? <i>No</i> getting rid of those. So... now our ship's full of things which aren't sure exactly which things they should be.\n\nI swear, if my favorite mug starts running away because my coffee's too hot for it <i>one</i> more time....";

        public readonly ItemIndex mimicRecalcDummy;

        //todo: expose this
        internal HashSet<ItemDef> mimicBlacklist = new HashSet<ItemDef>();

        public Mimic() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Mimic.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/mimicIcon.png");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();

            FakeInventory.blacklist.Add(itemDef);
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
        
        private Inventory inventory;
        internal FakeInventory fakeInv;

        private int _totalMimics = 0;
        public int totalMimics {get => _totalMimics; internal set {
                if(value > _totalMimics)
                    AddMimics(value - _totalMimics);
                else if(value < _totalMimics)
                    RemoveMimics(_totalMimics - value);
            }}

        public MimicInventory() {
            _mimickedCounts = new Dictionary<ItemIndex, int>();
            mimickedCounts = new ReadOnlyDictionary<ItemIndex, int>(_mimickedCounts);
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
            var newTotalMimics = Mimic.instance.GetCount(inventory);
            var iarrSel = GetSelection();
            if(iarrSel.Count <= 1) return;
            //int countToShuffle = Mathf.Min(count, Mathf.FloorToInt(Mimic.instance.mimicRng.nextNormalizedFloat * count * Mimic.instance.decayChance * 2f));)
            int totalChanged = 0;
            for(int i = 0; i < _totalMimics; i++) {
                if(Mimic.instance.rng.nextNormalizedFloat > Mimic.instance.decayChance) continue;
                totalChanged++;
                
                var toRemove = Mimic.instance.rng.NextElementUniform(_mimickedCounts.Keys.ToArray());
                var addOpts = iarrSel.Evaluate(Mimic.instance.rng.nextNormalizedFloat)
                    .Where(x => x != toRemove)
                    .ToArray();
                if(addOpts.Length == 0) continue;
                var toAdd = Mimic.instance.rng.NextElementUniform(addOpts);

                if(!_mimickedCounts.ContainsKey(toAdd)) _mimickedCounts.Add(toAdd, 1);
                else _mimickedCounts[toAdd] ++;
                _mimickedCounts[toRemove] --;
                if(_mimickedCounts[toRemove] < 1) _mimickedCounts.Remove(toRemove);
                fakeInv.GiveItem(toAdd);
                fakeInv.RemoveItem(toRemove);

                if(totalChanged > Mimic.instance.lagLimit) break;
            }
        }

        private WeightedSelection<ItemIndex[]> GetSelection() {
            var stacks = inventory.itemStacks.Select((val, ind) =>
                new KeyValuePair<ItemDef, int>(ItemCatalog.GetItemDef((ItemIndex)ind), fakeInv.GetRealItemCount((ItemIndex)ind)))
                .Where((x) => {
                    return x.Value > 0
                        && !x.Key.hidden
                        && x.Key.tier != ItemTier.NoTier
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

        internal void AddMimics(int count) {
            var iarrSel = GetSelection();
            if(iarrSel.Count < 1) return;
            for(int i = 0; i < count; i++) {
                var toAdd = Mimic.instance.rng.NextElementUniform(iarrSel.Evaluate(Mimic.instance.rng.nextNormalizedFloat));
                if(!_mimickedCounts.ContainsKey(toAdd)) _mimickedCounts.Add(toAdd, 1);
                else _mimickedCounts[toAdd] ++;
                _totalMimics++;

                fakeInv.GiveItem(toAdd);
            }
        }

        internal void RemoveMimics(int count) {
            var totalRemovals = Mathf.Min(count, _totalMimics);
            for(int i = 0; i < totalRemovals; i++) {
                var toRemove = Mimic.instance.rng.NextElementUniform(_mimickedCounts.Keys.ToArray());
                _mimickedCounts[toRemove] --;
                _totalMimics--;
                if(_mimickedCounts[toRemove] < 1) _mimickedCounts.Remove(toRemove);

                fakeInv.RemoveItem(toRemove);
            }
        }
        
        internal void Redistribute(ItemIndex ind) {
            if(!_mimickedCounts.ContainsKey(ind)) return;
            else if(_mimickedCounts.Count == 1) {
                RemoveMimics(_totalMimics);
                return;
            }
            var mimicsToMove = _mimickedCounts[ind];
            for(int i = 0; i < mimicsToMove; i++) {
                _mimickedCounts[ind] --;
                _totalMimics--;
                if(_mimickedCounts[ind] < 1) _mimickedCounts.Remove(ind);

                fakeInv.RemoveItem(ind);
            }
            AddMimics(mimicsToMove);
        }
    }
}