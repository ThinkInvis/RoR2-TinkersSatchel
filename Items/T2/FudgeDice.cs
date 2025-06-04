using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using System.Linq;

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

        [AutoConfigRoOIntSlider("{0:N0}", 1, 100)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of luck to provide per proc. Should be treated as value+1 due to the effect including a non-luck reroll.", AutoConfigFlags.PreventNetMismatch, 1, 100)]
        public int boostAmount { get; private set; } = 2;


        ////// Other Fields/Properties //////

        internal BuffDef readyBuff;
        internal BuffDef consumedBuff;
        internal GameObject cloverEffect;


        ////// TILER2 Module Setup //////
        public FudgeDice() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/FudgeDice.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/fudgeDiceIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            readyBuff = ScriptableObject.CreateInstance<BuffDef>();
            readyBuff.buffColor = Color.white;
            readyBuff.canStack = false;
            readyBuff.isDebuff = false;
            readyBuff.isCooldown = true;
            readyBuff.name = modInfo.shortIdentifier + "FudgeDiceReady";
            readyBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/fudgeDiceBuffIcon.png");
            ContentAddition.AddBuffDef(readyBuff);

            consumedBuff = ScriptableObject.CreateInstance<BuffDef>();
            consumedBuff.buffColor = Color.gray;
            consumedBuff.canStack = false;
            consumedBuff.isDebuff = false;
            consumedBuff.isCooldown = true;
            consumedBuff.name = modInfo.shortIdentifier + "FudgeDiceConsumed";
            consumedBuff.ignoreGrowthNectar = true;
            consumedBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/fudgeDiceBuffConsumedIcon.png");
            ContentAddition.AddBuffDef(consumedBuff);

            cloverEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Clover/CloverEffect.prefab").WaitForCompletion();
        }

        public override void Install() {
            base.Install();
            On.RoR2.Util.CheckRoll_float_float_CharacterMaster += Util_CheckRoll_float_float_CharacterMaster;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.Util.CheckRoll_float_float_CharacterMaster -= Util_CheckRoll_float_float_CharacterMaster;
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
        }



        ////// Hooks //////

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(body && body.master && GetCount(body) > 0 && !body.master.gameObject.GetComponent<FudgeDiceICD>())
                body.master.gameObject.AddComponent<FudgeDiceICD>();
        }

        private bool Util_CheckRoll_float_float_CharacterMaster(On.RoR2.Util.orig_CheckRoll_float_float_CharacterMaster orig, float percentChance, float luck, CharacterMaster effectOriginMaster) {
            var count = GetCount(effectOriginMaster);
            if(count > 0 && percentChance > 0f) {
                var icdCpt = effectOriginMaster.GetComponent<FudgeDiceICD>();
                if(!icdCpt) icdCpt = effectOriginMaster.gameObject.AddComponent<FudgeDiceICD>();
                if(icdCpt.stopwatch <= 0f) {
                    var firstRoll = orig(percentChance, luck, effectOriginMaster);
                    if(firstRoll) return firstRoll;
                    var reroll = orig(percentChance, luck + boostAmount, effectOriginMaster);
                    if(reroll) {
                        icdCpt.stopwatch = icd * (Mathf.Pow(1f - cdrStack, count - 1));
                        icdCpt.wasLucky = true;
                    }
                    return reroll;
                }
            }
            return orig(percentChance, luck, effectOriginMaster);
        }
    }

    public class FudgeDiceICD : MonoBehaviour {
        public float stopwatch = 0f;
        CharacterMaster master;
        CharacterBody body;
        //GameObject effectTarget;
        internal bool wasLucky;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            master = GetComponent<CharacterMaster>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void OnDisable() {
            if(body && NetworkServer.active) {
                body.RemoveBuff(FudgeDice.instance.consumedBuff);
                body.RemoveBuff(FudgeDice.instance.readyBuff);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            var count = FudgeDice.instance.GetCount(master);

            if(stopwatch > 0f && count > 0)
                stopwatch -= Time.fixedDeltaTime;

            if(!body) {
                body = master.GetBody();
                /*if(body) {
                    var etid = body.GetComponentsInChildren<ItemDisplay>().Where(x => x.name == "FudgeDice" || x.name == "FudgeDice(Clone)").FirstOrDefault();
                    if(etid) effectTarget = etid.gameObject;
                }*/ //not necessary until IDRs are in place
            }

            if(body && NetworkServer.active) {
                if(count == 0) {
                    body.RemoveBuff(FudgeDice.instance.readyBuff);
                    body.RemoveBuff(FudgeDice.instance.consumedBuff);
                } else {
                    if(stopwatch > 0f) {
                        if(!body.HasBuff(FudgeDice.instance.consumedBuff)) body.AddBuff(FudgeDice.instance.consumedBuff);
                        body.RemoveBuff(FudgeDice.instance.readyBuff);
                    } else {
                        if(!body.HasBuff(FudgeDice.instance.readyBuff)) body.AddBuff(FudgeDice.instance.readyBuff);
                        body.RemoveBuff(FudgeDice.instance.consumedBuff);
                    }
                }
                if(wasLucky) {
                    wasLucky = false;
                    var tgtTsf = body.coreTransform;
                    //if(effectTarget) tgtTsf = effectTarget.transform;
                    EffectManager.SpawnEffect(FudgeDice.instance.cloverEffect, new EffectData {
                        origin = tgtTsf.position,
                        rotation = tgtTsf.rotation
                    }, true);
                }
            }
        }
    }
}