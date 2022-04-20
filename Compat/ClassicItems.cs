using System.Runtime.CompilerServices;

namespace ThinkInvisible.TinkersSatchel {
    ///<summary>
    ///Provides safe hooks for the ClassicItems mod. Check Compat_ClassicItems.enabled before using any other contained members.
    ///</summary>
    public static class Compat_ClassicItems {

        private static bool? _enabled;
        public static bool enabled {
            get {
                if(_enabled == null) _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.ClassicItems");
                return (bool)_enabled;
            }
        }
    }
}