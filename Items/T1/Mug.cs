using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    public class Mug : Item<Mug> {

        ////// Item Data //////

        public override string displayName => "Sturdy Mug";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Damage});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to shoot extra, unpredictable projectiles.";
        protected override string GetDescString(string langid = null) => $"All projectile attacks gain a <style=cIsDamage>{Pct(procChance)} <style=cStack>(+{Pct(procChance)} per stack)</style></style> chance to fire <style=cIsDamage>an extra copy</style> with <color=#FF7F7F>{spreadConeHalfAngleDegr}° of inaccuracy</style>.";
        protected override string GetLoreString(string langid = null) => "An inscription around the base reads: \"Rock and Stone!\"";



        ////// Config //////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum degrees of spread to add to extra projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, 360f)]
        public float spreadConeHalfAngleDegr { get; private set; } = 17.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra projectile chance per item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float procChance { get; private set; } = 0.1f;



        /////// Other Fields/Properties //////

        public bool ignoreMugs = false;

        internal UnlockableDef unlockable;
        internal RoR2.Stats.StatDef whiffsStatDef;



        ////// TILER2 Module Setup //////
        #region TILER2 Module Setup
        public Mug() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Mug.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/mugIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            unlockable = UnlockableAPI.AddUnlockable<TkSatMugAchievement>();
            LanguageAPI.Add("TKSAT_MUG_ACHIEVEMENT_NAME", "...So I Fired Again");
            LanguageAPI.Add("TKSAT_MUG_ACHIEVEMENT_DESCRIPTION", "Miss 1,000 TOTAL projectile attacks.");

            itemDef.unlockableDef = unlockable;

            whiffsStatDef = RoR2.Stats.StatDef.Register("tksatMugAchievementProgress", RoR2.Stats.StatRecordType.Sum, RoR2.Stats.StatDataType.ULong, 0);
        }

        public override void Install() {
            base.Install();

            //main tracking
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;

            //blacklist
            On.EntityStates.Huntress.ArrowRain.DoFireArrowRain += ArrowRain_DoFireArrowRain;
            On.EntityStates.AimThrowableBase.FireProjectile += AimThrowableBase_FireProjectile;
            On.EntityStates.Treebot.Weapon.FireMortar2.Fire += FireMortar2_Fire;
            On.RoR2.MissileUtils.FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool += MissileUtils_FireMissile_MyKingdomForAStruct;
            On.EntityStates.Treebot.TreebotFireFruitSeed.OnEnter += TreebotFireFruitSeed_OnEnter;
            On.EntityStates.Engi.EngiWeapon.FireMines.OnEnter += FireMines_OnEnter;
            On.EntityStates.Mage.Weapon.PrepWall.OnExit += PrepWall_OnExit;
            On.EntityStates.Treebot.Weapon.CreatePounder.OnExit += CreatePounder_OnExit;
            On.EntityStates.Treebot.Weapon.AimFlower.FireProjectile += AimFlower_FireProjectile;
            On.EntityStates.FireFlower2.OnEnter += FireFlower2_OnEnter;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.EntityStates.LaserTurbine.FireMainBeamState.OnExit += FireMainBeamState_OnExit;
            On.EntityStates.Mage.Weapon.BaseThrowBombState.Fire += BaseThrowBombState_Fire;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= ProjectileManager_FireProjectile_FireProjectileInfo;
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;

            On.EntityStates.Huntress.ArrowRain.DoFireArrowRain -= ArrowRain_DoFireArrowRain;
            On.EntityStates.AimThrowableBase.FireProjectile -= AimThrowableBase_FireProjectile;
            On.EntityStates.Treebot.Weapon.FireMortar2.Fire -= FireMortar2_Fire;
            On.RoR2.MissileUtils.FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool -= MissileUtils_FireMissile_MyKingdomForAStruct;
            On.EntityStates.Treebot.TreebotFireFruitSeed.OnEnter -= TreebotFireFruitSeed_OnEnter;
            On.EntityStates.Engi.EngiWeapon.FireMines.OnEnter -= FireMines_OnEnter;
            On.EntityStates.Mage.Weapon.PrepWall.OnExit -= PrepWall_OnExit;
            On.EntityStates.Treebot.Weapon.CreatePounder.OnExit -= CreatePounder_OnExit;
            On.EntityStates.Treebot.Weapon.AimFlower.FireProjectile -= AimFlower_FireProjectile;
            On.EntityStates.FireFlower2.OnEnter -= FireFlower2_OnEnter;
            On.RoR2.GlobalEventManager.OnCharacterDeath -= GlobalEventManager_OnCharacterDeath;
            On.EntityStates.LaserTurbine.FireMainBeamState.OnExit -= FireMainBeamState_OnExit;
            On.EntityStates.Mage.Weapon.BaseThrowBombState.Fire -= BaseThrowBombState_Fire;
            On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
        }
        #endregion



        ////// Hooks //////
        #region Hooks
        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            ignoreMugs = true;
            orig(self, damageInfo, victim);
            ignoreMugs = false;
        }

        private void BaseThrowBombState_Fire(On.EntityStates.Mage.Weapon.BaseThrowBombState.orig_Fire orig, EntityStates.Mage.Weapon.BaseThrowBombState self) {
            var doIgnore = self is EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary || self is EntityStates.Mage.Weapon.ThrowIcebomb;
            if(doIgnore) ignoreMugs = true;
            orig(self);
            if(doIgnore) ignoreMugs = false;
        }

        private void FireMainBeamState_OnExit(On.EntityStates.LaserTurbine.FireMainBeamState.orig_OnExit orig, EntityStates.LaserTurbine.FireMainBeamState self) {
            ignoreMugs = true;
            orig(self);
            ignoreMugs = false;
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) {
            ignoreMugs = true;
            orig(self, damageReport);
            ignoreMugs = false;
        }

        private void FireFlower2_OnEnter(On.EntityStates.FireFlower2.orig_OnEnter orig, EntityStates.FireFlower2 self) {
            ignoreMugs = true;
            orig(self);
            ignoreMugs = false;
        }

        private void AimFlower_FireProjectile(On.EntityStates.Treebot.Weapon.AimFlower.orig_FireProjectile orig, EntityStates.Treebot.Weapon.AimFlower self) {
            ignoreMugs = true;
            orig(self);
            ignoreMugs = false;
        }

        private void TreebotFireFruitSeed_OnEnter(On.EntityStates.Treebot.TreebotFireFruitSeed.orig_OnEnter orig, EntityStates.Treebot.TreebotFireFruitSeed self) {
            ignoreMugs = true;
            orig(self);
            ignoreMugs = false;
        }

        private void FireMines_OnEnter(On.EntityStates.Engi.EngiWeapon.FireMines.orig_OnEnter orig, EntityStates.Engi.EngiWeapon.FireMines self) {
            ignoreMugs = true;
            orig(self);
            ignoreMugs = false;
        }

        private void PrepWall_OnExit(On.EntityStates.Mage.Weapon.PrepWall.orig_OnExit orig, EntityStates.Mage.Weapon.PrepWall self) {
            ignoreMugs = true;
            orig(self);
            ignoreMugs = false;
        }

        private void CreatePounder_OnExit(On.EntityStates.Treebot.Weapon.CreatePounder.orig_OnExit orig, EntityStates.Treebot.Weapon.CreatePounder self) {
            ignoreMugs = true;
            orig(self);
            ignoreMugs = false;
        }

        private void ArrowRain_DoFireArrowRain(On.EntityStates.Huntress.ArrowRain.orig_DoFireArrowRain orig, EntityStates.Huntress.ArrowRain self) {
            ignoreMugs = true;
            orig(self);
            ignoreMugs = false;
        }

        private void MissileUtils_FireMissile_MyKingdomForAStruct(On.RoR2.MissileUtils.orig_FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool orig, Vector3 position, CharacterBody attackerBody, ProcChainMask procChainMask, GameObject victim, float missileDamage, bool isCrit, GameObject projectilePrefab, DamageColorIndex damageColorIndex, Vector3 initialDirection, float force, bool addMissileProc) {
            ignoreMugs = true;
            orig(position, attackerBody, procChainMask, victim, missileDamage, isCrit, projectilePrefab, damageColorIndex, initialDirection, force, addMissileProc);
            ignoreMugs = false;
        }

        private void AimThrowableBase_FireProjectile(On.EntityStates.AimThrowableBase.orig_FireProjectile orig, EntityStates.AimThrowableBase self) {
            var doIgnore = self is EntityStates.Treebot.Weapon.AimMortar2 || self is EntityStates.Captain.Weapon.CallAirstrikeBase;
            if(doIgnore) ignoreMugs = true;
            orig(self);
            if(doIgnore) ignoreMugs = false;
        }

        private void FireMortar2_Fire(On.EntityStates.Treebot.Weapon.FireMortar2.orig_Fire orig, EntityStates.Treebot.Weapon.FireMortar2 self) {
            ignoreMugs = true;
            orig(self);
            ignoreMugs = false;
        }

        private void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self) {
            orig(self);
            if(ignoreMugs || !self.owner) return;
            var cpt = self.owner.GetComponent<CharacterBody>();
            if(!cpt) return;
            var count = GetCount(cpt);
            if(count <= 0) return;
            var totalChance = count * procChance;
            int procCount = (Util.CheckRoll(Wrap(totalChance, 0f, 100f), cpt.master) ? 1 : 0) + (int)Mathf.Floor(totalChance);
            if(procCount <= 0) return;
            self.bulletCount = (uint)procCount;
            self.maxSpread += spreadConeHalfAngleDegr;
            self.spreadPitchScale = 1f;
            self.spreadYawScale = 1f;
            orig(self);
        }

        private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, RoR2.Projectile.FireProjectileInfo fireProjectileInfo) {
            orig(self, fireProjectileInfo);
            if(ignoreMugs || !fireProjectileInfo.owner) return;
            var cpt = fireProjectileInfo.owner.GetComponent<CharacterBody>();
            if(!cpt) return;
            var count = GetCount(cpt);
            if(count <= 0) return;
            var totalChance = count * procChance;
            int procCount = (Util.CheckRoll(Wrap(totalChance, 0f, 100f), cpt.master) ? 1 : 0) + (int)Mathf.Floor(totalChance);
            var origRot = fireProjectileInfo.rotation;
            for(var i = 0; i < procCount; i++) {
                fireProjectileInfo.rotation = origRot * Quaternion.Euler(
                    (rng.nextNormalizedFloat - 0.5f) * spreadConeHalfAngleDegr,
                    (rng.nextNormalizedFloat - 0.5f) * spreadConeHalfAngleDegr,
                    (rng.nextNormalizedFloat - 0.5f) * spreadConeHalfAngleDegr);
                orig(self, fireProjectileInfo);
            }
        }
        #endregion
    }

    public class TkSatMugAchievement : RoR2.Achievements.BaseAchievement, IModdedUnlockableDataProvider {
        public string AchievementIdentifier => "TKSAT_MUG_ACHIEVEMENT_ID";
        public string UnlockableIdentifier => "TKSAT_MUG_UNLOCKABLE_ID";
        public string PrerequisiteUnlockableIdentifier => "";
        public string AchievementNameToken => "TKSAT_MUG_ACHIEVEMENT_NAME";
        public string AchievementDescToken => "TKSAT_MUG_ACHIEVEMENT_DESCRIPTION";
        public string UnlockableNameToken => Mug.instance.nameToken;

        public Sprite Sprite => TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/mugIcon.png");

        public System.Func<string> GetHowToUnlock => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

        public System.Func<string> GetUnlocked => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

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