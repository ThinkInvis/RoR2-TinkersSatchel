using RoR2;
using UnityEngine;
using TILER2;
using System.Linq;
using RoR2.Navigation;
using UnityEngine.Networking;
using System.Collections.Generic;
using R2API;
using static TILER2.MiscUtil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class Lodestone : Equipment<Lodestone> {

        ////// Equipment Data //////

        public override string displayName => "Lodestone";
        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered => true;
        public override float cooldown { get; protected set; } = 20f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Pull nearby enemies and allied item effects.";
        protected override string GetDescString(string langid = null) => $"<style=cIsUtility>Pull</style> enemies within {enemyRange:N0} m towards yourself for <style=cIsDamage>{Pct(baseDamageFrac)} base damage</style>. <style=cIsUtility>Pull</style> drops, orbs, and projectiles caused by ally items within {objectRange:N0} m to your location.";
        protected override string GetLoreString(string langid = null) => $"";



        ////// Config //////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of base damage to inflict.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseDamageFrac { get; private set; } = 4f;

        [AutoConfig("Range for pulling enemies.", AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever, 0f, float.MaxValue)]
        public float enemyRange { get; private set; } = 40f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Range for pulling other objects.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float objectRange { get; private set; } = 150f;



        ////// Other Fields/Properties //////

        private GameObject blackHolePrefab;
        public HashSet<string> validObjectNamesRB { get; private set; } = new HashSet<string>();
        public HashSet<string> validObjectNamesNoRB { get; private set; } = new HashSet<string>();
        const float PULL_FORCE = 60f;
        internal static UnlockableDef unlockable;



        ////// TILER2 Module Setup //////

        public Lodestone() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Lodestone.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/lodestoneIcon.png");
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
            validObjectNamesNoRB.UnionWith(new[] { //may have RB, but should teleport anyways
                "DeskplantWard(Clone)",
                "CrippleWard(Clone)",
                "WarbannerWard(Clone)",
                "DamageZoneWard(Clone)"
            });

            unlockable = UnlockableAPI.AddUnlockable<TkSatLodestoneAchievement>();
            LanguageAPI.Add("TKSAT_LODESTONE_ACHIEVEMENT_NAME", "Drive Me Closer");
            LanguageAPI.Add("TKSAT_LODESTONE_ACHIEVEMENT_DESCRIPTION", "Item Set: Close-range. Have 6 or more (of 15) at once.");

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
            var rbObjectsInRange = Physics.OverlapSphere(slot.characterBody.corePosition, objectRange, Physics.AllLayers, QueryTriggerInteraction.Collide)
                .Select(x => x.gameObject)
                .Where(x => validObjectNamesRB.Contains(x.name))
                .Select(x => x.GetComponent<Rigidbody>())
                .Where(x => x);
            var nonRbObjectsInRange = GameObject.FindObjectsOfType<GameObject>() //TODO: add colliders to all of these prefabs
                .Where(x => validObjectNamesNoRB.Contains(x.name)
                    && Vector3.Distance(x.transform.position, slot.characterBody.corePosition) < objectRange);

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
            var teamMembers = new List<TeamComponent>();
            bool isFF = FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off;
            var scan = ((TeamIndex[])Enum.GetValues(typeof(TeamIndex)));
            var myTeam = TeamComponent.GetObjectTeam(slot.characterBody.gameObject);
            foreach(var ind in scan) {
                if(isFF || myTeam != ind)
                    teamMembers.AddRange(TeamComponent.GetTeamMembers(ind));
            }
            teamMembers.Remove(slot.characterBody.teamComponent);
            float sqrad = enemyRange * enemyRange;
            foreach(TeamComponent tcpt in teamMembers) {
                var velVec = slot.characterBody.transform.position - tcpt.transform.position;
                if(velVec.sqrMagnitude <= sqrad && tcpt.body && !tcpt.body.isBoss && !tcpt.body.isChampion && tcpt.body.isActiveAndEnabled) {
                    var (vInitial, _) = MiscUtil.CalculateVelocityForFinalPosition(tcpt.transform.position, slot.characterBody.transform.position, 1f);
                    var mcpt = tcpt.body.GetComponent<IPhysMotor>();
                    tcpt.body.healthComponent.TakeDamage(new DamageInfo {
                        attacker = slot.characterBody.gameObject,
                        crit = slot.characterBody.RollCrit(),
                        damage = slot.characterBody.damage * baseDamageFrac,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic | DamageType.AOE,
                        canRejectForce = false,
                        force = (vInitial - ((mcpt != null) ? mcpt.velocity : Vector3.zero)) * ((mcpt != null) ? mcpt.mass : 1f),
                        position = tcpt.body.corePosition,
                        procChainMask = default,
                        procCoefficient = 1f
                    });
                }
            }
        }
    }

    public class TkSatLodestoneAchievement : RoR2.Achievements.BaseAchievement, IModdedUnlockableDataProvider {
        public string AchievementIdentifier => "TKSAT_LODESTONE_ACHIEVEMENT_ID";
        public string UnlockableIdentifier => "TKSAT_LODESTONE_UNLOCKABLE_ID";
        public string PrerequisiteUnlockableIdentifier => "";
        public string AchievementNameToken => "TKSAT_LODESTONE_ACHIEVEMENT_NAME";
        public string AchievementDescToken => "TKSAT_LODESTONE_ACHIEVEMENT_DESCRIPTION";
        public string UnlockableNameToken => Lodestone.instance.nameToken;

        public Sprite Sprite => TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/lodestoneIcon.png");

        public System.Func<string> GetHowToUnlock => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

        public System.Func<string> GetUnlocked => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

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
            if(Moustache.instance.GetCount(self.inventory) > 0 || VoidMoustache.instance.GetCount(self.inventory) > 0) matches++;

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