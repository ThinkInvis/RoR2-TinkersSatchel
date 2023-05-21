using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class HealsToDamage : Item<HealsToDamage> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });
        public override bool itemIsAIBlacklisted { get; protected set; } = false;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            healingRatio.ToString("0%"), activationRatio.ToString("0%"), buffDuration.ToString("N0"), (1f - overhealMalus).ToString("0%")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of healing to absorb, stacks hyperbolically.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float healingRatio { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Additional multiplier to HealingRatio for overheal.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float overhealMalus { get; private set; } = 0.25f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Absorbed healing required to activate the buff, relative to max health.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float activationRatio { get; private set; } = 3f;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 60f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the buff, once triggered.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffDuration { get; private set; } = 10f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 30f)]
        [AutoConfig("Base attack speed to add per buff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffMagnitudeAttackSpeed { get; private set; } = 5f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 30f)]
        [AutoConfig("Base damage to add per buff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffMagnitudeDamage { get; private set; } = 5f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 30f)]
        [AutoConfig("Base move speed to add per buff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffMagnitudeMoveSpeed { get; private set; } = 3f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 30f)]
        [AutoConfig("Base jump power to add per buff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffMagnitudeJumpPower { get; private set; } = 3f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 30f)]
        [AutoConfig("Base health regen to add per buff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffMagnitudeRegen { get; private set; } = 0f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 30f)]
        [AutoConfig("Base crit chance to add per buff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffMagnitudeCrit { get; private set; } = 10f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 30f)]
        [AutoConfig("Base armor to add per buff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffMagnitudeArmor { get; private set; } = 5f;



        ////// Other Fields/Properties //////

        public BuffDef damageBonusDisplayBuff { get; private set; }
        public BuffDef statBuff { get; private set; }
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
                childName = "Neck",
                localPos = new Vector3(0.2047F, 0.08958F, 0.1356F),
                localAngles = new Vector3(38.56324F, 226.4295F, 325.2418F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            damageBonusDisplayBuff = ScriptableObject.CreateInstance<BuffDef>();
            damageBonusDisplayBuff.buffColor = Color.red;
            damageBonusDisplayBuff.canStack = true;
            damageBonusDisplayBuff.isDebuff = false;
            damageBonusDisplayBuff.isCooldown = true;
            damageBonusDisplayBuff.name = modInfo.shortIdentifier + "HealsToDamageCharging";
            damageBonusDisplayBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(damageBonusDisplayBuff);

            statBuff = ScriptableObject.CreateInstance<BuffDef>();
            statBuff.buffColor = Color.green;
            statBuff.canStack = true;
            statBuff.isDebuff = false;
            statBuff.name = modInfo.shortIdentifier + "HealsToDamageActive";
            statBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(statBuff);
        }

        public override void Install() {
            base.Install();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        public override void Uninstall() {
            base.Uninstall();
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
            IL.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
            R2API.RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }



        ////// Hooks //////

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            if(!sender) return;
            var stacks = sender.GetBuffCount(statBuff);
            args.baseAttackSpeedAdd += stacks * buffMagnitudeAttackSpeed;
            args.baseDamageAdd += stacks * buffMagnitudeDamage;
            args.baseMoveSpeedAdd += stacks * buffMagnitudeMoveSpeed;
            args.baseJumpPowerAdd += stacks * buffMagnitudeJumpPower;
            args.baseRegenAdd += stacks * buffMagnitudeRegen;
            args.critAdd += stacks * buffMagnitudeCrit;
            args.armorAdd += stacks * buffMagnitudeArmor;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(GetCount(body) > 0 && !body.GetComponent<HealDamageConversionTracker>())
                body.gameObject.AddComponent<HealDamageConversionTracker>();
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
                            var missingHealth = self.fullHealth - self.health;
                            var totalWouldHeal = origAmount;
                            var totalOverheal = 0f;
                            if(origAmount > missingHealth) {
                                totalWouldHeal = missingHealth;
                                totalOverheal = origAmount - missingHealth;
                            }
                            var stackedHealingRatio = (1f - Mathf.Pow(1f - healingRatio, count));
                            var stolenHealth = totalWouldHeal * stackedHealingRatio;
                            var stolenOverheal = totalOverheal * stackedHealingRatio * overhealMalus;
                            hdct.ReceiveHealing(stolenHealth + stolenOverheal);
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
        float storedHealingRatio = 0f;
        CharacterBody body;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        public void ReceiveHealing(float healingRatio) {
            storedHealingRatio += healingRatio;
            if(storedHealingRatio >= HealsToDamage.instance.activationRatio) {
                var stacks = Mathf.FloorToInt(storedHealingRatio / HealsToDamage.instance.activationRatio);
                storedHealingRatio %= HealsToDamage.instance.activationRatio;
                for(var i = 0; i < stacks; i++)
                    body.AddTimedBuff(HealsToDamage.instance.statBuff.buffIndex, HealsToDamage.instance.buffDuration);
            }
            body.SetBuffCount(HealsToDamage.instance.damageBonusDisplayBuff.buffIndex, Mathf.FloorToInt(
                (storedHealingRatio / HealsToDamage.instance.activationRatio) * 100f));
        }
    }
}