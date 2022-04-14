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
        protected override string GetPickupString(string langid = null) => "Half your healed health grows a plant that provides a single-use damage bonus... <color=#FF7F7F>BUT you don't receive the converted healing.</color>";
        protected override string GetDescString(string langid = null) => $"Whenever you receive <style=cIsHealing>healing</style> that applies to your <style=cIsHealth>health</style> <style=cStack>(overheal, barrier, etc. do not count)</style>, {Pct(healingRatio)} <style=cStack>(+{Pct(healingRatio)} per stack, hyperbolic)</style> of this <style=cIsHealing>healing</style> will be <color=#FF7F7F>converted</color> into {extraConversionMalus:N1} points of <style=cIsDamage>base damage</style>. This bonus damage will be consumed by your next attack that deals more than <style=cIsDamage>{Pct(triggerBigHitFrac)} damage</style>. Stores up to {maxStoredDamageRatio + 1:N0}x your <style=cIsDamage>damage stat</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////
        
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of healing to absorb.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float healingRatio { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra multiplier on incoming damage buffer before converting into outgoing bonus damage.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float extraConversionMalus { get; private set; } = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum amount of outgoing bonus damage to store as a fraction of base damage.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float maxStoredDamageRatio { get; private set; } = 99f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Minimum fraction of damage stat required to proc on hit.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float triggerBigHitFrac { get; private set; } = 4f;



        ////// Other Fields/Properties //////

        public BuffDef damageBonusDisplayBuff { get; private set; }



        ////// TILER2 Module Setup //////

        public HealsToDamage() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/HealsToDamage.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/healsToDamageIcon.png");
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