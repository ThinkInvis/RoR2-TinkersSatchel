using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using System.Collections.Generic;
using System.Linq;
using R2API;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
    public class MotionTracker : Item<MotionTracker> {

        ////// Item Data //////
        
        public override string displayName => "Old War Lidar";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Damage});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Deal more damage to persistent combatants.";
        protected override string GetDescString(string langid = null) => $"Deal up to <style=cIsDamage>{Pct(damageFrac)} more damage <style=cStack>(+{Pct(damageFrac)} per stack)</style></style> to any enemy you have recently hit or been hit by, <style=cIsUtility>ramping up</style> over {damageTime:N0} seconds.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////
        ///
        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum damage bonus per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFrac { get; private set; } = 0.15f;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 300f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time in combat required to reach maximum damage bonus.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageTime { get; private set; } = 15f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, indicator VFX will be disabled.")]
        public bool disableVFX { get; private set; } = false;



        ////// Other Fields/Properties //////
        
        internal static UnlockableDef unlockable;
        internal static GameObject vfxPrefab;



        ////// TILER2 Module Setup //////
        
        public MotionTracker() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/MotionTracker.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/motionTrackerIcon.png");
            vfxPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/MotionTrackerIndicator.prefab");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var partMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matCritImpactHeavy.mat")
                .WaitForCompletion();

            vfxPrefab.transform.Find("Background/IndParticleR").GetComponent<ParticleSystemRenderer>().material = partMtl;
            vfxPrefab.transform.Find("Background/IndParticleL").GetComponent<ParticleSystemRenderer>().material = partMtl;

            var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
            var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/motionTrackerIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            LanguageAPI.Add(achiNameToken, "Why Won't You Die?!");
            LanguageAPI.Add(achiDescToken, "Fully charge a Teleporter without killing the boss or dying.");
            itemDef.unlockableDef = unlockable;
        }

        public override void Install() {
            base.Install();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public override void Uninstall() {
            base.Uninstall();
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }



        ////// Hooks //////

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(self && damageInfo.attacker) {
                var mtt = damageInfo.attacker.GetComponent<MotionTrackerTracker>();
                var count = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                if(mtt && count > 0) {
                    mtt.SetInCombat(self.gameObject);
                    damageInfo.damage *= 1f + mtt.GetCombatBonusScalar(self.gameObject) * count;
                }
            }

            orig(self, damageInfo);

            if(self && self.body && damageInfo.attacker) {
                var mtt = self.body.GetComponent<MotionTrackerTracker>();
                if(mtt)
                    mtt.SetInCombat(damageInfo.attacker);
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(GetCount(body) > 0 && !body.GetComponent<MotionTrackerTracker>())
                body.gameObject.AddComponent<MotionTrackerTracker>();
        }

    }

    public class MotionTrackerTracker : MonoBehaviour {
        const float COMBAT_TIMER = 6f;

        readonly Dictionary<GameObject, (float stopwatch, float duration, Indicator indicator)> activeCombatants = new Dictionary<GameObject, (float, float, Indicator)>();

        public float GetCombatBonusScalar(GameObject with) {
            if(!with || !activeCombatants.ContainsKey(with))
                return 0f;
            return Mathf.Clamp01(activeCombatants[with].duration / MotionTracker.instance.damageTime) * MotionTracker.instance.damageFrac;
        }

        public void SetInCombat(GameObject with) {
            if(!with) return;
            if(activeCombatants.ContainsKey(with))
                activeCombatants[with] = (COMBAT_TIMER, activeCombatants[with].duration, activeCombatants[with].indicator);
            else {
                activeCombatants[with] = (COMBAT_TIMER, 0f, TryAddIndicator(with));
            }
        }

        public Indicator TryAddIndicator(GameObject with) {
            Indicator ind = null;
            if(with && !MotionTracker.instance.disableVFX) {
                ind = new Indicator(gameObject, MotionTracker.vfxPrefab) {
                    targetTransform = with.transform,
                    active = true
                };

                if(ind.visualizerInstance) {
                    if(with.TryGetComponent<CharacterBody>(out var tgtBody))
                        ind.visualizerInstance.transform.position = tgtBody.corePosition;
                    var anim = ind.visualizerInstance.transform.Find("Background").GetComponent<Animator>();
                    anim.SetFloat("Speed", 1f / MotionTracker.instance.damageTime);
                    anim.PlayInFixedTime("ZeroIn");
                }
            }
            return ind;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            var frozenCombatants = activeCombatants.ToArray();
            foreach(var kvp in frozenCombatants) {
                var nsw = kvp.Value.stopwatch - Time.fixedDeltaTime;
                if(nsw <= 0f || !kvp.Key) {
                    activeCombatants.Remove(kvp.Key);
                    if(kvp.Value.indicator != null)
                        kvp.Value.indicator.active = false;
                } else
                    activeCombatants[kvp.Key] = (nsw, kvp.Value.duration + Time.fixedDeltaTime, kvp.Value.indicator);
            }
        }
    }

    [RegisterAchievement("TkSat_MotionTracker", "TkSat_MotionTrackerUnlockable", "")]
    public class TkSatMotionTrackerAchievement : RoR2.Achievements.BaseAchievement {
        static bool qualifies = false;

        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;
            On.RoR2.TeleporterInteraction.UpdateMonstersClear += TeleporterInteraction_UpdateMonstersClear;
            On.RoR2.CharacterMaster.OnBodyDeath += CharacterMaster_OnBodyDeath;
        }

        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter -= ChargingState_OnEnter;
            On.RoR2.TeleporterInteraction.UpdateMonstersClear -= TeleporterInteraction_UpdateMonstersClear;
            On.RoR2.CharacterMaster.OnBodyDeath -= CharacterMaster_OnBodyDeath;
        }

        private void CharacterMaster_OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body) {
            orig(self, body);
            if(localUser.cachedMaster == self)
                qualifies = false;
        }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, EntityStates.BaseState self) {
            orig(self);
            qualifies = true;
        }

        private void TeleporterInteraction_UpdateMonstersClear(On.RoR2.TeleporterInteraction.orig_UpdateMonstersClear orig, TeleporterInteraction self) {
            orig(self);
            if(localUser.cachedMaster && !localUser.cachedMaster.IsDeadAndOutOfLivesServer() && self && !self.monstersCleared && self.holdoutZoneController && self.holdoutZoneController.charge >= 1f && qualifies)
                Grant();
        }
    }
}