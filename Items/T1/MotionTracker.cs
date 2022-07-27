using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using System.Collections.Generic;
using System.Linq;
using R2API;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
    public class MotionTracker : Item<MotionTracker> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Damage});

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            damageFrac.ToString("P0"), damageTime.ToString("N0")
        };



        ////// Config //////
        
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
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public MotionTracker() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/MotionTracker.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/motionTrackerIcon.png");
            vfxPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/MotionTrackerIndicator.prefab");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/MotionTracker.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MainWeapon",
                localPos = new Vector3(-0.03808F, 0.34257F, 0.06901F),
                localAngles = new Vector3(359.0724F, 179.6853F, 269.6975F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MuzzleGun",
                localPos = new Vector3(0.02929F, 0.06318F, -0.23436F),
                localAngles = new Vector3(84.86104F, 180.0003F, 85.8542F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "GunL",
                localPos = new Vector3(0.13281F, 0.12421F, 0.0553F),
                localAngles = new Vector3(7.62563F, 181.4729F, 2.13429F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "LowerArmL",
                localPos = new Vector3(1.27773F, 3.88492F, -0.9831F),
                localAngles = new Vector3(9.66044F, 307.2604F, 269.1215F),
                localScale = new Vector3(4F, 4F, 4F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "CannonHeadL",
                localPos = new Vector3(-0.21459F, 0.27626F, 0.1353F),
                localAngles = new Vector3(359.9774F, 94.53657F, 265.8145F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "BowBase",
                localPos = new Vector3(-0.09708F, -0.03416F, -0.05435F),
                localAngles = new Vector3(358.2427F, 0.36579F, 72.98668F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechHandR",
                localPos = new Vector3(0.10542F, 0.14123F, 0.11133F),
                localAngles = new Vector3(351.9657F, 231.0573F, 261.3159F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.14348F, -0.01409F, 0.02138F),
                localAngles = new Vector3(9.93206F, 97.79813F, 278.8011F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(-0.03423F, -0.11768F, -0.03466F),
                localAngles = new Vector3(4.52847F, 47.9074F, 279.7864F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "LowerArmL",
                localPos = new Vector3(0.09233F, 2.16268F, -1.18963F),
                localAngles = new Vector3(1.85317F, 3.14276F, 277.0497F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "WeaponPlatformEnd",
                localPos = new Vector3(0.22507F, -0.21132F, 0.14108F),
                localAngles = new Vector3(357.0192F, 270.0101F, 268.9311F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "GunScope",
                localPos = new Vector3(0.06161F, -0.1885F, 0.19716F),
                localAngles = new Vector3(270.691F, 215.5541F, 234.1518F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "CannonEnd",
                localPos = new Vector3(0.13595F, -0.01651F, -0.24505F),
                localAngles = new Vector3(355.1688F, 345.0563F, 260.352F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var partMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matCritImpactHeavy.mat")
                .WaitForCompletion();

            vfxPrefab.transform.Find("Background/IndParticleR").GetComponent<ParticleSystemRenderer>().material = partMtl;
            vfxPrefab.transform.Find("Background/IndParticleL").GetComponent<ParticleSystemRenderer>().material = partMtl;

            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/motionTrackerIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
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

        readonly Dictionary<GameObject, (float stopwatch, float duration, Indicator indicator)> activeCombatants = new();

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