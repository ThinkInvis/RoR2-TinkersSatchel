using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;
using System.Collections.Generic;
using R2API;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class Lodestone : Equipment<Lodestone> {

        ////// Equipment Data //////

        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered { get; protected set; } = true;
        public override float cooldown { get; protected set; } = 20f;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            enemyRange.ToString("N0"), baseDamageFrac.ToString("0%"), objectRange.ToString("N0")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of base damage to inflict.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseDamageFrac { get; private set; } = 4f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc coefficient of the equipment attack.", AutoConfigFlags.None, 0f, 1f)]
        public float procCoefficient { get; private set; } = 1f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 1000f)]
        [AutoConfig("Range for pulling enemies.", AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever, 0f, float.MaxValue)]
        public float enemyRange { get; private set; } = 40f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Range for pulling other objects.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float objectRange { get; private set; } = 150f;



        ////// Other Fields/Properties //////

        private GameObject blackHolePrefab;
        public HashSet<string> validObjectNamesRB { get; private set; } = new HashSet<string>();
        const float PULL_FORCE = 60f;
        internal static UnlockableDef unlockable;
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public Lodestone() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Lodestone.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/lodestoneIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Lodestone.prefab");
        }

        public override void SetupModifyEquipmentDef() {
            base.SetupModifyEquipmentDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.22045F, -0.06626F, 0.11193F),
                localAngles = new Vector3(359.0299F, 357.3219F, 25.2928F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.38728F, 0.00965F, -0.06446F),
                localAngles = new Vector3(31.87035F, 332.9695F, 3.18838F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.23353F, -0.00868F, -0.08696F),
                localAngles = new Vector3(27.00084F, 326.5775F, 4.93487F),
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

            var tempPfb = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GravSphere").InstantiateClone("temporary setup prefab", false);
            var proj = tempPfb.GetComponent<RoR2.Projectile.ProjectileSimple>();
            proj.desiredForwardSpeed = 0;
            proj.lifetime = 0.5f;
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
            var spsShape = spsPart.shape;
            spsShape.radius = 30f;

            blackHolePrefab = tempPfb.InstantiateClone("LodestoneProcPrefab", true);
            UnityEngine.Object.Destroy(tempPfb);

            ContentAddition.AddProjectile(blackHolePrefab);

            validObjectNamesRB.UnionWith(new[] {
                "HealPack(Clone)",
                "StickyBomb(Clone)",
                "TkSatPixieMovePack(Clone)",
                "TkSatPixieAttackPack(Clone)",
                "TkSatPixieDamagePack(Clone)",
                "TkDatPixieArmorPack(Clone)",
                "AmmoPack(Clone)",
                "BonusMoneyPack(Clone)",
                "ShurikenProjectile(Clone)",
                "FireMeatBall(Clone)",
                "DeathProjectile(Clone)",
                "BeamSphere(Clone)",
                "GravSphere(Clone)",
                "Sawmerang(Clone)",
                "LunarSunProjectile(Clone)"
            });

            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/lodestoneIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            equipmentDef.unlockableDef = unlockable;
        }

        public override void Install() {
            base.Install();
        }

        public override void Uninstall() {
            base.Uninstall();
        }



        ////// Hooks //////

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            if(!slot.characterBody) return false;

            RoR2.Projectile.ProjectileManager.instance.FireProjectile(
                blackHolePrefab,
                slot.characterBody.corePosition, Quaternion.identity,
                slot.characterBody.gameObject,
                0f, 0f, false);

            PullEnemies(slot);
            PullObjects(slot);

            return true;
        }

        void PullObjects(EquipmentSlot slot) {
            float range = objectRange;

            var rbObjectsInRange = Physics.OverlapSphere(slot.characterBody.corePosition, range, Physics.AllLayers, QueryTriggerInteraction.Collide)
                .Select(x => x.gameObject)
                .Where(x => validObjectNamesRB.Contains(x.name))
                .Select(x => x.GetComponent<Rigidbody>())
                .Where(x => x);
            var nonRbObjectsInRange =
                MiscObjectTrackerModule.readOnlyWarbanners
                .Union(MiscObjectTrackerModule.readOnlyCrippleWards)
                .Union(MiscObjectTrackerModule.readOnlyDeskplants)
                .Union(MiscObjectTrackerModule.readOnlyRandomDamageZones)
                .Where(x => Vector3.Distance(x.transform.position, slot.characterBody.corePosition) < range);

            foreach(var rb in rbObjectsInRange) {
                var sticky = rb.gameObject.GetComponent<RoR2.Projectile.ProjectileStickOnImpact>();
                if(sticky) {
                    sticky.Detach();
                    sticky.enabled = false;
                }

                var velVec = slot.characterBody.transform.position - rb.transform.position;

                if(rb.useGravity && !rb.gameObject.name.Contains("TkSatPixie")) {
                    var (vInitial, tFinal) = MiscUtil.CalculateVelocityForFinalPosition(rb.transform.position, slot.characterBody.transform.position, 1f);
                    velVec = vInitial;
                } else {
                    velVec.Normalize();
                    velVec *= PULL_FORCE;
                }
                rb.AddForce(velVec - rb.velocity, ForceMode.VelocityChange);
            }

            foreach(var nrb in nonRbObjectsInRange) {
                nrb.transform.position = slot.characterBody.corePosition;
            }
        }

        void PullEnemies(EquipmentSlot slot) {
            float range = enemyRange;
            float damage = slot.characterBody.damage * baseDamageFrac;

            var teamMembers = new List<TeamComponent>();
            bool isFF = FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off;
            var scan = ((TeamIndex[])Enum.GetValues(typeof(TeamIndex)));
            var myTeam = TeamComponent.GetObjectTeam(slot.characterBody.gameObject);
            foreach(var ind in scan) {
                if(isFF || myTeam != ind)
                    teamMembers.AddRange(TeamComponent.GetTeamMembers(ind));
            }
            teamMembers.Remove(slot.characterBody.teamComponent);
            float sqrad = range * range;
            foreach(TeamComponent tcpt in teamMembers) {
                var velVec = slot.characterBody.transform.position - tcpt.transform.position;
                if(velVec.sqrMagnitude <= sqrad && tcpt.body && !tcpt.body.isBoss && !tcpt.body.isChampion && tcpt.body.isActiveAndEnabled) {
                    var (vInitial, _) = MiscUtil.CalculateVelocityForFinalPosition(tcpt.transform.position, slot.characterBody.transform.position, 1f);
                    var mcpt = tcpt.body.GetComponent<IPhysMotor>();
                    tcpt.body.healthComponent.TakeDamage(new DamageInfo {
                        attacker = slot.characterBody.gameObject,
                        crit = slot.characterBody.RollCrit(),
                        damage = damage,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = new DamageTypeCombo(DamageType.Generic | DamageType.AOE, DamageTypeExtended.Generic, DamageSource.Equipment),
                        canRejectForce = false,
                        force = (vInitial - ((mcpt != null) ? mcpt.velocity : Vector3.zero)) * ((mcpt != null) ? mcpt.mass : 1f),
                        position = tcpt.body.corePosition,
                        procChainMask = default,
                        procCoefficient = procCoefficient
                    });
                }
            }
        }
    }

    [RegisterAchievement("TkSat_Lodestone", "TkSat_LodestoneUnlockable", "", 2u)]
    public class TkSatLodestoneAchievement : RoR2.Achievements.BaseAchievement {
        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
        }

        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
        }

        private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self) {
            orig(self);
            if(localUser.cachedMaster != self) return;
            int matches = 0;
            if(self.inventory.GetItemCount(RoR2Content.Items.Mushroom) > 0) matches++;
            if(self.inventory.GetItemCount(RoR2Content.Items.NearbyDamageBonus) > 0) matches++;
            if(Moustache.instance.GetCount(self.inventory) > 0 || VillainousVisage.instance.GetCount(self.inventory) > 0) matches++;

            if(self.inventory.GetItemCount(RoR2Content.Items.Thorns) > 0) matches++;
            if(KleinBottle.instance.GetCount(self.inventory) > 0) matches++;

            if(self.inventory.GetItemCount(RoR2Content.Items.Icicle) > 0) matches++;
            if(self.inventory.GetItemCount(RoR2Content.Items.ShockNearby) > 0) matches++;
            if(self.inventory.GetItemCount(RoR2Content.Items.NovaOnHeal) > 0) matches++;
            if(Headset.instance.GetCount(self.inventory) > 0) matches++;

            if(self.inventory.GetItemCount(RoR2Content.Items.SiphonOnLowHealth) > 0) matches++;
            if(self.inventory.GetItemCount(RoR2Content.Items.SprintWisp) > 0) matches++;

            if(self.inventory.GetItemCount(DLC1Content.Items.LunarSun) > 0) matches++;

            if(self.inventory.currentEquipmentIndex == RoR2Content.Equipment.Cleanse.equipmentIndex
                || self.inventory.alternateEquipmentIndex == RoR2Content.Equipment.Cleanse.equipmentIndex)
                matches++;
            if(self.inventory.currentEquipmentIndex == RoR2Content.Equipment.BurnNearby.equipmentIndex
                || self.inventory.alternateEquipmentIndex == RoR2Content.Equipment.BurnNearby.equipmentIndex)
                matches++;
            if(matches >= 6)
                Grant();
        }
    }
}