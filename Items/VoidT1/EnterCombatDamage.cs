using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using System;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;

namespace ThinkInvisible.TinkersSatchel {
    public class EnterCombatDamage : Item<EnterCombatDamage> {

        ////// Item Data //////

        public override string displayName => "Villainous Visage";
        public override ItemTier itemTier => ItemTier.VoidTier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Deal more damage when given time to plot. <style=cIsVoid>Corrupts all Macho Moustaches</style>.";
        protected override string GetDescString(string langid = null) => $"While out of combat, build up a <style=cIsDamage>damage buff</style> that will last <style=cIsDamage>{buffDuration:N0} seconds</style> once in combat. Builds <style=cIsDamage>{Pct(damageFracRate)} damage per second <style=cStack>(+{Pct(damageFracRate)} per stack)</style></style>, up to <style=cIsDamage>{Pct(damageFracMax)} <style=cStack>(+{Pct(damageFracMax)} per stack)</style></style>. <style=cIsVoid>Corrupts all Macho Moustaches</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////
        
        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fractional damage bonus per second per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFracRate { get; private set; } = 0.03f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Maximum fractional damage bonus per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFracMax { get; private set; } = 0.15f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Duration of the damage buff once triggered.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffDuration { get; private set; } = 2f;



        ////// Other Fields/Properties //////

        public BuffDef activeBuff { get; private set; }
        public BuffDef chargingBuff { get; private set; }
        public BuffDef readyBuff { get; private set; }
        public Sprite buffIconResource;
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////
        public EnterCombatDamage() {
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

            activeBuff = ScriptableObject.CreateInstance<BuffDef>();
            activeBuff.buffColor = new Color(0.85f, 0.2f, 0.2f);
            activeBuff.canStack = false;
            activeBuff.isDebuff = false;
            activeBuff.name = "TKSATEnterCombatDamageActive";
            activeBuff.iconSprite = buffIconResource;
            ContentAddition.AddBuffDef(activeBuff);

            readyBuff = ScriptableObject.CreateInstance<BuffDef>();
            readyBuff.buffColor = new Color(0.85f, 0.85f, 0.2f);
            readyBuff.canStack = false;
            readyBuff.isDebuff = false;
            readyBuff.name = "TKSATEnterCombatDamageReady";
            readyBuff.iconSprite = buffIconResource;
            ContentAddition.AddBuffDef(readyBuff);

            chargingBuff = ScriptableObject.CreateInstance<BuffDef>();
            chargingBuff.buffColor = new Color(0.4f, 0.4f, 0.4f);
            chargingBuff.canStack = false;
            chargingBuff.isDebuff = false;
            chargingBuff.name = "TKSATEnterCombatDamageCharging";
            chargingBuff.iconSprite = buffIconResource;
            ContentAddition.AddBuffDef(chargingBuff);

            itemDef.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset")
                .WaitForCompletion();

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
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public override void Uninstall() {
            base.Uninstall();
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }



        ////// Hooks //////

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(GetCount(body) > 0 && !body.GetComponent<EnterCombatDamageTracker>())
                body.gameObject.AddComponent<EnterCombatDamageTracker>();
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(damageInfo != null && damageInfo.attacker) {
                var vmdc = damageInfo.attacker.GetComponent<EnterCombatDamageTracker>();
                var body = damageInfo.attacker.GetComponent<CharacterBody>();
                if(vmdc && body) {
                    damageInfo.damage *= 1f + vmdc.charge;
                }
            }
            orig(self, damageInfo);
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class EnterCombatDamageTracker : MonoBehaviour {
        public float charge = 0f;
        public bool isActive = false;

        CharacterBody body;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(!NetworkServer.active) return;

            if(body.outOfCombat && body.outOfDanger) {
                if(isActive) {
                    charge = 0f;
                    isActive = false;
                    body.SetBuffCount(EnterCombatDamage.instance.activeBuff.buffIndex, 0);
                }
                var count = EnterCombatDamage.instance.GetCount(body);
                if(count <= 0) {
                    body.SetBuffCount(EnterCombatDamage.instance.chargingBuff.buffIndex, 0);
                    body.SetBuffCount(EnterCombatDamage.instance.readyBuff.buffIndex, 0);
                    charge = 0f;
                    return;
                }
                var chargeDelta = Time.fixedDeltaTime * EnterCombatDamage.instance.damageFracRate * (float)count;
                var chargeMax = EnterCombatDamage.instance.damageFracMax * (float)count;
                charge = Mathf.Min(charge + chargeDelta, chargeMax);
                body.SetBuffCount(EnterCombatDamage.instance.chargingBuff.buffIndex, (charge >= chargeMax) ? 0 : 1);
                body.SetBuffCount(EnterCombatDamage.instance.readyBuff.buffIndex, (charge >= chargeMax) ? 1 : 0);
            } else {
                body.SetBuffCount(EnterCombatDamage.instance.chargingBuff.buffIndex, 0);
                body.SetBuffCount(EnterCombatDamage.instance.readyBuff.buffIndex, 0);
                if(!isActive && charge > 0f) {
                    isActive = true;
                    body.AddTimedBuff(EnterCombatDamage.instance.activeBuff, EnterCombatDamage.instance.buffDuration);
                }
                if(!body.HasBuff(EnterCombatDamage.instance.activeBuff)) {
                    charge = 0f;
                    isActive = false;
                }
            }
        }
    }
}