using RoR2;
using RoR2.Skills;
using System.Runtime.CompilerServices;

namespace ThinkInvisible.TinkersSatchel {
    ///<summary>
    ///Provides safe hooks for the Standalone Ancient Scepter mod. Check Compat_AncientScepter.enabled before using any other contained members.
    ///</summary>
    internal static class Compat_AncientScepter {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillDef targetVariantDef) {
            return AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(replacingDef, targetBodyName, targetVariantDef);
        }

        private static bool? _enabled;
        internal static bool enabled {
            get {
                if(_enabled == null) _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter");
                return (bool)_enabled;
            }
        }
    }
}