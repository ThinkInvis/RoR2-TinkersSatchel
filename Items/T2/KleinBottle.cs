using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class KleinBottle : Item<KleinBottle> {

        ////// Item Data //////
        
        public override string displayName => "Unstable Klein Bottle";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Utility});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to push or pull nearby enemies on taking damage.";
        protected override string GetDescString(string langid = null) => $"After taking damage, {Pct(procChance, 1, 1f)} <style=cStack>(+{Pct(procChance, 1, 1f)} per stack, mult.)</style> chance to <style=cIsUtility>push</style> or <style=cIsUtility>pull</style> <style=cStack>(pulls on melee survivors)</style> enemies within {PULL_RADIUS:N0} m for <style=cIsDamage>{Pct(damageFrac)} damage</style>. <style=cStack>Has an internal cooldown of {PROC_ICD:N1} s.</style>";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance for Unstable Klein Bottle to proc; stacks multiplicatively.", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procChance { get; private set; } = 8f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Damage multiplier stat of the attack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFrac { get; private set; } = 0.5f;

        [AutoConfigRoOString()]
        [AutoConfig("Which survivor body names count as melee and proc pull instead of push (comma-delimited, leading/trailing whitespace will be ignored). MUL-T has special hardcoded handling for detecting Power-Saw, but will never count as melee if not in this list.",
            AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever)]
        public string meleeBodyNamesConfig { get; private set; } = "CrocoBody, MercBody, LoaderBody, ToolbotBody";

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, MeleeBodyNamesConfig becomes a blacklist instead of a whitelist.",
            AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever)]
        public bool invertBodyNames { get; private set; } = false;


        ////// Other Fields/Properties //////

        const float PULL_FORCE = 30f;
        const float PULL_RADIUS = 20f;
        const float PULL_VFX_DURATION = 0.2f;
        const float PROC_ICD = 0.5f;

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
                localPos = new Vector3(-0.6739F, -1.47899F, 1.63122F),
                localAngles = new Vector3(354.4511F, 7.12517F, 355.0916F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.24835F, 0.13692F, 0.12219F),
                localAngles = new Vector3(19.74273F, 338.7649F, 343.2596F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                childName = "Stomach",
                localPos = new Vector3(0.17437F, -0.01902F, 0.11239F),
                localAngles = new Vector3(14.62809F, 338.0782F, 18.2589F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F),
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.28481F, -0.22564F, -0.12889F),
                localAngles = new Vector3(0.98176F, 51.91312F, 23.00177F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.16876F, -0.10376F, 0.02998F),
                localAngles = new Vector3(357.5521F, 355.006F, 105.9485F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "ThighR",
                localPos = new Vector3(-0.08794F, 0.03176F, -0.06409F),
                localAngles = new Vector3(350.6662F, 317.2625F, 21.97947F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(2.33895F, -0.34548F, 0.80107F),
                localAngles = new Vector3(311.4177F, 7.89006F, 354.1869F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(0.75783F, -0.10773F, 0.00385F),
                localAngles = new Vector3(308.2326F, 10.8672F, 329.0782F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.28636F, -0.3815F, -0.06912F),
                localAngles = new Vector3(352.4358F, 63.85439F, 6.83272F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.17554F, -0.13447F, -0.0436F),
                localAngles = new Vector3(15.08189F, 9.51543F, 15.89409F),
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

            var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
            var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/kleinBottleIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            LanguageAPI.Add(achiNameToken, "Can't Touch This");
            LanguageAPI.Add(achiDescToken, "Block, or take 1 or less points of damage from, 3 attacks in a row.");
            itemDef.unlockableDef = unlockable;
        }

        public override void SetupConfig() {
            base.SetupConfig();
            meleeSurvivorBodyNames.UnionWith(meleeBodyNamesConfig.Split(',')
                .Select(x => x.Trim() + "(Clone)"));
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

                if(Time.fixedTime - cpt.LastTimestamp < PROC_ICD)
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
                    float sqrad = PULL_RADIUS * PULL_RADIUS;
                    var isCrit = self.body.RollCrit();
                    foreach(TeamComponent tcpt in teamMembers) {
                        var velVec = tcpt.transform.position - self.transform.position;
                        if(velVec.sqrMagnitude <= sqrad) {

                            bool shouldPull = meleeSurvivorBodyNames.Contains(self.body.name);
                            if(self.body.name == "ToolbotBody(Clone)")
                                shouldPull &= self.body.skillLocator.primary.skillDef.skillName == "FireBuzzsaw";

                            if(invertBodyNames) shouldPull = !shouldPull;

                            if(shouldPull) {
                                var (vInitial, _) = CalculateVelocityForFinalPosition(tcpt.transform.position, self.transform.position, 0f);
                                velVec = vInitial;
                            } else {
                                float theta;

                                if(velVec.x == 0 && velVec.z == 0)
                                    theta = UnityEngine.Random.value * Mathf.PI * 2f;
                                else
                                    theta = Mathf.Atan2(velVec.z, velVec.x);

                                float mag = velVec.magnitude;
                                if(mag == 0) mag = velVec.y;

                                var pitch = Mathf.Asin(velVec.y / mag);
                                pitch = Remap(pitch, -1, 1, 0.325f, 0.675f);
                                velVec = new Vector3(Mathf.Cos(theta) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Sin(theta) * Mathf.Cos(pitch));
                            }

                            if(tcpt.body && tcpt.body.isActiveAndEnabled) {
                                if(tcpt.body.healthComponent) tcpt.body.healthComponent.TakeDamage(new DamageInfo {
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
                                    procCoefficient = 1f
                                });
                                var mcpt = tcpt.body.GetComponent<IPhysMotor>();
                                if(mcpt != null && !tcpt.body.isBoss && !tcpt.body.isChampion)
                                    mcpt.ApplyForceImpulse(new PhysForceInfo {
                                        force = velVec * (shouldPull ? 1 : PULL_FORCE) * mcpt.mass,
                                        ignoreGroundStick = true,
                                        disableAirControlUntilCollision = false
                                    });
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

    [RegisterAchievement("TkSat_KleinBottle", "TkSat_KleinBottleUnlockable", "")]
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