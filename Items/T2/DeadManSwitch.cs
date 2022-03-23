using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class DeadManSwitch : Item<DeadManSwitch> {

        ////// Item Data //////

        public override string displayName => "Pulse Monitor";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.EquipmentRelated, ItemTag.Utility, ItemTag.LowHealth });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Activate your equipment for free at low health.";
        protected override string GetDescString(string langid = null) => $"Falling below <style=cIsHealth>25% health</style> activates your <style=cIsUtility>equipment</style> without putting it on cooldown. This effect has its own cooldown equal to the cooldown of the activated equipment <style=cStack>(-{Pct(cdrStack)} per stack, mult.)</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplicative internal cooldown reduction per stack past the first.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float cdrStack { get; private set; } = 0.15f;

        [AutoConfig("If true, also applies equipment cooldown reduction from other sources to the ICD. If false, only cdrStack is applied.", AutoConfigFlags.PreventNetMismatch)]
        public bool externalCdr { get; private set; } = false;



        ////// Other Fields/Properties //////




        ////// TILER2 Module Setup //////
        public DeadManSwitch() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/DeadManSwitch.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/deadManSwitchIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
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

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(GetCount(body) > 0 && !body.GetComponent<DeadManSwitchTracker>())
                body.gameObject.AddComponent<DeadManSwitchTracker>();
        }

    }

    [RequireComponent(typeof(CharacterBody))]
    public class DeadManSwitchTracker : MonoBehaviour {
        float icd = 0f;
        CharacterBody body;

        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        void FixedUpdate() {
            if(!body.healthComponent.alive) return;
            var count = DeadManSwitch.instance.GetCount(body);
            if(count <= 0) return;
            var eqp = EquipmentCatalog.GetEquipmentDef(body.equipmentSlot.equipmentIndex);
            if(icd <= 0f && body.healthComponent.isHealthLow && eqp != null) {
                icd = Mathf.Pow(1f - DeadManSwitch.instance.cdrStack, count - 1)
                    * eqp.cooldown
                    * (DeadManSwitch.instance.externalCdr ? body.inventory.CalculateEquipmentCooldownScale() : 1f);
                body.equipmentSlot.PerformEquipmentAction(eqp);
            }
        }
    }
}