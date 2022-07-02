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
                localPos = new Vector3(0.23353F, -0.00868F, -0.08696F),
                localAngles = new Vector3(27.00084F, 326.5775F, 4.93487F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.6739F, -1.47899F, 1.63122F),
                localAngles = new Vector3(354.4511F, 7.12517F, 355.0916F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.24835F, 0.13692F, 0.12219F),
                localAngles = new Vector3(19.74273F, 338.7649F, 343.2596F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                childName = "Stomach",
                localPos = new Vector3(0.17437F, -0.01902F, 0.11239F),
                localAngles = new Vector3(14.62809F, 338.0782F, 18.2589F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F),
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.28481F, -0.22564F, -0.12889F),
                localAngles = new Vector3(0.98176F, 51.91312F, 23.00177F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.16876F, -0.10376F, 0.02998F),
                localAngles = new Vector3(357.5521F, 355.006F, 105.9485F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "ThighR",
                localPos = new Vector3(-0.08794F, 0.03176F, -0.06409F),
                localAngles = new Vector3(350.6662F, 317.2625F, 21.97947F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Chest",
                localPos = new Vector3(2.33895F, -0.34548F, 0.80107F),
                localAngles = new Vector3(311.4177F, 7.89006F, 354.1869F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "PlatformBase",
                localPos = new Vector3(0.75783F, -0.10773F, 0.00385F),
                localAngles = new Vector3(308.2326F, 10.8672F, 329.0782F),
                localScale = new Vector3(1F, 1F, 1F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.28636F, -0.3815F, -0.06912F),
                localAngles = new Vector3(352.4358F, 63.85439F, 6.83272F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.17554F, -0.13447F, -0.0436F),
                localAngles = new Vector3(15.08189F, 9.51543F, 15.89409F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
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