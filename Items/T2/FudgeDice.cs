using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;

namespace ThinkInvisible.TinkersSatchel {
    public class FudgeDice : Item<FudgeDice> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility });
        public override bool itemIsAIBlacklisted { get; protected set; } = true;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            icd.ToString("N0"), cdrStack.ToString("0%"), boostAmount.ToString()
        };



        ////// Config ///////

        [AutoConfigRoOSlider("{0:P0}", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Base cooldown at first stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float icd { get; private set; } = 20f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplicative internal cooldown reduction per stack past the first.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float cdrStack { get; private set; } = 0.1f;

        [AutoConfigRoOIntSlider("{0:N0}", 0, 100)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of luck to provide per proc.", AutoConfigFlags.PreventNetMismatch, 1, 100)]
        public int boostAmount { get; private set; } = 9;


        ////// Other Fields/Properties //////



        ////// TILER2 Module Setup //////
        public FudgeDice() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/FudgeDice.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/fudgeDiceIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            On.RoR2.Util.CheckRoll_float_float_CharacterMaster += Util_CheckRoll_float_float_CharacterMaster;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.Util.CheckRoll_float_float_CharacterMaster -= Util_CheckRoll_float_float_CharacterMaster;
        }



        ////// Hooks //////

        private bool Util_CheckRoll_float_float_CharacterMaster(On.RoR2.Util.orig_CheckRoll_float_float_CharacterMaster orig, float percentChance, float luck, CharacterMaster effectOriginMaster) {
            var count = GetCount(effectOriginMaster);
            if(count > 0 && percentChance > 0f) {
                var icdCpt = effectOriginMaster.GetComponent<FudgeDiceICD>();
                if(!icdCpt) icdCpt = effectOriginMaster.gameObject.AddComponent<FudgeDiceICD>();
                if(icdCpt.stopwatch <= 0f) {
                    icdCpt.stopwatch = icd * (Mathf.Pow(1f - cdrStack, count - 1));
                    return orig(percentChance, luck + boostAmount, effectOriginMaster);
                }
            }
            return orig(percentChance, luck, effectOriginMaster);
        }
    }

    public class FudgeDiceICD : MonoBehaviour {
        public float stopwatch = 0f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(stopwatch > 0f)
                stopwatch -= Time.fixedDeltaTime;
        }
    }
}