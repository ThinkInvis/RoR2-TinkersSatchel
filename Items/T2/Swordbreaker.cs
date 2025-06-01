using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static R2API.RecalculateStatsAPI;
using UnityEngine.Networking;
using R2API;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;

namespace ThinkInvisible.TinkersSatchel {
    public class Swordbreaker : Item<Swordbreaker> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Healing, ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            shieldAmt.ToString("N0"), sparkCount.ToString("N0"), spreadConeHalfAngleDegr.ToString("N1"), rawDamage.ToString("P0"), icd.ToString("N1")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Base damage of this item's projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float rawDamage { get; private set; } = 0.4f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Amount of flat shield given by this item per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float shieldAmt { get; private set; } = 50f;

        [AutoConfigRoOIntSlider("{0:N0}", 1, 10)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Number of projectiles fired by this item per stack.", AutoConfigFlags.PreventNetMismatch, 1, int.MaxValue)]
        public int sparkCount { get; private set; } = 3;

        [AutoConfigRoOSlider("{0:N2}°", 0f, 180f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum degrees of spread to add to projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, 180f)]
        public float spreadConeHalfAngleDegr { get; private set; } = 5f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Internal cooldown for firing projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float icd { get; private set; } = 0.5f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, self-damage will not proc this item.", AutoConfigFlags.PreventNetMismatch)]
        public bool disableSelfDamage { get; private set; } = true;



        ////// Other Fields/Properties //////

        public GameObject projectilePrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public Swordbreaker() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Swordbreaker.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/swordbreakerIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            projectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/SwordbreakerProjectile.prefab");

            var explosionPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/LightningStakeNova.prefab")
                .WaitForCompletion();

            var expl = projectilePrefab.GetComponent<ProjectileImpactExplosion>();
            expl.explosionEffect = explosionPrefab;

            ContentAddition.AddProjectile(projectilePrefab);
        }

        public override void Install() {
            base.Install();
            GetStatCoefficients += Swordbreaker_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public override void Uninstall() {
            base.Uninstall();
            GetStatCoefficients -= Swordbreaker_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }



        ////// Hooks //////
        private void Swordbreaker_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            if(!sender) return;
            args.baseShieldAdd += GetCount(sender) * shieldAmt;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(!NetworkServer.active || !self) {
                orig(self, damageInfo);
                return;
            }
            var shieldBeforeDamage = self.shield;
            orig(self, damageInfo);
            if(!self || !self.body || !self.alive
                || !damageInfo.attacker || !damageInfo.attacker.transform || (damageInfo.attacker == self.gameObject && disableSelfDamage)
                || damageInfo.procChainMask.HasProc(ProcType.Thorns))
                return;
            var count = GetCount(self.body);
            if(count == 0) return;
            var dShield = self.shield - shieldBeforeDamage;
            if(dShield >= 0) return;
            var icdCpt = self.GetComponent<SwordbreakerICD>();
            if(!icdCpt) icdCpt = self.gameObject.AddComponent<SwordbreakerICD>();
            if(icdCpt.stopwatch > 0f) return;
            icdCpt.stopwatch = icd;
            var totalProjectiles = sparkCount * count;
            var projDamage = self.body.damage * rawDamage;
            var sourcePos = damageInfo.position;
            if(self.body.mainHurtBox && self.body.mainHurtBox.collider)
                sourcePos = self.body.mainHurtBox.collider.bounds.center;
            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            var targetPos = damageInfo.attacker.transform.position;
            if(attackerBody && attackerBody.mainHurtBox && attackerBody.mainHurtBox.collider)
                targetPos = attackerBody.mainHurtBox.collider.bounds.center;
            var targetVector = (targetPos - sourcePos).normalized;
            if(attackerBody && attackerBody.rigidbody) targetPos += attackerBody.rigidbody.velocity * 0.5f;
            var targetRotation = Quaternion.LookRotation(targetVector);
            for(var i = 0; i < totalProjectiles; i++) {
                var pcm = default(ProcChainMask);
                pcm.AddProc(ProcType.Thorns);
                var randomFuseTime = 0.3f + (rng.nextNormalizedFloat * 2f - 1f) * 0.1f;
                var fpi = new RoR2.Projectile.FireProjectileInfo {
                    crit = self.body.RollCrit(),
                    damage = projDamage,
                    damageColorIndex = DamageColorIndex.Item,
                    force = 0,
                    owner = self.gameObject,
                    position = sourcePos + targetVector * (self.body.radius + 1.5f),
                    procChainMask = pcm,
                    projectilePrefab = projectilePrefab,
                    rotation = rng.ApplyRandomSpread(targetRotation, spreadConeHalfAngleDegr),
                    useFuseOverride = true,
                    fuseOverride = randomFuseTime,
                    useSpeedOverride = true,
                    speedOverride = (targetPos - sourcePos).magnitude * 2f / randomFuseTime // * integration factor (speed reduces over time), / projectile lifetime
                    * (1f + (rng.nextNormalizedFloat * 2f - 1f) * Mathf.Sin(spreadConeHalfAngleDegr * Mathf.PI / 180f)) //random speed spread roughly correlated with angle spread
                };
                ProjectileManager.instance.FireProjectile(fpi);
            }
        }
    }

    public class SwordbreakerICD : MonoBehaviour {
        public float stopwatch = 0f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(stopwatch > 0f)
                stopwatch -= Time.fixedDeltaTime;
        }
    }
}