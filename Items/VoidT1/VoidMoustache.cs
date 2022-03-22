using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using System;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class VoidMoustache : Item<VoidMoustache> {

        ////// Item Data //////

        public override string displayName => "Villainous Moustache";
        public override ItemTier itemTier => ItemTier.VoidTier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Deal more damage after plotting an evil plan.";
        protected override string GetDescString(string langid = null) => $"While out of combat, build <style=cIsDamage>{Pct(damageFracRate)} damage per second <style=cStack>(+{Pct(damageFracRate)} per stack)</style></style>, up to <style=cIsDamage>{Pct(damageFracMax)} <style=cStack>(+{Pct(damageFracMax)} per stack)</style></style>, towards a buff that will last <style=cIsDamage>{buffDuration:N0} seconds</style> in combat. <style=cIsVoid>Corrupts all Macho Moustaches</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fractional damage bonus per second per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFracRate { get; private set; } = 0.02f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum fractional damage bonus per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFracMax { get; private set; } = 0.2f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the damage buff once triggered.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffDuration { get; private set; } = 2f;



        ////// Other Fields/Properties //////

        public BuffDef voidMoustacheActiveBuff { get; private set; }
        public BuffDef voidMoustacheChargingBuff { get; private set; }
        public BuffDef voidMoustacheReadyBuff { get; private set; }



        ////// TILER2 Module Setup //////
        public VoidMoustache() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/VoidMoustache.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/voidMoustacheIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            voidMoustacheActiveBuff = ScriptableObject.CreateInstance<BuffDef>();
            voidMoustacheActiveBuff.buffColor = new Color(0.85f, 0.2f, 0.2f);
            voidMoustacheActiveBuff.canStack = false;
            voidMoustacheActiveBuff.isDebuff = false;
            voidMoustacheActiveBuff.name = "TKSATVoidMoustacheActive";
            voidMoustacheActiveBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(voidMoustacheActiveBuff);

            voidMoustacheReadyBuff = ScriptableObject.CreateInstance<BuffDef>();
            voidMoustacheReadyBuff.buffColor = new Color(0.85f, 0.85f, 0.2f);
            voidMoustacheReadyBuff.canStack = false;
            voidMoustacheReadyBuff.isDebuff = false;
            voidMoustacheReadyBuff.name = "TKSATVoidMoustacheReady";
            voidMoustacheReadyBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(voidMoustacheReadyBuff);

            voidMoustacheChargingBuff = ScriptableObject.CreateInstance<BuffDef>();
            voidMoustacheChargingBuff.buffColor = new Color(0.4f, 0.4f, 0.4f);
            voidMoustacheChargingBuff.canStack = false;
            voidMoustacheChargingBuff.isDebuff = false;
            voidMoustacheChargingBuff.name = "TKSATVoidMoustacheCharging";
            voidMoustacheChargingBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(voidMoustacheChargingBuff);
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
            if(GetCount(body) > 0 && !body.GetComponent<VoidMoustacheDamageTracker>())
                body.gameObject.AddComponent<VoidMoustacheDamageTracker>();
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(damageInfo?.attacker) {
                var vmdc = damageInfo.attacker.GetComponent<VoidMoustacheDamageTracker>();
                var body = damageInfo.attacker.GetComponent<CharacterBody>();
                if(vmdc && body) {
                    damageInfo.damage *= 1f + vmdc.charge;
                }
            }
            orig(self, damageInfo);
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class VoidMoustacheDamageTracker : MonoBehaviour {
        public float charge = 0f;
        public bool isActive = false;

        CharacterBody body;

        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        void FixedUpdate() {
            if(!NetworkServer.active) return;

            if(body.outOfCombat) {
                if(isActive) {
                    charge = 0f;
                    isActive = false;
                    body.RemoveBuff(VoidMoustache.instance.voidMoustacheActiveBuff);
                }
                var count = VoidMoustache.instance.GetCount(body);
                var chargeDelta = Time.fixedDeltaTime * VoidMoustache.instance.damageFracRate * count;
                var chargeMax = VoidMoustache.instance.damageFracMax * count;
                charge = Mathf.Min(charge + chargeDelta, chargeMax);
                if(charge == chargeMax && !body.HasBuff(VoidMoustache.instance.voidMoustacheReadyBuff)) {
                    body.RemoveBuff(VoidMoustache.instance.voidMoustacheChargingBuff);
                    body.AddBuff(VoidMoustache.instance.voidMoustacheReadyBuff);
                } else if(charge != chargeMax && !body.HasBuff(VoidMoustache.instance.voidMoustacheChargingBuff))
                    body.AddBuff(VoidMoustache.instance.voidMoustacheChargingBuff);
            } else {
                if(!isActive && charge > 0f) {
                    isActive = true;
                    body.RemoveBuff(VoidMoustache.instance.voidMoustacheChargingBuff);
                    body.RemoveBuff(VoidMoustache.instance.voidMoustacheReadyBuff);
                    body.AddTimedBuff(VoidMoustache.instance.voidMoustacheActiveBuff, VoidMoustache.instance.buffDuration);
                }
                if(!body.HasBuff(VoidMoustache.instance.voidMoustacheActiveBuff)) {
                    charge = 0f;
                    isActive = false;
                }
            }
        }
    }
}