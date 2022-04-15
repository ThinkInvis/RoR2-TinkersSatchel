using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using R2API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
    public class VoidGoldenGear : Item<VoidGoldenGear> {

        ////// Item Data //////

        public override string displayName => "Armor Prism";
        public override ItemTier itemTier => ItemTier.VoidTier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Healing});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Gain massive armor by focusing your item build. <style=cIsVoid>Corrupts all Armor Crystals</style>.";
        protected override string GetDescString(string langid = null) => $"Gain <style=cIsHealing>armor</style> based on your currently held <style=cIsUtility>types of item</style> (fewer is better). Having only Armor Prisms gives <style=cIsHealing>{armorAmtBase:N0} armor</style> <style=cStack>(+{Pct(armorStacking)} per stack, inverse-exponential)</style>; each subsequent item type <style=cIsUtility>reduces armor by {Pct(1f-varietyExp)}</style>. <style=cIsVoid>Corrupts all Armor Crystals</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Armor given at minimum item type variety (1).", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int armorAmtBase { get; private set; } = 500;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Multiplier of armor scaling per additional item type (higher = less penalty).", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float varietyExp { get; private set; } = 0.875f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Exponential multiplier for increased ArmorAmt per stack (higher = more powerful).", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float armorStacking { get; private set; } = 0.25f;



        ////// Other Fields/Properties //////

        public BuffDef voidGoldenGearBuff { get; private set; }

        private static HashSet<int> validItemTypeCache;



        ////// TILER2 Module Setup //////

        public VoidGoldenGear() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/VoidGoldenGear.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/voidGoldenGearIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            voidGoldenGearBuff = ScriptableObject.CreateInstance<BuffDef>();
            voidGoldenGearBuff.buffColor = new Color(0.35f, 0.15f, 0.65f);
            voidGoldenGearBuff.canStack = true;
            voidGoldenGearBuff.isDebuff = false;
            voidGoldenGearBuff.name = "TKSATVoidGoldenGear";
            voidGoldenGearBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffGenericShield.tif")
                .WaitForCompletion();
            ContentAddition.AddBuffDef(voidGoldenGearBuff);

            itemDef.requiredExpansion = RoR2.ExpansionManagement.ExpansionCatalog.expansionDefs.FirstOrDefault(x => x.nameToken == "DLC1_NAME");

            On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
                var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                isp.relationships = new[] {new ItemDef.Pair {
                    itemDef1 = GoldenGear.instance.itemDef,
                    itemDef2 = itemDef
                }};
                orig(providers.Concat(new[] {isp}).ToArray());
            };
        }

        public override void Install() {
            base.Install();

            On.RoR2.CharacterBody.FixedUpdate += On_CBFixedUpdate;
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.CharacterBody.FixedUpdate -= On_CBFixedUpdate;
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }



        ////// Hooks //////

        private void On_CBFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);
            UpdateGGBuff(self);
        }

        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            if(!sender) return;
            var cpt = sender.GetComponent<VoidGoldenGearComponent>();
            if(cpt) args.armorAdd += cpt.calculatedArmorBonus;
        }



        ////// Public Methods //////

        public float CalculateArmor(Inventory inv) {
            var count = GetCount(inv);
            if(count <= 0) return 0;
            var types = GetTotalItemTypes(inv);

            var baseArmor = armorAmtBase * Mathf.Pow(count, armorStacking);
            return baseArmor * Mathf.Pow(varietyExp, types - 1);
        }



        ////// Non-Public Methods //////

        private static int GetTotalItemTypes(Inventory inv) {
            if(validItemTypeCache == null) {
                validItemTypeCache = new HashSet<int>();
                for(var i = 0; i < inv.itemStacks.Length; i++) {
                    var idef = ItemCatalog.GetItemDef((ItemIndex)i);
                    if(idef == null || idef.hidden) continue;
                    var itier = ItemTierCatalog.GetItemTierDef(idef.tier);
                    if(itier != null && itier.isDroppable) validItemTypeCache.Add(i);
                }
            }

            int retv = 0;

            for(var i = 0; i < inv.itemStacks.Length; i++) {
                if(inv.itemStacks[i] > 0 && validItemTypeCache.Contains(i)) retv++;
            }

            return retv;
        }

        void UpdateGGBuff(CharacterBody cb) {
            var cpt = cb.GetComponent<VoidGoldenGearComponent>();
            if(!cpt) cpt = cb.gameObject.AddComponent<VoidGoldenGearComponent>();

            cpt.calculatedArmorBonus = CalculateArmor(cb.inventory);

            var tgtBuffStacks = Mathf.FloorToInt(cpt.calculatedArmorBonus);

            int currBuffStacks = cb.GetBuffCount(voidGoldenGearBuff);
            if(tgtBuffStacks != currBuffStacks)
                cb.SetBuffCount(voidGoldenGearBuff.buffIndex, tgtBuffStacks);
        }
    }

    public class VoidGoldenGearComponent : MonoBehaviour {
        public float calculatedArmorBonus = 0;
    }
}