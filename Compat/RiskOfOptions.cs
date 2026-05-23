using BepInEx.Configuration;
using System;
using System.Runtime.CompilerServices;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using UnityEngine;
using UnityEngine.Events;

namespace ThinkInvisible.TinkersSatchel {
    ///<summary>
    ///Provides safe hooks for the RiskOfOptions mod. Check Compat_RiskOfOptions.enabled before using any other contained members.
    ///</summary>
    public static class Compat_RiskOfOptions {
        public struct OptionIdentityStrings {
            public string category;
            public string name;
            public string description;
            public string modName;
            public string modGuid;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetupMod(string modGuid, string modName, string description, Sprite icon = null) {
            ModSettingsManager.SetModDescription(description, modGuid, modName);
            if(icon != null)
                ModSettingsManager.SetModIcon(icon, modGuid, modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOption_CheckBox(ConfigEntry<bool> configEntry, OptionIdentityStrings ident, bool restartRequired, Func<bool> isDisabledDelegate) {
            ModSettingsManager.AddOption(new CheckBoxOption(configEntry, new CheckBoxConfig {
                category = ident.category,
                name = ident.name,
                restartRequired = restartRequired,
                description = ident.description,
                checkIfDisabled = () => { return isDisabledDelegate(); }
            }), ident.modGuid, ident.modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOption_Slider(ConfigEntry<float> configEntry, OptionIdentityStrings ident, float min, float max, string formatString, bool restartRequired, Func<bool> isDisabledDelegate) {
            ModSettingsManager.AddOption(new SliderOption(configEntry, new SliderConfig {
                category = ident.category,
                name = ident.name,
                max = max,
                min = min,
                formatString = formatString,
                restartRequired = restartRequired,
                description = ident.description,
                checkIfDisabled = () => { return isDisabledDelegate(); }
            }), ident.modGuid, ident.modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOption_IntSlider(ConfigEntry<int> configEntry, OptionIdentityStrings ident, int min, int max, string formatString, bool restartRequired, Func<bool> isDisabledDelegate) {
            ModSettingsManager.AddOption(new IntSliderOption(configEntry, new IntSliderConfig {
                category = ident.category,
                name = ident.name,
                max = max,
                min = min,
                formatString = formatString,
                restartRequired = restartRequired,
                description = ident.description,
                checkIfDisabled = () => { return isDisabledDelegate(); }
            }), ident.modGuid, ident.modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOption_Choice(ConfigEntryBase configEntry, OptionIdentityStrings ident, bool restartRequired, Func<bool> isDisabledDelegate) {
            ModSettingsManager.AddOption(new ChoiceOption(configEntry, new ChoiceConfig {
                category = ident.category,
                name = ident.name,
                restartRequired = restartRequired,
                description = ident.description,
                checkIfDisabled = () => { return isDisabledDelegate(); }
            }), ident.modGuid, ident.modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOption_Keybind(ConfigEntry<KeyboardShortcut> configEntry, OptionIdentityStrings ident, bool restartRequired, Func<bool> isDisabledDelegate) {
            ModSettingsManager.AddOption(new KeyBindOption(configEntry, new KeyBindConfig {
                category = ident.category,
                name = ident.name,
                restartRequired = restartRequired,
                description = ident.description,
                checkIfDisabled = () => { return isDisabledDelegate(); }
            }), ident.modGuid, ident.modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOption_String(ConfigEntry<string> configEntry, OptionIdentityStrings ident, bool restartRequired, Func<bool> isDisabledDelegate) {
            ModSettingsManager.AddOption(new StringInputFieldOption(configEntry, new InputFieldConfig {
                category = ident.category,
                name = ident.name,
                restartRequired = restartRequired,
                description = ident.description,
                checkIfDisabled = () => { return isDisabledDelegate(); }
            }), ident.modGuid, ident.modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOption_StepSlider(ConfigEntry<float> configEntry, OptionIdentityStrings ident, float min, float max, float step, string formatString, bool restartRequired, Func<bool> isDisabledDelegate) {
            ModSettingsManager.AddOption(new StepSliderOption(configEntry, new StepSliderConfig {
                category = ident.category,
                name = ident.name,
                min = min,
                max = max,
                formatString = formatString,
                increment = step,
                restartRequired = restartRequired,
                description = ident.description,
                checkIfDisabled = () => { return isDisabledDelegate(); }
            }), ident.modGuid, ident.modName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOption_Button(OptionIdentityStrings ident, string text, UnityAction del) {
            ModSettingsManager.AddOption(new GenericButtonOption(ident.name, ident.category, ident.description, text, del), ident.modGuid, ident.modName);
        }

        private static bool? _enabled;
        public static bool enabled {
            get {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                return (bool)_enabled;
            }
        }
    }
}
