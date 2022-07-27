using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    public class Moustache : Item<Moustache> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            stackingDamageFrac.ToString("P1"), maxRange.ToString("N0"), (eliteBonus+1f).ToString("N1"), (champBonus+1f).ToString("N1"), (eliteBonus+champBonus+1f).ToString("N1")
        };



        ////// Config ///////
        
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
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////
        public Moustache() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Moustache.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/moustacheIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Moustache.prefab");
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
                localPos = new Vector3(0.00302F, -0.0027F, 0.13631F),
                localAngles = new Vector3(36.27278F, 247.2008F, 328.2445F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.00546F, -0.01886F, 0.16446F),
                localAngles = new Vector3(24.81351F, 237.1489F, 301.2852F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.00014F, 0.15422F, 0.19054F),
                localAngles = new Vector3(24.10973F, 239.8015F, 308.2874F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(-0.00896F, 4.0349F, 0.16361F),
                localAngles = new Vector3(28.74446F, 117.1109F, 49.42778F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(-0.00119F, -0.062F, 0.16026F),
                localAngles = new Vector3(24.0072F, 238.9276F, 305.8187F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.00084F, 0.09298F, 0.14167F),
                localAngles = new Vector3(37.04324F, 259.6818F, 345.4377F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.00069F, 0.01292F, 0.15108F),
                localAngles = new Vector3(16.0607F, 234.2282F, 295.3604F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.00163F, -0.05424F, 0.12578F),
                localAngles = new Vector3(22.03966F, 237.6127F, 305.6128F),
                localScale = new Vector3(0.15F, 0.15F, 0.15F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.00282F, -0.0651F, 0.1334F),
                localAngles = new Vector3(29.90835F, 241.8379F, 315.8166F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.10163F, 2.8788F, -1.49165F),
                localAngles = new Vector3(59.11346F, 61.52309F, 306.2351F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(0.0046F, -0.6473F, 0.45684F),
                localAngles = new Vector3(14.55587F, 232.0287F, 292.289F),
                localScale = new Vector3(1F, 1F, 1F)
            }, new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FlowerBase",
                localPos = new Vector3(0.27899F, 0.55739F, 0.97507F),
                localAngles = new Vector3(14.55587F, 251.2795F, 292.289F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.00138F, 0.01733F, 0.13007F),
                localAngles = new Vector3(25.15422F, 247.6927F, 314.3745F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.07618F, 0.04021F, 0.17666F),
                localAngles = new Vector3(28.40403F, 204.2041F, 312.3828F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
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

            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/moustacheIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
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