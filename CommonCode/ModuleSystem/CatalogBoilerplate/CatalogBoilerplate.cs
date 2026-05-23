using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    /// <summary>
    /// A wrapper for T2Module that provides some fields/properties common to most RoR2 catalog types.
    /// </summary>
    public abstract class CatalogBoilerplate : Module {
        public string nameToken { get; private protected set; }
        public string pickupToken { get; private protected set; }
        public string descToken { get; private protected set; }
        public string loreToken { get; private protected set; }

        protected virtual string[] GetNameStringArgs(string langID = null) => new string[0];
        protected virtual string[] GetPickupStringArgs(string langID = null) => new string[0];
        protected virtual string[] GetDescStringArgs(string langID = null) => new string[0];
        protected virtual string[] GetLoreStringArgs(string langID = null) => new string[0];

        /// <summary>Used by the module system to request language token value updates (object name). If langID is null, the request is for the invariant token.</summary>
        protected virtual string GetNameString(string langID = null) {
            try {
                return string.Format(CommonCode.GetBestLanguage(langID)?.GetLocalizedFormattedStringByToken(nameToken ?? "Language load error! (null token)", GetNameStringArgs(langID)) ?? "Language load error!");
            } catch(FormatException) {
                TinkersSatchelPlugin._logger.LogError($"Argument count mismatch while retrieving string {nameToken}");
                return $"Language load error! (argument count mismatch; expected {GetNameStringArgs(langID).Length} total)";
            }
        }
        /// <summary>Used by the module system to request language token value updates (pickup text, where applicable). If langID is null, the request is for the invariant token.</summary>
        protected virtual string GetPickupString(string langID = null) {
            try {
                return string.Format(CommonCode.GetBestLanguage(langID)?.GetLocalizedFormattedStringByToken(pickupToken ?? "Language load error! (null token)", GetPickupStringArgs(langID)) ?? "Language load error!");
            } catch(FormatException) {
                TinkersSatchelPlugin._logger.LogError($"Argument count mismatch while retrieving string {pickupToken}");
                return $"Language load error! (argument count mismatch; expected {GetPickupStringArgs(langID).Length} total)";
            }
        }
        /// <summary>Used by the module system to request language token value updates (description text). If langID is null, the request is for the invariant token.</summary>
        protected virtual string GetDescString(string langID = null) {
            try {
                return string.Format(CommonCode.GetBestLanguage(langID)?.GetLocalizedFormattedStringByToken(descToken ?? "Language load error! (null token)", GetDescStringArgs(langID)) ?? "Language load error!");
            } catch(FormatException) {
                TinkersSatchelPlugin._logger.LogError($"Argument count mismatch while retrieving string {descToken}");
                return $"Language load error! (argument count mismatch; expected {GetDescStringArgs(langID).Length} total)";
            }
        }
        /// <summary>Used by the module system to request language token value updates (lore text, where applicable). If langID is null, the request is for the invariant token.</summary>
        protected virtual string GetLoreString(string langID = null) {
            try {
                return string.Format(CommonCode.GetBestLanguage(langID)?.GetLocalizedFormattedStringByToken(loreToken ?? "Language load error! (null token)", GetLoreStringArgs(langID)) ?? "Language load error!");
            } catch(FormatException) {
                TinkersSatchelPlugin._logger.LogError($"Argument count mismatch while retrieving string {loreToken}");
                return $"Language load error! (argument count mismatch; expected {GetLoreStringArgs(langID).Length} total)";
            }
        }
        /// <summary>Used by the module system to request pickup/logbook model updates, where applicable. Return null (default behavior) to keep the original.</summary>
        protected virtual GameObject GetPickupModel() {
            return null;
        }

        public CatalogBoilerplate() {
            CatalogBoilerplateModule.allInstances.Add(this);
        }

        public override void SetupConfig() {
            base.SetupConfig();
            ConfigEntryChanged += (sender, args) => {
                if((args.flags & AutoConfigUpdateActionTypes.InvalidateModel) == AutoConfigUpdateActionTypes.InvalidateModel) {
                    var newModel = GetPickupModel();
                    if(newModel != null) {
                        if(pickupDef != null) pickupDef.displayPrefab = newModel;
                        if(logbookEntry != null) logbookEntry.modelPrefab = newModel;
                    }
                }
            };
        }

        public override void RefreshPermanentLanguage() {
            var ldis = Language.GetString("TKSAT_CONFIG_DISABLED");
            permanentGenericLanguageTokens[nameToken + "_RENDERED"] = (enabled ? "" : ldis) + GetNameString();
            permanentGenericLanguageTokens[pickupToken + "_RENDERED"] = (enabled ? "" : ldis) + GetPickupString();
            permanentGenericLanguageTokens[descToken + "_RENDERED"] = (enabled ? "" : $"{ldis}\n") + GetDescString();
            permanentGenericLanguageTokens[loreToken + "_RENDERED"] = "" + GetLoreString();

            foreach(var lang in Language.languagesByName.Keys) {
                if(!permanentSpecificLanguageTokens.ContainsKey(lang)) permanentSpecificLanguageTokens.Add(lang, new Dictionary<string, string>());
                var specLang = permanentSpecificLanguageTokens[lang];
                var ldisSpec = Language.GetString("TKSAT_CONFIG_DISABLED", lang);
                specLang[nameToken + "_RENDERED"] = (enabled ? "" : ldisSpec) + GetNameString(lang);
                specLang[pickupToken + "_RENDERED"] = (enabled ? "" : ldisSpec) + GetPickupString(lang);
                specLang[descToken + "_RENDERED"] = (enabled ? "" : $"{ldisSpec}\n") + GetDescString(lang);
                specLang[loreToken + "_RENDERED"] = "" + GetLoreString(lang);
            }

            base.RefreshPermanentLanguage();
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
            nameToken = $"{modInfo.longIdentifier.ToUpper()}_{name.ToUpper()}_NAME";
            descToken = $"{modInfo.longIdentifier.ToUpper()}_{name.ToUpper()}_DESC";
            pickupToken = $"{modInfo.longIdentifier.ToUpper()}_{name.ToUpper()}_PICKUP";
            loreToken = $"{modInfo.longIdentifier.ToUpper()}_{name.ToUpper()}_LORE";
        }

        public virtual void SetupCatalogReady() { }

        public override void Install() {
            base.Install();
            if(PreGameController.instance)
                PreGameController.instance.RecalculateModifierAvailability();
        }

        public override void Uninstall() {
            base.Uninstall();
            if(PreGameController.instance)
                PreGameController.instance.RecalculateModifierAvailability();
        }

        public PickupDef pickupDef {get; internal set;}
        public PickupIndex pickupIndex {get; internal set;}
        public RoR2.UI.LogBook.Entry logbookEntry {get; internal set; }
        public RuleDef ruleDef { get; internal set; }

        protected internal override AutoConfigUpdateActionTypes defaultEnabledUpdateFlags => AutoConfigUpdateActionTypes.None;
        public override bool managedEnable => true;
        public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferUntilNextStage;
        public override AutoConfigUpdateActionTypes enabledConfigUpdateActionTypes => AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats | AutoConfigUpdateActionTypes.InvalidateDropTable;

        ///<summary>A generic GameObject to use as the object's model.</summary>
        public GameObject modelResource {get; protected set;} = null;
        ///<summary>A sprite object to use as the object's icon.</summary>
        public Sprite iconResource {get; protected set;} = null;

        ///<summary>The object's display name in the mod's default language. Will be used in config files; should also be used in generic language tokens.</summary>
        [Obsolete("No longer in use. Replaced by LanguageAPI systems: use a language file or similar to define token ModIdent_ClassName_NAME, e.g. MYMOD_MYITEM_NAME.")]
        public virtual string displayName { get; } = null;

        public static void ConsoleDump(BepInEx.Logging.ManualLogSource logger, FilingDictionary<CatalogBoilerplate> instances) {
            int longestClassName = 0;
            int longestObjectName = 0;
            var allStrings = new List<ConsoleStrings>();
            foreach(CatalogBoilerplate x in instances) {
                var strings = x.GetConsoleStrings();
                allStrings.Add(strings);
                longestClassName = Mathf.Max(strings.className.Length, longestClassName);
                longestObjectName = Mathf.Max(strings.objectName.Length, longestObjectName);
            }

            logger.LogMessage("Index dump follows (pairs of name / index):");
            foreach(ConsoleStrings strings in allStrings) {
                logger.LogMessage($"{strings.className.PadLeft(longestClassName)} {strings.objectName.PadRight(longestObjectName)} / {strings.formattedIndex}");
            }
        }

        public struct ConsoleStrings {
            public string className;
            public string objectName;
            public string formattedIndex;
        }
        public virtual ConsoleStrings GetConsoleStrings() {
            return new ConsoleStrings {
                className = "Other",
                objectName = this.name,
                formattedIndex = "N/A"
                };
        }
    }
}