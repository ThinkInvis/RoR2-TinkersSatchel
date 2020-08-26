using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API.Utils;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class Mimic : Item<Mimic> {
        public override string displayName => "Mostly-Tame Mimic";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Utility});

        [AutoItemConfig("Time between batches of re-mimics.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float decayRate {get; private set;} = 3f;

        [AutoItemConfig("Chance for each individual Mimic to re-mimic per proc.", AutoItemConfigFlags.None, 0f, 1f)]
        public float decayChance {get; private set;} = 0.15f;

        [AutoItemConfig("No more than this many Mimics (per player) will change at the same time. Recommended to keep at a low number (below 100) for performance reasons.", AutoItemConfigFlags.None, 0, int.MaxValue)]
        public int lagLimit {get; private set;} = 50;
        
        //[AutoItemConfig("If true, will not be added to drop tables; will instead have a chance to replace normal items on pickup. NYI!")]
        //public bool isCamo {get; private set;} = false;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Mimics your other items at random.";
        protected override string NewLangDesc(string langid = null) => "Picks one of your other items to <style=cIsUtility>mimic</style> <style=cStack>(each stack is tracked separately)</style>. <style=cIsUtility>Every " + decayRate.ToString("N0") + " seconds</style>, the mimic has a <style=cIsUtility>" + Pct(decayChance, 1) + " chance to switch to a new item</style>.";
        protected override string NewLangLore(string langid = null) => "This is getting out of hand.\n\nA couple weeks ago, Jameson figured out that the mimics can be tamed... but only enough to keep them from chomping your fingers off. That was good enough for most of the crew. A few of them also became of the opinion that they're kind of cute -- the ones that aren't trying to kill us planetside, anyway.\n\nSo we've started taking them as pets. Didn't account for one thing until it was too late, though: their mimickry and hiding instincts? <i>No</i> getting rid of those. So... now our ship's full of things which aren't sure exactly which things they should be.\n\nI swear, if my favorite mug starts running away because my coffee's too hot for it <i>one</i> more time....";

        public readonly ItemIndex mimicRecalcDummy;

        public Mimic() {
            modelPathName = "@TinkersSatchel:Assets/TinkersSatchel/Prefabs/Mimic.prefab";
            iconPathName = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/mimicIcon.png";
        }

        protected override void LoadBehavior() {
            On.RoR2.Inventory.GiveItem += On_InvGiveItem;
            On.RoR2.Inventory.RemoveItem += On_InvRemoveItem;
        }

        protected override void UnloadBehavior() {
            On.RoR2.Inventory.GiveItem -= On_InvGiveItem;
            On.RoR2.Inventory.RemoveItem -= On_InvRemoveItem;
        }
        
        private void On_InvGiveItem(On.RoR2.Inventory.orig_GiveItem orig, Inventory self, ItemIndex itemIndex, int count) {
            orig(self, itemIndex, count);
            if(count <= 0) return;
            var minv = self.gameObject.GetComponent<MimicInventory>();
            if(!minv) minv = self.gameObject.AddComponent<MimicInventory>();
            minv.totalMimics = minv.fakeInv.GetRealItemCount(regIndex);
        }

        private void On_InvRemoveItem(On.RoR2.Inventory.orig_RemoveItem orig, Inventory self, ItemIndex itemIndex, int count) {
            orig(self, itemIndex, count);
            if(count <= 0) return;
            var minv = self.gameObject.GetComponent<MimicInventory>();
            if(!minv) minv = self.gameObject.AddComponent<MimicInventory>();
            if(itemIndex != regIndex) {
                if(minv.fakeInv.GetRealItemCount(itemIndex) == 0)
                    minv.Redistribute(itemIndex);
            } else {
                minv.totalMimics = minv.fakeInv.GetRealItemCount(regIndex);
            }
        }
	}

    [RequireComponent(typeof(Inventory))]
    public class MimicInventory : MonoBehaviour {
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

        private void Awake() {
            inventory = GetComponent<Inventory>();
            fakeInv = GetComponent<FakeInventory>();
            if(!fakeInv) fakeInv = gameObject.AddComponent<FakeInventory>();
        }

        private void FixedUpdate() {
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                stopwatch = Mimic.instance.decayRate;
                Shuffle();
            }
        }

        private void Shuffle() {
            var newTotalMimics = Mimic.instance.GetCount(inventory);
            var iarrSel = GetSelection();
            if(iarrSel.Length <= 1) return;
            //int countToShuffle = Mathf.Min(count, Mathf.FloorToInt(Mimic.instance.mimicRng.nextNormalizedFloat * count * Mimic.instance.decayChance * 2f));)
            int totalChanged = 0;
            ItemIndex lastAdd = (ItemIndex)(-1);
            for(int i = 0; i < _totalMimics; i++) {
                if(Mimic.instance.itemRng.nextNormalizedFloat > Mimic.instance.decayChance) continue;
                totalChanged++;
                
                var toRemove = Mimic.instance.itemRng.NextElementUniform(_mimickedCounts.Keys.ToArray());
                var toAdd = (ItemIndex)Mimic.instance.itemRng.NextElementUniform(iarrSel.Where(x => x.Key != (int)toRemove).ToArray()).Key;

                if(!_mimickedCounts.ContainsKey(toAdd)) _mimickedCounts.Add(toAdd, 1);
                else _mimickedCounts[toAdd] ++;
                _mimickedCounts[toRemove] --;
                if(_mimickedCounts[toRemove] < 1) _mimickedCounts.Remove(toRemove);
                inventory.itemStacks[(int)toAdd] ++;
                inventory.itemStacks[(int)toRemove] --;
                fakeInv.itemStacks[(int)toAdd] ++;
                fakeInv.itemStacks[(int)toRemove] --;
                lastAdd = toAdd;

                if(totalChanged > Mimic.instance.lagLimit) break;
            }
            if(totalChanged > 0) {
                inventory.itemStacks[(int)lastAdd] --;
                fakeInv.itemStacks[(int)lastAdd] --;
                inventory.GiveItem(lastAdd);
                fakeInv.GiveItem(lastAdd);
            }
        }

        private KeyValuePair<int, int>[] GetSelection() {
            return inventory.itemStacks.Select((val,ind) => new KeyValuePair<int,int>(ind,val-fakeInv.itemStacks[ind])).Where(x=>x.Value>0 && x.Key != (int)Mimic.instance.regIndex).ToArray();
        }

        internal void AddMimics(int count) {
            var iarrSel = GetSelection();
            if(iarrSel.Length < 1) return;
            for(int i = 0; i < count; i++) {
                var toAdd = (ItemIndex)Mimic.instance.itemRng.NextElementUniform(iarrSel).Key;
                if(!_mimickedCounts.ContainsKey(toAdd)) _mimickedCounts.Add(toAdd, 1);
                else _mimickedCounts[toAdd] ++;
                _totalMimics++;
                if(i == count - 1) {
                    inventory.GiveItem(toAdd); //only update on last item added to avoid spamming onInventoryUpdate
                    fakeInv.GiveItem(toAdd);
                } else {
                    //would normally update itemAcquisitionOrder here if necessary, but mimics can only ever copy items that the target inventory already has some of
                    inventory.itemStacks[(int)toAdd] ++;
                    fakeInv.itemStacks[(int)toAdd] ++;
                }
            }
        }

        internal void RemoveMimics(int count) {
            var totalRemovals = Mathf.Min(count, _totalMimics);
            for(int i = 0; i < totalRemovals; i++) {
                var toRemove = Mimic.instance.itemRng.NextElementUniform(_mimickedCounts.Keys.ToArray());
                _mimickedCounts[toRemove] --;
                _totalMimics--;
                if(_mimickedCounts[toRemove] < 1) _mimickedCounts.Remove(toRemove);
                if(i == count - 1) {
                    fakeInv.RemoveItem(toRemove);
                    inventory.RemoveItem(toRemove); //only update on last item added to avoid spamming onInventoryUpdate
                } else {
                    fakeInv.itemStacks[(int)toRemove] --;
                    inventory.itemStacks[(int)toRemove] --;
                }
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
                if(i == mimicsToMove - 1) {
                    inventory.RemoveItem(ind); //only update on last item added to avoid spamming onInventoryUpdate
                    fakeInv.RemoveItem(ind);
                } else {
                    inventory.itemStacks[(int)ind] --;
                    fakeInv.itemStacks[(int)ind] --;
                }
            }
            AddMimics(mimicsToMove);
        }
    }
}