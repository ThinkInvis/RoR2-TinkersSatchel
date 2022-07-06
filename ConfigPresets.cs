using TILER2;
using System.Collections.Generic;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
	public class ConfigPresets : T2Module<ConfigPresets> {
        public override bool managedEnable => false;

        void SetEnabled(IEnumerable<T2Module> modules, bool enab) {
            HashSet<BepInEx.Configuration.ConfigFile> needsManualSave = new();
            foreach(var module in modules) {
                var bind = module.FindConfig(nameof(module.enabled));
                bind.configEntry.BoxedValue = enab;
                if(!bind.configEntry.ConfigFile.SaveOnConfigSet)
                    needsManualSave.Add(bind.configEntry.ConfigFile);
            }
            foreach(var sv in needsManualSave) {
                sv.Save();
            }
        }

        public override void SetupConfig() {
            base.SetupConfig();

            if(!Compat_RiskOfOptions.enabled) return;

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Enable...",
                name = "Items",
                description = "Enable All Items",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allModules.Where(m => m is Item), true));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Disable...",
                name = "Items",
                description = "Disable All Items",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allModules.Where(m => m is Item), false));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Enable...",
                name = "Equipment",
                description = "Enable All Equipment",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allModules.Where(m => m is Equipment), true));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Disable...",
                name = "Equipment",
                description = "Disable All Equipment",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allModules.Where(m => m is Equipment), false));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Enable...",
                name = "Artifacts",
                description = "Enable All Artifacts",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allModules.Where(m => m is Artifact), true));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Disable...",
                name = "Artifacts",
                description = "Disable All Artifacts",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allModules.Where(m => m is Artifact), false));

            HashSet<T2Module> allSkills = new() {
                CommandoPrimaryPulse.instance,
                CommandoUtilityJinkJet.instance,
                CommandoSpecialPlasmaGrenade.instance,
                EngiPrimaryFlak.instance,
                EngiSecondaryChaff.instance,
                EngiUtilitySpeedispenser.instance
            };
            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Enable...",
                name = "Skills",
                description = "Enable All Skills",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allSkills, true));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Disable...",
                name = "Skills",
                description = "Disable All Skills",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allSkills, false));

            HashSet<T2Module> allDrones = new() {
                BulwarkDrone.instance,
                ItemDrone.instance
            };
            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Enable...",
                name = "Drones",
                description = "Enable All Drones",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allDrones, true));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Disable...",
                name = "Drones",
                description = "Disable All Drones",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allDrones, false));
        }
	}
}