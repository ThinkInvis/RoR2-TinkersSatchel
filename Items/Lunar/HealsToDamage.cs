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
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Damage });
        public override bool itemIsAIBlacklisted { get; protected set; } = false;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Half of damage and healing received combine into damage on your next strike... <color=#FF7F7F>BUT you heal for 50% less.</color>";
        protected override string GetDescString(string langid = null) => $"Store <color=#FF7F7F>nutrients</color> on <style=cIsDamage>taking damage</style> equal to <style=cIsDamage>{Pct(damageRatio)} <style=cStack>(+{Pct(damageRatio)} per stack, hyperbolic)</style></style> of the damage taken, up to {maxStoredNutrientsRatio:N0}x your <style=cIsHealth>max health</style>. <style=cIsHealing>{Pct(healingRatio)} <style=cStack>(+{Pct(healingRatio)} per stack, hyperbolic)</style> of incoming healing</style> is <color=#FF7F7F>blocked</color>. Each point of blocked <style=cIsHealing>healing</style> instead converts 1 point of <color=#FF7F7F>nutrients</color> into {extraConversionMalus:N1} points of <style=cIsDamage>base damage</style>, up to {maxStoredDamageRatio + 1:N0}x your <style=cIsDamage>damage stat</style>. This bonus damage will be consumed by your next attack that deals more than <style=cIsDamage>{Pct(triggerBigHitFrac)} damage</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Ratio of damage taken to incoming damage buffer.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageRatio { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of healing to absorb.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float healingRatio { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra multiplier on incoming damage buffer before converting into outgoing bonus damage.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float extraConversionMalus { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum amount of incoming damage to store as a fraction of max health.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float maxStoredNutrientsRatio { get; private set; } = 10f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum amount of outgoing bonus damage to store as a fraction of base damage.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float maxStoredDamageRatio { get; private set; } = 99f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Minimum fraction of damage stat required to proc on hit.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float triggerBigHitFrac { get; private set; } = 4f;



        ////// Other Fields/Properties //////

        public BuffDef nutrientsDisplayBuff { get; private set; }
        public BuffDef damageBonusDisplayBuff { get; private set; }



        ////// TILER2 Module Setup //////

        public HealsToDamage() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/HealsToDamage.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/healsToDamageIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            nutrientsDisplayBuff = ScriptableObject.CreateInstance<BuffDef>();
            nutrientsDisplayBuff.buffColor = Color.green;
            nutrientsDisplayBuff.canStack = true;
            nutrientsDisplayBuff.isDebuff = false;
            nutrientsDisplayBuff.name = modInfo.shortIdentifier + "HealsToDamage1";
            nutrientsDisplayBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(nutrientsDisplayBuff);

            damageBonusDisplayBuff = ScriptableObject.CreateInstance<BuffDef>();
            damageBonusDisplayBuff.buffColor = Color.red;
            damageBonusDisplayBuff.canStack = true;
            damageBonusDisplayBuff.isDebuff = false;
            damageBonusDisplayBuff.name = modInfo.shortIdentifier + "HealsToDamage2";
            damageBonusDisplayBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(damageBonusDisplayBuff);
        }

        public override void Install() {
            base.Install();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;
        }

        public override void Uninstall() {
            base.Uninstall();
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
            IL.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
        }



        ////// Hooks //////
        
        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(GetCount(body) > 0 && !body.GetComponent<HealDamageConversionTracker>())
                body.gameObject.AddComponent<HealDamageConversionTracker>();
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(damageInfo != null && damageInfo.attacker) {
                if(damageInfo.attacker.TryGetComponent<HealDamageConversionTracker>(out var hdct) && damageInfo.attacker.TryGetComponent<CharacterBody>(out var body)) {
                    var damageFrac = damageInfo.damage / body.damage;
                    if(damageFrac / body.damage > triggerBigHitFrac) {
                        damageInfo.damage = (body.damage + hdct.EmptyDamageBuffer()) * damageFrac;
                    }
                }
            }
            float h1 = 0;
            if(self) h1 = self.health;
            orig(self, damageInfo);
            if(self && self.health < h1 && self.body && self.body.TryGetComponent<HealDamageConversionTracker>(out var hdct2)) {
                var count = GetCount(self.body);
                if(count > 0)
                    hdct2.ReceiveDamage((1f - Mathf.Pow(1f - damageRatio, count)) * (h1 - self.health));
            }
        }

        private void HealthComponent_Heal(ILContext il) {
            ILCursor c = new ILCursor(il);
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
                            var rem = origAmount * (1f - Mathf.Pow(1f - healingRatio, count));
                            hdct.ReceiveHealing(rem);
                            return origAmount - rem;
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
        float storedDamageIn = 0f;
        float storedDamageOut = 0f;
        CharacterBody body;

        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        public void ReceiveDamage(float preMultipliedDamage) {
            storedDamageIn += preMultipliedDamage;
            storedDamageIn = Mathf.Min(storedDamageIn, HealsToDamage.instance.maxStoredNutrientsRatio * body.healthComponent.fullHealth);
            UpdateBuffs();
        }

        public void ReceiveHealing(float healing) {
            var amt = Mathf.Min(healing, storedDamageIn);
            storedDamageIn -= amt;
            storedDamageOut += amt * HealsToDamage.instance.extraConversionMalus;
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
            body.SetBuffCount(HealsToDamage.instance.nutrientsDisplayBuff.buffIndex, Mathf.FloorToInt(
                storedDamageIn * 100f
                / (HealsToDamage.instance.maxStoredNutrientsRatio * body.healthComponent.fullHealth)));
            body.SetBuffCount(HealsToDamage.instance.damageBonusDisplayBuff.buffIndex, Mathf.FloorToInt(
                storedDamageOut * 100f
                / HealsToDamage.instance.maxStoredDamageRatio * body.damage));
        }
    }
}