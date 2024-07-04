using RoR2;
using UnityEngine;
using TILER2;
using R2API.Utils;
using UnityEngine.Networking;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
    public class Compass : Equipment<Compass> {

        ////// Equipment Data //////

        public override bool isLunar => true;
        public override bool canBeRandomlyTriggered { get; protected set; } = false;
        public override bool isEnigmaCompatible { get; protected set; } = false;
        public override float cooldown {get; protected set;} = 180f;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            applyPunishStack ? GetBestLanguage(langID).GetLocalizedStringByToken("TINKERSSATCHEL_COMPASS_DESC_PUNISH") : "",
            useLimitType == UseLimitType.NTimesPerStage
                ? GetBestLanguage(langID).GetLocalizedFormattedStringByToken("TINKERSSATCHEL_COMPASS_DESC_PERSTAGE", useLimitCount.ToString("N0"))
                : (useLimitType == UseLimitType.NTimesPerCharacter
                    ? GetBestLanguage(langID).GetLocalizedFormattedStringByToken("TINKERSSATCHEL_COMPASS_DESC_PERPLAYER", useLimitCount.ToString("N0"))
                    : "")
        };



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
        public bool applyPunishStack { get; private set; } = true;



        ////// Other Fields/Properties //////

        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public Compass() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/SilverCompass.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/compassIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/SilverCompass.prefab");
        }

        public override void SetupModifyEquipmentDef() {
            base.SetupModifyEquipmentDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.15335F, -0.05014F, 0.17195F),
                localAngles = new Vector3(346.3313F, 36.07016F, 351.1957F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.38892F, 0.01025F, -0.06457F),
                localAngles = new Vector3(339.381F, 93.94829F, 25.04659F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.20335F, 0.01981F, -0.07599F),
                localAngles = new Vector3(327.5557F, 138.6586F, 0.88774F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "SpineStomach2",
                localPos = new Vector3(-0.92336F, 0.70959F, 0.72718F),
                localAngles = new Vector3(302.9044F, 267.5414F, 83.33694F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.27383F, 0.13691F, 0.12219F),
                localAngles = new Vector3(21.35251F, 37.17607F, 173.8083F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.2156F, -0.0444F, -0.01822F),
                localAngles = new Vector3(39.06491F, 76.18316F, 175.7196F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.22401F, -0.27785F, -0.1303F),
                localAngles = new Vector3(0.07325F, 101.4427F, 23.21065F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.18725F, -0.07315F, 0.00197F),
                localAngles = new Vector3(14.68723F, 92.61344F, 204.8852F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.23272F, 0.09982F, 0.00448F),
                localAngles = new Vector3(28.25812F, 66.48277F, 174.458F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(2.03917F, -0.68604F, -0.45417F),
                localAngles = new Vector3(356.4003F, 85.95611F, 10.24933F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(0.92678F, 0.14718F, -0.33047F),
                localAngles = new Vector3(353.9659F, 91.51791F, 330.8354F),
                localScale = new Vector3(0.7F, 0.7F, 0.7F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.24315F, -0.56634F, 0.06337F),
                localAngles = new Vector3(17.53913F, 95.07683F, 3.09531F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Center",
                localPos = new Vector3(0.17438F, -0.04065F, 0.00101F),
                localAngles = new Vector3(349.123F, 100.1741F, 339.1085F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            #endregion
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