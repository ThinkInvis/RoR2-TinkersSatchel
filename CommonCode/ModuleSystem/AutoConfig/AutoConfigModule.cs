using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.Networking;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace ThinkInvisible.TinkersSatchel {
    internal class AutoConfigModule : Module<AutoConfigModule> {
        public override bool managedEnable => false;

        internal static bool globalStatsDirty = false;
        internal static bool globalDropsDirty = false;
        internal static bool globalLanguageDirty = false;

        public override void SetupConfig() {
            base.SetupConfig();
            
            On.RoR2.Networking.NetworkManagerSystem.Disconnect += On_GNMDisconnect;
            
            SceneManager.sceneLoaded += Evt_USMSceneLoaded;
        }

        internal static void Update() {
            if(!(Run.instance != null && Run.instance.isActiveAndEnabled)) {
                globalStatsDirty = false;
                globalDropsDirty = false;
            } else {
                if(globalStatsDirty) {
                    globalStatsDirty = false;
                    CharacterMaster.readOnlyInstancesList.Where(x => x.hasBody && x.GetBody().healthComponent.alive).ToList().ForEach(cm => {if(cm.hasBody) cm.GetBody().RecalculateStats();});
                }
                if(globalDropsDirty) {
                    globalDropsDirty = false;
                    Run.instance.OnRuleBookUpdated(Run.instance.networkRuleBookComponent);
                    Run.instance.BuildDropTable();
                }
            }
            if(globalLanguageDirty) {
                globalLanguageDirty = false;
                Language.SetCurrentLanguage(Language.currentLanguageName);
            }
        }

        internal static void On_GNMDisconnect(On.RoR2.Networking.NetworkManagerSystem.orig_Disconnect orig, NetworkManagerSystem self) {
            orig(self);
            AutoConfigBinding.CleanupDirty(true);
        }

        internal static void Evt_USMSceneLoaded(Scene scene, LoadSceneMode mode) {
            AutoConfigBinding.CleanupDirty(false);
        }
    }

    /// <summary>Used in AutoConfigAttribute to modify the behavior of AutoConfig.</summary>
    [Flags]
    public enum AutoConfigFlags {
        None = 0,
        ///<summary>If UNSET (default): expects acceptableValues to contain 0 or 2 values, which will be added to an AcceptableValueRange. If SET: an AcceptableValueList will be used instead.</summary>
        AVIsList = 1 << 0,
        ///<summary>If SET: will cache config changes, through auto-update or otherwise, and prevent them from applying to the attached property until the next stage transition.</summary>
        DeferUntilNextStage = 1 << 1,
        ///<summary>If SET: will cache config changes, through auto-update or otherwise, and prevent them from applying to the attached property while there is an active run. Takes precedence over DeferUntilNextStage and PreventConCmd.</summary>
        DeferUntilEndGame = 1 << 2,
        ///<summary>If SET: the attached property will never be changed by config, only read once during game startup. If combined with PreventNetMismatch, mismatches will cause the client to be kicked. Takes precedence over DeferUntilNextStage, DeferUntilEndGame, and PreventConCmd.</summary>
        DeferForever = 1 << 3,
        ///<summary>If SET: will prevent the AIC_set console command from being used on this AutoConfig.</summary>
        PreventConCmd = 1 << 4,
        ///<summary>If SET: will stop the property value from being changed by the initial config read during BindAll.</summary>
        NoInitialRead = 1 << 5,
        ///<summary>If SET: will bind individual items in an IDictionary instead of the entire collection.</summary>
        BindDict = 1 << 6
    }

    ///<summary>Used in AutoConfigUpdateActionsAttribute to perform some useful stock actions when the property's config entry is updated.</summary>
    ///<remarks>Implementation of these flags is left to classes that inherit AutoConfig, except for InvalidateStats and AnnounceToRun.</remarks>
    [Flags]
    public enum AutoConfigUpdateActionTypes {
        None = 0,
        ///<summary>Causes an immediate update to the linked item's language registry.</summary>
        InvalidateLanguage = 1 << 0,
        ///<summary>Causes an immediate update to the linked item's pickup model.</summary>
        InvalidateModel = 1 << 1,
        ///<summary>Causes a next-frame RecalculateStats on all copies of CharacterMaster which are alive and have a CharacterBody.</summary>
        InvalidateStats = 1 << 2,
        ///<summary>Causes a next-frame drop table recalculation.</summary>
        InvalidateDropTable = 1 << 3
    }

    public class AutoConfigUpdateActionEventArgs : EventArgs {
        ///<summary>Any flags passed to the event by an AutoConfigUpdateActionsAttribute.</summary>
        public AutoConfigUpdateActionTypes flags;
        ///<summary>The value that the property had before being set.</summary>
        public object oldValue;
        ///<summary>The value that the property has been set to.</summary>
        public object newValue;
        ///<summary>The AutoItemConfig which received an update.</summary>
        public AutoConfigBinding target;
        ///<summary>Suppresses the AnnounceToRun flag.</summary>
        public bool silent;
    }
    
    ///<summary>Causes some actions to be automatically performed when a property's config entry is updated.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigUpdateActionsAttribute : Attribute {
        public readonly AutoConfigUpdateActionTypes flags;
        public readonly bool ignoreDefault;
        public AutoConfigUpdateActionsAttribute(AutoConfigUpdateActionTypes flags, bool ignoreDefault = false) {
            this.flags = flags;
            this.ignoreDefault = ignoreDefault;
        }
    }

    ///<summary>Properties in an AutoConfigContainer that have this attribute will be automatically bound to a BepInEx config file when AutoConfigContainer.BindAll is called.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigAttribute : Attribute {
        public readonly string name = null;
        public readonly string desc = null;
        public readonly AcceptableValueBase avb = null;
        public readonly Type avbType = null;
        public readonly AutoConfigFlags flags;
        public AutoConfigAttribute(string name, string desc, AutoConfigFlags flags = AutoConfigFlags.None, params object[] acceptableValues) : this(desc, flags, acceptableValues) {
            this.name = name;
        }

        public AutoConfigAttribute(string desc, AutoConfigFlags flags = AutoConfigFlags.None, params object[] acceptableValues) : this(flags, acceptableValues) {
            this.desc = desc;
        }
        
        public AutoConfigAttribute(AutoConfigFlags flags = AutoConfigFlags.None, params object[] acceptableValues) {
            if(acceptableValues.Length > 0) {
                var avList = (flags & AutoConfigFlags.AVIsList) == AutoConfigFlags.AVIsList;
                if(!avList && acceptableValues.Length != 2) throw new ArgumentException("Range mode for acceptableValues (flag AVIsList not set) requires either 0 or 2 params; received " + acceptableValues.Length + ".\nThe description provided was: \"" + desc + "\".");
                var iType = acceptableValues[0].GetType();
                for(var i = 1; i < acceptableValues.Length; i++) {
                    if(iType != acceptableValues[i].GetType()) throw new ArgumentException("Types of all acceptableValues must match");
                }
                var avbVariety = avList ? typeof(AcceptableValueList<>).MakeGenericType(iType) : typeof(AcceptableValueRange<>).MakeGenericType(iType);
                this.avb = (AcceptableValueBase)Activator.CreateInstance(avbVariety, acceptableValues);
                this.avbType = iType;
            }
            this.flags = flags;
        }
    }

    ///<summary>Used to register an AutoConfigAttribute with the Risk Of Options mod.</summary>
    public abstract class BaseAutoConfigRoOAttribute : Attribute {
        public string nameOverride;
        public string catOverride;
        public abstract Type requiredType { get; }

        public BaseAutoConfigRoOAttribute(string nameOverride = null, string catOverride = null) {
            this.nameOverride = nameOverride;
            this.catOverride = catOverride;
        }

        public abstract void Apply(ConfigEntryBase cfe, Compat_RiskOfOptions.OptionIdentityStrings identStrings, bool deferForever, Func<bool> isDisabledDelegate);
    }

    ///<summary>Used to register an AutoConfigAttribute with the Risk Of Options mod as a slider. Only supports float properties; use AutoConfigRoOIntSliderAttribute for ints.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigRoOSliderAttribute : BaseAutoConfigRoOAttribute {
        public string format;
        public float min;
        public float max;
        public override Type requiredType => typeof(float);

        public AutoConfigRoOSliderAttribute(string format, float min, float max, string nameOverride = null, string catOverride = null) : base(nameOverride, catOverride) {
            this.format = format;
            this.min = min;
            this.max = max;
        }

        public override void Apply(ConfigEntryBase cfe, Compat_RiskOfOptions.OptionIdentityStrings identStrings, bool deferForever, Func<bool> isDisabledDelegate) {
            Compat_RiskOfOptions.AddOption_Slider((ConfigEntry<float>)cfe, identStrings,
                min, max, format,
                deferForever, isDisabledDelegate);
        }
    }

    ///<summary>Used to register an AutoConfigAttribute with the Risk Of Options mod as a stepped slider. Only supports float properties; use AutoConfigRoOIntSliderAttribute for ints.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigRoOStepSliderAttribute : BaseAutoConfigRoOAttribute {
        public string format;
        public float min;
        public float max;
        public float step;
        public override Type requiredType => typeof(float);

        public AutoConfigRoOStepSliderAttribute(string format, float min, float max, float step, string nameOverride = null, string catOverride = null) : base(nameOverride, catOverride) {
            this.format = format;
            this.min = min;
            this.max = max;
            this.step = step;
        }

        public override void Apply(ConfigEntryBase cfe, Compat_RiskOfOptions.OptionIdentityStrings identStrings, bool deferForever, Func<bool> isDisabledDelegate) {
            Compat_RiskOfOptions.AddOption_StepSlider((ConfigEntry<float>)cfe, identStrings,
                min, max, step, format,
                deferForever, isDisabledDelegate);
        }
    }

    ///<summary>Used to register an AutoConfigAttribute with the Risk Of Options mod as a slider. Only supports int properties; use AutoConfigRoOSliderAttribute for floats.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigRoOIntSliderAttribute : BaseAutoConfigRoOAttribute {
        public string format;
        public int min;
        public int max;
        public override Type requiredType => typeof(int);

        public AutoConfigRoOIntSliderAttribute(string format, int min, int max, string nameOverride = null, string catOverride = null) : base(nameOverride, catOverride) {
            this.format = format;
            this.min = min;
            this.max = max;
        }

        public override void Apply(ConfigEntryBase cfe, Compat_RiskOfOptions.OptionIdentityStrings identStrings, bool deferForever, Func<bool> isDisabledDelegate) {
            Compat_RiskOfOptions.AddOption_IntSlider((ConfigEntry<int>)cfe, identStrings,
                min, max, format,
                deferForever, isDisabledDelegate);
        }
    }

    ///<summary>Used to register an AutoConfigAttribute with the Risk Of Options mod as a dropdown list. Only supports enum properties.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigRoOChoiceAttribute : BaseAutoConfigRoOAttribute {
        public override Type requiredType => typeof(Enum);

        public AutoConfigRoOChoiceAttribute(string nameOverride = null, string catOverride = null) : base(nameOverride, catOverride) {
        }

        public override void Apply(ConfigEntryBase cfe, Compat_RiskOfOptions.OptionIdentityStrings identStrings, bool deferForever, Func<bool> isDisabledDelegate) {
            Compat_RiskOfOptions.AddOption_Choice(cfe, identStrings,
                deferForever, isDisabledDelegate);
        }
    }

    ///<summary>Used to register an AutoConfigAttribute with the Risk Of Options mod as a keybind. Only supports BepInEx KeyboardShortcut properties.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigRoOKeybindAttribute : BaseAutoConfigRoOAttribute {
        public override Type requiredType => typeof(KeyboardShortcut);

        public AutoConfigRoOKeybindAttribute(string nameOverride = null, string catOverride = null) : base(nameOverride, catOverride) {
        }

        public override void Apply(ConfigEntryBase cfe, Compat_RiskOfOptions.OptionIdentityStrings identStrings, bool deferForever, Func<bool> isDisabledDelegate) {
            Compat_RiskOfOptions.AddOption_Keybind((ConfigEntry<KeyboardShortcut>)cfe, identStrings,
                deferForever, isDisabledDelegate);
        }
    }

    ///<summary>Used to register an AutoConfigAttribute with the Risk Of Options mod as a checkbox. Only supports bool properties.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigRoOCheckboxAttribute : BaseAutoConfigRoOAttribute {
        public override Type requiredType => typeof(bool);

        public AutoConfigRoOCheckboxAttribute(string nameOverride = null, string catOverride = null) : base(nameOverride, catOverride) {
        }

        public override void Apply(ConfigEntryBase cfe, Compat_RiskOfOptions.OptionIdentityStrings identStrings, bool deferForever, Func<bool> isDisabledDelegate) {
            Compat_RiskOfOptions.AddOption_CheckBox((ConfigEntry<bool>)cfe, identStrings,
                deferForever, isDisabledDelegate);
        }
    }

    ///<summary>Used to register an AutoConfigAttribute with the Risk Of Options mod as a text input. Only supports string properties.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigRoOStringAttribute : BaseAutoConfigRoOAttribute {
        public override Type requiredType => typeof(string);

        public AutoConfigRoOStringAttribute(string nameOverride = null, string catOverride = null) : base(nameOverride, catOverride) {
        }

        public override void Apply(ConfigEntryBase cfe, Compat_RiskOfOptions.OptionIdentityStrings identStrings, bool deferForever, Func<bool> isDisabledDelegate) {
            Compat_RiskOfOptions.AddOption_String((ConfigEntry<string>)cfe, identStrings,
                deferForever, isDisabledDelegate);
        }
    }

    ///<summary>Used to point the Risk Of Options mod to the owner plugin of an AutoConfigContainer or property. If not present, AutoConfig will attempt to read mod info from your plugin assembly's exported types.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoConfigRoOInfoOverridesAttribute : Attribute {
        public string modGuid;
        public string modName;
        public string categoryName;
        public string entryName;

        public AutoConfigRoOInfoOverridesAttribute(string guid, string name, string cat = null, string ent = null) {
            modGuid = guid;
            modName = name;
            categoryName = cat;
            entryName = ent;
        }
        public AutoConfigRoOInfoOverridesAttribute(System.Type ownerPluginType, string cat = null, string ent = null) {
            var plugin = ownerPluginType.GetCustomAttribute<BepInPlugin>();
            if(plugin == null) {
                TinkersSatchelPlugin._logger.LogError($"AutoConfigContainerRoOInfoAttribute received an invalid type {ownerPluginType.Name} with no BepInPluginAttribute");
                return;
            }
            modGuid = plugin.GUID;
            modName = plugin.Name;
            categoryName = cat;
            entryName = ent;
        }
    }
}
