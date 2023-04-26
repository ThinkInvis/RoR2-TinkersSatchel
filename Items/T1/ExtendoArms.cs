using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class ExtendoArms : Item<ExtendoArms> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            pbaoeRange.ToString("N1"), resizeAmount.ToString("0.0%"), damageAmount.ToString("0.0%"), speedAmount.ToString("0.0%"), rangeAmount.ToString("0.0%")
        };



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:P1}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Hitbox scale increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float resizeAmount { get; private set; } = 0.125f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Projectile velocity increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float speedAmount { get; private set; } = 0.075f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Hitscan range increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float rangeAmount { get; private set; } = 0.075f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Global attack damage increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageAmount { get; private set; } = 0.05f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum distance between character and center of an AoE to count as a PBAoE.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pbaoeRange { get; private set; } = 5f;



        ////// Other Fields/Properties //////

        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public ExtendoArms() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ExtendoArms.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/extendoArmsIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/ExtendoArms.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "SideWeapon",
                localPos = new Vector3(0.04871F, -0.0517F, -0.05809F),
                localAngles = new Vector3(339.1259F, 332.7074F, 338.0398F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.1149F, -0.0598F, -0.09023F),
                localAngles = new Vector3(359.4513F, 311.727F, 0.74839F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "GunL",
                localPos = new Vector3(0.07202F, 0.30722F, 0.05609F),
                localAngles = new Vector3(28.85717F, 119.0983F, 356.3144F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(-1.90314F, 3.86177F, -0.64298F),
                localAngles = new Vector3(323.6013F, 260.4215F, 8.38977F),
                localScale = new Vector3(6F, 6F, 6F)
            }, new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(2.05511F, 3.51125F, -0.54888F),
                localAngles = new Vector3(334.2471F, 99.08073F, 173.5355F),
                localScale = new Vector3(6F, 6F, 6F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.0634F, 0.39865F, -0.0453F),
                localAngles = new Vector3(293.338F, 94.84317F, 271.7993F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(0.01928F, 0.36876F, -0.00257F),
                localAngles = new Vector3(275.5237F, 76.88559F, 122.6735F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechHandR",
                localPos = new Vector3(0.04269F, 0.51183F, -0.91419F),
                localAngles = new Vector3(288.8077F, 265.6227F, 70.82057F),
                localScale = new Vector3(0.6F, 0.6F, 0.6F)
            }, new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechHandL",
                localPos = new Vector3(0.02639F, 0.61712F, -0.89687F),
                localAngles = new Vector3(286.3646F, 79.14763F, 309.79F),
                localScale = new Vector3(0.6F, 0.6F, 0.6F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MuzzleLeft",
                localPos = new Vector3(-0.03328F, -0.14288F, 0.09997F),
                localAngles = new Vector3(38.08634F, 339.2956F, 199.7518F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.05242F, 0.36935F, -0.05418F),
                localAngles = new Vector3(301.2559F, 228.3403F, 300.5837F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.45932F, 3.44736F, 0.37264F),
                localAngles = new Vector3(286.5494F, 304.4642F, 137.6044F),
                localScale = new Vector3(6F, 6F, 6F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "WeaponPlatformEnd",
                localPos = new Vector3(-0.00078F, -0.09947F, 0.1032F),
                localAngles = new Vector3(277.5991F, 92.14574F, 260.9365F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "GunRoot",
                localPos = new Vector3(0.04973F, 0.00476F, -0.19888F),
                localAngles = new Vector3(62.8163F, 278.6512F, 278.3401F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Hand",
                localPos = new Vector3(-0.20936F, 0.36034F, -0.01036F),
                localAngles = new Vector3(308.7173F, 263.5378F, 190.6475F),
                localScale = new Vector3(0.6F, 0.6F, 0.6F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            IL.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
            On.RoR2.BlastAttack.Fire += BlastAttack_Fire;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
            On.RoR2.Projectile.ProjectileController.Awake += ProjectileController_Awake;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
        }

        public override void Uninstall() {
            base.Uninstall();
            IL.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;
            On.RoR2.BlastAttack.Fire -= BlastAttack_Fire;
            R2API.RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;
            On.RoR2.Projectile.ProjectileController.Awake -= ProjectileController_Awake;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= ProjectileManager_FireProjectile_FireProjectileInfo;
        }



        ////// Public API //////

        public static float GetRangeMultiplier(CharacterBody body) {
            if(!ExtendoArms.instance.enabled || !body) return 1f;
            var count = ExtendoArms.instance.GetCount(body.inventory);
            return 1f + count * ExtendoArms.instance.resizeAmount;
        }

        [Obsolete("Damage bonus provided by this item is now global, and applied to the character's damage stat.")]
        public static float GetDamageMultiplier(CharacterBody body) {
            if(!ExtendoArms.instance.enabled || !body) return 1f;
            var count = ExtendoArms.instance.GetCount(body.inventory);
            return 1f + count * ExtendoArms.instance.damageAmount;
        }



        ////// Hooks //////
        
        private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, RoR2.Projectile.FireProjectileInfo fireProjectileInfo) {
            if(self && fireProjectileInfo.owner && fireProjectileInfo.owner.TryGetComponent<CharacterBody>(out var ownerBody)) {
                fireProjectileInfo.speedOverride *= 1f + GetCount(ownerBody) * speedAmount;
            }
            orig(self, fireProjectileInfo);
        }

        private BlastAttack.Result BlastAttack_Fire(On.RoR2.BlastAttack.orig_Fire orig, BlastAttack self) {
            var origRadius = self.radius;
            if(self.attacker && self.attacker.TryGetComponent<CharacterBody>(out var attackerBody)) {
                if(Vector3.Distance(attackerBody.corePosition, self.position) < pbaoeRange) {
                    var count = GetCount(attackerBody);
                    self.radius *= 1f + count * resizeAmount;
                }
            }
            var retv = orig(self);
            self.radius = origRadius;
            return retv;
        }

        private void OverlapAttack_Fire(ILContext il) {
            var c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Transform>("get_lossyScale"))) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Vector3, OverlapAttack, Vector3>>((origFullExtents, self) => {
                    if(!self.attacker) return origFullExtents;
                    var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                    return origFullExtents * (1f + resizeAmount * count);
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("ExtendoArms: failed to apply IL hook (OverlapAttack_Fire), target instructions not found. Item will not apply a hitbox scale bonus.");
            }
        }

        private void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self) {
            float mdMult = 1f;
            if(self.owner && self.owner.TryGetComponent<CharacterBody>(out var ownerBody))
                mdMult += GetCount(ownerBody) * rangeAmount;
            self.maxDistance *= mdMult;
            orig(self);
            self.maxDistance /= mdMult;
        }

        private void ProjectileController_Awake(On.RoR2.Projectile.ProjectileController.orig_Awake orig, RoR2.Projectile.ProjectileController self) {
            orig(self);
            if(!self.owner || !self.owner.TryGetComponent<CharacterBody>(out var cb)) return;
            var count = GetCount(cb);
            if(count == 0) return;

            float speedMult = 1f + count * speedAmount;

            if(self.TryGetComponent<RoR2.Projectile.ProjectileSimple>(out var ps)) {
                ps.desiredForwardSpeed *= speedMult;
                ps.oscillateSpeed *= speedMult;
                ps.oscillateMagnitude *= speedMult;
                var vol = ps.velocityOverLifetime;
                if(vol != null) {
                    for(var i = 0; i < vol.length; i++) {
                        vol.keys[i].value *= speedMult;
                    }
                }
            }

            if(self.TryGetComponent<RoR2.Projectile.BoomerangProjectile>(out var bp)) {
                bp.travelSpeed *= speedMult;
            }

            if(self.TryGetComponent<RoR2.Projectile.MissileController>(out var mc)) {
                mc.maxVelocity *= speedMult;
                mc.acceleration *= speedMult;
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args) {
            if(!sender) return;
            args.damageMultAdd += GetCount(sender) * damageAmount;
        }
    }
}