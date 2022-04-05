using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
    public class Moustache : Item<Moustache> {

        ////// Item Data //////

        public override string displayName => "Macho Moustache";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Deal more damage when surrounded.";
        protected override string GetDescString(string langid = null) => $"Gain <style=cIsDamage>+{Pct(damageFrac)} base damage <style=cStack>(+{Pct(damageFrac)} per stack, linear)</style></style> per enemy within <style=cIsDamage>{range:N0} m</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Enemy scan range, in meters.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float range { get; private set; } = 10f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fractional damage bonus per enemy per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFrac { get; private set; } = 0.05f;



        ////// Other Fields/Properties //////

        public BuffDef moustacheBuff { get; private set; }
        internal static UnlockableDef unlockable;



        ////// TILER2 Module Setup //////
        public Moustache() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Moustache.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/moustacheIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            moustacheBuff = ScriptableObject.CreateInstance<BuffDef>();
            moustacheBuff.buffColor = new Color(0.85f, 0.2f, 0.2f);
            moustacheBuff.canStack = true;
            moustacheBuff.isDebuff = false;
            moustacheBuff.name = "TKSATMoustache";
            moustacheBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(moustacheBuff);

            unlockable = UnlockableAPI.AddUnlockable<TkSatMoustacheAchievement>();
            LanguageAPI.Add("TKSAT_MOUSTACHE_ACHIEVEMENT_NAME", "Imperiled");
            LanguageAPI.Add("TKSAT_MOUSTACHE_ACHIEVEMENT_DESCRIPTION", "Be very close to 5 or more enemies at once.");

            itemDef.unlockableDef = unlockable;
        }

        public override void Install() {
            base.Install();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        public override void Uninstall() {
            base.Uninstall();
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
        }



        ////// Hooks //////

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(GetCount(body) > 0 && !body.GetComponent<MoustacheDamageTracker>())
                body.gameObject.AddComponent<MoustacheDamageTracker>();
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            if(!sender) return;
            var cpt = sender.GetComponent<MoustacheDamageTracker>();
            if(!cpt) return;
            sender.SetBuffCount(moustacheBuff.buffIndex, GetCount(sender) * cpt.lastEnemiesTracked);
            args.damageMultAdd += cpt.lastEnemiesTracked * GetCount(sender) * damageFrac;
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class MoustacheDamageTracker : MonoBehaviour {
        public int lastEnemiesTracked = 0;

        float _stopwatch = 0f;
        CharacterBody body;

        const float UPDATE_INTERVAL = 2f;

        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        void FixedUpdate() {
            _stopwatch--;
            if(_stopwatch < 0f) {
                _stopwatch = UPDATE_INTERVAL;
                var targets = MiscUtil.GatherEnemies(TeamComponent.GetObjectTeam(this.gameObject));
                lastEnemiesTracked = 0;
                foreach(var target in targets) {
                    if(Vector3.Distance(target.transform.position, this.transform.position) < Moustache.instance.range)
                        lastEnemiesTracked++;
                }

                body.MarkAllStatsDirty();
            }
        }
    }

    public class TkSatMoustacheAchievement : RoR2.Achievements.BaseAchievement, IModdedUnlockableDataProvider {
        public string AchievementIdentifier => "TKSAT_MOUSTACHE_ACHIEVEMENT_ID";
        public string UnlockableIdentifier => "TKSAT_MOUSTACHE_UNLOCKABLE_ID";
        public string PrerequisiteUnlockableIdentifier => "";
        public string AchievementNameToken => "TKSAT_MOUSTACHE_ACHIEVEMENT_NAME";
        public string AchievementDescToken => "TKSAT_MOUSTACHE_ACHIEVEMENT_DESCRIPTION";
        public string UnlockableNameToken => Moustache.instance.nameToken;

        public Sprite Sprite => TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/moustacheIcon.png");

        public System.Func<string> GetHowToUnlock => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

        public System.Func<string> GetUnlocked => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new[] {
            Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
        }
        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.CharacterBody.FixedUpdate -= CharacterBody_FixedUpdate;
        }

        float stopwatch = 0f;
        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);
            if(!self || self != localUser.cachedBody) return;
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                stopwatch = 0.5f;
                var mdt = self.GetComponent<MoustacheDamageTracker>();
                if(!mdt)
                    mdt = self.gameObject.AddComponent<MoustacheDamageTracker>();
                if(mdt.lastEnemiesTracked >= 5)
                    Grant();
            }
        }
    }
}