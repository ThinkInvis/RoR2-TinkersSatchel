using RoR2;
using UnityEngine;
using TILER2;
using R2API.Utils;
using UnityEngine.Networking;
using R2API;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
    public class Compass : Equipment<Compass> {

        ////// Equipment Data //////

        public override string displayName => "Silver Compass";
        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 180f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Shows you a path... <style=cDeath>BUT it will be fraught with danger.</style>";
        protected override string GetDescString(string langid = null) =>
            $"<style=cIsUtility>Immediately reveals the teleporter</style>. Also adds two stacks of <style=cShrine>Challenge of the Mountain</style> to the current stage{(applyPunishStack ? ", <style=cDeath>one of which will not provide extra item drops</style>" : "")}.{(useLimitType == UseLimitType.NTimesPerStage ? $" Works only {useLimitCount} time{NPlur(useLimitCount)} per stage." : (useLimitType == UseLimitType.NTimesPerCharacter ? $" Works only {useLimitCount} time{NPlur(useLimitCount)} per player per stage." : ""))}";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        public enum UseLimitType {
            Unlimited, NTimesPerCharacter, NTimesPerStage
        }

        [AutoConfigRoOChoice()]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("How to limit uses of Silver Compass.", AutoConfigFlags.None)]
        public UseLimitType useLimitType { get; private set; } = UseLimitType.NTimesPerCharacter;

        [AutoConfigRoOIntSlider("{0:N0}", 0, 10)]
        [AutoConfig("Number of limited uses if UseLimitType is not Unlimited.", AutoConfigFlags.None, 0, int.MaxValue)]
        public int useLimitCount { get; private set; } = 1;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, an extra stack of Shrine of the Mountain which has no reward will be applied.", AutoConfigFlags.None)]
        public bool applyPunishStack { get; private set; } = false;



        ////// TILER2 Module Setup //////

        public Compass() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/SilverCompass.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/compassIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            LanguageAPI.Add("TKSAT_COMPASS_USE_MESSAGE", "<style=cDeath>{0} seeks a path...</style>");
            LanguageAPI.Add("TKSAT_COMPASS_USE_MESSAGE_2P", "<style=cDeath>You seek a path...</style>");
        }
        


        ////// Hooks //////

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
			if (TeleporterInteraction.instance
                && slot.characterBody && slot.characterBody.master
                && slot.characterBody.master.playerCharacterMasterController) {
                if(useLimitType == UseLimitType.NTimesPerStage) {
                    if(!TeleporterInteraction.instance.gameObject.TryGetComponent<SilverCompassFlag>(out var teleFlag))
                        teleFlag = TeleporterInteraction.instance.gameObject.AddComponent<SilverCompassFlag>();

                    if(teleFlag.timesUsed >= useLimitCount) return false;

                    teleFlag.timesUsed++;

                } else if(useLimitType == UseLimitType.NTimesPerCharacter) {
                    if(!slot.gameObject.TryGetComponent<SilverCompassFlag>(out var slotFlag))
                        slotFlag = slot.gameObject.AddComponent<SilverCompassFlag>();

                    if(slotFlag.timesUsed >= useLimitCount) return false;

                    slotFlag.timesUsed++;
                }
            } else return false;

            TeleporterInteraction.instance.AddShrineStack();
            if(applyPunishStack)
                TeleporterInteraction.instance.shrineBonusStacks++;

			Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage {
				subjectAsCharacterBody = slot.characterBody,
				baseToken = "TKSAT_COMPASS_USE_MESSAGE"
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

    public class SilverCompassFlag : MonoBehaviour {
        public int timesUsed = 0;
    }

    public class TargetSpinnerAnim : MonoBehaviour {
        public float rotateTime = 0.5f;
        public float delayTime = 1f;
        public Vector3 rotateAxis;

        private float targPos = -1f;
        private float currVel;
        private float currPos;
        private float stopwatch;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Update() {
            stopwatch -= Time.deltaTime;
            if(stopwatch < 0f) {
                targPos = Random.value * Mathf.PI * 2;
                stopwatch = rotateTime + delayTime;
            }
            currPos = Mathf.SmoothDampAngle(currPos, targPos, ref currVel, rotateTime);
            this.gameObject.transform.Rotate(rotateAxis, currVel);
        }
    }
}