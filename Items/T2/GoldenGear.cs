using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;

namespace ThinkInvisible.TinkersSatchel {
    public class GoldenGear : Item<GoldenGear> {
        public override string displayName => "Armor Crystal";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Healing});

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Gold required for the first point of armor. Scales with difficulty level.", AutoItemConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int goldAmt {get;private set;} = 3;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Exponential factor of GoldAmt scaling per additional point of armor.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float goldExp {get;private set;} = 0.02f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Inverse-exponential multiplier for reduced GoldAmt per stack (higher = more powerful).", AutoItemConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float goldReduc {get;private set;} = 0.1f;

        [AutoItemConfig("Minimum possible goldAmt as affected by item stacking.", AutoItemConfigFlags.PreventNetMismatch, float.Epsilon, float.MaxValue)]
        public float goldMin {get;private set;} = 0.0001f;

        [AutoItemConfig("If true, deployables (e.g. Engineer turrets) with Armor Crystal will benefit from their master's money.",
            AutoItemConfigFlags.PreventNetMismatch)]
        public bool inclDeploys {get;private set;} = true;

        public BuffIndex goldenGearBuff {get;private set;}
        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Gain armor by hoarding money.";
        protected override string NewLangDesc(string langid = null) => "Gain <style=cIsHealing>armor</style> based on your currently held <style=cIsUtility>money</style>. The first point of <style=cIsHealing>armor</style> costs <style=cIsUtility>$" + goldAmt.ToString("N0") + "</style> <style=cStack>(-" + Pct(goldReduc) + " per stack, exponential; scales with difficulty)</style>; each subsequent point <style=cIsUtility>costs " + Pct(goldExp) + " more</style> than the last.";
        protected override string NewLangLore(string langid = null) => "";
        
        public float CalculateArmor(uint money, int stacks) {
            if(money == 0 || stacks <= 0) return 0;
            var baseCost = Mathf.Max(Run.instance.GetDifficultyScaledCost(goldAmt) * Mathf.Pow(1f - goldReduc, stacks - 1f),
                goldMin);
            return Mathf.Log(money * goldExp / baseCost + 1f) / Mathf.Log(goldExp + 1f);
        }

        public GoldenGear() {
            modelPathName = "@TinkersSatchel:Assets/TinkersSatchel/Prefabs/GoldenGear.prefab";
            iconPathName = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/goldenGearIcon.png";

            onAttrib += (tokenIdent, namePrefix) => {
                var goldenGearBuffDef = new R2API.CustomBuff(new BuffDef {
                    buffColor = new Color(0.85f, 0.8f, 0.3f),
                    canStack = true,
                    isDebuff = false,
                    name = "TKSATGoldenGear",
                    iconPath = "textures/bufficons/texBuffGenericShield"
                });
                goldenGearBuff = R2API.BuffAPI.Add(goldenGearBuffDef);
            };
        }

        protected override void LoadBehavior() {
            On.RoR2.CharacterBody.FixedUpdate += On_CBFixedUpdate;
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }

        protected override void UnloadBehavior() {
            On.RoR2.CharacterBody.FixedUpdate -= On_CBFixedUpdate;
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }

        private void On_CBFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);
            UpdateGGBuff(self);
        }

        void UpdateGGBuff(CharacterBody cb) {
            var cpt = cb.GetComponent<GoldenGearComponent>();
            if(!cpt) cpt = cb.gameObject.AddComponent<GoldenGearComponent>();

            var newMoney = cb.master?.money ?? 0;
            if(inclDeploys) {
                var dplc = cb.GetComponent<Deployable>();
                if(dplc) newMoney += dplc.ownerMaster?.money ?? 0;
            }
            var newDiff = Run.instance.difficultyCoefficient;
            var newIcnt = GetCount(cb);

            bool didChange = false;
            if(cpt.cachedMoney != newMoney) {
                didChange = true;
                cpt.cachedMoney = newMoney;
            }
            if(cpt.cachedDiff != newDiff) {
                didChange = true;
                cpt.cachedDiff = newDiff;
            }
            if(cpt.cachedIcnt != newIcnt) {
                didChange = true;
                cpt.cachedIcnt = newIcnt;
            }
            if(!didChange) return;

            cpt.calculatedArmorBonus = CalculateArmor(cpt.cachedMoney, cpt.cachedIcnt);

            var tgtBuffStacks = Mathf.FloorToInt(cpt.calculatedArmorBonus);

            int currBuffStacks = cb.GetBuffCount(goldenGearBuff);
            if(tgtBuffStacks != currBuffStacks)
                cb.SetBuffCount(goldenGearBuff, tgtBuffStacks);
        }
        
        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            var cpt = sender.GetComponent<GoldenGearComponent>();
            if(cpt) args.armorAdd += cpt.calculatedArmorBonus;
        }
    }

    public class GoldenGearComponent : MonoBehaviour {
        public uint cachedMoney = 0u;
        public int cachedIcnt = 0;
        public float cachedDiff = 0f;
        public float calculatedArmorBonus = 0;
    }
}