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
        protected override string GetPickupString(string langid = null) => "The bigger the fight, the higher your damage.";
        protected override string GetDescString(string langid = null) => $"Gain <style=cIsDamage>+{Pct(stackingDamageFrac, 1)} damage <style=cStack>(+{Pct(stackingDamageFrac, 1)} per stack, linear)</style></style> per in-combat or in-danger enemy within <style=cIsDamage>{maxRange:N0} m</style>. Elites count as {eliteBonus+1 : N1} enemies, bosses count as {champBonus+1 : N1} enemies, and elite bosses count as {eliteBonus+champBonus+1} enemies.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////
        ///
        [AutoConfigRoOSlider("{0:N0} m", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Enemy scan range, in meters.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float maxRange { get; private set; } = 100f;

        [AutoConfigRoOSlider("{0:P2}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fractional damage bonus per enemy per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float stackingDamageFrac { get; private set; } = 0.01f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Enemy count to add per elite in range (also separately counted as normal enemies; total count is this + 1).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float eliteBonus { get; private set; } = 1f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Enemy count to add per champion/boss in range (also separately counted as normal enemies and as elites).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float champBonus { get; private set; } = 2f;



        ////// Other Fields/Properties //////

        public BuffDef moustacheBuff { get; private set; }
        internal static UnlockableDef unlockable;



        ////// TILER2 Module Setup //////
        public Moustache() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Moustache.prefab");
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

            var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
            var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/moustacheIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            LanguageAPI.Add(achiNameToken, "Big Brawl");
            LanguageAPI.Add(achiDescToken, "Participate in a very busy fight.");
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
            sender.SetBuffCount(moustacheBuff.buffIndex, Mathf.FloorToInt(GetCount(sender) * cpt.lastEnemyScoreTracked));
            args.damageMultAdd += cpt.lastEnemyScoreTracked * GetCount(sender) * stackingDamageFrac;
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class MoustacheDamageTracker : MonoBehaviour {
        public float lastEnemyScoreTracked = 0;

        float _stopwatch = 0f;
        CharacterBody body;

        const float UPDATE_INTERVAL = 2f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            _stopwatch--;
            if(_stopwatch < 0f) {
                _stopwatch = UPDATE_INTERVAL;
                var targets = MiscUtil.GatherEnemies(TeamComponent.GetObjectTeam(this.gameObject));
                lastEnemyScoreTracked = 0;
                foreach(var target in targets) {
                    if(Vector3.Distance(target.transform.position, this.transform.position) < Moustache.instance.maxRange) {
                        if(!target.body || (target.body.outOfCombat && target.body.outOfDanger)) continue;
                        lastEnemyScoreTracked += 1f;
                        if(target.body.isElite)
                            lastEnemyScoreTracked += Moustache.instance.eliteBonus;
                        if(target.body.isBoss || target.body.isChampion)
                            lastEnemyScoreTracked += Moustache.instance.champBonus;
                    }
                }

                body.MarkAllStatsDirty();
            }
        }
    }

    [RegisterAchievement("TkSat_Moustache", "TkSat_MoustacheUnlockable", "")]
    public class TkSatMoustacheAchievement : RoR2.Achievements.BaseAchievement {
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
                if(mdt.lastEnemyScoreTracked >= 30f)
                    Grant();
            }
        }
    }
}