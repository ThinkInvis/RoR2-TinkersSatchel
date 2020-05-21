using BepInEx;
using R2API;
using R2API.Utils;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Path = System.IO.Path;
using TILER2;
using static TILER2.MiscUtil;

//TODO:
// Add missing documentation in... a whole lotta places... whoops.
// Change H3AD-5T V2 to a green item if removing the stomp effect?
// Add lots of missing items!
// Watch for R2API.StatsAPI or similar, for use in some items like Bitter Root, Mysterious Vial, Rusty Jetpack
// Find out how to safely and instantaneously change money counter, for cases like Life Savings that shouldn't have the sound effects
// Engineer turrets spammed errors during FixedUpdate and/or RecalculateStats at one point?? Probably resolved now but keep an eye out for things like this

namespace ThinkInvisible.TinkersSatchel {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, "1.2.1")]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI), nameof(BuffAPI), nameof(LoadoutAPI))]
    public class TinkersSatchelPlugin:BaseUnityPlugin {
        public const string ModVer = "1.0.0";
        public const string ModName = "TinkersSatchel";
        public const string ModGuid = "com.ThinkInvisible.TinkersSatchel";

        private static ConfigFile cfgFile;
        
        internal static FilingDictionary<ItemBoilerplate> masterItemList = new FilingDictionary<ItemBoilerplate>();
        
        private TinkersSatchelPlugin() {
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TinkersSatchel.tinkerssatchel_assets")) {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@TinkersSatchel", bundle);
                ResourcesAPI.AddProvider(provider);
            }
            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            masterItemList = ItemBoilerplate.InitAll("TinkersSatchel");
            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupConfig(cfgFile);
            }

            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupAttributes("TINKSATCH", "TKSCH");
                Debug.Log("TKSCH"+x.itemCodeName + ": " + (x is Equipment ? ("EQP"+((int)((Equipment)x).regIndex).ToString()) : ((int)((Item)x).regIndex).ToString()));
            }
        }

        private void Awake() {
            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupBehavior();
            }
        }
    }
}
