using RoR2;
using RoR2.Skills;
using System;
using System.Runtime.CompilerServices;

namespace ThinkInvisible.TinkersSatchel {
    ///<summary>
    ///Provides safe hooks for the Dronemeld mod. Check Compat_Dronemeld.enabled before using any other contained members except SafeGetCount.
    ///</summary>
    public static class Compat_Dronemeld {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static int SafeGetStackCount(Inventory inv) {
            if(!enabled) return 0;
            return GetStackCount(inv);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static int GetStackCount(Inventory inv) {
            return inv.GetItemCount(Dronemeld.DronemeldPlugin.stackItem);
        }

        private static bool? _enabled;
        public static bool enabled {
            get {
                if(_enabled == null) _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.Dronemeld");
                return (bool)_enabled;
            }
        }
    }
}