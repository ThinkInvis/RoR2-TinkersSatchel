using BepInEx;
using R2API.Utils;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Path = System.IO.Path;
using System.Linq;
using UnityEngine.AddressableAssets;
using System;

namespace ThinkInvisible.TinkersSatchel {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AncientScepter.AncientScepterMain.ModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(Dronemeld.DronemeldPlugin.ModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class TinkersSatchelPlugin:BaseUnityPlugin {
        public const string ModVer = "6.0.1";
        public const string ModName = "TinkersSatchel";
        public const string ModGuid = "com.ThinkInvisible.TinkersSatchel";

        private static ConfigFile cfgFile;
        
        internal static FilingDictionary<Module> allModules = new();
        
        internal static BepInEx.Logging.ManualLogSource _logger;

        internal static AssetBundle resources;

        Module[] earlyLoad;

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

            Module.SetupModuleClass();

            var modInfo = new Module.ModInfo {
                displayName = "Tinker's Satchel",
                longIdentifier = "TinkersSatchel",
                shortIdentifier = "TKSAT",
                mainConfigFile = cfgFile
            };

            allModules = Module.InitAll<Module>(modInfo);

            earlyLoad = new Module[] { CommonCode.instance, TauntDebuffModule.instance, TimedSkillDisableModule.instance };
            Module.SetupAll_PluginAwake(earlyLoad);
            Module.SetupAll_PluginAwake(allModules.Except(earlyLoad));

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
                    material.shader = Addressables.LoadAssetAsync<Shader>(material.shader.name[5..])
                        .WaitForCompletion();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        private void Start() {
            Module.SetupAll_PluginStart(earlyLoad);
            Module.SetupAll_PluginStart(allModules.Except(earlyLoad));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        private void Update() {
            if(!RoR2.RoR2Application.loadFinished) return;
            AutoConfigModule.Update();
        }
    }
}
