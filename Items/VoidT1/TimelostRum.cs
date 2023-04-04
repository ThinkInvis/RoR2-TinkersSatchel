using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using System.Collections.Generic;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class TimelostRum : Item<TimelostRum> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.VoidTier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Damage});

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            procChance.ToString("0%"), delayTime.ToString("N0")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Delay per extra projectile.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float delayTime { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra projectile chance per item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float procChance { get; private set; } = 0.1f;



        /////// Other Fields/Properties //////

        public int ignoreStack = 0;
        public List<(BulletAttack bi, float timestamp, float delay)> delayedBulletAttacks = new();
        public List<(FireProjectileInfo fpi, float timestamp, float delay)> delayedProjectiles = new();
        public GameObject idrPrefab { get; private set; }
		
		

        ////// TILER2 Module Setup //////
        #region TILER2 Module Setup
        public TimelostRum() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/TimelostRum.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/timelostRumIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/TimelostRum.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.13119F, -0.06393F, 0.16501F),
                localAngles = new Vector3(346.8067F, 149.1297F, 148.1501F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.2665F, 0.11811F, -0.16573F),
                localAngles = new Vector3(22.71485F, 296.1542F, 2.15007F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.18271F, 0.07455F, 0.18935F),
                localAngles = new Vector3(341.6932F, 224.9207F, 311.7038F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-2.63318F, 1.12878F, -0.76901F),
                localAngles = new Vector3(338.0058F, 81.70578F, 309.2676F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.14437F, -0.07553F, 0.19465F),
                localAngles = new Vector3(10.26335F, 206.6532F, 332.7241F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(-0.11582F, -0.02862F, 0.15584F),
                localAngles = new Vector3(39.45496F, 143.1629F, 354.4092F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.10281F, 0.05827F, 0.18212F),
                localAngles = new Vector3(21.50457F, 126.9122F, 37.36323F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(-0.12145F, -0.01252F, 0.16036F),
                localAngles = new Vector3(18.989F, 142.332F, 10.45503F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(-0.14285F, -0.05033F, 0.13735F),
                localAngles = new Vector3(23.13149F, 126.7038F, 336.1982F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(-1.24654F, 3.66212F, -2.11572F),
                localAngles = new Vector3(60.13687F, 53.0797F, 164.6161F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FlowerBase",
                localPos = new Vector3(0.75783F, 0.7607F, 0.26494F),
                localAngles = new Vector3(51.73802F, 240.3004F, 148.5376F),
                localScale = new Vector3(0.7F, 0.7F, 0.7F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.41541F, -0.09895F, -0.00461F),
                localAngles = new Vector3(10.09852F, 293.4927F, 1.29182F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.17329F, 0.23201F, 0.15111F),
                localAngles = new Vector3(60.5887F, 239.5703F, 206.8694F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            itemDef.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset")
                .WaitForCompletion();

            On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
                var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                isp.relationships = new[] {new ItemDef.Pair {
                    itemDef1 = Mug.instance.itemDef,
                    itemDef2 = itemDef
                }};
                orig(providers.Concat(new[] { isp }).ToArray());
            };
        }

        public override void SetupBehavior() {
            base.SetupBehavior();

            itemDef.unlockableDef = Mug.instance.unlockable;
        }

        public override void Install() {
            base.Install();

            //main tracking
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;

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
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= ProjectileManager_FireProjectile_FireProjectileInfo;
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;
            On.RoR2.Run.FixedUpdate -= Run_FixedUpdate;

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
        }
        #endregion



        ////// Hooks //////
        #region Hooks
        private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self) {
            orig(self);
            if(ignoreStack > 0) {
                TinkersSatchelPlugin._logger.LogError("TimelostRum: ignoreStack was not empty on new frame, clearing. May be a cascading effect of another error, or a mod may be misusing ignoreStack.");
                ignoreStack = 0;
            }
            ignoreStack++;
            for(var i = delayedBulletAttacks.Count - 1; i >= 0; i--) {
                if(delayedBulletAttacks[i].bi == null) {
                    delayedBulletAttacks.RemoveAt(i);
                    continue;
                }

                if(Time.fixedTime - delayedBulletAttacks[i].timestamp > delayedBulletAttacks[i].delay) {
                    delayedBulletAttacks[i].bi.Fire();
                    delayedBulletAttacks.RemoveAt(i);
                }
            }
            for(var i = delayedProjectiles.Count - 1; i >= 0; i--) {
                if(Time.fixedTime - delayedProjectiles[i].timestamp > delayedProjectiles[i].delay) {
                    ProjectileManager.instance.FireProjectile(delayedProjectiles[i].fpi);
                    delayedProjectiles.RemoveAt(i);
                }
            }
            ignoreStack--;
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
            var offsetOrigin = self.origin;
            if(self.weapon && self.weapon.TryGetComponent<ModelLocator>(out var mloc) && mloc.modelTransform && mloc.modelTransform.TryGetComponent<ChildLocator>(out var cloc)) {
                var muzzle = cloc.FindChild(self.muzzleName);
                if(muzzle)
                    offsetOrigin = muzzle.position;
            }
            for(var i = 1; i <= procCount; i++)
                delayedBulletAttacks.Add((new BulletAttack {
                    aimVector = self.aimVector, bulletCount = self.bulletCount, damage = self.damage,
                    damageColorIndex = self.damageColorIndex, damageType = self.damageType, falloffModel = self.falloffModel,
                    filterCallback = self.filterCallback, force = self.force, hitCallback = self.hitCallback,
                    HitEffectNormal = self.HitEffectNormal, hitEffectPrefab = self.hitEffectPrefab, hitMask = self.hitMask,
                    isCrit = self.isCrit, maxDistance = self.maxDistance, maxSpread = self.maxSpread,
                    minSpread = self.minSpread, modifyOutgoingDamageCallback = self.modifyOutgoingDamageCallback, muzzleName = self.muzzleName,
                    origin = offsetOrigin, owner = self.owner, procChainMask = self.procChainMask,
                    procCoefficient = self.procCoefficient, queryTriggerInteraction = self.queryTriggerInteraction, radius = self.radius,
                    smartCollision = self.smartCollision, sniper = self.sniper, spreadPitchScale = self.spreadPitchScale,
                    spreadYawScale = self.spreadYawScale, stopperMask = self.stopperMask, tracerEffectPrefab = self.tracerEffectPrefab,
                    weapon = CommonCode.worldSpaceWeaponDummy
                }, Time.fixedTime, i * delayTime));
        }

        private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, ProjectileManager self, FireProjectileInfo fireProjectileInfo) {
            orig(self, fireProjectileInfo);
            if(ignoreStack > 0 || !self || !fireProjectileInfo.owner || !fireProjectileInfo.projectilePrefab || fireProjectileInfo.projectilePrefab.GetComponent<Deployable>()) return;
            var cpt = fireProjectileInfo.owner.GetComponent<CharacterBody>();
            if(!cpt) return;
            var count = GetCount(cpt);
            if(count <= 0) return;
            var totalChance = count * procChance;
            int procCount = (Util.CheckRoll(Wrap(totalChance * 100f, 0f, 100f), cpt.master) ? 1 : 0) + (int)Mathf.Floor(totalChance);
            for(var i = 1; i <= procCount; i++)
                delayedProjectiles.Add((fireProjectileInfo, Time.fixedTime, i * delayTime));
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            ignoreStack++;
            orig(self, damageInfo, victim);
            ignoreStack--;
        }

        private void BaseThrowBombState_Fire(On.EntityStates.Mage.Weapon.BaseThrowBombState.orig_Fire orig, EntityStates.Mage.Weapon.BaseThrowBombState self) {
            var doIgnore = self is EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary || self is EntityStates.Mage.Weapon.ThrowIcebomb;
            if(doIgnore) ignoreStack++;
            orig(self);
            if(doIgnore) ignoreStack--;
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
            ignoreStack--;
        }

        private void AimFlower_FireProjectile(On.EntityStates.Treebot.Weapon.AimFlower.orig_FireProjectile orig, EntityStates.Treebot.Weapon.AimFlower self) {
            ignoreStack++;
            orig(self);
            ignoreStack--;
        }

        private void TreebotFireFruitSeed_OnEnter(On.EntityStates.Treebot.TreebotFireFruitSeed.orig_OnEnter orig, EntityStates.Treebot.TreebotFireFruitSeed self) {
            ignoreStack++;
            orig(self);
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
            ignoreStack--;
        }

        private void AimThrowableBase_FireProjectile(On.EntityStates.AimThrowableBase.orig_FireProjectile orig, EntityStates.AimThrowableBase self) {
            var doIgnore = self is EntityStates.Treebot.Weapon.AimMortar2 || self is EntityStates.Captain.Weapon.CallAirstrikeBase;
            if(doIgnore) ignoreStack++;
            orig(self);
            if(doIgnore) ignoreStack--;
        }

        private void FireMortar2_Fire(On.EntityStates.Treebot.Weapon.FireMortar2.orig_Fire orig, EntityStates.Treebot.Weapon.FireMortar2 self) {
            ignoreStack++;
            orig(self);
            ignoreStack--;
        }

        private bool EquipmentSlot_FireGummyClone(On.RoR2.EquipmentSlot.orig_FireGummyClone orig, EquipmentSlot self) {
            ignoreStack++;
            var retv = orig(self);
            ignoreStack--;
            return retv;
        }
        #endregion
    }
}