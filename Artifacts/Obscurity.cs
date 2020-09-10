using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using TILER2;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public class Obscurity : Artifact<Obscurity> {
        public override string displayName => "Artifact of Obscurity";

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangDesc(string langid = null) => "Items exchange appearances and names randomly. Watch closely and learn quickly.";

        List<string> randomFirstWords = new List<string>();
        List<string> randomSecondWords = new List<string>();
        List<string> randomThirdWords = new List<string>();
        Dictionary<ItemIndex,ItemIndex> currentMapping = new Dictionary<ItemIndex, ItemIndex>();

        struct CachedItemInfo {
            public string iconPath;
            public string modelPath;
            public string nameToken;
            public string pickupToken;
            public string descToken;
        }
        private Dictionary<ItemIndex, CachedItemInfo> origItemCache = new Dictionary<ItemIndex, CachedItemInfo>();

        public Obscurity() {
            iconPathName = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/danger_on.png";
            iconPathNameDisabled = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/danger_off.png";
            LanguageAPI.Add("TKSAT_SCRAMBLED_PICKUP", "Something about this item is... off.");
            LanguageAPI.Add("TKSAT_SCRAMBLED_DESC", "Something about this item is... off. There's no way to know what it really does until you use it.");
        }

        private void RebuildItems() {
            foreach(var pickup in PickupCatalog.allPickups) {
                if(pickup.interactContextToken != "ITEM_PICKUP_CONTEXT" || pickup.itemIndex >= ItemIndex.Count || pickup.itemIndex < 0) continue;
                var itemDef = ItemCatalog.GetItemDef(pickup.itemIndex);
                pickup.displayPrefab = itemDef.pickupModelPrefab;
                pickup.nameToken = itemDef.nameToken;
                pickup.iconTexture = itemDef.pickupIconTexture;
                pickup.iconSprite = itemDef.pickupIconSprite;
            }
        }

        private void BuildLanguageTables() {
            randomFirstWords.Clear();
            randomSecondWords.Clear();
            randomThirdWords.Clear();
            for(ItemIndex i = 0; i < (ItemIndex)ItemCatalog.itemCount; i++) {
                var idef = ItemCatalog.GetItemDef(i);
                if(idef == null || !idef.inDroppableTier || !Run.instance.availableItems.Contains(i)) continue;
                var wordsSplit = Language.GetString(idef.nameToken).Split(' ');
                if(wordsSplit.Length > 0)
                    randomFirstWords.Add(wordsSplit[0]);
                if(wordsSplit.Length > 1)
                    randomSecondWords.Add(wordsSplit[1]);
                if(wordsSplit.Length > 2)
                    randomThirdWords.Add(wordsSplit[2]);
            }
        }

        private void Scramble() {
            if(origItemCache.Count > 0) Unscramble();
            List<ItemIndex> itemsToPopulate = new List<ItemIndex>();
            for(ItemIndex i = 0; i < (ItemIndex)ItemCatalog.itemCount; i++) {
                var idef = ItemCatalog.GetItemDef(i);
                if(!idef.inDroppableTier || !Run.instance.availableItems.Contains(i)) continue;
                itemsToPopulate.Add(i);
                CacheItemInfo(i);
                ScrambleText(i);
            }
            while(itemsToPopulate.Count > 1) {
                var sel1 = itemRng.NextElementUniform(itemsToPopulate);
                itemsToPopulate.Remove(sel1);
                var sel2 = itemRng.NextElementUniform(itemsToPopulate);
                itemsToPopulate.Remove(sel2);
                currentMapping[sel1] = sel2;
                ExchangeModels(sel1, sel2);
            }
        }

        private void Unscramble() {
            foreach(var i in origItemCache.Keys)
                RestoreItemInfo(i);
            origItemCache.Clear();
            currentMapping.Clear();
        }

        private void ScrambleText(ItemIndex ind) {
            var def = ItemCatalog.GetItemDef(ind);
            def.pickupToken = "TKSAT_SCRAMBLED_PICKUP";
            def.descriptionToken = "TKSAT_SCRAMBLED_DESC";
            List<string> words = new List<string>();
            var numFirstWords = Mathf.Max(itemRng.RangeInt(1, 4)-1, 1);
            for(var i = 0; i < numFirstWords; i++)
                words.Add(itemRng.NextElementUniform(randomFirstWords));
            var numSecondWords = Mathf.Max(itemRng.RangeInt(1, 4)-1, 0);
            for(var i = 0; i < numSecondWords; i++)
                words.Add(itemRng.NextElementUniform(randomSecondWords));
            if(numSecondWords > 0) {
                var numThirdWords = Mathf.Max(itemRng.RangeInt(2, 7)-4, 0);
                for(var i = 0; i < numThirdWords; i++)
                    words.Add(itemRng.NextElementUniform(randomThirdWords));
            }
            def.nameToken = string.Join(" ", words.ToArray());
        }

        private void ExchangeModels(ItemIndex ind1, ItemIndex ind2) {
            var def1 = ItemCatalog.GetItemDef(ind1);
            var def2 = ItemCatalog.GetItemDef(ind2);
            var model1 = def1.pickupModelPath;
            var model2 = def2.pickupModelPath;
            def2.pickupModelPath = model1;
            def1.pickupModelPath = model2;
            var icon1 = def1.pickupIconPath;
            var icon2 = def2.pickupIconPath;
            def2.pickupIconPath = icon1;
            def1.pickupIconPath = icon2;
        }

        private void CacheItemInfo(ItemIndex ind) {
            var def = ItemCatalog.GetItemDef(ind);
            origItemCache[ind] = new CachedItemInfo {
                descToken = def.descriptionToken,
                iconPath = def.pickupIconPath,
                modelPath = def.pickupModelPath,
                nameToken = def.nameToken,
                pickupToken = def.pickupToken
            };
        }

        private void RestoreItemInfo(ItemIndex ind) {
            var def = ItemCatalog.GetItemDef(ind);
            var cachedInfo = origItemCache[ind];
            def.descriptionToken = cachedInfo.descToken;
            def.pickupToken = cachedInfo.pickupToken;
            def.nameToken = cachedInfo.nameToken;
            def.pickupIconPath = cachedInfo.iconPath;
            def.pickupModelPath = cachedInfo.modelPath;
        }

        protected override void LoadBehavior() {
            On.RoR2.Run.Start += Run_Start;
            On.RoR2.Run.OnDestroy += Run_OnDestroy;
            On.RoR2.CharacterModel.EnableItemDisplay += CharacterModel_EnableItemDisplay;
            On.RoR2.CharacterModel.DisableItemDisplay += CharacterModel_DisableItemDisplay;
        }
        protected override void UnloadBehavior() {
            On.RoR2.Run.Start -= Run_Start;
            On.RoR2.Run.OnDestroy -= Run_OnDestroy;
            On.RoR2.CharacterModel.EnableItemDisplay -= CharacterModel_EnableItemDisplay;
            On.RoR2.CharacterModel.DisableItemDisplay -= CharacterModel_DisableItemDisplay;
        }

        private void CharacterModel_EnableItemDisplay(On.RoR2.CharacterModel.orig_EnableItemDisplay orig, CharacterModel self, ItemIndex itemIndex) {
            if(IsActiveAndEnabled() && currentMapping.ContainsKey(itemIndex))
                itemIndex = currentMapping[itemIndex];
            orig(self,itemIndex);
        }
        
        private void CharacterModel_DisableItemDisplay(On.RoR2.CharacterModel.orig_DisableItemDisplay orig, CharacterModel self, ItemIndex itemIndex) {
            if(IsActiveAndEnabled() && currentMapping.ContainsKey(itemIndex))
                itemIndex = currentMapping[itemIndex];
            orig(self,itemIndex);
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self) {
            orig(self);
            if(IsActiveAndEnabled()) {
                BuildLanguageTables();
                Scramble();
                RebuildItems();
            }
        }

        private void Run_OnDestroy(On.RoR2.Run.orig_OnDestroy orig, Run self) {
            orig(self);
            if(IsActiveAndEnabled()) {
                Unscramble();
                RebuildItems();
            }
        }
    }
}