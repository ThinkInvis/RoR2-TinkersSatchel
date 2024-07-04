using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using TILER2;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public class CursesKeepOSP : T2Module<CursesKeepOSP> {

        public CursesKeepOSP() {
            enabled = false; //default value only, config may override
        }



        ////// TILER2 Module Setup //////
        
        public override bool managedEnable => true;

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
                x => x.MatchCallOrCallvirt<Mathf>("Max"),
                x => x.MatchCallOrCallvirt<CharacterBody>("set_oneShotProtectionFraction"));
            if(ILFound) {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, CharacterBody, float>>((origFrac, body) => body.oneShotProtectionFraction);
            } else {
                TinkersSatchelPlugin._logger.LogError("failed to apply IL patch (CurseKeepOSP)! CurseKeepOSP module will not work.");
            }
        }
    }
}