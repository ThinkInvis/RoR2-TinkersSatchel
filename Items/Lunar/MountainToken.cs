using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using EntityStates;
using System.Linq;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    public class MountainToken : Item<MountainToken> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.HoldoutZoneRelated });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            maxUngroundedTime.ToString("N1")
        };



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:N1} s", 0f, 60f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time spent in midair after which all bonus item stacks will have been removed and granted to enemies. Bonus items decay linearly over this timespan.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float maxUngroundedTime { get; private set; } = 20f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 600f)]
        [AutoConfig("Amount of extra monster credits to assign to the teleporter event per stack (scales with time).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float difficultyBonus { get; private set; } = 100f;



        ////// Other Fields/Properties //////

        public BuffDef eligibilityBuff { get; private set; }


        ////// TILER2 Module Setup //////

        public MountainToken() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/MountainToken.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/mountainTokenIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            eligibilityBuff = ScriptableObject.CreateInstance<BuffDef>();
            eligibilityBuff.buffColor = Color.blue;
            eligibilityBuff.canStack = true;
            eligibilityBuff.isDebuff = false;
            eligibilityBuff.name = "TKSATMountainTokenEligibility";
            eligibilityBuff.iconSprite = itemDef.pickupIconSprite;
            ContentAddition.AddBuffDef(eligibilityBuff);
        }

        public override void Install() {
            base.Install();

            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;
            IL.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter_IL;

        }

        public override void Uninstall() {
            base.Uninstall();

            IL.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter -= ChargingState_OnEnter;
            IL.RoR2.TeleporterInteraction.ChargingState.OnEnter -= ChargingState_OnEnter_IL;
        }



        ////// Hooks //////

        private void BossGroup_DropRewards(ILContext il) {
            ILCursor c = new(il);

            int locRewardCount = -1;
            if(c.TryGotoNext(
                i => i.MatchCall<BossGroup>("get_bonusRewardCount"),
                i => i.MatchAdd(),
                i => i.MatchStloc(out locRewardCount))
                && c.TryGotoNext(MoveType.After,
                i => i.MatchLdcR4(360f),
                i => i.MatchLdloc(locRewardCount))) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<int, BossGroup, int>>((origRewards, bg) => {
                    int extraRewardsSingular = 0;
                    foreach(var nu in NetworkUser.readOnlyInstancesList) {
                        if(!nu.isParticipating) continue;
                        var body = nu.GetCurrentBody();
                        if(body.TryGetComponent<MountainTokenTracker>(out var mtt)) {
                            extraRewardsSingular += mtt.Stacks;
                            mtt.Unset();
                        }
                    }
                    return origRewards + extraRewardsSingular;
                });
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Stloc, locRewardCount);
            } else {
                TinkersSatchelPlugin._logger.LogError("MountainToken: Failed to apply IL hook (BossGroup_DropRewards), item will not provide extra teleporter rewards");
            }
        }

        private void ChargingState_OnEnter_IL(ILContext il) {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.Before,
                i => i.MatchStfld<RoR2.CombatDirector>(nameof(CombatDirector.monsterCredit)));
            c.EmitDelegate<Func<float, float>>((origCredits) => {
                int msBonusCount = 0;
                foreach(var nu in NetworkUser.readOnlyInstancesList) {
                    if(!nu.isParticipating) continue;
                    var body = nu.GetCurrentBody();
                    msBonusCount += GetCount(body);
                }
                return origCredits + msBonusCount * difficultyBonus * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f);
            });
        }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, BaseState self) {
            orig(self);
            foreach(var cb in UnityEngine.Object.FindObjectsOfType<CharacterBody>())
                if(GetCount(cb) > 0 && !cb.TryGetComponent<MountainTokenTracker>(out _))
                    cb.gameObject.AddComponent<MountainTokenTracker>();
            foreach(var mtt in UnityEngine.Object.FindObjectsOfType<MountainTokenTracker>())
                mtt.Reset();
        }

        public static void GrantToEnemiesInTeleporter() {
            if(!TeleporterInteraction.instance || !TeleporterInteraction.instance.bossGroup || !TeleporterInteraction.instance.bossGroup.dropTable) return;
            PickupIndex pickupIndex = PickupIndex.none;

            var fallback = TeleporterInteraction.instance.bossGroup.forceTier3Reward
                    ? Run.instance.availableTier3DropList
                    : Run.instance.availableTier2DropList;

            if(TeleporterInteraction.instance.bossGroup.dropTable) {
                pickupIndex = CommonCode.GenerateAISafePickup(MountainToken.instance.rng, TeleporterInteraction.instance.bossGroup.dropTable, fallback);
            } else {
                pickupIndex = CommonCode.GenerateAISafePickup(MountainToken.instance.rng, fallback);
            }

            if(!CatalogUtil.TryGetItemDef(pickupIndex, out var itemDef)) return;

            var enemies = MiscUtil.GatherEnemies(TeamIndex.Player, TeamIndex.Neutral, TeamIndex.None)
                .Where(e => e.body && e.body.inventory && TeleporterInteraction.instance.holdoutZoneController.IsBodyInChargingRadius(e.body));
            foreach(var enemy in enemies)
                RoR2.Orbs.ItemTransferOrb.DispatchItemTransferOrb(TeleporterInteraction.instance.holdoutZoneController.transform.position, enemy.body.inventory, itemDef.itemIndex, 1);
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class MountainTokenTracker : MonoBehaviour {
        CharacterBody body;
        float ungroundedTime = 0f;
        int _stacks = 0;
        public int Stacks {
            get { return _stacks; }
            private set {
                _stacks = value;
                if(body)
                    body.SetBuffCount(MountainToken.instance.eligibilityBuff.buffIndex, _stacks);
            }
        }
        int maxStacks = 0;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(body.characterMotor && !body.characterMotor.isGrounded && TeleporterInteraction.instance && TeleporterInteraction.instance.activationState == TeleporterInteraction.ActivationState.Charging && Stacks > 0) {
                ungroundedTime += Time.fixedDeltaTime;
                if(ungroundedTime > MountainToken.instance.maxUngroundedTime / (float)maxStacks) {
                    ungroundedTime = 0;
                    Stacks--;
                    MountainToken.GrantToEnemiesInTeleporter();
                }
            }
        }

        public void Reset() {
            ungroundedTime = 0f;
            maxStacks = MountainToken.instance.GetCount(body);
            Stacks = maxStacks;
        }

        public void Unset() {
            Stacks = 0;
        }
    }
}