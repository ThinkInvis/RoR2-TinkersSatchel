using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class VillainousVisage : Item<VillainousVisage> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.VoidTier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            buffDuration.ToString("N0"), damageFrac.ToString("0%")
        };



        ////// Config ///////
        
        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fractional stealth attack damage bonus per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFrac { get; private set; } = 0.13f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Duration of the stealth buff once triggered.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffDuration { get; private set; } = 1.3f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, self-damage will not proc this item.", AutoConfigFlags.PreventNetMismatch)]
        public bool disableSelfDamage { get; private set; } = true;



        ////// Other Fields/Properties //////

        public BuffDef activeBuff { get; private set; }
        public BuffDef minStealthBuff { get; private set; }
        public Sprite buffIconResource;
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////
        public VillainousVisage() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/EnterCombatDamage.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/enterCombatDamageIcon.png");
            buffIconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/enterCombatDamageBuff.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/EnterCombatDamage.prefab");
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
                localPos = new Vector3(0.00501F, -0.00193F, 0.12535F),
                localAngles = new Vector3(358.3085F, 4.25592F, 0.28647F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.0005F, -0.0129F, 0.15562F),
                localAngles = new Vector3(11.92738F, 0.11231F, 359.9818F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.00029F, 0.16431F, 0.19222F),
                localAngles = new Vector3(5.89388F, 0.02066F, 359.9918F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.03406F, 5.18621F, 0.31968F),
                localAngles = new Vector3(289.403F, 178.8767F, 359.7504F),
                localScale = new Vector3(4F, 4F, 4F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(-0.00107F, -0.06354F, 0.16326F),
                localAngles = new Vector3(356.9086F, 0.00055F, 359.9797F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.00055F, -0.08314F, 0.13153F),
                localAngles = new Vector3(5.91824F, 0.01733F, 359.9877F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.00113F, 0.02785F, 0.15536F),
                localAngles = new Vector3(9.7558F, 357.37F, 0.27498F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.00026F, -0.05488F, 0.12542F),
                localAngles = new Vector3(12.64564F, 358.3316F, 0.90731F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.00025F, -0.06431F, 0.13606F),
                localAngles = new Vector3(346.3878F, 359.941F, 0.00644F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.09223F, 2.98203F, -1.31453F),
                localAngles = new Vector3(8.1579F, 183.8814F, 4.21555F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(-0.00084F, -0.57127F, 0.37927F),
                localAngles = new Vector3(59.53689F, 0F, 0F),
                localScale = new Vector3(1F, 1F, 1F)
            }, new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FlowerBase",
                localPos = new Vector3(0.23973F, 0.54818F, 0.99367F),
                localAngles = new Vector3(16.77598F, 14.84138F, 359.2693F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0F, 0.0247F, 0.1245F),
                localAngles = new Vector3(24.01353F, 0F, 0F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(-0.07638F, 0.04526F, 0.18096F),
                localAngles = new Vector3(0.561F, 321.2224F, 349.0082F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            activeBuff = ScriptableObject.CreateInstance<BuffDef>();
            activeBuff.buffColor = new Color(0.85f, 0.2f, 0.2f);
            activeBuff.canStack = false;
            activeBuff.isDebuff = false;
            activeBuff.name = "TKSATVillainousVisageActive";
            activeBuff.iconSprite = buffIconResource;
            ContentAddition.AddBuffDef(activeBuff);

            minStealthBuff = ScriptableObject.CreateInstance<BuffDef>();
            minStealthBuff.canStack = false;
            minStealthBuff.isDebuff = false;
            minStealthBuff.name = "TKSATVillainousVisageIcd";
            minStealthBuff.isHidden = true;
            ContentAddition.AddBuffDef(minStealthBuff);

            On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
                var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                isp.relationships = new[] {new ItemDef.Pair {
                    itemDef1 = Moustache.instance.itemDef,
                    itemDef2 = itemDef
                }};
                orig(providers.Concat(new[] { isp }).ToArray());
            };
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
            itemDef.unlockableDef = Moustache.unlockable; //apply in later stage to make sure Moustache loads first
        }

        public override void Install() {
            base.Install();
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public override void Uninstall() {
            base.Uninstall();
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }



        ////// Hooks //////

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport) {
            if(damageReport.attackerMaster && GetCount(damageReport.attackerMaster) > 0
                && (damageReport.victimIsBoss || damageReport.victimIsChampion || damageReport.victimIsElite)) {
                var body = damageReport.attackerMaster.GetBody();
                if(body) {
                    body.AddTimedBuff(RoR2Content.Buffs.Cloak, buffDuration);
                    body.AddTimedBuff(activeBuff, buffDuration);
                    body.AddTimedBuff(minStealthBuff, 0.13f);
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(damageInfo != null && damageInfo.attacker
                && (!disableSelfDamage || damageInfo.attacker != self.gameObject)
                && damageInfo.attacker.TryGetComponent<CharacterBody>(out var attackerBody)
                && attackerBody.HasBuff(activeBuff) && !attackerBody.HasBuff(minStealthBuff)) {
                attackerBody.ClearTimedBuffs(activeBuff);
                attackerBody.ClearTimedBuffs(RoR2Content.Buffs.Cloak);
                damageInfo.damage *= 1f + GetCount(attackerBody) * damageFrac;
            }
            orig(self, damageInfo);
        }
    }
}