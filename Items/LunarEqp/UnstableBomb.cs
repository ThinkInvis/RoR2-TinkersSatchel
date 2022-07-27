using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;

namespace ThinkInvisible.TinkersSatchel {
    public class UnstableBomb : Equipment<UnstableBomb> {

        ////// Equipment Data //////

        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 40f;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            damageFrac.ToString("P0")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 500f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percentage of base damage to deal.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float damageFrac { get; private set; } = 80f;



        ////// Other Fields/Properties //////

        public GameObject projectilePrefab;
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public UnstableBomb() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/UnstableBomb.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/unstableBombIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/UnstableBomb.prefab");
        }

        public override void SetupModifyEquipmentDef() {
            base.SetupModifyEquipmentDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());
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
                position = aimRay.origin,
                rotation = aimRot
            });

            return true;
        }
    }

    [RequireComponent(typeof(CharacterBody), typeof(ProjectileExplosion))]
    public class ProjectileExplodeOnDeath : MonoBehaviour, IOnKilledServerReceiver {
        public void OnKilledServer(DamageReport damageReport) {
            var pbody = GetComponent<CharacterBody>();
            var pexp = GetComponent<ProjectileExplosion>();
            if(!pbody || !pexp) return;
            pexp.Detonate();
        }
    }
}