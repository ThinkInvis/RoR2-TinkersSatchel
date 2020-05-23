using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class Mimic : Item<Mimic> {
        public override string displayName => "Mostly-Tame Mimic";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Utility});

        [AutoItemConfig("Time between batches of re-mimics.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float decayRate {get; private set;} = 3f;

        [AutoItemConfig("Chance for each individual Mimic to re-mimic per proc.", AutoItemConfigFlags.None, 0f, 1f)]
        public float decayChance {get; private set;} = 0.15f;
        
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
            On.RoR2.UI.ItemInventoryDisplay.UpdateDisplay += On_IIDUpdateDisplay;
        }

        protected override void UnloadBehavior() {
            On.RoR2.Inventory.GiveItem -= On_InvGiveItem;
            On.RoR2.Inventory.RemoveItem -= On_InvRemoveItem;
            On.RoR2.UI.ItemInventoryDisplay.UpdateDisplay -= On_IIDUpdateDisplay;
        }
        
        private void On_InvGiveItem(On.RoR2.Inventory.orig_GiveItem orig, Inventory self, ItemIndex itemIndex, int count) {
            if(count <= 0 || MimicInventory.internalOpInProgress) {
                orig(self, itemIndex, count);
                return;
            }
            var minv = self.gameObject.GetComponent<MimicInventory>();
            if(itemIndex == regIndex) {
                if(!minv) {
                    minv = self.gameObject.AddComponent<MimicInventory>();
                    minv.ownerInventory = self;
                }
                
                var iarr = (int[])typeof(Inventory).GetFieldCached("itemStacks").GetValue(self);
                var iarrSel = iarr.Select((val,ind) => new KeyValuePair<int,int>(ind,val)).Where(x=>x.Value>0 && x.Key != (int)regIndex).ToArray();
                if(iarrSel.Length == 0) {
                    orig(self, itemIndex, count);
                    minv.nothingToMimic = true;
                } else {
                    minv.nothingToMimic = false;
                    for(int i = 0; i < count; i++)
                        minv.AddMimic(iarrSel);
                }
            } else {
                orig(self, itemIndex, count);
                if(minv && minv.nothingToMimic) {
                    minv.nothingToMimic = false;
                    var numMimics = GetCount(self);
                    self.RemoveItem(regIndex, numMimics);
                    minv.AddMimic(itemIndex, numMimics);
                }
            }
        }

        private void On_InvRemoveItem(On.RoR2.Inventory.orig_RemoveItem orig, Inventory self, ItemIndex itemIndex, int count) {
            int origCount = self.GetItemCount(itemIndex);
            orig(self, itemIndex, count);
            if(MimicInventory.internalOpInProgress) return;
            if(count > 0 && itemIndex != regIndex) {
                var minv = self.gameObject.GetComponent<MimicInventory>();
                if(!minv) return;
                minv.MaybeRemoveMimics(itemIndex, count, origCount);
            }
        }

        private void On_IIDUpdateDisplay(On.RoR2.UI.ItemInventoryDisplay.orig_UpdateDisplay orig, RoR2.UI.ItemInventoryDisplay self) {
            orig(self);
            Inventory inv = (Inventory)typeof(RoR2.UI.ItemInventoryDisplay).GetFieldCached("inventory").GetValue(self);
            var mim = inv?.gameObject.GetComponent<MimicInventory>();
            if(mim) {
                List<RoR2.UI.ItemIcon> icons = (List<RoR2.UI.ItemIcon>)typeof(RoR2.UI.ItemInventoryDisplay).GetFieldCached("itemIcons").GetValue(self);
                foreach(var icon in icons) {
                    ItemIndex ind = (ItemIndex)typeof(RoR2.UI.ItemIcon).GetFieldCached("itemIndex").GetValue(icon);
                    var mimTextPfx = "\n<color=#C18FE0>+";
                    //strip original mimic text, if any
                    var origInd = icon.stackText.text.IndexOf(mimTextPfx);
                    if(origInd >= 0)
                        icon.stackText.text = icon.stackText.text.Substring(0, origInd);

                    if(!mim.mimickedCounts.ContainsKey(ind)) continue;
                    
                    //add new mimic text
                    int count = (int)typeof(RoR2.UI.ItemIcon).GetFieldCached("itemCount").GetValue(icon);
                    icon.SetItemIndex(ind, Mathf.Max(count-mim.mimickedCounts[ind], 0));
                    var mimText = mimTextPfx + mim.mimickedCounts[ind] + "</color>";
                    if(!icon.stackText.enabled) {
                        icon.stackText.enabled = true;
                        icon.stackText.text = ((count == mim.mimickedCounts[ind]) ? "0" : "") + mimText;
                    } else {
                        icon.stackText.text += mimText;
                    }
                }
            }
        }
	}

    public class MimicInventory : MonoBehaviour {
        private float stopwatch = 0f;
        private readonly Dictionary<ItemIndex, int> _mimickedCounts;
        public readonly ReadOnlyDictionary<ItemIndex, int> mimickedCounts;
        internal int totalMimics = 0;
        internal bool nothingToMimic = false;
        internal Inventory ownerInventory;

        public static bool internalOpInProgress {get; private set;} = false;

        public MimicInventory() {
            _mimickedCounts = new Dictionary<ItemIndex, int>();
            mimickedCounts = new ReadOnlyDictionary<ItemIndex, int>(_mimickedCounts);
        }

        private void FixedUpdate() {
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                stopwatch = Mimic.instance.decayRate;
                Shuffle();
            }
        }

        private void Shuffle() {
            var iarr = (int[])typeof(Inventory).GetFieldCached("itemStacks").GetValue(ownerInventory);
            var iarrSel = iarr.Select((val,ind) => new KeyValuePair<int,int>(ind,val)).Where(x=>x.Value>0 && x.Key != (int)Mimic.instance.regIndex).ToArray();
            if(iarrSel.Length <= 1) return;
            internalOpInProgress = true;
            //int countToShuffle = Mathf.Min(count, Mathf.FloorToInt(Mimic.instance.mimicRng.nextNormalizedFloat * count * Mimic.instance.decayChance * 2f));)
            int totalChanged = 0;
            for(int i = 0; i < totalMimics; i++) {
                if(Mimic.instance.itemRng.nextNormalizedFloat > Mimic.instance.decayChance) continue;
                totalChanged++;
                
                var toRemove = Mimic.instance.itemRng.NextElementUniform(_mimickedCounts.Keys.ToArray());
                var toAdd = (ItemIndex)Mimic.instance.itemRng.NextElementUniform(iarrSel.Where(x => x.Key != (int)toRemove).ToArray()).Key;

                if(!_mimickedCounts.ContainsKey(toAdd)) _mimickedCounts.Add(toAdd, 1);
                else _mimickedCounts[toAdd] ++;
                _mimickedCounts[toRemove] --;
                if(_mimickedCounts[toRemove] < 1) _mimickedCounts.Remove(toRemove);
                ownerInventory.GiveItem(toAdd);
                ownerInventory.RemoveItem(toRemove);

                if(totalChanged > Mimic.instance.lagLimit) break;
            }
            internalOpInProgress = false;
        }

        internal void RemoveMimic() {
            if(_mimickedCounts.Keys.Count < 1) return;
            var toRemove = Mimic.instance.itemRng.NextElementUniform(_mimickedCounts.Keys.ToArray());
            _mimickedCounts[toRemove] --;
            totalMimics--;
            if(_mimickedCounts[toRemove] < 1) _mimickedCounts.Remove(toRemove);
            internalOpInProgress = true;
            ownerInventory.RemoveItem(toRemove);
            internalOpInProgress = false;
        }
        internal bool AddMimic(KeyValuePair<int,int>[] iarrSel) {
            var toAdd = (ItemIndex)Mimic.instance.itemRng.NextElementUniform(iarrSel).Key;
            if(!_mimickedCounts.ContainsKey(toAdd)) _mimickedCounts.Add(toAdd, 1);
            else _mimickedCounts[toAdd] ++;
            totalMimics++;
            internalOpInProgress = true;
            ownerInventory.GiveItem(toAdd);
            internalOpInProgress = false;
            return true;
        }
        internal void AddMimic(ItemIndex addedIndex, int addedCount) {
            if(!_mimickedCounts.ContainsKey(addedIndex)) _mimickedCounts.Add(addedIndex, addedCount);
            else _mimickedCounts[addedIndex] += addedCount;
            totalMimics += addedCount;
            internalOpInProgress = true;
            ownerInventory.GiveItem(addedIndex, addedCount);
            internalOpInProgress = false;
        }
        internal void MaybeRemoveMimics(ItemIndex removedIndex, int removedCount, int totalCount) {
            if(!_mimickedCounts.ContainsKey(removedIndex)) return;
            //TODO: make sure the math here is ok (starts and stops at right points)
            for(int i = totalCount; i > totalCount - removedCount; i--) {
                float removeChance = 100f*_mimickedCounts[removedIndex]/i;
                if(Util.CheckRoll(removeChance)) {
                    _mimickedCounts[removedIndex] --;
                    totalMimics--;
                    if(_mimickedCounts[removedIndex] < 1) {
                        _mimickedCounts.Remove(removedIndex);
                        return;
                    }
                }
            }
        }
    }
}