using RoR2;
using TILER2;

namespace ThinkInvisible.TinkersSatchel {
	public class EquipmentDroneLabels : T2Module<EquipmentDroneLabels> {

        ////// TILER2 Module Setup //////
        
        public override bool managedEnable => true;

        public override void Install() {
            base.Install();

            On.RoR2.CharacterBody.GetDisplayName += CharacterBody_GetDisplayName;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.CharacterBody.GetDisplayName -= CharacterBody_GetDisplayName;
        }



        ////// Hooks //////
        
        private string CharacterBody_GetDisplayName(On.RoR2.CharacterBody.orig_GetDisplayName orig, CharacterBody self) {
            var retv = orig(self);
            if(self.name != "EquipmentDroneBody(Clone)") return retv;

            var eqp = EquipmentCatalog.GetEquipmentDef(self.inventory.currentEquipmentIndex);
            if(eqp) {
                return $"{retv} (<color=#{ColorCatalog.GetColorHexString(eqp.colorIndex)}>{Language.GetString(eqp.nameToken)}</color>)";
            } else {
                return $"{retv} (<color=#AAAAAA>???</color>)";
            }
        }
    }
}