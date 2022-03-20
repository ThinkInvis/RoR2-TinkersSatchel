using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using RoR2.Orbs;

namespace ThinkInvisible.TinkersSatchel {
    public class KleinBottle : Item<KleinBottle> {

        ////// Item Data //////
        
        public override string displayName => "Unstable Klein Bottle";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Utility});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to push nearby enemies on taking damage.";
        protected override string GetDescString(string langid = null) => $"{Pct(procChance, 1, 1f)} (+{Pct(procChance, 1, 1f)} per stack, mult.) chance to <style=cIsUtility>push away</style> enemies within {PULL_RADIUS:N0} m after taking damage.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////
        
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance for Unstable Klein Bottle to proc; stacks multiplicatively.", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procChance { get; private set; } = 5f;



        ////// Other Fields/Properties //////

        const float PULL_FORCE = 20f;
        const float PULL_RADIUS = 20f;
        const float PULL_VFX_DURATION = 0.2f;

        private GameObject blackHolePrefab;



        ////// TILER2 Module Setup //////
        
        public KleinBottle() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/KleinBottle.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/kleinBottleIcon.png");
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
                    foreach(TeamComponent tcpt in teamMembers) {
                        var velVec = tcpt.transform.position - self.transform.position;
                        if(velVec.sqrMagnitude <= sqrad) {
                            velVec.y += 0.5f * velVec.magnitude;
                            velVec = velVec.normalized;
                            if(tcpt.body && !tcpt.body.isBoss && !tcpt.body.isChampion && tcpt.body.isActiveAndEnabled) {
                                var mcpt = tcpt.body.GetComponent<IPhysMotor>();
                                if(mcpt != null)
                                    mcpt.ApplyForceImpulse(new PhysForceInfo {
                                        force = velVec * PULL_FORCE,
                                        massIsOne = true,
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
}