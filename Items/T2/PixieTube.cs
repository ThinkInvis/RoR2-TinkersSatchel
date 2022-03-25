using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class PixieTube : Item<PixieTube> {

        ////// Item Data //////

        public override string displayName => "Pixie Tube";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage, ItemTag.Utility });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Drop random buffs on using non-primary skills.";
        protected override string GetDescString(string langid = null) => $"You drop 1 (+1 per stack) random <style=cIsUtility>elemental wisp</style> when you <style=cIsUtility>use a non-primary skill</style>. <style=cIsUtility>Elemental wisps</style> can be picked up by any ally as a small, stacking buff for 10 seconds: <color=#ffaa77>+3% damage</color>, <color=#9999ff>5% movement speed</color>, <color=#eeff55>5% attack speed</color>, or <color=#997755>10 armor</color>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////




        ////// Other Fields/Properties //////

        GameObject[] prefabs;
        BuffDef moveBuff;
        BuffDef attackBuff;
        BuffDef damageBuff;
        BuffDef armorBuff;



        ////// TILER2 Module Setup //////
        
        public PixieTube() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/PixieTube.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/pixieTubeIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var colors = new[] { Color.blue, Color.yellow, Color.red, Color.green };
            var sharedBuffIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texBarrelIcon.tif")
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
            attackBuff.buffColor = colors[0];
            attackBuff.canStack = true;
            attackBuff.isDebuff = false;
            attackBuff.name = modInfo.shortIdentifier + "PixieAttackSpeed";
            attackBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(attackBuff);

            damageBuff = ScriptableObject.CreateInstance<BuffDef>();
            damageBuff.buffColor = colors[0];
            damageBuff.canStack = true;
            damageBuff.isDebuff = false;
            damageBuff.name = modInfo.shortIdentifier + "PixieDamage";
            damageBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(damageBuff);

            armorBuff = ScriptableObject.CreateInstance<BuffDef>();
            armorBuff.buffColor = colors[0];
            armorBuff.canStack = true;
            armorBuff.isDebuff = false;
            armorBuff.name = modInfo.shortIdentifier + "PixieArmor";
            armorBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(armorBuff);

            var origPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HealPack");
            var prefabs = new[] {
                origPrefab.InstantiateClone("TkSatTempSetupPrefab1", false),
                origPrefab.InstantiateClone("TkSatTempSetupPrefab2", false),
                origPrefab.InstantiateClone("TkSatTempSetupPrefab3", false),
                origPrefab.InstantiateClone("TkSatTempSetupPrefab4", false) };
            var bufftypes = new[] { moveBuff, attackBuff, damageBuff, armorBuff };
            var finalNames = new[] { "TkSatPixieMovePack", "TkSatPixieAttackPack", "TkSatPixieDamagePack", "TkSatPixieArmorPack" };
            for(var i = 0; i < prefabs.Length; i++) {
                var prefab = prefabs[i];

                var trail = prefab.transform.Find("HealthOrbEffect/TrailParent/Trail").gameObject;
                var tren = trail.GetComponent<TrailRenderer>();
                tren.material.SetTexture("_RemapTex", rampTex);

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
                bpkp.buffDuration = 10f;
                bpkp.baseObject = prefabs[i];

                var grav = prefab.transform.Find("GravitationController").gameObject;
                var delay = prefab.AddComponent<ActivateAfterDelay>();
                pickup.SetActive(false);
                grav.SetActive(false);
                delay.targets.Add(pickup);
                delay.targets.Add(grav);
                delay.delay = 2.5f;

                prefabs[i] = prefabs[i].InstantiateClone(finalNames[i], true);
            }
        }

        public override void Install() {
            base.Install();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
        }

        public override void Uninstall() {
            base.Uninstall();
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
        }



        ////// Hooks //////

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill) {
            orig(self, skill);
            if(!NetworkServer.active) return;
            if(self && self.skillLocator
                && self.skillLocator.FindSkillSlot(skill) != SkillSlot.Primary) {
                var count = GetCount(self);
                for(var i = 0; i < count; i++) {
                    var orb = Object.Instantiate(rng.NextElementUniform(prefabs), self.transform.position, UnityEngine.Random.rotation);
                    if(self.teamComponent)
                        orb.GetComponent<TeamFilter>().teamIndex = self.teamComponent.teamIndex;
                    NetworkServer.Spawn(orb);
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            if(!sender) return;
            args.armorAdd += sender.GetBuffCount(armorBuff) * 10f;
            args.attackSpeedMultAdd += sender.GetBuffCount(attackBuff) * 0.05f;
            args.damageMultAdd += sender.GetBuffCount(damageBuff) * 0.03f;
            args.moveSpeedMultAdd += sender.GetBuffCount(moveBuff) * 0.05f;
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
}