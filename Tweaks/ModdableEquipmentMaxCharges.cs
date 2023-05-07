using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using TILER2;

namespace ThinkInvisible.TinkersSatchel {
	public class ModdableEquipmentMaxCharges : T2Module<ModdableEquipmentMaxCharges> {
        public override bool managedEnable => true;

        public override void Install() {
            base.Install();

            IL.RoR2.Inventory.UpdateEquipment += Inventory_UpdateEquipment;
        }

        public override void Uninstall() {
            base.Uninstall();

            IL.RoR2.Inventory.UpdateEquipment -= Inventory_UpdateEquipment;
        }


        private void Inventory_UpdateEquipment(ILContext il) {
            ILCursor c = new(il);

            c.GotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                i => i.MatchAdd());

            int locMaxSlotsIndex = -1;
            c.GotoNext(i => i.MatchStloc(out locMaxSlotsIndex));

            int locSlotIndex = -1;
            c.GotoNext(MoveType.Before,
                i => i.MatchLdloc(out locSlotIndex),
                i => i.MatchLdelemAny<RoR2.EquipmentState>());

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, locSlotIndex);

            c.EmitDelegate<Func<Inventory, uint, byte>>((inv, slot) => (byte)inv.GetEquipmentSlotMaxCharges((byte)slot));

            c.Emit(OpCodes.Stloc, locMaxSlotsIndex);
        }
    }
}