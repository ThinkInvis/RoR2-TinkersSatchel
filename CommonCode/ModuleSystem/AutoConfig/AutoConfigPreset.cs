using System;
using System.Collections.Generic;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public static class AutoConfigPresetExtensions {
        public static void ApplyPreset(this AutoConfigContainer container, string name) {
            HashSet<BepInEx.Configuration.ConfigFile> needsManualSave = new();

            foreach(var bind in container.bindings) {
                var presets = bind.boundProperty.GetCustomAttributes(typeof(AutoConfigPresetAttribute), true).Cast<AutoConfigPresetAttribute>();
                var matchingPreset = presets.FirstOrDefault(p => p.presetName == name);
                if(matchingPreset != null) {
                    bind.configEntry.BoxedValue = matchingPreset.boxedValue;
                    if(!bind.configEntry.ConfigFile.SaveOnConfigSet)
                        needsManualSave.Add(bind.configEntry.ConfigFile);
                }
            }

            foreach(var sv in needsManualSave) {
                sv.Save();
            }
        }
    }

    ///<summary>Use with an AutoConfigAttribute to attach metadata for a config preset.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class AutoConfigPresetAttribute : Attribute {
        public readonly string presetName;
        public readonly object boxedValue;

        public AutoConfigPresetAttribute(string name, object value) {
            this.presetName = name;
            this.boxedValue = value;
        }
    }
}
