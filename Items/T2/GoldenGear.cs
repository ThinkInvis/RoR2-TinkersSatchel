using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static R2API.RecalculateStatsAPI;
using R2API;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class GoldenGear : Item<GoldenGear> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Healing});

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            goldAmt.ToString("N0"), goldExp.ToString("0%"), duration.ToString("N1")
        };



        ////// Config //////

        [AutoConfigRoOIntSlider("${0:N0}", 1, 1000)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Gold required for the first point of armor. Scales with difficulty level.", AutoConfigFlags.PreventNetMismatch, 1, int.MaxValue)]
        public int goldAmt { get; private set; } = 2;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Exponential reduction to points of armor past the first.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float goldExp { get; private set; } = 0.01f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Duration of each point of armor.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float duration { get; private set; } = 5f;



        ////// Other Fields/Properties //////
        
        public BuffDef goldenGearBuff { get; private set; }
        internal static UnlockableDef unlockable;



        ////// TILER2 Module Setup //////

        public GoldenGear() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/GoldenGear.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/goldenGearIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            goldenGearBuff = ScriptableObject.CreateInstance<BuffDef>();
            goldenGearBuff.buffColor = new Color(0.85f, 0.8f, 0.3f);
            goldenGearBuff.canStack = true;
            goldenGearBuff.isDebuff = false;
            goldenGearBuff.name = "TKSATGoldenGear";
            goldenGearBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffGenericShield.tif")
                .WaitForCompletion();
            ContentAddition.AddBuffDef(goldenGearBuff);

            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/goldenGearIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            itemDef.unlockableDef = unlockable;

            modelResource.transform.Find("GoldenGear2").GetComponent<MeshRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Chest1/matChest1.mat").WaitForCompletion();
        }

        public override void Install() {
            base.Install();

            On.RoR2.CharacterMaster.GiveMoney += CharacterMaster_GiveMoney;
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.CharacterMaster.GiveMoney -= CharacterMaster_GiveMoney;
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }



        ////// Hooks //////
        
        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            if(!sender) return;
            var cpt = sender.GetComponent<GoldenGearComponent>();
            if(cpt) {
                var armorBonus = Mathf.Log(cpt.totalBuff * goldExp + 1f) / Mathf.Log(goldExp + 1f) * GetCount(sender);
                sender.SetBuffCount(GoldenGear.instance.goldenGearBuff.buffIndex, Mathf.FloorToInt(armorBonus));
                args.armorAdd += armorBonus;
            }
        }

        private void CharacterMaster_GiveMoney(On.RoR2.CharacterMaster.orig_GiveMoney orig, CharacterMaster self, uint amount) {
            orig(self, amount);
            if(!self.hasBody) return;
            var stacks = GetCount(self);
            if(stacks == 0) return;

            var cb = self.GetBody();

            var cpt = cb.GetComponent<GoldenGearComponent>();
            if(!cpt) cpt = cb.gameObject.AddComponent<GoldenGearComponent>();

            cpt.cachedMoney += amount;
        }
    }

    public class GoldenGearComponent : MonoBehaviour {
        public uint cachedMoney = 0u;
        readonly List<(int count, float timestamp)> stacks = new();
        public int totalBuff { get; private set; }

        void FixedUpdate() {
            var moneyPerStack = Run.instance.GetDifficultyScaledCost(GoldenGear.instance.goldAmt);
            var newStacks = Mathf.FloorToInt(cachedMoney / moneyPerStack);

            if(newStacks > 0) {
                cachedMoney -= (uint)(newStacks * moneyPerStack);
                stacks.Add((newStacks, Time.fixedTime));
            }

            stacks.RemoveAll(stack => (Time.fixedTime - stack.timestamp) > GoldenGear.instance.duration);
            var newTotalBuff = stacks.Sum(stack => stack.count);
            if(TryGetComponent<CharacterBody>(out var cb) && newTotalBuff != totalBuff)
                cb.statsDirty = true;
            totalBuff = newTotalBuff;
        }
    }


    public class ShrineFailTracker : MonoBehaviour {
        public int failCount = 0;
    }

    [RegisterAchievement("TkSat_GoldenGear", "TkSat_GoldenGearUnlockable", null, typeof(TkSatGoldenGearServerAchievement))]
    public class TkSatGoldenGearAchievement : RoR2.Achievements.BaseAchievement {
        public override void OnInstall() {
            base.OnInstall();
            base.SetServerTracked(true);
        }

        public override void OnUninstall() {
            base.OnUninstall();
        }

        private class TkSatGoldenGearServerAchievement : RoR2.Achievements.BaseServerAchievement {
            public override void OnInstall() {
                base.OnInstall();

                On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
            }

            public override void OnUninstall() {
                base.OnUninstall();

                On.RoR2.ShrineChanceBehavior.AddShrineStack -= ShrineChanceBehavior_AddShrineStack;
            }

            private void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator) {
                var spc0 = self.successfulPurchaseCount;
                orig(self, activator);
                var spc1 = self.successfulPurchaseCount;

                var sft = self.gameObject.GetComponent<ShrineFailTracker>();
                if(!sft)
                    sft = self.gameObject.AddComponent<ShrineFailTracker>();

                var wasFail = spc0 == spc1;

                if(wasFail) {
                    sft.failCount++;
                } else if(sft.failCount >= 4) {
                    var currBody = serverAchievementTracker.networkUser.GetCurrentBody();
                    if(currBody && currBody.GetComponent<Interactor>() == activator)
                        Grant();
                }
            }
        }
    }
}