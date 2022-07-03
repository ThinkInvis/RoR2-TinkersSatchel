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
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.EquipmentRelated, ItemTag.Utility, ItemTag.LowHealth });
        public override bool itemIsAIBlacklisted { get; protected set; } = true;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Auto-activate your equipment for free at low health.";
        protected override string GetDescString(string langid = null) => $"Falling below <style=cIsHealth>25% health</style> activates your <style=cIsUtility>equipment</style> without putting it on cooldown. This effect has its own cooldown equal to the cooldown of the activated equipment <style=cStack>(-{Pct(cdrStack)} per stack, mult.)</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplicative internal cooldown reduction per stack past the first.", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
        public float cdrStack { get; private set; } = 0.15f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, also applies equipment cooldown reduction from other sources to the ICD. If false, only cdrStack is applied.", AutoConfigFlags.PreventNetMismatch)]
        public bool externalCdr { get; private set; } = false;



        ////// Other Fields/Properties //////

        public BuffDef deadManSwitchBuff { get; private set; }
        UnlockableDef unlockable;



        ////// TILER2 Module Setup //////
        public DeadManSwitch() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/DeadManSwitch.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/deadManSwitchIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
            var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/deadManSwitchIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            LanguageAPI.Add(achiNameToken, "Nine Lives");
            LanguageAPI.Add(achiDescToken, "Survive falling to low health 9 times in the same run (must return to above 50% health each time).");
            itemDef.unlockableDef = unlockable;

            deadManSwitchBuff = ScriptableObject.CreateInstance<BuffDef>();
            deadManSwitchBuff.buffColor = Color.red;
            deadManSwitchBuff.canStack = false;
            deadManSwitchBuff.isDebuff = false;
            deadManSwitchBuff.isCooldown = true;
            deadManSwitchBuff.name = "TKSATDeadManSwitch";
            deadManSwitchBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/deadManSwitchBuff.png");
            ContentAddition.AddBuffDef(deadManSwitchBuff);
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
            if(GetCount(body) > 0
                && body.healthComponent && body.equipmentSlot
                && !body.GetComponent<DeadManSwitchTracker>())
                body.gameObject.AddComponent<DeadManSwitchTracker>();
        }

    }

    [RequireComponent(typeof(CharacterBody))]
    public class DeadManSwitchTracker : MonoBehaviour {
        float icd = 0f;
        CharacterBody body;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(!body.healthComponent.alive) return;
            if(icd > 0f) {
                icd -= Time.fixedDeltaTime;
            }
            var count = DeadManSwitch.instance.GetCount(body);
            if(count <= 0) return;
            var eqp = EquipmentCatalog.GetEquipmentDef(body.equipmentSlot.equipmentIndex);
            if(icd <= 0f && body.healthComponent.isHealthLow && eqp != null) {
                icd = Mathf.Pow(1f - DeadManSwitch.instance.cdrStack, count - 1)
                    * eqp.cooldown
                    * (DeadManSwitch.instance.externalCdr ? body.inventory.CalculateEquipmentCooldownScale() : 1f);
                body.AddTimedBuff(DeadManSwitch.instance.deadManSwitchBuff, icd);
                body.equipmentSlot.PerformEquipmentAction(eqp);
            }
        }
    }

    [RegisterAchievement("TkSat_DeadManSwitch", "TkSat_DeadManSwitchUnlockable", "")]
    public class TkSatDeadManSwitchAchievement : RoR2.Achievements.BaseAchievement {
        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.HealthComponent.FixedUpdate += HealthComponent_FixedUpdate;
        }

        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.HealthComponent.FixedUpdate -= HealthComponent_FixedUpdate;
        }


        private void HealthComponent_FixedUpdate(On.RoR2.HealthComponent.orig_FixedUpdate orig, HealthComponent self) {
            orig(self);
            if(!self || !self.alive || !self.body || localUser == null || !self.body.masterObject || !localUser.cachedMasterObject || self.body.masterObject != localUser.cachedMasterObject) return;
            var cpt = self.body.masterObject.GetComponent<DeadManSwitchAchievementTracker>();
            if(!cpt) cpt = self.body.masterObject.AddComponent<DeadManSwitchAchievementTracker>();
            if(self.isHealthLow) {
                if(!cpt.lowHealthHysteresis) {
                    cpt.lowHealthHysteresis = true;
                    cpt.totalLowHealthThisRun++;
                }
            } else if((self.health + self.shield) / self.fullCombinedHealth > 0.5f) {
                cpt.lowHealthHysteresis = false;
                if(cpt.totalLowHealthThisRun >= 9)
                    Grant();
            }
        }
    }

    public class DeadManSwitchAchievementTracker : MonoBehaviour {
        public int totalLowHealthThisRun = 0;
        public bool lowHealthHysteresis = false;
    }
}