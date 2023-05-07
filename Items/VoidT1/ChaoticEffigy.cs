using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using System.Linq;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class ChaoticEffigy : Item<ChaoticEffigy> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.VoidTier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            dropChance.ToString("P0"), dropRange.ToString("N0"), dropDuration.ToString("N0")
        };



        ////// Config ///////
        
        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fractional chance to create a totem on kill per stack, hyperbolic.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float dropChance { get; private set; } = 0.01f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 500f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Range of totem objects.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float dropRange { get; private set; } = 40f;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 60f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Lifetime of totem objects.", AutoConfigFlags.DeferForever, 0f, float.MaxValue)]
        public float dropDuration { get; private set; } = 10f;



        ////// Other Fields/Properties //////

        GameObject totemPrefab;



        ////// TILER2 Module Setup //////
        public ChaoticEffigy() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/VoidMimic.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/voidMimicIcon.png");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
                var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                isp.relationships = new[] {new ItemDef.Pair {
                    itemDef1 = Mimic.instance.itemDef,
                    itemDef2 = itemDef
                }};
                orig(providers.Concat(new[] { isp }).ToArray());
            };

            totemPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/ChaoticEffigyTotem.prefab");

            var ren = totemPrefab.transform.Find("WardRangeScale/WardRangeInd").gameObject.GetComponent<MeshRenderer>();
            ren.material = ItemWard.stockIndicatorPrefab.transform.Find("IndicatorSphere").gameObject.GetComponent<MeshRenderer>().material;
            ren.material.SetTexture("_RemapTex",
                Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampDefault.png")
                .WaitForCompletion());
            ren.material.SetFloat("_AlphaBoost", 0.475f);
            ren.material.SetColor("_CutoffScroll", new Color(0.8f, 0.8f, 0.85f));
            ren.material.SetColor("_RimColor", new Color(0.8f, 0.8f, 0.85f));

            totemPrefab.GetComponent<DestroyOnTimer>().duration = dropDuration;

            PrefabAPI.RegisterNetworkPrefab(totemPrefab);
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }

        public override void Install() {
            base.Install();

            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        public override void Uninstall() {
            base.Uninstall();

            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
        }



        ////// Hooks //////

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj) {
            if(!obj.attackerMaster || !obj.victimBody) return;
            var count = GetCount(obj.attackerMaster);
            if(count <= 0 || !Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(count * dropChance), obj.attackerMaster)) return;
            var table = (obj.victimIsChampion | obj.victimIsBoss) ? Run.instance.largeChestDropTierSelector
                : (obj.victimIsElite ? Run.instance.mediumChestDropTierSelector
                : Run.instance.smallChestDropTierSelector);
            var pind = rng.NextElementUniform(table.Evaluate(rng.nextNormalizedFloat).Where(p => PickupCatalog.GetPickupDef(p).itemIndex != ItemIndex.None).ToArray());
            var inst = GameObject.Instantiate(totemPrefab);
            inst.transform.position = obj.victimBody.corePosition;
            NetworkServer.Spawn(inst);
            inst.GetComponent<ItemWard>().ServerAddItem(PickupCatalog.GetPickupDef(pind).itemIndex);
        }
    }
}