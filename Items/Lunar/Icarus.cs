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
    public class Icarus : Item<Icarus> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Lunar;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            chargeFreq.ToString("N1"), maxStacks.ToString("N0"), (igniteChance / 100f).ToString("0%"), igniteDamage.ToString("P0"), armorDebuff.ToString("N0"), (1f / decayFreqMult).ToString("P0")
        };



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:N1} s", 0f, 15f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Airtime required to accumulate a buff/debuff stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float chargeFreq { get; private set; } = 1f;

        [AutoConfigRoOIntSlider("{0:N0}", 1, 100)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum buff/debuff stacks per item stack.", AutoConfigFlags.PreventNetMismatch, 1, int.MaxValue)]
        public int maxStacks { get; private set; } = 5;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How fast to decay charge while grounded, relative to chargeFreq.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float decayFreqMult { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Ignite chance at full strength (does not stack).", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float igniteChance { get; private set; } = 15f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Relative ignite damage per stack at full strength (linear).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float igniteDamage { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Armor penalty per stack at full strength (linear).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float armorDebuff { get; private set; } = 50f;



        ////// Other Fields/Properties //////

        public BuffDef statusBuff { get; private set; }



        ////// TILER2 Module Setup //////

        public Icarus() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Icarus.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/icarusIcon.png");
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
        }

        public override void Uninstall() {
            base.Uninstall();
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
        }



        ////// Hooks //////

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody obj) {
            if(GetCount(obj) > 0 && !obj.TryGetComponent<IcarusTracker>(out _))
                obj.gameObject.AddComponent<IcarusTracker>();
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class IcarusTracker : MonoBehaviour {
        CharacterBody body;
        float charge = 0f;
        int stacks = 0;

        void Awake() {
            body = GetComponent<CharacterBody>();
            body.onInventoryChanged += Body_onInventoryChanged;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        void OnDestroy() {
            body.onInventoryChanged -= Body_onInventoryChanged;
            GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
        }

        void FixedUpdate() {
            if(body.characterMotor && !body.characterMotor.isGrounded) {
                charge += Time.fixedDeltaTime / Icarus.instance.chargeFreq;
                var count = Icarus.instance.GetCount(body);
                if(charge > stacks) charge = stacks;
            } else {
                charge -= Time.fixedDeltaTime / Icarus.instance.chargeFreq * Icarus.instance.decayFreqMult;
                if(charge < 0f) charge = 0f;
            }
        }

        private void Body_onInventoryChanged() {
            stacks = Icarus.instance.GetCount(body);
            if(stacks == 0) Destroy(this);
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report) {
            if(report == null || !report.victimBody || !report.attackerBody || report.attackerBody != body) return;

            if(!Util.CheckRoll(Icarus.instance.igniteChance * Mathf.Min(charge, 1f), report.attackerMaster)) return;

            var dot = new InflictDotInfo {
                victimObject = report.victim.gameObject,
                attackerObject = report.attacker,
                totalDamage = new float?(report.damageDealt * Icarus.instance.igniteDamage * charge),
                dotIndex = DotController.DotIndex.Burn,
                damageMultiplier = 1f
            };
            if(report.attackerMaster)
                StrengthenBurnUtils.CheckDotForUpgrade(report.attackerMaster.inventory, ref dot);
            DotController.InflictDot(ref dot);
        }
    }
}