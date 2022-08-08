using RoR2;
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

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static CharacterMaster TryApply(CharacterMaster ownerMaster, string targetPrefabName) {
            return Dronemeld.DronemeldPlugin.TryApply(ownerMaster, targetPrefabName);
        }

        private static bool? _enabled;
        public static bool enabled {
            get {
                if(_enabled == null) {
                    if(BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.ThinkInvisible.Dronemeld", out var pi)) {
                        var versionOK = pi.Metadata.Version >= new System.Version(1, 3, 0);
                        _enabled = versionOK;
                        if(!versionOK) {
                            TinkersSatchelPlugin._logger.LogError("Dronemeld is installed, but has an older version than supported. Install Dronemeld 1.3.0 or later to enable compatibility for Item Drone/Bulwark Drone.");
                        }
                    } else {
                        _enabled = false;
                    }
                }
                return (bool)_enabled;
            }
        }
    }
}