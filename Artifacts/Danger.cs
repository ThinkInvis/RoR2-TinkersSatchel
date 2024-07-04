using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using TILER2;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public class Danger : Artifact<Danger> {

        ////// TILER2 Module Setup //////
        
        public Danger() {
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/danger_on.png");
            iconResourceDisabled = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/danger_off.png");
        }

        public override void Install() {
            base.Install();
            IL.RoR2.CharacterBody.RecalculateStats += IL_CBRecalcStats;
        }

        public override void Uninstall() {
            base.Uninstall();
            IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
        }

        

        ////// Hooks //////

        private void IL_CBRecalcStats(ILContext il) {
            ILCursor c = new(il);
            bool ILFound = c.TryGotoNext(
                x=>x.MatchCallOrCallvirt<CharacterBody>("set_hasOneShotProtection"));
            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterBody, bool>>((origSet, body)=>{
                    return body.isPlayerControlled && !IsActiveAndEnabled();
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("failed to apply IL patch (Artifact of Danger, set OHP flag)! Artifact will not prevent OHP while enabled.");
            }
        }
    }
}