using RoR2;
using UnityEngine;
using TILER2;
using R2API.Utils;
using UnityEngine.Networking;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    public class Compass : Equipment<Compass> {
        public override string displayName => "Silver Compass";
        public override bool isLunar => true;
        public override float cooldown {get; protected set;} = 180f;

        [AutoConfig("0: Allows unlimited uses per stage. 1: Only once per character per stage. 2: Only once per stage.", AutoConfigFlags.None, 0, 2)]
        public int useLimitPerStage {get; private set;} = 1;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Shows you a path... <style=cDeath>but it will be fraught with danger.</style>";
        protected override string GetDescString(string langid = null) => 
            "<style=cIsUtility>Immediately reveals the teleporter</style>. Also adds two stacks of <style=cShrine>Challenge of the Mountain</style> to the current stage, <style=cDeath>one of which will not provide extra item drops</style>." +
            (useLimitPerStage == 2 ? " Works only once per stage." :
            (useLimitPerStage == 1 ? " Works only once per player per stage." :
            ""));
        protected override string GetLoreString(string langid = null) => null;

        public Compass() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/SilverCompass.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/compassIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            LanguageAPI.Add("TINKSATCH_COMPASS_USE_MESSAGE", "<style=cDeath>{0} seeks a path...</style>");
            LanguageAPI.Add("TINKSATCH_COMPASS_USE_MESSAGE_2P", "<style=cDeath>You seek a path...</style>");
        }
        
        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
			if (TeleporterInteraction.instance
                && slot.characterBody?.master?.playerCharacterMasterController
                && !slot.GetComponent<SilverCompassFlag>()
                && !TeleporterInteraction.instance.GetComponent<SilverCompassFlag>()) {
				TeleporterInteraction.instance.AddShrineStack();
                if(useLimitPerStage == 2) TeleporterInteraction.instance.gameObject.AddComponent<SilverCompassFlag>();
                else if(useLimitPerStage == 1) slot.gameObject.AddComponent<SilverCompassFlag>();
            } else return false;
            TeleporterInteraction.instance.shrineBonusStacks++;
			Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage {
				subjectAsCharacterBody = slot.characterBody,
				baseToken = "TINKSATCH_COMPASS_USE_MESSAGE"
			});

            var pctrl = slot.characterBody.master.playerCharacterMasterController.GetFieldValue<PingerController>("pingerController");
            typeof(PingerController).GetMethodCached("SetCurrentPing").Invoke(pctrl, new object[] {
			    new PingerController.PingInfo{
				    active = true,
				    origin = slot.characterBody.corePosition,
				    normal = Vector3.zero,
				    targetNetworkIdentity = TeleporterInteraction.instance.GetComponent<NetworkIdentity>()
			    }
            });

            return true;
        }
	}

    public class SilverCompassFlag : MonoBehaviour {}
}