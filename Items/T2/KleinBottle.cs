using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    public class KleinBottle : Item<KleinBottle> {

        ////// Item Data //////
        
        public override string displayName => "Unstable Klein Bottle";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Utility});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to implode on hit.";
        protected override string GetDescString(string langid = null) => $"{Pct(procChance, 1, 1f)} (+{Pct(procChance, 1, 1f)} per stack, mult.) chance to <style=cIsUtility>pull</style> nearby enemies on hit.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////
        
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance for Unstable Klein Bottle to proc; stacks multiplicatively.", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procChance { get; private set; } = 5f;



        ////// Other Fields/Properties //////

        const float PULL_FORCE = 1000f;
        const float PULL_RADIUS = 15f;
        const float PULL_DURATION = 0.1f;

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
            proj.lifetime = PULL_DURATION;
            var projCtrl = tempPfb.GetComponent<RoR2.Projectile.ProjectileController>();
            projCtrl.procCoefficient = 0;
            var dmg = proj.GetComponent<RoR2.Projectile.ProjectileDamage>();
            dmg.damage = 0f;
            dmg.enabled = false;
            var force = tempPfb.GetComponent<RadialForce>();
            force.forceMagnitude = -PULL_FORCE;
            force.radius = PULL_RADIUS;
            
            var sph = tempPfb.transform.Find("Sphere");
            sph.gameObject.SetActive(false);
            
            var stl = tempPfb.transform.Find("SwingTrail, Light");
            var stlPart = stl.GetComponent<ParticleSystem>();
            var stlPartSoL = stlPart.sizeOverLifetime;
            var mmc = new ParticleSystem.MinMaxCurve(1f, 0f);
            stlPartSoL.size = mmc;

            blackHolePrefab = tempPfb.InstantiateClone("KleinBottleProcPrefab", true);
            Object.Destroy(tempPfb);

            ContentAddition.AddProjectile(blackHolePrefab);
        }

        public override void Install() {
            base.Install();

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        public override void Uninstall() {
            base.Uninstall();
        }



        ////// Hooks //////

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            orig(self, damageInfo, victim);
            var cb = damageInfo.attacker?.GetComponent<CharacterBody>();
            if(!cb) return;
            var m = cb.master;
            var count = GetCount(m);
            if(count <= 0) return;

            var pChance = (1f - Mathf.Pow(1 - procChance / 100f, count)) * 100f;
            var proc = Util.CheckRoll(pChance, cb.master);
            if(proc) {
                RoR2.Projectile.ProjectileManager.instance.FireProjectile(
                    blackHolePrefab,
                    damageInfo.position, Quaternion.identity,
                    damageInfo.attacker,
                    0f, 0f, false);
            }
        }
    }
}