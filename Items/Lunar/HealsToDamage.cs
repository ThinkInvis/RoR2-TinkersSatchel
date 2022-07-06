using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class HealsToDamage : Item<HealsToDamage> {

        ////// Item Data //////

        public override string displayName => "Hydroponic Cell";
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });
        public override bool itemIsAIBlacklisted { get; protected set; } = false;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Half your healed health grows a plant that provides a single-use damage bonus... <color=#FF7F7F>BUT you don't receive the converted healing.</color>";
        protected override string GetDescString(string langid = null) => $"Whenever you receive <style=cIsHealing>healing</style> that applies to your <style=cIsHealth>health</style> <style=cStack>(overheal, barrier, etc. do not count)</style>, {Pct(healingRatio)} <style=cStack>(+{Pct(healingRatio)} per stack, hyperbolic)</style> of this <style=cIsHealing>healing</style> will be <color=#FF7F7F>converted</color> into <style=cIsDamage>base damage</style> at a ratio of {(1/extraConversionMalus):N2}:1. This bonus damage will be consumed by your next attack that deals more than <style=cIsDamage>{Pct(triggerBigHitFrac)} damage</style>. Stores up to {maxStoredDamageRatio + 1:N0}x your <style=cIsDamage>damage stat</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of healing to absorb.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float healingRatio { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra multiplier on incoming damage buffer before converting into outgoing bonus damage.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float extraConversionMalus { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("x{0:N0}", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum amount of outgoing bonus damage to store as a fraction of base damage.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float maxStoredDamageRatio { get; private set; } = 99f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 50f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Minimum fraction of damage stat required to proc on hit.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float triggerBigHitFrac { get; private set; } = 4f;



        ////// Other Fields/Properties //////

        public BuffDef damageBonusDisplayBuff { get; private set; }
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public HealsToDamage() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/HealsToDamage.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/healsToDamageIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/HealsToDamage.prefab");
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
                localPos = new Vector3(0.08787F, -0.04989F, -0.03935F),
                localAngles = new Vector3(30.13251F, 293.0449F, 327.4671F),
                localScale = new Vector3(0.15F, 0.15F, 0.15F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "ClavicleR",
                localPos = new Vector3(-0.00645F, 0.03074F, -0.1937F),
                localAngles = new Vector3(20.21178F, 14.35402F, 86.373F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.22486F, 0.10162F, 0.07297F),
                localAngles = new Vector3(32.10413F, 257.2959F, 308.3725F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(-2.54985F, -1.75031F, -0.71476F),
                localAngles = new Vector3(358.8855F, 96.69154F, 7.19764F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HeadCenter",
                localPos = new Vector3(0.20697F, -0.07894F, 0.1159F),
                localAngles = new Vector3(33.8656F, 224.6563F, 247.6386F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Head",
                localPos = new Vector3(0.16213F, 0.05587F, -0.00046F),
                localAngles = new Vector3(23.45041F, 267.2077F, 272.5279F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Neck",
                localPos = new Vector3(-0.13388F, 0.07636F, 0.00536F),
                localAngles = new Vector3(28.35369F, 79.95956F, 216.4976F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.07164F, 0.3223F, 0.04066F),
                localAngles = new Vector3(10.5521F, 257.2036F, 293.1262F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Neck",
                localPos = new Vector3(-0.13381F, 0.12336F, -0.04349F),
                localAngles = new Vector3(4.97062F, 83.73137F, 278.397F),
                localScale = new Vector3(0.15F, 0.15F, 0.15F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(0.10636F, 3.7239F, -1.98175F),
                localAngles = new Vector3(82.16266F, 358.3847F, 196.6429F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "FlowerBase",
                localPos = new Vector3(0.78308F, 1.38417F, 0.17772F),
                localAngles = new Vector3(14.83111F, 255.5348F, 316.8267F),
                localScale = new Vector3(0.6F, 0.6F, 0.6F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Neck",
                localPos = new Vector3(0.02508F, 0.05103F, -0.06036F),
                localAngles = new Vector3(30.09493F, 9.06698F, 266.63F),
                localScale = new Vector3(0.1F, 0.1F, 0.1F)
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

            damageBonusDisplayBuff = ScriptableObject.CreateInstance<BuffDef>();
            damageBonusDisplayBuff.buffColor = Color.red;
            damageBonusDisplayBuff.canStack = true;
            damageBonusDisplayBuff.isDebuff = false;
            damageBonusDisplayBuff.name = modInfo.shortIdentifier + "HealsToDamage";
            damageBonusDisplayBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(damageBonusDisplayBuff);
        }

        public override void Install() {
            base.Install();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;
        }

        public override void Uninstall() {
            base.Uninstall();
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
            IL.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
        }



        ////// Hooks //////
        
        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(GetCount(body) > 0 && !body.GetComponent<HealDamageConversionTracker>())
                body.gameObject.AddComponent<HealDamageConversionTracker>();
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            orig(self, damageInfo, victim);

            if(!damageInfo.rejected && damageInfo.attacker && victim && victim.TryGetComponent<HealthComponent>(out var victimHealth) && damageInfo.attacker.TryGetComponent<CharacterBody>(out var attackerBody) && damageInfo.attacker.TryGetComponent<HealDamageConversionTracker>(out var hdct)) {
                var damageFrac = damageInfo.damage / attackerBody.damage;
                if(damageFrac >= triggerBigHitFrac) {
                    victimHealth.TakeDamage(new DamageInfo {
                        attacker = damageInfo.attacker,
                        canRejectForce = true,
                        crit = false,
                        damage = damageFrac * hdct.EmptyDamageBuffer(),
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = DamageType.Silent | DamageType.BypassArmor | DamageType.BypassBlock,
                        force = Vector3.zero,
                        inflictor = damageInfo.inflictor,
                        position = damageInfo.position,
                        procChainMask = damageInfo.procChainMask,
                        procCoefficient = 0f
                    });
                }
            }
        }

        private void HealthComponent_Heal(ILContext il) {
            ILCursor c = new(il);
            int amtArgIndex = -1;
            int num2LocIndex = -1;
            if(c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(out amtArgIndex),
                x => x.MatchStloc(out num2LocIndex),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.health)),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<HealthComponent>("get_fullHealth")
                )) {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg, amtArgIndex);
                c.EmitDelegate<Func<HealthComponent, float, float>>((self, origAmount) => {
                    if(self && self.body && self.body.TryGetComponent<HealDamageConversionTracker>(out var hdct)) {
                        var count = GetCount(self.body);
                        if(count > 0) {
                            var totalWouldHeal = Mathf.Min(origAmount, self.fullHealth - self.health);
                            var stolenHealth = totalWouldHeal * (1f - Mathf.Pow(1f - healingRatio, count));
                            hdct.ReceiveHealing(stolenHealth);
                            return origAmount - stolenHealth;
                        }
                    }

                    return origAmount;
                });
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Starg, amtArgIndex);
                c.Emit(OpCodes.Stloc, num2LocIndex);
            } else {
                TinkersSatchelPlugin._logger.LogError("HealsToDamage: failed to apply IL hook (HealthComponent_Heal), target instructions not found. Item will not work.");
            }
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class HealDamageConversionTracker : MonoBehaviour {
        float storedDamageOut = 0f;
        CharacterBody body;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        public void ReceiveHealing(float healing) {
            storedDamageOut += healing * HealsToDamage.instance.extraConversionMalus;
            storedDamageOut = Mathf.Min(storedDamageOut, HealsToDamage.instance.maxStoredDamageRatio * body.damage);
            UpdateBuffs();
        }

        public float EmptyDamageBuffer() {
            var sdo = storedDamageOut;
            storedDamageOut = 0;
            UpdateBuffs();
            return sdo;
        }

        void UpdateBuffs() {
            body.SetBuffCount(HealsToDamage.instance.damageBonusDisplayBuff.buffIndex, Mathf.FloorToInt(
                storedDamageOut * 100f
                / (HealsToDamage.instance.maxStoredDamageRatio * body.damage)));
        }
    }
}