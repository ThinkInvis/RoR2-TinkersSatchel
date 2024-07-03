using BepInEx;
using R2API;
using R2API.Utils;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Path = System.IO.Path;
using TILER2;
using static TILER2.MiscUtil;
using System.Linq;
using UnityEngine.AddressableAssets;
using System;

namespace ThinkInvisible.TinkersSatchel {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, TILER2Plugin.ModVer)]
    [BepInDependency(AncientScepter.AncientScepterMain.ModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(Dronemeld.DronemeldPlugin.ModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class TinkersSatchelPlugin:BaseUnityPlugin {
        public const string ModVer = "4.2.0";
        public const string ModName = "TinkersSatchel";
        public const string ModGuid = "com.ThinkInvisible.TinkersSatchel";

        private static ConfigFile cfgFile;
        
        internal static FilingDictionary<T2Module> allModules = new();
        
        internal static BepInEx.Logging.ManualLogSource _logger;

        internal static AssetBundle resources;

        T2Module[] earlyLoad;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        private void Awake() {
            _logger = Logger;

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TinkersSatchel.tinkerssatchel_assets")) {
                resources = AssetBundle.LoadFromStream(stream);
            }

            try {
                UnstubShaders();
            } catch(Exception ex) {
                _logger.LogError($"Shader unstub failed: {ex} {ex.Message}");
            }

            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            var modInfo = new T2Module.ModInfo {
                displayName = "Tinker's Satchel",
                longIdentifier = "TinkersSatchel",
                shortIdentifier = "TKSAT",
                mainConfigFile = cfgFile
            };
            allModules = T2Module.InitAll<T2Module>(modInfo);

            earlyLoad = new T2Module[] { CommonCode.instance, TauntDebuffModule.instance, TimedSkillDisableModule.instance };
            T2Module.SetupAll_PluginAwake(earlyLoad);
            T2Module.SetupAll_PluginAwake(allModules.Except(earlyLoad));

            foreach(var mod in allModules.Except(earlyLoad)) {
                if(mod is Item item) {
                    item.itemDef.requiredExpansion = item.itemTier switch {
                        RoR2.ItemTier.VoidTier1 or RoR2.ItemTier.VoidTier2 or RoR2.ItemTier.VoidTier3 => CommonCode.voidExpansionDef,
                        _ => CommonCode.expansionDef,
                    };
                } else if(mod is Equipment equipment) {
                    equipment.equipmentDef.requiredExpansion = CommonCode.expansionDef;
                } else if(mod is Artifact artifact) {
                    artifact.artifactDef.requiredExpansion = CommonCode.expansionDef;
                }
            }
        }

        private void UnstubShaders() {
            var materials = resources.LoadAllAssets<Material>();
            foreach(Material material in materials)
                if(material.shader.name.StartsWith("STUB_"))
                    material.shader = Addressables.LoadAssetAsync<Shader>(material.shader.name.Substring(5))
                        .WaitForCompletion();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        private void Start() {
            T2Module.SetupAll_PluginStart(earlyLoad);
            T2Module.SetupAll_PluginStart(allModules.Except(earlyLoad));
        }
    }
}
