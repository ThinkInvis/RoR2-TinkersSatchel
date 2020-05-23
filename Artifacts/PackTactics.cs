﻿using RoR2;
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
using static TILER2.StatHooks;

namespace ThinkInvisible.TinkersSatchel {
    public class PackTactics : Artifact<PackTactics> {
        public override string displayName => "Artifact of Tactics";

        [AutoItemConfig("Combatants within this distance (in meters) of teammates will buff them if Artifact of Tactics is enabled.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float baseRadius {get; private set;} = 25f;

        [AutoItemConfig("Extra move speed multiplier added per stack of the Tactics buff.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float speedMod {get; private set;} = 0.05f;

        [AutoItemConfig("Extra damage multiplier added per stack of the Tactics buff.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float damageMod {get; private set;} = 0.1f;

        [AutoItemConfig("Extra armor added per stack of the Tactics buff.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float armorMod {get; private set;} = 15f;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangDesc(string langid = null) => "All combatants give nearby teammates small, stacking boosts to speed, damage, and armor.";

        public BuffIndex tacticsBuff {get;private set;}
        public GameObject tacticsWardPrefab {get;private set;}

        public PackTactics() {
            iconPathName = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/tactics_on.png";
            iconPathNameDisabled = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/tactics_off.png";
            onAttrib += (tokenIdent, namePrefix) => {
                var tacticsBuffDef = new CustomBuff(new BuffDef {
                    buffColor = Color.white,
                    canStack = true,
                    isDebuff = false,
                    name = namePrefix + "TacticsBuff",
                    iconPath = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/tactics_on.png"
                });
                tacticsBuff = BuffAPI.Add(tacticsBuffDef);

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
                bw.buffType = tacticsBuff;
			    tacticsWardPrefab = tacticsPrefabPrefab.InstantiateClone("TacticsAuraPrefab");
			    UnityEngine.Object.Destroy(tacticsPrefabPrefab);
            };
            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(baseRadius) && NetworkServer.active) {
                    foreach(var w in TacticsWard.instances)
                        w.GetComponent<BuffWard>().Networkradius = (float)args.newValue;
                }
            };
        }

        protected override void LoadBehavior() {
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
            if(IsActiveAndEnabled()) {
                foreach(var cm in AliveList())
                    if(cm.hasBody) AddWard(cm.GetBody());
            }
            On.RoR2.CharacterMaster.OnBodyStart += On_CMOnBodyStart;
        }

        protected override void UnloadBehavior() {
            On.RoR2.CharacterMaster.OnBodyStart -= On_CMOnBodyStart;
            foreach(var w in TacticsWard.instances)
                UnityEngine.Object.Destroy(w.gameObject);
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }
        
        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
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

        private void AddWard(CharacterBody body) {
            var cpt = body.GetComponentInChildren<TacticsWard>()?.gameObject;
            if(!cpt) {
				cpt = UnityEngine.Object.Instantiate(tacticsWardPrefab);
				cpt.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
				cpt.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
            }
        }
    }

    internal class TacticsWard : MonoBehaviour {
        internal static List<TacticsWard> instances = new List<TacticsWard>();
        private void Awake() {
            instances.Add(this);
        }
        private void OnDestroy() {
            instances.Remove(this);
        }
    }
}