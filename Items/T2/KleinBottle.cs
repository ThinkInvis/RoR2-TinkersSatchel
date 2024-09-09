﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class KleinBottle : Item<KleinBottle> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Utility});

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            (procChance/100f).ToString("0.0%"), pullRadius.ToString("N0"), pullTime.ToString("N1"), damageFrac.ToString("0%"), procIcd.ToString("N1")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance for Unstable Klein Bottle to proc; stacks multiplicatively.", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procChance { get; private set; } = 8f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Range of the Unstable Klein Bottle effect.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pullRadius { get; private set; } = 20f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Vertical hold distance of the Unstable Klein Bottle's float effect.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pullHeight { get; private set; } = 2f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Radius of random motion of the Unstable Klein Bottle's float effect.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pullWobble { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the Unstable Klein Bottle's float effect and stun.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pullTime { get; private set; } = 1f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 5f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Internal cooldown of the Unstable Klein Bottle effect.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float procIcd { get; private set; } = 1.5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Damage multiplier stat of the attack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFrac { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc coefficient of the item attack.", AutoConfigFlags.None, 0f, 1f)]
        public float procCoefficient { get; private set; } = 1f;



        ////// Other Fields/Properties //////

        const float PULL_VFX_DURATION = 0.2f;

        private GameObject blackHolePrefab;

        internal static UnlockableDef unlockable;

        public HashSet<string> meleeSurvivorBodyNames { get; private set; } = new HashSet<string>();
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public KleinBottle() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/KleinBottle.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/kleinBottleIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/KleinBottle.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.18385F, -0.05021F, 0.21923F),
                localAngles = new Vector3(2.6735F, 135.1785F, 13.09227F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.13168F, 0.02659F, 0.19437F),
                localAngles = new Vector3(299.1736F, 346.5668F, 36.47252F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.12711F, 0.00711F, 0.21108F),
                localAngles = new Vector3(8.32233F, 215.3281F, 68.41956F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(1.8815F, 0.55862F, -1.43218F),
                localAngles = new Vector3(31.47537F, 44.48933F, 67.88132F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(-0.17431F, 0.06083F, -0.16633F),
                localAngles = new Vector3(357.3008F, 336.6205F, 214.8969F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.16192F, 0.04258F, 0.14787F),
                localAngles = new Vector3(0.29875F, 190.6623F, 78.13039F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(-0.29402F, -0.14115F, 0.14596F),
                localAngles = new Vector3(359.3754F, 127.6388F, 22.99833F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(-0.16458F, 0.02023F, -0.16988F),
                localAngles = new Vector3(10.37267F, 353.8983F, 232.3469F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.22396F, 0.03669F, 0.10444F),
                localAngles = new Vector3(10.59909F, 116.392F, 7.43709F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-1.99444F, -0.82445F, 1.32131F),
                localAngles = new Vector3(322.5328F, 44.39863F, 31.38298F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(-0.801F, 0.21188F, 0.38861F),
                localAngles = new Vector3(308.2326F, 10.8672F, 329.0782F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(-0.24509F, -0.04663F, 0.20478F),
                localAngles = new Vector3(340.0907F, 84.28757F, 340.4817F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.07793F, 0.15426F, 0.20927F),
                localAngles = new Vector3(71.3228F, 4.0839F, 204.0715F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var tempPfb = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GravSphere").InstantiateClone("temporary setup prefab", false);
            var proj = tempPfb.GetComponent<RoR2.Projectile.ProjectileSimple>();
            proj.desiredForwardSpeed = 0;
            proj.lifetime = PULL_VFX_DURATION;
            var projCtrl = tempPfb.GetComponent<RoR2.Projectile.ProjectileController>();
            projCtrl.procCoefficient = 0;
            var dmg = proj.GetComponent<RoR2.Projectile.ProjectileDamage>();
            dmg.damage = 0f;
            dmg.enabled = false;
            var force = tempPfb.GetComponent<RadialForce>();
            force.enabled = false;
            
            var sph = tempPfb.transform.Find("Sphere");
            sph.gameObject.SetActive(false);

            var sps = tempPfb.transform.Find("Sparks");
            var spsPart = sps.GetComponent<ParticleSystem>();
            var spsMain = spsPart.main;
            spsMain.startSpeed = new ParticleSystem.MinMaxCurve(10f, 30f);
            var spsShape = spsPart.shape;
            spsShape.radius = 4f;

            blackHolePrefab = tempPfb.InstantiateClone("KleinBottleProcPrefab", true);
            UnityEngine.Object.Destroy(tempPfb);

            ContentAddition.AddProjectile(blackHolePrefab);

            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/kleinBottleIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            itemDef.unlockableDef = unlockable;
        }

        public override void SetupConfig() {
            base.SetupConfig();
        }

        public override void Install() {
            base.Install();

            On.RoR2.HealthComponent.UpdateLastHitTime += HealthComponent_UpdateLastHitTime;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.HealthComponent.UpdateLastHitTime += HealthComponent_UpdateLastHitTime;
        }



        ////// Hooks //////

        private void HealthComponent_UpdateLastHitTime(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker) {
            orig(self, damageValue, damagePosition, damageIsSilent, attacker);
            if(NetworkServer.active && self.body && damageValue > 0f) {
                var cpt = self.GetComponent<KleinBottleTimeTracker>();
                if(!cpt)
                    cpt = self.gameObject.AddComponent<KleinBottleTimeTracker>();

                if(Time.fixedTime - cpt.LastTimestamp < procIcd)
                    return;
                else
                    cpt.LastTimestamp = Time.fixedTime;
                var count = GetCount(self.body);
                var pChance = (1f - Mathf.Pow(1 - procChance / 100f, count)) * 100f;
                var proc = Util.CheckRoll(pChance, self.body.master);
                if(proc) {
                    RoR2.Projectile.ProjectileManager.instance.FireProjectile(
                        blackHolePrefab,
                        self.body.corePosition, Quaternion.identity,
                        self.body.gameObject,
                        0f, 0f, false);

                    var teamMembers = new List<TeamComponent>();
                    bool isFF = FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off;
                    var scan = ((TeamIndex[])Enum.GetValues(typeof(TeamIndex)));
                    var myTeam = TeamComponent.GetObjectTeam(self.body.gameObject);
                    foreach(var ind in scan) {
                        if(isFF || myTeam != ind)
                            teamMembers.AddRange(TeamComponent.GetTeamMembers(ind));
                    }
                    teamMembers.Remove(self.body.teamComponent);
                    float sqrad = pullRadius * pullRadius;
                    var isCrit = self.body.RollCrit();
                    foreach(TeamComponent tcpt in teamMembers) {
                        var velVec = tcpt.transform.position - self.transform.position;
                        if(velVec.sqrMagnitude <= sqrad) {
                            if(tcpt.body && tcpt.body.isActiveAndEnabled) {
                                if(tcpt.body.healthComponent.gameObject.TryGetComponent<FloatDebuffController>(out var existingDebuff)) {
                                    existingDebuff.RenewFloat();
                                } else {
                                    var debuff = tcpt.body.healthComponent.gameObject.AddComponent<FloatDebuffController>();
                                    debuff.deferredDamageInfo = new DamageInfo {
                                        attacker = self.gameObject,
                                        canRejectForce = true,
                                        crit = isCrit,
                                        damage = self.body.damage * damageFrac,
                                        damageColorIndex = DamageColorIndex.Item,
                                        damageType = DamageType.AOE,
                                        force = Vector3.zero,
                                        inflictor = null,
                                        position = tcpt.body.corePosition,
                                        procChainMask = default,
                                        procCoefficient = procCoefficient
                                    };
                                    debuff.InflictFloat();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public class KleinBottleTimeTracker : MonoBehaviour {
        public float LastTimestamp = 0f;
    }

    [Obsolete("Replaced by FloatDebuffController.")]
    [RequireComponent(typeof(HealthComponent))]
    public class KleinBottleDebuff : MonoBehaviour {
        HealthComponent healthComponent;
        IPhysMotor motor;
        bool started = false;
        public Vector3 targetHoldPos;
        public float holdStopwatch;
        public float wobbleSeed;
        public DamageInfo deferredDamageInfo;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            healthComponent = GetComponent<HealthComponent>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(!started) return;
            holdStopwatch -= Time.fixedDeltaTime;
            var wobbleParam = (holdStopwatch+wobbleSeed) * Mathf.PI * 0.5f;
            var targetPos = targetHoldPos + new Vector3(Mathf.Cos(wobbleParam), Mathf.Cos(wobbleParam*2), Mathf.Cos(wobbleParam*3)) * KleinBottle.instance.pullWobble;
            var velVec = (targetPos - healthComponent.body.transform.position);
            motor.ApplyForceImpulse(new PhysForceInfo {
                force = (velVec.normalized * 8f - motor.velocity) * motor.mass,
                ignoreGroundStick = true,
                disableAirControlUntilCollision = false
            });
            if(holdStopwatch <= 0f) {
                motor.ApplyForceImpulse(new PhysForceInfo {
                    force = new Vector3(0, -25f, 0) * motor.mass,
                    ignoreGroundStick = true,
                    disableAirControlUntilCollision = false
                });
                InflictDamage();
            }
        }

        public void RenewFloat() {
            healthComponent.TakeDamage(deferredDamageInfo);
            holdStopwatch = KleinBottle.instance.pullTime;
            healthComponent.GetComponent<SetStateOnHurt>().SetStun(KleinBottle.instance.pullTime);
        }

        public void InflictFloat() {
            if(!healthComponent.TryGetComponent<SetStateOnHurt>(out var ssoh) || !healthComponent.TryGetComponent<IPhysMotor>(out motor) || !ssoh.canBeStunned) {
                InflictDamage();
            } else {
                targetHoldPos = healthComponent.transform.position + new Vector3(0, KleinBottle.instance.pullHeight, 0);
                wobbleSeed = KleinBottle.instance.rng.nextNormalizedFloat;
                holdStopwatch = KleinBottle.instance.pullTime;
                ssoh.SetStun(KleinBottle.instance.pullTime);
            }
            started = true;
        }

        public void InflictDamage() {
            healthComponent.TakeDamage(deferredDamageInfo);
            Destroy(this);
        }
    }

    [RegisterAchievement("TkSat_KleinBottle", "TkSat_KleinBottleUnlockable", "", 2u)]
    public class TkSatKleinBottleAchievement : RoR2.Achievements.BaseAchievement {
        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }

        int consecutiveBlocks = 0;
        float lastHit = 0f;
        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            var prevH = self.combinedHealth;
            orig(self, damageInfo);
            if(!self || !self.body || self.body != localUser.cachedBody) return;
            var currH = self.combinedHealth;
            if((Time.fixedTime) - lastHit < 0.1f) return;
            lastHit = Time.fixedTime;
            if(damageInfo.rejected || prevH - currH <= 1f)
                consecutiveBlocks++;
            else
                consecutiveBlocks = 0;

            if(consecutiveBlocks >= 3)
                Grant();
        }
    }
}