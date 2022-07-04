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

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Presets",
                name = "ItemsOn",
                description = "Enable All Items/Equipment",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allModules.Where(m => m is Item or Equipment), true));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Presets",
                name = "ItemsOff",
                description = "Disable All Items/Equipment",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allModules.Where(m => m is Item or Equipment), false));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Presets",
                name = "ArtisOn",
                description = "Enable All Artifacts",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allModules.Where(m => m is Artifact), true));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Presets",
                name = "ArtisOff",
                description = "Disable All Artifacts",
            }, "Set", () => SetEnabled(allModules.Where(m => m is Artifact), true));
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"

            HashSet<T2Module> allSkills = new() {
                CommandoPrimaryPulse.instance,
                CommandoUtilityJinkJet.instance,
                CommandoSpecialPlasmaGrenade.instance,
                EngiPrimaryFlak.instance,
                EngiSecondaryChaff.instance,
                EngiUtilitySpeedispenser.instance
            };
            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Presets",
                name = "SkillsOn",
                description = "Enable All Skills",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allSkills, true));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Presets",
                name = "SkillsOff",
                description = "Disable All Skills",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allSkills, false));

            HashSet<T2Module> allDrones = new() {
                BulwarkDrone.instance,
                ItemDrone.instance
            };
            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Presets",
                name = "DronesOn",
                description = "Enable All Drones",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allDrones, true));

            Compat_RiskOfOptions.AddOption_Button(new Compat_RiskOfOptions.OptionIdentityStrings {
                category = "Presets",
                name = "DronesOff",
                description = "Disable All Drones",
                modGuid = TinkersSatchelPlugin.ModGuid + "Presets",
                modName = TinkersSatchelPlugin.ModName + "Presets"
            }, "Set", () => SetEnabled(allDrones, false));
        }
	}
}