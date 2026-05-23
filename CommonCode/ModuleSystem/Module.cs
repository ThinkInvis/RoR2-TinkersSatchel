using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public abstract class Module<T>:Module where T : Module<T> {
        public static T instance {get;private set;}

        protected Module() {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting Module was instantiated twice");
            instance = this as T;
        }
    }

    /// <summary>
    /// Provides a relatively low-extra-code pattern for dividing a mod into smaller modules, each of which has its own config category managed by AutoConfig.
    /// </summary>
    public abstract class Module : AutoConfigContainer {
        internal static void SetupModuleClass() {
            On.RoR2.Run.Start += On_RunStart;
            On.RoR2.Language.SetCurrentLanguage += Language_SetCurrentLanguage;
        }

        private static void On_RunStart(On.RoR2.Run.orig_Start orig, Run self) {
            orig(self);
            if(!NetworkServer.active) return;
            var rngGenerator = new Xoroshiro128Plus(self.seed);
            foreach(var module in _allModules)
                module.rng = new Xoroshiro128Plus(rngGenerator.nextUlong);
        }

        private static void Language_SetCurrentLanguage(On.RoR2.Language.orig_SetCurrentLanguage orig, string newCurrentLanguageName) {
            orig(newCurrentLanguageName);
            foreach(var module in allModules) {
                module.RefreshPermanentLanguage();
                if(module.enabled) {
                    if(module.languageInstalled)
                        module.UninstallLanguage();
                    module.InstallLanguage();
                }
            }
            AutoConfigModule.globalLanguageDirty = false;
        }

        private static readonly FilingDictionary<Module> _allModules = new();
        public static readonly ReadOnlyFilingDictionary<Module> allModules = _allModules.AsReadOnly();

        public bool enabled { get; protected internal set; } = true;

        public readonly string name;

        ///<summary>If true, Module.enabled will be registered as a config entry.</summary>
        public virtual bool managedEnable => true;
        ///<summary>If true and managedEnabled is true, Module.enabled will be registered as a Risk Of Options option.</summary>
        public virtual bool managedEnableRoO => true;
        ///<summary>If managedEnable is true, this will be appended to the module's enable/disable config description.</summary>
        public virtual string enabledConfigDescription => null;
        ///<summary>If managedEnable is true, this will be used for the resultant config entry.</summary>
        public virtual AutoConfigFlags enabledConfigFlags => AutoConfigFlags.None;
        ///<summary>If managedEnable is true, this will be used for the resultant config entry.</summary>
        public virtual AutoConfigUpdateActionTypes enabledConfigUpdateActionTypes => AutoConfigUpdateActionTypes.InvalidateLanguage;

        protected readonly List<LanguageAPI.LanguageOverlay> languageOverlays = new();
        protected readonly List<LanguageAPI.LanguageOverlay> permanentLanguageOverlays = new();
        protected readonly Dictionary<string, string> genericLanguageTokens = new();
        protected readonly Dictionary<string, Dictionary<string, string>> specificLanguageTokens = new();
        protected readonly Dictionary<string, string> permanentGenericLanguageTokens = new();
        protected readonly Dictionary<string, Dictionary<string, string>> permanentSpecificLanguageTokens = new();
        public bool languageInstalled { get; private set; } = false;
        public bool permanentLanguageInstalled { get; private set; } = false;

        ///<summary>A server-only rng instance based on the current run's seed.</summary>
        public Xoroshiro128Plus rng { get; internal set; }

        ///<summary>Contains various information relating to the mod owning this module.</summary>
        public ModInfo modInfo {get; private set;}

        ///<summary>Will be prepended to the category name of all config entries of this subclass of T2Module.</summary>
        public virtual string configCategoryPrefix => "Modules.";

        /// <summary>
        /// Implement to handle AutoItemConfig binding and other related actions. With standard base plugin setup, will be performed before SetupAttributes and SetupBehavior.
        /// </summary>
        public virtual void SetupConfig() {
            var moduleConfigName = $"{configCategoryPrefix}{name}";
            if(managedEnable) {
                Bind(typeof(Module).GetProperty(nameof(enabled)), modInfo.mainConfigFile, modInfo.displayName, moduleConfigName, new AutoConfigAttribute(
                    $"{((enabledConfigDescription != null) ? (enabledConfigDescription + "\n") : "")}Set to False to disable this module, and as much of its content as can be disabled after initial load. Doing so may cause changes in other modules as well.",
                    enabledConfigFlags), enabledConfigUpdateActionTypes != AutoConfigUpdateActionTypes.None ? new AutoConfigUpdateActionsAttribute(enabledConfigUpdateActionTypes) : null);
                if(managedEnableRoO && Compat_RiskOfOptions.enabled) {
                    var binding = bindings.First(x => x.boundProperty == typeof(Module).GetProperty(nameof(enabled)));
                    BindRoO(binding, new AutoConfigRoOCheckboxAttribute());
                }

            }
            BindAll(modInfo.mainConfigFile, modInfo.displayName, moduleConfigName);
            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(enabled)) {
                    if((bool)args.newValue == true) {
                        Install();
                    } else {
                        Uninstall();
                        if(languageInstalled)
                            UninstallLanguage();
                    }
                    RefreshPermanentLanguage();
                }
                if(args.flags.HasFlag(AutoConfigUpdateActionTypes.InvalidateLanguage)) {
                    if(enabled) {
                        if(languageInstalled)
                            UninstallLanguage();
                        InstallLanguage();
                    }
                    RefreshPermanentLanguage();
                }
            };
        }

        ///<summary>
        ///Implement to handle registration with RoR2 catalogs.
        ///</summary>
        public virtual void SetupAttributes() {}

        ///<summary>Third stage of setup. Should be used to apply permanent hooks and other similar things.</summary>
        public virtual void SetupBehavior() {}

        ///<summary>Fourth stage of setup. Will be performed after all catalogs have initialized.</summary>
        public virtual void SetupLate() {}

        ///<summary>Fifth stage of setup. Should be used to perform final, non-permanent hooks and changes.</summary>
        public virtual void Install() {}

        ///<summary>Should undo EVERY change made by Install.</summary>
        public virtual void Uninstall() {}

        ///<summary>Will be called once after initial language setup, and also if/when the module is installed after setup. Automatically loads tokens from genericLanguageTokens/specificLanguageTokens into R2API Language Overlays in languageOverlays.</summary>
        public virtual void InstallLanguage() {
            languageOverlays.Add(LanguageAPI.AddOverlay(genericLanguageTokens));
            languageOverlays.Add(LanguageAPI.AddOverlay(specificLanguageTokens));
            languageInstalled = true;
            AutoConfigModule.globalLanguageDirty = true;
        }

        ///<summary>Will be called if/when the module is uninstalled after setup, and before any language installation after the first. Automatically undoes all R2API Language Overlays registered to languageOverlays.</summary>
        public virtual void UninstallLanguage() {
            foreach(var overlay in languageOverlays) {
                overlay.Remove();
            }
            languageOverlays.Clear();
            languageInstalled = false;
            AutoConfigModule.globalLanguageDirty = true;
        }

        ///<summary>Handles language that should never be uninstalled for an extended period (e.g. item tokens), contained in permanentGenericLanguageTokens/permanentSpecificLanguageTokens.</summary>
        public virtual void RefreshPermanentLanguage() {
            if(permanentLanguageInstalled) {
                foreach(var overlay in permanentLanguageOverlays)
                    overlay.Remove();
            }
            permanentLanguageOverlays.Clear();
            permanentLanguageOverlays.Add(LanguageAPI.AddOverlay(permanentGenericLanguageTokens));
            permanentLanguageOverlays.Add(LanguageAPI.AddOverlay(permanentSpecificLanguageTokens));
            AutoConfigModule.globalLanguageDirty = true;
        }

        /// <summary>
        /// Call to scan your plugin's assembly for classes inheriting directly from a specific subtype of Module, initialize all of them, and prepare a list for further setup.
        /// Has special handling for the MyClass : ModuleOrModuleSubclass&lt;MyClass&gt; pattern.
        /// </summary>
        /// <returns>A FilingDictionary containing all instances that this method just initialized.</returns>
        public static FilingDictionary<T> InitDirect<T>(ModInfo modInfo) where T:Module {
            return InitAll<T>(modInfo, Assembly.GetCallingAssembly(), t => (t.BaseType.IsGenericType
                ? (t.BaseType.GenericTypeArguments[0] == t && t.BaseType.BaseType == typeof(T))
                : t.BaseType == typeof(T)));
        }

        /// <summary>
        /// Call to scan your plugin's assembly for classes inheriting directly or indirectly from a specific subtype of Module, initialize all of them, and prepare a list for further setup.
        /// </summary>
        /// <returns>A FilingDictionary containing all instances that this method just initialized.</returns>
        public static FilingDictionary<T> InitAll<T>(ModInfo modInfo, Func<Type, bool> extraTypeChecks = null) where T:Module {
            return InitAll<T>(modInfo, Assembly.GetCallingAssembly(), extraTypeChecks);
        }

        private static FilingDictionary<T> InitAll<T>(ModInfo modInfo, Assembly callingAssembly, Func<Type, bool> extraTypeChecks) where T:Module {
            var f = new FilingDictionary<T>();
            foreach(Type type in callingAssembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(T)) && (extraTypeChecks?.Invoke(t) ?? true))) {
                var newModule = (T)Activator.CreateInstance(type, nonPublic: true);
                newModule.modInfo = modInfo;
                f.Add(newModule);
            }
            return f;
        }
        
        /// <summary>
        /// Call to scan your plugin's assembly for classes inheriting directly from Module, initialize all of them, and prepare a list for further setup.
        /// </summary>
        /// <returns>A FilingDictionary containing all instances that this method just initialized.</returns>
        public static FilingDictionary<Module> InitModules(ModInfo modInfo) {
            return InitAll<Module>(modInfo, Assembly.GetCallingAssembly(), t => (t.BaseType.IsGenericType
                ? (t.BaseType.GenericTypeArguments[0] == t && t.BaseType.BaseType == typeof(Module))
                : t.BaseType == typeof(Module)));
        }

        public static void SetupAll_PluginAwake(IEnumerable<Module> modulesToSetup) {
            foreach(var module in modulesToSetup) {
                module.SetupConfig();
            }
            foreach(var module in modulesToSetup) {
                module.SetupAttributes();
            }
            foreach(var module in modulesToSetup) {
                module.SetupBehavior();
            }
        }

        public static void SetupAll_PluginStart(IEnumerable<Module> modulesToSetup, bool installUnmanaged = false) {
            foreach(var module in modulesToSetup) {
                module.SetupLate();
            }
            foreach(var module in modulesToSetup) {
                if((installUnmanaged || module.managedEnable) && module.enabled)
                    module.Install();
            }
        }

        protected Module() {
            name = GetType().Name;
            _allModules.Add(this);
        }

        public struct ModInfo {
            public string displayName;
            public string longIdentifier;
            public string shortIdentifier;
            public ConfigFile mainConfigFile;
        }
    }
}