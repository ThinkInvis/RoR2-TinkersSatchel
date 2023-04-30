using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using EntityStates;
using System.Linq;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    public class WaxFeather : Item<WaxFeather> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Lunar;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            chargeFreq.ToString("N1"), maxStacks.ToString("N0"), (igniteChance / 100f).ToString("0%"), igniteDamage.ToString("P0"), gravityBuff.ToString("P0"), armorDebuff.ToString("N0"), speedDebuff.ToString("0%"), (1f / decayFreqMult).ToString("P0")
        };



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:N1} s", 0f, 15f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Airtime required to accumulate a buff/debuff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float chargeFreq { get; private set; } = 0.4f;

        [AutoConfigRoOIntSlider("{0:N0}", 1, 100)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum buff/debuff stacks per item stack.", AutoConfigFlags.PreventNetMismatch, 1, int.MaxValue)]
        public int maxStacks { get; private set; } = 10;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How fast to decay charge while grounded, relative to chargeFreq.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float decayFreqMult { get; private set; } = 3f;

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Ignite chance at full strength (does not stack).", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float igniteChance { get; private set; } = 15f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Relative ignite damage per stack at full strength (linear).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float igniteDamage { get; private set; } = 0.2f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Armor penalty per stack at full strength (linear).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float armorDebuff { get; private set; } = 5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Gravity reduction per stack at full strength (hyperbolic).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float gravityBuff { get; private set; } = 0.015f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Speed penalty per stack at full strength (linear divisor).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float speedDebuff { get; private set; } = 0.01f;



        ////// Other Fields/Properties //////

        public BuffDef statusBuff { get; private set; }



        ////// TILER2 Module Setup //////

        public WaxFeather() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/WaxFeather.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/waxFeatherIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            statusBuff = ScriptableObject.CreateInstance<BuffDef>();
            statusBuff.buffColor = new Color(1f, 0.6f, 0.2f);
            statusBuff.canStack = true;
            statusBuff.isDebuff = false;
            statusBuff.name = "TKSATIcarusDisplay";
            statusBuff.iconSprite = itemDef.pickupIconSprite;
            ContentAddition.AddBuffDef(statusBuff);
        }

        public override void Install() {
            base.Install();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        public override void Uninstall() {
            base.Uninstall();
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }



        ////// Hooks //////

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody obj) {
            if(GetCount(obj) > 0 && !obj.TryGetComponent<IcarusTracker>(out _))
                obj.gameObject.AddComponent<IcarusTracker>();
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            args.moveSpeedReductionMultAdd += sender.GetBuffCount(statusBuff) * speedDebuff;
            args.armorAdd -= sender.GetBuffCount(statusBuff) * armorDebuff;
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class IcarusTracker : MonoBehaviour {
        CharacterBody body;
        float charge = 0f;
        int stacks = 0;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
            body.onInventoryChanged += Body_onInventoryChanged;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void OnDestroy() {
            body.onInventoryChanged -= Body_onInventoryChanged;
            GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            int prevCharge = Mathf.FloorToInt(charge);
            var onGround = body.characterMotor && !body.characterMotor.isGrounded;
            if(onGround) {
                charge += Time.fixedDeltaTime / WaxFeather.instance.chargeFreq;
                if(charge > stacks) charge = stacks;
            } else {
                charge -= Time.fixedDeltaTime / WaxFeather.instance.chargeFreq * WaxFeather.instance.decayFreqMult;
                if(charge < 0f) charge = 0f;
            }
            int currCharge = Mathf.FloorToInt(charge);
            if(currCharge != prevCharge) {
                body.SetBuffCount(WaxFeather.instance.statusBuff.buffIndex, currCharge);
                body.statsDirty = true;
            }
            if(onGround)
                body.characterMotor.velocity -= Physics.gravity * Mathf.Min(1f - Mathf.Pow(1f - WaxFeather.instance.gravityBuff, charge), 0.95f) * Time.fixedDeltaTime;
        }

        private void Body_onInventoryChanged() {
            stacks = WaxFeather.instance.GetCount(body) * WaxFeather.instance.maxStacks;
            if(stacks == 0) {
                body.SetBuffCount(WaxFeather.instance.statusBuff.buffIndex, 0);
                body.statsDirty = true;
                Destroy(this);
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report) {
            if(report == null || !report.victimBody || !report.attackerBody || report.attackerBody != body) return;

            if(!Util.CheckRoll(WaxFeather.instance.igniteChance * Mathf.Min(charge, 1f), report.attackerMaster)) return;

            var dot = new InflictDotInfo {
                victimObject = report.victim.gameObject,
                attackerObject = report.attacker,
                totalDamage = new float?(report.damageDealt * WaxFeather.instance.igniteDamage * charge),
                dotIndex = DotController.DotIndex.Burn,
                damageMultiplier = 1f
            };
            if(report.attackerMaster)
                StrengthenBurnUtils.CheckDotForUpgrade(report.attackerMaster.inventory, ref dot);
            DotController.InflictDot(ref dot);
        }
    }
}