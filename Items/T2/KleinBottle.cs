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
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Utility});

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



        ////// TILER2 Module Setup //////
        
        public KleinBottle() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/KleinBottle.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/kleinBottleIcon.png");
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