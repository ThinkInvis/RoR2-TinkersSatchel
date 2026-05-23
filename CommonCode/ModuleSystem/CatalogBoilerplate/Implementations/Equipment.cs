using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public abstract class Equipment<T>:Equipment where T : Equipment<T> {
        public static T instance {get;private set;}

        public Equipment() {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Equipment was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class Equipment : CatalogBoilerplate {
        public override string configCategoryPrefix => "Equipments.";

        public EquipmentIndex catalogIndex {
            get {
                if(equipmentDef == null) {
                    TinkersSatchelPlugin._logger.LogError($"Module Equipment {name} has null EquipmentDef! Cannot retrieve EquipmentIndex");
                    return EquipmentIndex.None;
                }
                return equipmentDef.equipmentIndex;
            }
        }
        public EquipmentDef equipmentDef {get; private set;}
        public CustomEquipment customEquipment {get; private set;}

        protected ItemDisplayRuleDict displayRules = new();

        [AutoConfigRoOString()]
        [AutoConfig("The internal name of this equipment for use in other config entries. No effect if changed; will be reset on game launch.")]
        public virtual string configNameInternal { get; protected set; } = null;

        [AutoConfigRoOString()]
        [AutoConfig("The name token of this equipment for use in other config entries. No effect if changed; will be reset on game launch.")]
        public virtual string configNameToken { get; protected set; } = null;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 300f)]
        [AutoConfig("The base cooldown of the equipment, in seconds.", AutoConfigFlags.DeferUntilNextStage, 0f, float.MaxValue)]
        public virtual float cooldown {get; protected set;} = 45f; //TODO: add a getter function to update ingame cooldown properly if in use; marked as DeferUntilNextStage until then

        [AutoConfigRoOCheckbox()]
        [AutoConfig("Whether the equipment can be granted by Artifact of Enigma.", AutoConfigFlags.DeferForever)]
        public virtual bool isEnigmaCompatible { get; protected set; } = true;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("Whether the equipment can be triggered by Bottled Chaos.", AutoConfigFlags.DeferForever)]
        public virtual bool canBeRandomlyTriggered { get; protected set; } = true;

        public virtual bool isLunar => false;

        public override void SetupConfig() {
            base.SetupConfig();
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            equipmentDef.name = modInfo.shortIdentifier + name;
            equipmentDef.pickupModelPrefab = modelResource;
            equipmentDef.pickupIconSprite = iconResource;
            equipmentDef.nameToken = this.nameToken + "_RENDERED";
            equipmentDef.pickupToken = this.pickupToken + "_RENDERED";
            equipmentDef.descriptionToken = this.descToken + "_RENDERED";
            equipmentDef.loreToken = this.loreToken + "_RENDERED";
            equipmentDef.cooldown = cooldown;
            equipmentDef.enigmaCompatible = isEnigmaCompatible;
            equipmentDef.canBeRandomlyTriggered = canBeRandomlyTriggered;
            equipmentDef.isLunar = isLunar;
            equipmentDef.canDrop = true;
            if(isLunar) 
				equipmentDef.colorIndex = ColorCatalog.ColorIndex.LunarItem;

            SetupModifyEquipmentDef();

            customEquipment = new CustomEquipment(equipmentDef, displayRules);
            ItemAPI.Add(customEquipment);

            EquipmentCatalog.availability.CallWhenAvailable(this.SetupCatalogReady);
        }

        public override void SetupCatalogReady() {
            base.SetupCatalogReady();
            var ce1 = bindings.Find(x => x.boundProperty.Name == nameof(configNameInternal)).configEntry;
            ce1.BoxedValue = this.equipmentDef.name;
            if(!ce1.ConfigFile.SaveOnConfigSet) ce1.ConfigFile.Save();
            var ce2 = bindings.Find(x => x.boundProperty.Name == nameof(configNameToken)).configEntry;
            ce2.BoxedValue = this.equipmentDef.nameToken;
            if(!ce2.ConfigFile.SaveOnConfigSet) ce2.ConfigFile.Save();
        }

        public virtual void SetupModifyEquipmentDef() { }

        public override void Install() {
            base.Install();
            On.RoR2.EquipmentSlot.PerformEquipmentAction += Evt_ESPerformEquipmentAction;
            equipmentDef.pickupIconSprite = iconResource;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.EquipmentSlot.PerformEquipmentAction -= Evt_ESPerformEquipmentAction;
            equipmentDef.pickupIconSprite = CatalogBoilerplateModule.lockIcon;
        }

        private bool Evt_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef def) {
            if(enabled && def.equipmentIndex == catalogIndex) return PerformEquipmentAction(self);
            else return orig(self, def);
        }

        protected abstract bool PerformEquipmentAction(EquipmentSlot slot);
        
        public bool HasEquipment(Inventory inv, bool inMain = true, bool inAlt = false) {
            return (inMain &&
                (inv != null ? inv.currentEquipmentIndex : EquipmentIndex.None) == catalogIndex)
                || (inAlt &&
                (inv != null ? inv.alternateEquipmentIndex : EquipmentIndex.None) == catalogIndex);
        }

        public bool HasEquipment(CharacterBody body) {
            var eqpIndex = EquipmentIndex.None;
            if(body && body.equipmentSlot) eqpIndex = body.equipmentSlot.equipmentIndex;
            return eqpIndex == catalogIndex;
        }

        public override ConsoleStrings GetConsoleStrings() {
            return new ConsoleStrings {
                className = "Equipment",
                objectName = this.name,
                formattedIndex = ((int)this.catalogIndex).ToString()
            };
        }
    }
}
