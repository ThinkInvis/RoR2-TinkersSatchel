using RoR2;
using UnityEngine;
using TILER2;
using R2API.Utils;
using UnityEngine.Networking;
using R2API;
using static TILER2.MiscUtil;
using System.Collections.Generic;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using static R2API.RecalculateStatsAPI;

namespace ThinkInvisible.TinkersSatchel {
    public class PackTactics : Artifact<PackTactics> {

        ////// Artifact Data //////

        public override string displayName => "Artifact of Tactics";

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetDescString(string langid = null) => "All combatants give nearby teammates small, stacking boosts to speed, damage, and armor.";



        ////// Config //////

        [AutoConfig("Combatants within this distance (in meters) of teammates will buff them if Artifact of Tactics is enabled.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseRadius { get; private set; } = 25f;

        [AutoConfig("Extra move speed multiplier added per stack of the Tactics buff.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float speedMod { get; private set; } = 0.05f;

        [AutoConfig("Extra damage multiplier added per stack of the Tactics buff.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float damageMod { get; private set; } = 0.1f;

        [AutoConfig("Extra armor added per stack of the Tactics buff.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float armorMod { get; private set; } = 15f;



        ////// Other Fields/Properties //////

        public BuffDef tacticsBuff {get;private set;}
        public GameObject tacticsWardPrefab {get;private set;}



        ////// TILER2 Module Setup //////
        #region TILER2 Module Setup
        public PackTactics() {
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/tactics_on.png");
            iconResourceDisabled = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/tactics_off.png");
        }

        public override void SetupConfig() {
            base.SetupConfig();

            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(baseRadius) && NetworkServer.active) {
                    foreach(var w in TacticsWard.instances)
                        w.GetComponent<BuffWard>().Networkradius = (float)args.newValue;
                }
            };
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
            tacticsBuff = ScriptableObject.CreateInstance<BuffDef>();
            tacticsBuff.buffColor = Color.white;
            tacticsBuff.canStack = true;
            tacticsBuff.isDebuff = false;
            tacticsBuff.name = modInfo.shortIdentifier + "TacticsBuff";
            tacticsBuff.iconSprite = iconResource;
            ContentAddition.AddBuffDef(tacticsBuff);

            var tacticsPrefabPrefab = new GameObject("TacticsAuraPrefabPrefab");
            tacticsPrefabPrefab.AddComponent<NetworkIdentity>();
            tacticsPrefabPrefab.AddComponent<TeamFilter>();
            tacticsPrefabPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;
            tacticsPrefabPrefab.AddComponent<TacticsWard>();
            var bw = tacticsPrefabPrefab.AddComponent<BuffWard>();
            bw.invertTeamFilter = false;
            bw.expires = false;
            bw.animateRadius = false;
            bw.radius = baseRadius;
            bw.rangeIndicator = null;
            bw.Networkradius = baseRadius;
            bw.buffDuration = 1f;
            bw.interval = 1f;
            bw.buffDef = tacticsBuff;
            tacticsWardPrefab = tacticsPrefabPrefab.InstantiateClone("TacticsAuraPrefab", true);
            UnityEngine.Object.Destroy(tacticsPrefabPrefab);
        }

        public override void Install() {
            base.Install();

            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
            if(IsActiveAndEnabled()) {
                foreach(var cm in AliveList())
                    if(cm.hasBody) AddWard(cm.GetBody());
            }
            On.RoR2.CharacterMaster.OnBodyStart += On_CMOnBodyStart;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.CharacterMaster.OnBodyStart -= On_CMOnBodyStart;
            foreach(var w in TacticsWard.instances)
                UnityEngine.Object.Destroy(w.gameObject);
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }
        #endregion



        ////// Hooks //////

        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            if(!sender) return;
            var totalBuffs = Mathf.Max(sender.GetBuffCount(tacticsBuff) - 1, 0);
            args.moveSpeedMultAdd += totalBuffs * speedMod;
            args.baseDamageAdd += totalBuffs * damageMod;
            args.armorAdd += totalBuffs * armorMod;
        }
        
        private void On_CMOnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body) {
            orig(self, body);
            if(NetworkServer.active && IsActiveAndEnabled())
                AddWard(body);
        }



        ////// Non-Public Methods //////

        private void AddWard(CharacterBody body) {
            if(!body) return;
            var cpt = body.GetComponentInChildren<TacticsWard>();
            if(!cpt || !cpt.gameObject) {
				var cptObj = UnityEngine.Object.Instantiate(tacticsWardPrefab);
				cptObj.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
				cptObj.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
            }
        }
    }

    internal class TacticsWard : MonoBehaviour {
        internal static List<TacticsWard> instances = new List<TacticsWard>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void Awake() {
            instances.Add(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void OnDestroy() {
            instances.Remove(this);
        }
    }
}