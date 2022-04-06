using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using System;
using UnityEngine.Networking;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class VoidMoustache : Item<VoidMoustache> {

        ////// Item Data //////

        public override string displayName => "Villainous Visage";
        public override ItemTier itemTier => ItemTier.VoidTier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Deal more damage when given time to plot. <style=cIsVoid>Corrupts all Macho Moustaches</style>";
        protected override string GetDescString(string langid = null) => $"While out of combat, build up a <style=cIsDamage>damage buff</style> that will last <style=cIsDamage>{buffDuration:N0} seconds</style> once in combat. Builds <style=cIsDamage>{Pct(damageFracRate)} damage per second <style=cStack>(+{Pct(damageFracRate)} per stack)</style></style>, up to <style=cIsDamage>{Pct(damageFracMax)} <style=cStack>(+{Pct(damageFracMax)} per stack)</style></style>. <style=cIsVoid>Corrupts all Macho Moustaches</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fractional damage bonus per second per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFracRate { get; private set; } = 0.03f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Maximum fractional damage bonus per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFracMax { get; private set; } = 0.15f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Duration of the damage buff once triggered.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffDuration { get; private set; } = 2f;



        ////// Other Fields/Properties //////

        public BuffDef voidMoustacheActiveBuff { get; private set; }
        public BuffDef voidMoustacheChargingBuff { get; private set; }
        public BuffDef voidMoustacheReadyBuff { get; private set; }
        public Sprite buffIconResource;



        ////// TILER2 Module Setup //////
        public VoidMoustache() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/VoidMoustache.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/voidMoustacheIcon.png");
            buffIconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/voidMoustacheBuff.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            voidMoustacheActiveBuff = ScriptableObject.CreateInstance<BuffDef>();
            voidMoustacheActiveBuff.buffColor = new Color(0.85f, 0.2f, 0.2f);
            voidMoustacheActiveBuff.canStack = false;
            voidMoustacheActiveBuff.isDebuff = false;
            voidMoustacheActiveBuff.name = "TKSATVoidMoustacheActive";
            voidMoustacheActiveBuff.iconSprite = buffIconResource;
            ContentAddition.AddBuffDef(voidMoustacheActiveBuff);

            voidMoustacheReadyBuff = ScriptableObject.CreateInstance<BuffDef>();
            voidMoustacheReadyBuff.buffColor = new Color(0.85f, 0.85f, 0.2f);
            voidMoustacheReadyBuff.canStack = false;
            voidMoustacheReadyBuff.isDebuff = false;
            voidMoustacheReadyBuff.name = "TKSATVoidMoustacheReady";
            voidMoustacheReadyBuff.iconSprite = buffIconResource;
            ContentAddition.AddBuffDef(voidMoustacheReadyBuff);

            voidMoustacheChargingBuff = ScriptableObject.CreateInstance<BuffDef>();
            voidMoustacheChargingBuff.buffColor = new Color(0.4f, 0.4f, 0.4f);
            voidMoustacheChargingBuff.canStack = false;
            voidMoustacheChargingBuff.isDebuff = false;
            voidMoustacheChargingBuff.name = "TKSATVoidMoustacheCharging";
            voidMoustacheChargingBuff.iconSprite = buffIconResource;
            ContentAddition.AddBuffDef(voidMoustacheChargingBuff);

            itemDef.requiredExpansion = RoR2.ExpansionManagement.ExpansionCatalog.expansionDefs.FirstOrDefault(x => x.nameToken == "DLC1_NAME");

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
            if(GetCount(body) > 0 && !body.GetComponent<VoidMoustacheDamageTracker>())
                body.gameObject.AddComponent<VoidMoustacheDamageTracker>();
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(damageInfo != null && damageInfo.attacker) {
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

            if(body.outOfCombat && body.outOfDanger) {
                if(isActive) {
                    charge = 0f;
                    isActive = false;
                    body.SetBuffCount(VoidMoustache.instance.voidMoustacheActiveBuff.buffIndex, 0);
                }
                var count = VoidMoustache.instance.GetCount(body);
                if(count <= 0) {
                    body.SetBuffCount(VoidMoustache.instance.voidMoustacheChargingBuff.buffIndex, 0);
                    body.SetBuffCount(VoidMoustache.instance.voidMoustacheReadyBuff.buffIndex, 0);
                    charge = 0f;
                    return;
                }
                var chargeDelta = Time.fixedDeltaTime * VoidMoustache.instance.damageFracRate * (float)count;
                var chargeMax = VoidMoustache.instance.damageFracMax * (float)count;
                charge = Mathf.Min(charge + chargeDelta, chargeMax);
                body.SetBuffCount(VoidMoustache.instance.voidMoustacheChargingBuff.buffIndex, (charge >= chargeMax) ? 0 : 1);
                body.SetBuffCount(VoidMoustache.instance.voidMoustacheReadyBuff.buffIndex, (charge >= chargeMax) ? 1 : 0);
            } else {
                body.SetBuffCount(VoidMoustache.instance.voidMoustacheChargingBuff.buffIndex, 0);
                body.SetBuffCount(VoidMoustache.instance.voidMoustacheReadyBuff.buffIndex, 0);
                if(!isActive && charge > 0f) {
                    isActive = true;
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