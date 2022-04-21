using RoR2;
using RoR2.Skills;
using System;
using System.Runtime.CompilerServices;

namespace ThinkInvisible.TinkersSatchel {
    ///<summary>
    ///Provides safe hooks for the ClassicItems mod. Check Compat_ClassicItems.enabled before using any other contained members.
    ///</summary>
    public static class Compat_ClassicItems {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static int CheckProc(CharacterBody body, EquipmentDef eqp) {
            return ClassicItems.Embryo.CheckLastEmbryoProc(body, eqp);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static int CheckProc(EquipmentSlot slot, EquipmentDef eqp) {
            return ClassicItems.Embryo.CheckLastEmbryoProc(slot, eqp);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterEmbryoHook(EquipmentDef equipment, string descAppendToken = null, Func<string> configDisplayNameDelegate = null, Action installHooksDelegate = null, Action uninstallHooksDelegate = null, Action<CharacterBody> addComponentsDelegate = null, Action setupAttributesDelegate = null, Action setupConfigDelegate = null) {
            ClassicItems.Embryo.RegisterHook(equipment, descAppendToken, configDisplayNameDelegate, installHooksDelegate, uninstallHooksDelegate, addComponentsDelegate, setupAttributesDelegate, setupConfigDelegate);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillSlot targetSlot, int targetVariant) {
            return ClassicItems.Scepter.instance.RegisterScepterSkill(replacingDef, targetBodyName, targetSlot, targetVariant);
        }

        private static bool? _enabled;
        public static bool enabled {
            get {
                if(_enabled == null) _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.ClassicItems");
                return (bool)_enabled;
            }
        }
    }
}