using RoR2;
using UnityEngine;
using TILER2;
using R2API.Utils;
using static TILER2.MiscUtil;
using UnityEngine.Networking;
using R2API;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
    public class UnstableBomb : Equipment<UnstableBomb> {

        ////// Equipment Data //////

        public override string displayName => "Faulty Mortar Tube";
        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered => false;
        public override float cooldown {get; protected set;} = 40f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Throw a bomb that will detonate when damaged... <style=cDeath>BUT it may damage survivors too.</style>";
        protected override string GetDescString(string langid = null) =>
            $"Throw a <style=cIsDamage>live mortar shell</style> that will embed in the ground. After taking any damage, or after 10 seconds, the shell <style=cIsDamage>explodes for {Pct(damageFrac)} damage</style> <style=cDeath>to ALL characters in range</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percentage of base damage to deal.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float damageFrac { get; private set; } = 80f;



        ////// Other Fields/Properties //////

        public GameObject projectilePrefab;


        ////// TILER2 Module Setup //////

        public UnstableBomb() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/UnstableBomb.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/unstableBombIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            projectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/UnstableBombShell.prefab");
            var expl = projectilePrefab.GetComponent<ProjectileExplosion>();
            expl.explosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/FusionCellDestructible/FusionCellExplosion.prefab")
                .WaitForCompletion();
            projectilePrefab.GetComponent<SphereCollider>().material = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatBouncy.physicMaterial")
                .WaitForCompletion();

            var ghost = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/UnstableBombShellGhost.prefab");
            var ghostPart = ghost.GetComponent<ParticleSystemRenderer>();
            ghostPart.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLargeDirectional.mat")
                .WaitForCompletion();
            var indFixedRen = ghostPart.transform.Find("IndicatorFixed").GetComponent<MeshRenderer>();
            indFixedRen.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Captain/matCaptainAirstrikeAltAreaIndicatorOuter.mat")
                .WaitForCompletion();
            var indPulseRen = ghostPart.transform.Find("IndicatorPulse").GetComponent<MeshRenderer>();
            indPulseRen.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Captain/matCaptainAirstrikeAltAreaIndicatorInner.mat")
                .WaitForCompletion();

            ContentAddition.AddProjectile(projectilePrefab);
        }
        


        ////// Hooks //////

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            var aimRay = slot.GetAimRay();
            var aimRot = Quaternion.LookRotation(aimRay.direction);
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo {
                projectilePrefab = this.projectilePrefab,
                crit = slot.characterBody.RollCrit(),
                damage = slot.characterBody.damage * damageFrac,
                damageColorIndex = DamageColorIndex.Item,
                force = 100f,
                owner = slot.gameObject,
                position = aimRay.origin + aimRay.direction * 2f,
                rotation = aimRot,
                speedOverride = 30f,
                useSpeedOverride = true
            });

            return true;
        }
	}
}