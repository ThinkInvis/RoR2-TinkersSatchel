using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class Mug : Item<Mug> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Damage});

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            procChance.ToString("0%"), spreadConeHalfAngleDegr.ToString("N1"), meleeProjectileDamage.ToString("0%"), missileProjectileDamage.ToString("0%")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:N2}°", 0f, 180f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum degrees of spread to add to extra projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, 180f)]
        public float spreadConeHalfAngleDegr { get; private set; } = 17.5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra projectile chance per item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float procChance { get; private set; } = 0.1f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc coefficient on melee projectiles.", AutoConfigFlags.DeferForever, 0f, 1f)]
        public float procCoefficient { get; private set; } = 0.25f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 2f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proportion of melee attack damage on fired projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float meleeProjectileDamage { get; private set; } = 0.25f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 2f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proportion of missile/orb attack damage on fired projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float missileProjectileDamage { get; private set; } = 0.25f;



        /////// Other Fields/Properties //////

        public int ignoreStack = 0;
        internal UnlockableDef unlockable;
        internal RoR2.Stats.StatDef whiffsStatDef;
        public GameObject idrPrefab { get; private set; }
        public GameObject projectilePrefab { get; private set; }
        readonly HashSet<System.WeakReference<OverlapAttack>> firedAttacks = new();
        const float GC_INTERVAL = 2f;
        float _gcStopwatch;


        ////// TILER2 Module Setup //////
        #region TILER2 Module Setup
        public Mug() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Mug.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/mugIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Mug.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.06835F, 0.16783F, 0.09433F),
                localAngles = new Vector3(25.97133F, 305.0472F, 11.71204F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(0.05086F, 0.2088F, -0.06317F),
                localAngles = new Vector3(62.28717F, 25.6124F, 348.0484F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.12966F, 0.15299F, -0.0402F),
                localAngles = new Vector3(0.87372F, 2.25879F, 165.0614F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Finger21R",
                localPos = new Vector3(0.43142F, 1.38371F, 0.20466F),
                localAngles = new Vector3(45.73375F, 69.30614F, 58.74778F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.04411F, 0.16008F, 0.06094F),
                localAngles = new Vector3(79.03846F, 351.9577F, 103.1133F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.11064F, 0.20138F, -0.08752F),
                localAngles = new Vector3(26.35323F, 342.7217F, 140.326F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(0.03298F, 0.20086F, -0.07431F),
                localAngles = new Vector3(46.46626F, 70.60059F, 13.98411F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(0.02393F, 0.23256F, -0.02758F),
                localAngles = new Vector3(313.8161F, 7.28315F, 85.73673F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.05918F, 0.21561F, 0.12523F),
                localAngles = new Vector3(298.1304F, 163.4056F, 93.55658F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandR",
                localPos = new Vector3(-1.23454F, 0.95378F, 0.02811F),
                localAngles = new Vector3(292.6725F, 349.3662F, 181.1039F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FootFrontR",
                localPos = new Vector3(0.12889F, 0.73694F, -0.22601F),
                localAngles = new Vector3(306.1341F, 230.459F, 186.0314F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.06001F, 0.08116F, 0.04366F),
                localAngles = new Vector3(29.0242F, 323.4066F, 316.9256F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Neck",
                localPos = new Vector3(-0.05641F, 0.09276F, -0.2513F),
                localAngles = new Vector3(68.92164F, 301.1333F, 308.5242F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
            var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/mugIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            itemDef.unlockableDef = unlockable;

            whiffsStatDef = RoR2.Stats.StatDef.Register("tksatMugAchievementProgress", RoR2.Stats.StatRecordType.Sum, RoR2.Stats.StatDataType.ULong, 0);

            projectilePrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Projectiles/MugProjectile.prefab");
            projectilePrefab.GetComponent<ProjectileImpactExplosion>().blastProcCoefficient = procCoefficient;
            ContentAddition.AddProjectile(projectilePrefab);
        }

        public override void Install() {
            base.Install();

            //main tracking
            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
            On.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;

            //blacklist
            On.EntityStates.Huntress.ArrowRain.DoFireArrowRain += ArrowRain_DoFireArrowRain;
            On.EntityStates.AimThrowableBase.FireProjectile += AimThrowableBase_FireProjectile;
            On.EntityStates.Treebot.Weapon.FireMortar2.Fire += FireMortar2_Fire;
            On.RoR2.MissileUtils.FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool += MissileUtils_FireMissile_MyKingdomForAStruct;
            On.EntityStates.Treebot.TreebotFireFruitSeed.OnEnter += TreebotFireFruitSeed_OnEnter;
            On.EntityStates.Mage.Weapon.PrepWall.OnExit += PrepWall_OnExit;
            On.EntityStates.Treebot.Weapon.CreatePounder.OnExit += CreatePounder_OnExit;
            On.EntityStates.Treebot.Weapon.AimFlower.FireProjectile += AimFlower_FireProjectile;
            On.EntityStates.FireFlower2.OnEnter += FireFlower2_OnEnter;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.EntityStates.LaserTurbine.FireMainBeamState.OnExit += FireMainBeamState_OnExit;
            On.EntityStates.Mage.Weapon.BaseThrowBombState.Fire += BaseThrowBombState_Fire;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.EquipmentSlot.FireGummyClone += EquipmentSlot_FireGummyClone;

            //custom impl
            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.FireOrbArrow += FireSeekingArrow_FireOrbArrow;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.Run.FixedUpdate -= Run_FixedUpdate;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= ProjectileManager_FireProjectile_FireProjectileInfo;
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;
            On.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;

            On.EntityStates.Huntress.ArrowRain.DoFireArrowRain -= ArrowRain_DoFireArrowRain;
            On.EntityStates.AimThrowableBase.FireProjectile -= AimThrowableBase_FireProjectile;
            On.EntityStates.Treebot.Weapon.FireMortar2.Fire -= FireMortar2_Fire;
            On.RoR2.MissileUtils.FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool -= MissileUtils_FireMissile_MyKingdomForAStruct;
            On.EntityStates.Treebot.TreebotFireFruitSeed.OnEnter -= TreebotFireFruitSeed_OnEnter;
            On.EntityStates.Mage.Weapon.PrepWall.OnExit -= PrepWall_OnExit;
            On.EntityStates.Treebot.Weapon.CreatePounder.OnExit -= CreatePounder_OnExit;
            On.EntityStates.Treebot.Weapon.AimFlower.FireProjectile -= AimFlower_FireProjectile;
            On.EntityStates.FireFlower2.OnEnter -= FireFlower2_OnEnter;
            On.RoR2.GlobalEventManager.OnCharacterDeath -= GlobalEventManager_OnCharacterDeath;
            On.EntityStates.LaserTurbine.FireMainBeamState.OnExit -= FireMainBeamState_OnExit;
            On.EntityStates.Mage.Weapon.BaseThrowBombState.Fire -= BaseThrowBombState_Fire;
            On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
            On.RoR2.EquipmentSlot.FireGummyClone -= EquipmentSlot_FireGummyClone;

            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.FireOrbArrow -= FireSeekingArrow_FireOrbArrow;
        }
        #endregion



        ////// Hooks //////
        
        private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self) {
            orig(self);
            if(ignoreStack > 0) {
                TinkersSatchelPlugin._logger.LogError("Mug: ignoreStack was not empty on new frame, clearing. May be a cascading effect of another error, or a mod may be misusing ignoreStack.");
                ignoreStack = 0;
            }
            _gcStopwatch -= Time.fixedDeltaTime;
            if(_gcStopwatch <= 0f) {
                firedAttacks.RemoveWhere(r => !r.TryGetTarget(out _));
                _gcStopwatch = GC_INTERVAL;
            }
        }

        private bool OverlapAttack_Fire(On.RoR2.OverlapAttack.orig_Fire orig, OverlapAttack self, List<HurtBox> hitResults) {
            var retv = orig(self, hitResults);
            if(self.attacker && self.attacker.TryGetComponent<CharacterBody>(out var attackerBody)
                && !firedAttacks.Any(x => x.TryGetTarget(out var t) && t == self)) {
                if(GetCount(attackerBody) > 0) {
                    ignoreStack++;

                    FireCustomProjectiles(attackerBody, self.damage * meleeProjectileDamage, self.procChainMask);
                    firedAttacks.Add(new(self));

                    ignoreStack--;
                }
            }
            return retv;
        }

        private void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self) {
            orig(self);
            if(ignoreStack > 0 || !self.owner) return;
            var cpt = self.owner.GetComponent<CharacterBody>();
            if(!cpt) return;
            var count = GetCount(cpt);
            if(count <= 0) return;
            var totalChance = count * procChance;
            int procCount = (Util.CheckRoll(Wrap(totalChance * 100f, 0f, 100f), cpt.master) ? 1 : 0) + (int)Mathf.Floor(totalChance);
            if(procCount <= 0) return;
            self.bulletCount = (uint)procCount;
            self.maxSpread += spreadConeHalfAngleDegr;
            self.spreadPitchScale = 1f;
            self.spreadYawScale = 1f;
            orig(self);
        }

        private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, RoR2.Projectile.FireProjectileInfo fireProjectileInfo) {
            orig(self, fireProjectileInfo);
            if(ignoreStack > 0 || !self || !fireProjectileInfo.owner || !fireProjectileInfo.projectilePrefab || fireProjectileInfo.projectilePrefab.GetComponent<Deployable>()) return;
            var cpt = fireProjectileInfo.owner.GetComponent<CharacterBody>();
            if(!cpt) return;
            var count = GetCount(cpt);
            if(count <= 0) return;
            var totalChance = count * procChance;
            int procCount = (Util.CheckRoll(Wrap(totalChance * 100f, 0f, 100f), cpt.master) ? 1 : 0) + (int)Mathf.Floor(totalChance);
            var origRot = fireProjectileInfo.rotation;
            for(var i = 0; i < procCount; i++) {
                fireProjectileInfo.rotation = origRot * Quaternion.Euler(
                    (rng.nextNormalizedFloat - 0.5f) * spreadConeHalfAngleDegr,
                    (rng.nextNormalizedFloat - 0.5f) * spreadConeHalfAngleDegr,
                    (rng.nextNormalizedFloat - 0.5f) * spreadConeHalfAngleDegr);
                orig(self, fireProjectileInfo);
            }
        }

        #region Ignore / Custom Impl Hooks
        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            ignoreStack++;
            orig(self, damageInfo, victim);
            ignoreStack--;
        }

        private void FireSeekingArrow_FireOrbArrow(On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.orig_FireOrbArrow orig, EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self) {
            if(NetworkServer.active && self.characterBody && self.firedArrowCount < self.maxArrowCount && self.arrowReloadTimer <= 0f && self.initialOrbTarget)
                FireCustomProjectiles(self.characterBody, self.characterBody.damage * self.orbDamageCoefficient, default);
            orig(self);
        }

        private void BaseThrowBombState_Fire(On.EntityStates.Mage.Weapon.BaseThrowBombState.orig_Fire orig, EntityStates.Mage.Weapon.BaseThrowBombState self) {
            var doIgnore = self is EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary || self is EntityStates.Mage.Weapon.ThrowIcebomb;
            if(doIgnore) ignoreStack++;
            orig(self);
            if(doIgnore) {
                FireCustomProjectiles(self.characterBody, self.damageStat, default);
                ignoreStack--;
            }
        }

        private void FireMainBeamState_OnExit(On.EntityStates.LaserTurbine.FireMainBeamState.orig_OnExit orig, EntityStates.LaserTurbine.FireMainBeamState self) {
            ignoreStack++;
            orig(self);
            ignoreStack--;
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) {
            ignoreStack++;
            orig(self, damageReport);
            ignoreStack--;
        }

        private void FireFlower2_OnEnter(On.EntityStates.FireFlower2.orig_OnEnter orig, EntityStates.FireFlower2 self) {
            ignoreStack++;
            orig(self);
            FireCustomProjectiles(self.characterBody, self.damageStat, default);
            ignoreStack--;
        }

        private void AimFlower_FireProjectile(On.EntityStates.Treebot.Weapon.AimFlower.orig_FireProjectile orig, EntityStates.Treebot.Weapon.AimFlower self) {
            ignoreStack++;
            orig(self);
            FireCustomProjectiles(self.characterBody, self.damageStat, default);
            ignoreStack--;
        }

        private void TreebotFireFruitSeed_OnEnter(On.EntityStates.Treebot.TreebotFireFruitSeed.orig_OnEnter orig, EntityStates.Treebot.TreebotFireFruitSeed self) {
            ignoreStack++;
            orig(self);
            FireCustomProjectiles(self.characterBody, self.damageStat, default);
            ignoreStack--;
        }

        private void PrepWall_OnExit(On.EntityStates.Mage.Weapon.PrepWall.orig_OnExit orig, EntityStates.Mage.Weapon.PrepWall self) {
            ignoreStack++;
            orig(self);
            ignoreStack--;
        }

        private void CreatePounder_OnExit(On.EntityStates.Treebot.Weapon.CreatePounder.orig_OnExit orig, EntityStates.Treebot.Weapon.CreatePounder self) {
            ignoreStack++;
            orig(self);
            ignoreStack--;
        }

        private void ArrowRain_DoFireArrowRain(On.EntityStates.Huntress.ArrowRain.orig_DoFireArrowRain orig, EntityStates.Huntress.ArrowRain self) {
            ignoreStack++;
            orig(self);
            ignoreStack--;
        }

        private void MissileUtils_FireMissile_MyKingdomForAStruct(On.RoR2.MissileUtils.orig_FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool orig, Vector3 position, CharacterBody attackerBody, ProcChainMask procChainMask, GameObject victim, float missileDamage, bool isCrit, GameObject projectilePrefab, DamageColorIndex damageColorIndex, Vector3 initialDirection, float force, bool addMissileProc) {
            ignoreStack++;
            orig(position, attackerBody, procChainMask, victim, missileDamage, isCrit, projectilePrefab, damageColorIndex, initialDirection, force, addMissileProc);
            FireCustomProjectiles(attackerBody, missileDamage * missileProjectileDamage, procChainMask);
            ignoreStack--;
        }

        private void AimThrowableBase_FireProjectile(On.EntityStates.AimThrowableBase.orig_FireProjectile orig, EntityStates.AimThrowableBase self) {
            var doIgnore = self is EntityStates.Treebot.Weapon.AimMortar2 || self is EntityStates.Captain.Weapon.CallAirstrikeBase;
            if(doIgnore) ignoreStack++;
            orig(self);
            if(doIgnore) {
                FireCustomProjectiles(self.characterBody, self.damageStat, default);
                ignoreStack--;
            }
        }

        private void FireMortar2_Fire(On.EntityStates.Treebot.Weapon.FireMortar2.orig_Fire orig, EntityStates.Treebot.Weapon.FireMortar2 self) {
            ignoreStack++;
            orig(self);
            FireCustomProjectiles(self.characterBody, self.damageStat, default);
            ignoreStack--;
        }

        private bool EquipmentSlot_FireGummyClone(On.RoR2.EquipmentSlot.orig_FireGummyClone orig, EquipmentSlot self) {
            ignoreStack++;
            var retv = orig(self);
            ignoreStack--;
            return retv;
        }
        #endregion



        ////// Private API //////

        void FireCustomProjectiles(CharacterBody attackerBody, float damage, ProcChainMask procChainMask) {
            if(!attackerBody) return;
            var count = GetCount(attackerBody);
            var totalChance = count * procChance;
            int procCount = (Util.CheckRoll(Wrap(totalChance * 100f, 0f, 100f), attackerBody.master) ? 1 : 0) + (int)Mathf.Floor(totalChance);
            var origRot = Quaternion.LookRotation(attackerBody.inputBank ? attackerBody.inputBank.aimDirection : (attackerBody.characterDirection ? attackerBody.characterDirection.forward : attackerBody.transform.forward), attackerBody.transform.up);
            for(var i = 0; i < procCount; i++) {
                ProjectileManager.instance.FireProjectile(new FireProjectileInfo {
                    crit = attackerBody.RollCrit(),
                    damage = damage,
                    damageColorIndex = DamageColorIndex.Item,
                    force = 0f,
                    owner = attackerBody.gameObject,
                    rotation = origRot * Quaternion.Euler(
                        (rng.nextNormalizedFloat - 0.5f) * spreadConeHalfAngleDegr,
                        (rng.nextNormalizedFloat - 0.5f) * spreadConeHalfAngleDegr,
                        (rng.nextNormalizedFloat - 0.5f) * spreadConeHalfAngleDegr),
                    position = attackerBody.corePosition,
                    procChainMask = procChainMask,
                    projectilePrefab = projectilePrefab
                });
            }
        }
    }

    [RegisterAchievement("TkSat_Mug", "TkSat_MugUnlockable", "")]
    public class TkSatMugAchievement : RoR2.Achievements.BaseAchievement {
        bool bulletAttackDidHit = false;

        public override float ProgressForAchievement() {
            return userProfile.statSheet.GetStatValueULong(Mug.instance.whiffsStatDef) / 1000f;
        }

        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.Projectile.ProjectileController.OnCollisionEnter += ProjectileController_OnCollisionEnter;
            On.RoR2.Projectile.ProjectileController.OnTriggerEnter += ProjectileController_OnTriggerEnter;
            On.RoR2.Projectile.ProjectileController.OnDestroy += ProjectileController_OnDestroy;
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
            On.RoR2.BulletAttack.ProcessHit += BulletAttack_ProcessHit;
        }

        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.Projectile.ProjectileController.OnCollisionEnter -= ProjectileController_OnCollisionEnter;
            On.RoR2.Projectile.ProjectileController.OnTriggerEnter -= ProjectileController_OnTriggerEnter;
            On.RoR2.Projectile.ProjectileController.OnDestroy -= ProjectileController_OnDestroy;
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;
            On.RoR2.BulletAttack.ProcessHit -= BulletAttack_ProcessHit;
        }

        private void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self) {
            bulletAttackDidHit = false;
            orig(self);
            if(!bulletAttackDidHit && self.owner == this.localUser.cachedBodyObject) {
                userProfile.statSheet.PushStatValue(Mug.instance.whiffsStatDef, 1UL);
                if(ProgressForAchievement() >= 1.0f)
                    Grant();
            }
        }

        private bool BulletAttack_ProcessHit(On.RoR2.BulletAttack.orig_ProcessHit orig, BulletAttack self, ref BulletAttack.BulletHit hitInfo) {
            var retv = orig(self, ref hitInfo);
            if(hitInfo.hitHurtBox && hitInfo.hitHurtBox.name != "TempHurtbox") {
                bulletAttackDidHit = true;
            }
            return retv;
        }

        private void ProjectileController_OnCollisionEnter(On.RoR2.Projectile.ProjectileController.orig_OnCollisionEnter orig, RoR2.Projectile.ProjectileController self, Collision collision) {
            orig(self, collision);
            if(!collision.gameObject) return;
            var hb = collision.gameObject.GetComponent<HurtBox>();
            if(hb && hb.healthComponent) {
                self.gameObject.AddComponent<ProjectileHasValidHitFlag>();
            }
        }

        private void ProjectileController_OnTriggerEnter(On.RoR2.Projectile.ProjectileController.orig_OnTriggerEnter orig, RoR2.Projectile.ProjectileController self, Collider collider) {
            orig(self, collider);
            if(!collider.gameObject) return;
            var hb = collider.gameObject.GetComponent<HurtBox>();
            if(hb && hb.healthComponent) {
                self.gameObject.AddComponent<ProjectileHasValidHitFlag>();
            }
        }

        private void ProjectileController_OnDestroy(On.RoR2.Projectile.ProjectileController.orig_OnDestroy orig, RoR2.Projectile.ProjectileController self) {
            orig(self);
            if(!self.GetComponent<ProjectileHasValidHitFlag>()
                && self.owner == this.localUser.cachedBodyObject) {
                userProfile.statSheet.PushStatValue(Mug.instance.whiffsStatDef, 1UL);
                if(ProgressForAchievement() >= 1.0f)
                    Grant();
            }

        }
    }

    public class ProjectileHasValidHitFlag : MonoBehaviour {}
}