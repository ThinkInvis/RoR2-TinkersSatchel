using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class Moustache : Item<Moustache> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            (procChance / 100f).ToString("0%"), procDuration.ToString("N1"), damageResist.ToString("0%"), knockResist.ToString("0%")
        };



        ////// Config ///////
        
        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance to trigger this item's effect on hit, per stack (hyperbolic).", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float procChance { get; private set; } = 4f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fixed taunt duration.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float procDuration { get; private set; } = 5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fixed damage resistance vs. taunted enemies.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float damageResist { get; private set; } = 0.15f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fixed knockback resistance vs. taunted enemies.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float knockResist { get; private set; } = 0.5f;



        ////// Other Fields/Properties //////

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

            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/moustacheIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            itemDef.unlockableDef = unlockable;
        }

        public override void Install() {
            base.Install();
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += SetStateOnHurt_OnTakeDamageServer;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.SetStateOnHurt.OnTakeDamageServer -= SetStateOnHurt_OnTakeDamageServer;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }



        ////// Hooks //////

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(damageInfo.attacker && self.body && GetCount(self.body) > 0 && damageInfo.attacker.TryGetComponent<CharacterBody>(out var attackerBody) && attackerBody.HasBuff(TauntDebuffModule.tauntDebuff)) {
                damageInfo.damage *= 1f - damageResist;
                damageInfo.force *= 1f - knockResist;
            }
            orig(self, damageInfo);
        }

        private void SetStateOnHurt_OnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, DamageReport damageReport) {
            orig(self, damageReport);
            if(!self.targetStateMachine || !self.spawnedOverNetwork || !damageReport.attackerMaster || !damageReport.victimBody) return;
            var count = GetCount(damageReport.attackerMaster);
            if(count > 0 && Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(count * damageReport.damageInfo.procCoefficient * procChance), damageReport.attackerMaster))
                damageReport.victimBody.AddTimedBuff(TauntDebuffModule.tauntDebuff, procDuration);
        }
    }

    [RegisterAchievement("TkSat_Moustache", "TkSat_MoustacheUnlockable", "", 1u)]
    public class TkSatMoustacheAchievement : RoR2.Achievements.BaseAchievement {
        const float UPDATE_INTERVAL = 2f;
        const float MAX_RANGE = 100f;
        const int TARGET_COUNT = 30;
        float _stopwatch = 0f;

        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
        }
        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.CharacterBody.FixedUpdate -= CharacterBody_FixedUpdate;
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);
            if(!self || self != localUser.cachedBody) return;
            _stopwatch -= Time.fixedDeltaTime;
            if(_stopwatch < 0f) {
                _stopwatch = UPDATE_INTERVAL;
                int count = MiscUtil.GatherEnemies(self.teamComponent.teamIndex).Count(e => e.body && (!e.body.outOfCombat || !e.body.outOfDanger) && Vector3.Distance(e.transform.position, self.transform.position) < MAX_RANGE);
                if(count > TARGET_COUNT)
                    Grant();
            }
        }
    }
}