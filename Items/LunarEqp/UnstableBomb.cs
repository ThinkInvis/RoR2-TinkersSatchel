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

        public override string displayName => "Faulty Mortar Tube";
        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 40f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Throw a bomb that will detonate when damaged... <style=cDeath>BUT it may damage survivors too.</style>";
        protected override string GetDescString(string langid = null) =>
            $"Throw a <style=cIsDamage>live mortar shell</style> that will embed in the ground. After taking any damage, or after 10 seconds, the shell <style=cIsDamage>explodes for {Pct(damageFrac)} damage</style> to <style=cDeath>ALL characters</style> in range.";
        protected override string GetLoreString(string langid = null) => "";



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

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MainWeapon",
                localPos = new Vector3(-0.13327F, 0.58052F, 0.01348F),
                localAngles = new Vector3(314.2399F, 89.92023F, 0.96294F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MuzzleGun",
                localPos = new Vector3(0.11561F, 0.00569F, -0.13956F),
                localAngles = new Vector3(357.6518F, 315.6935F, 269.5259F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "GunL",
                localPos = new Vector3(0.1758F, 0.0349F, 0.01145F),
                localAngles = new Vector3(320.1846F, 107.3099F, 158.9261F),
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