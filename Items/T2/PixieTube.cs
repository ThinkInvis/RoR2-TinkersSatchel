using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class PixieTube : Item<PixieTube> {

        ////// Item Data //////

        public override string displayName => "Pixie Tube";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage, ItemTag.Utility });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Drop random buffs on using non-primary skills.";
        protected override string GetDescString(string langid = null) => $"You drop 1 <style=cStack>(+1 per stack)</style> random <style=cIsUtility>elemental wisp</style> when you <style=cIsUtility>use a non-primary skill</style>. <style=cIsUtility>Elemental wisps</style> can be picked up by any ally as a small, stacking buff for {buffDuration:N0} seconds: <color=#ffaa77>+{Pct(buffDamageAmt)} damage</color>, <color=#9999ff>+{Pct(buffMoveAmt)} movement speed</color>, <color=#eeff55>+{Pct(buffAttackAmt)} attack speed</color>, or <color=#997755>{buffArmorAmt:N0} armor</color>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfig("Duration of all Pixie Tube buffs.", AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever, 0f, float.MaxValue)]
        public float buffDuration { get; private set; } = 10f;

        [AutoConfig("Fractional move speed bonus from the Water buff.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffMoveAmt { get; private set; } = 0.05f;

        [AutoConfig("Fractional attack speed bonus from the Air buff.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffAttackAmt { get; private set; } = 0.05f;

        [AutoConfig("Fractional damage bonus from the Fire buff.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffDamageAmt { get; private set; } = 0.03f;

        [AutoConfig("Flat armor bonus from the Earth buff.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffArmorAmt { get; private set; } = 10f;



        ////// Other Fields/Properties //////

        GameObject[] prefabs;
        BuffDef moveBuff;
        BuffDef attackBuff;
        BuffDef damageBuff;
        BuffDef armorBuff;
        RoR2.Skills.SkillDef[] blacklistedSkills;
        const float PICKUP_ARMING_DELAY = 2f;



        ////// TILER2 Module Setup //////

        public PixieTube() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/PixieTube.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/pixieTubeIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            blacklistedSkills = new[] {
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiCancelTargetingDummy"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiConfirmTargetDummy"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceTurret"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceWalkerTurret"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiHarpoons"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/ToolbotBody/ToolbotBodySwap"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/CaptainCancelDummy"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/PrepSupplyDrop")
            };

            var colors = new[] { new Color(0.15f, 0.25f, 1f), new Color(1f, 1f, 0.4f), new Color(1f, 0.25f, 0.1f), new Color(0.5f, 0.35f, 0.2f) };
            var sharedBuffIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texBarrelIcon.png")
                .WaitForCompletion();
            var rampTex = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampDefault.png")
                .WaitForCompletion();

            moveBuff = ScriptableObject.CreateInstance<BuffDef>();
            moveBuff.buffColor = colors[0];
            moveBuff.canStack = true;
            moveBuff.isDebuff = false;
            moveBuff.name = modInfo.shortIdentifier + "PixieMoveSpeed";
            moveBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(moveBuff);

            attackBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackBuff.buffColor = colors[1];
            attackBuff.canStack = true;
            attackBuff.isDebuff = false;
            attackBuff.name = modInfo.shortIdentifier + "PixieAttackSpeed";
            attackBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(attackBuff);

            damageBuff = ScriptableObject.CreateInstance<BuffDef>();
            damageBuff.buffColor = colors[2];
            damageBuff.canStack = true;
            damageBuff.isDebuff = false;
            damageBuff.name = modInfo.shortIdentifier + "PixieDamage";
            damageBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(damageBuff);

            armorBuff = ScriptableObject.CreateInstance<BuffDef>();
            armorBuff.buffColor = colors[3];
            armorBuff.canStack = true;
            armorBuff.isDebuff = false;
            armorBuff.name = modInfo.shortIdentifier + "PixieArmor";
            armorBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(armorBuff);

            var origPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HealPack");
            prefabs = new[] {
                origPrefab.InstantiateClone("TkSatTempSetupPrefab1", false),
                origPrefab.InstantiateClone("TkSatTempSetupPrefab2", false),
                origPrefab.InstantiateClone("TkSatTempSetupPrefab3", false),
                origPrefab.InstantiateClone("TkSatTempSetupPrefab4", false) };
            var bufftypes = new[] { moveBuff, attackBuff, damageBuff, armorBuff };
            var finalNames = new[] { "TkSatPixieMovePack", "TkSatPixieAttackPack", "TkSatPixieDamagePack", "TkSatPixieArmorPack" };
            for(var i = 0; i < prefabs.Length; i++) {
                var prefab = prefabs[i];
                
                var vros = prefab.GetComponent<VelocityRandomOnStart>();
                vros.enabled = false;

                var dstroy = prefab.GetComponent<DestroyOnTimer>();
                dstroy.duration += PICKUP_ARMING_DELAY;

                var blinker = prefab.GetComponent<BeginRapidlyActivatingAndDeactivating>();
                blinker.delayBeforeBeginningBlinking = dstroy.duration - 1f;

                var trail = prefab.transform.Find("HealthOrbEffect/TrailParent/Trail").gameObject;
                var tren = trail.GetComponent<TrailRenderer>();
                tren.material.SetTexture("_RemapTex", rampTex);
                tren.material.SetColor("_TintColor", colors[i]);

                var core = prefab.transform.Find("HealthOrbEffect/VFX/Core").gameObject;
                var cren = core.GetComponent<ParticleSystem>();
                var ccol = cren.colorOverLifetime;
                ccol.color = new ParticleSystem.MinMaxGradient(colors[i], colors[i].AlphaMultiplied(0f));

                var pulse = prefab.transform.Find("HealthOrbEffect/VFX/PulseGlow").gameObject;
                var pren = pulse.GetComponent<ParticleSystem>();
                var pcol = pren.colorOverLifetime;
                pcol.color = new ParticleSystem.MinMaxGradient(colors[i], colors[i].AlphaMultiplied(0f));

                var pickup = prefab.transform.Find("PickupTrigger").gameObject;
                pickup.GetComponent<HealthPickup>().enabled = false;
                var bpkp = pickup.AddComponent<BuffPickup>();
                bpkp.buffDef = bufftypes[i];
                bpkp.teamFilter = prefab.GetComponent<TeamFilter>();
                bpkp.pickupEffect = pickup.GetComponent<HealthPickup>().pickupEffect;
                bpkp.buffDuration = buffDuration;
                bpkp.baseObject = prefabs[i];

                var grav = prefab.transform.Find("GravitationController").gameObject;
                var delay = prefab.AddComponent<ActivateAfterDelay>();
                pickup.SetActive(false);
                grav.SetActive(false);
                delay.targets.Add(pickup);
                delay.targets.Add(grav);
                delay.delay = PICKUP_ARMING_DELAY;

                var gravramp = grav.AddComponent<GravitationRamp>();
                gravramp.rangeStart = 6f;
                gravramp.rangeEnd = 36f;
                gravramp.duration = dstroy.duration - PICKUP_ARMING_DELAY;

                prefabs[i] = prefabs[i].InstantiateClone(finalNames[i], true);
            }
        }

        public override void Install() {
            base.Install();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.EntityStates.Engi.EngiMissilePainter.Fire.FireMissile += Fire_FireMissile;
            On.EntityStates.Engi.EngiWeapon.PlaceTurret.FixedUpdate += PlaceTurret_FixedUpdate;
        }

        public override void Uninstall() {
            base.Uninstall();
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
            On.EntityStates.Engi.EngiMissilePainter.Fire.FireMissile -= Fire_FireMissile;
        }



        ////// Private Methods //////

        void SpawnWisp(Vector3 pos, TeamIndex team) {
            var vvec = Quaternion.AngleAxis(UnityEngine.Random.value * 360f, Vector3.up) * (new Vector3(1f, 1f, 0f).normalized * 15f);
            var orb = Object.Instantiate(rng.NextElementUniform(prefabs), pos, UnityEngine.Random.rotation);
            orb.GetComponent<TeamFilter>().teamIndex = team;
            orb.GetComponent<Rigidbody>().velocity = vvec;
            NetworkServer.Spawn(orb);
        }



        ////// Hooks //////

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill) {
            orig(self, skill);
            if(!NetworkServer.active) return;
            if(self && self.skillLocator
                && self.skillLocator.FindSkillSlot(skill) != SkillSlot.Primary
                && !blacklistedSkills.Contains(skill.skillDef)) {
                var count = GetCount(self);
                for(var i = 0; i < count; i++) {
                    SpawnWisp(self.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None);
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            if(!sender) return;
            args.armorAdd += sender.GetBuffCount(armorBuff) * buffArmorAmt;
            args.attackSpeedMultAdd += sender.GetBuffCount(attackBuff) * buffAttackAmt;
            args.damageMultAdd += sender.GetBuffCount(damageBuff) * buffDamageAmt;
            args.moveSpeedMultAdd += sender.GetBuffCount(moveBuff) * buffMoveAmt;
        }

        private void Fire_FireMissile(On.EntityStates.Engi.EngiMissilePainter.Fire.orig_FireMissile orig, EntityStates.Engi.EngiMissilePainter.Fire self, HurtBox target, Vector3 position) {
            orig(self, target, position);
            var count = GetCount(self.characterBody);
            for(var i = 0; i < count; i++) {
                SpawnWisp(self.characterBody.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None);
            }
        }

        private void PlaceTurret_FixedUpdate(On.EntityStates.Engi.EngiWeapon.PlaceTurret.orig_FixedUpdate orig, EntityStates.Engi.EngiWeapon.PlaceTurret self) {
            orig(self);
            if((self.inputBank.skill1.down || self.inputBank.skill4.justPressed) && self.currentPlacementInfo.ok && self.exitCountdown == 0.25f && self.exitPending) {
                var count = GetCount(self.characterBody);
                for(var i = 0; i < count; i++) {
                    SpawnWisp(self.characterBody.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None);
                }
            }
        }
    }

    public class ActivateAfterDelay : MonoBehaviour {
        public float delay;
        public List<GameObject> targets = new List<GameObject>();

        float stopwatch = 0f;

        void FixedUpdate() {
            if(!isActiveAndEnabled) return;
            if(stopwatch < delay) {
                stopwatch += Time.fixedDeltaTime;
                if(stopwatch >= delay) {
                    foreach(var target in targets) {
                        target.SetActive(true);
                    }
                }
            }
        }

        public void Reset() {
            foreach(var target in targets) {
                target.SetActive(false);
            }
            stopwatch = 0f;
        }
    }

    [RequireComponent(typeof(GravitatePickup), typeof(SphereCollider))]
    public class GravitationRamp : MonoBehaviour {
        public float rangeStart;
        public float rangeEnd;
        public float duration;

        public float stopwatch;

        GravitatePickup grav;
        SphereCollider coll;

        void Awake() {
            grav = GetComponent<GravitatePickup>();
            coll = GetComponent<SphereCollider>();
        }

        void FixedUpdate() {
            if(stopwatch < duration)
                stopwatch += Time.fixedDeltaTime;

            coll.radius = Mathf.Lerp(rangeStart, rangeEnd, stopwatch / duration);
        }
    }
}