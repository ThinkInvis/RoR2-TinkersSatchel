using BepInEx;
using R2API;
using R2API.Utils;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Path = System.IO.Path;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, TILER2Plugin.ModVer)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(PrefabAPI), nameof(RecalculateStatsAPI), nameof(UnlockableAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class TinkersSatchelPlugin:BaseUnityPlugin {
        public const string ModVer = "1.9.6";
        public const string ModName = "TinkersSatchel";
        public const string ModGuid = "com.ThinkInvisible.TinkersSatchel";

        private static ConfigFile cfgFile;
        
        internal static FilingDictionary<CatalogBoilerplate> masterItemList = new FilingDictionary<CatalogBoilerplate>();
        
        internal static BepInEx.Logging.ManualLogSource _logger;

        internal static AssetBundle resources;

        private void Awake() {
            _logger = Logger;

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TinkersSatchel.tinkerssatchel_assets")) {
                resources = AssetBundle.LoadFromStream(stream);
            }
            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            masterItemList = T2Module.InitAll<CatalogBoilerplate>(new T2Module.ModInfo {
                displayName = "Tinker's Satchel",
                longIdentifier = "TinkersSatchel",
                shortIdentifier = "TKSAT",
                mainConfigFile = cfgFile
            });

            T2Module.SetupAll_PluginAwake(masterItemList);
        }

        private void Start() {
            T2Module.SetupAll_PluginStart(masterItemList);
        }
    }
}
