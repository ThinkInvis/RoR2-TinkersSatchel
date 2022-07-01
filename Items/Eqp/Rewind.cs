using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using System.Linq;
using System.Collections.Generic;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using EntityStates;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
    public class Rewind : Equipment<Rewind> {

        ////// Equipment Data //////

        public override string displayName => "Causal Camera";
        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered { get; protected set; } = true;
        public override float cooldown {get; protected set;} = 90f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Phase briefly and rewind yourself 10 seconds.";
        protected override string GetDescString(string langid = null) =>
            $"Phase out of existence for <style=cIsUtility>{phaseDuration:N1} seconds</style>. <style=cIsUtility>Rewind</style> your <style=cIsUtility>position</style>, <style=cIsHealth>health</style>, and <style=cIsUtility>skill cooldowns <style=cStack>(except equipment)</style></style> to their states from up to <style=cIsUtility>{rewindDuration:N1} seconds ago</style>. <style=cStack>Cannot use with less than {icd:N1} seconds saved.</style>";
        protected override string GetLoreString(string langid = null) => $"Order: TIL-1.8c Temporal Imaging Lens Prototype\r\nTracking Number: 88***********\r\nEstimated Delivery: {rewindDuration:N0} seconds from now\r\nShipping Method:  Closed Timelike Curve\r\nShipping Address: Petrichor V\r\nShipping Details: \r\n\r\nFixed the problem with the slow-moving anomalies (< 0.01c) coming up blurry in photos. Hopefully we'll get better results on those now.\r\n\r\nDO NOT SHAKE -- I had to sacrifice some stasis field integrity. Already called UES logistics twice to correct the manifest when their equipment auto-detected localized time travel.\r\n";



        ////// Config //////

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the phasing effect, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float phaseDuration { get; private set; } = 2f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum rewind time, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float rewindDuration { get; private set; } = 10f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Minimum rewind time, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float minDuration { get; private set; } = 2f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time after use before beginning to record again, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float icd { get; private set; } = 2f;

        [AutoConfigRoOSlider("{0:N2} s", 0.05f, 10f)]
        [AutoConfig("Time between saved player states, in seconds.", AutoConfigFlags.PreventNetMismatch, 0.05f, float.MaxValue)]
        public float frameInterval { get; private set; } = 0.5f;



        ////// Other Fields/Properties //////

        SerializableEntityStateType rewindStateType;
        internal RoR2.Skills.SkillDef[] blacklistedSkills;
        public BuffDef rewindBuff { get; private set; }
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public Rewind() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Rewind.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/rewindIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Rewind.prefab");
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
                localPos = new Vector3(0.22045F, -0.06626F, 0.11193F),
                localAngles = new Vector3(359.0299F, 357.3219F, 25.2928F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.38728F, 0.00965F, -0.06446F),
                localAngles = new Vector3(31.87035F, 332.9695F, 3.18838F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
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
            rewindStateType = ContentAddition.AddEntityState<RewindState>(out _);
            R2API.Networking.NetworkingAPI.RegisterMessageType<MsgRewind>();

            blacklistedSkills = new[] {
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/CallSupplyDropHealing"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/CallSupplyDropHacking"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/CallSupplyDropShocking"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/CallSupplyDropEquipmentRestock"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/CaptainSkillUsedUp"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/CaptainCancelDummy")
            };

            rewindBuff = ScriptableObject.CreateInstance<BuffDef>();
            rewindBuff.buffColor = Color.white;
            rewindBuff.canStack = true;
            rewindBuff.isDebuff = false;
            rewindBuff.isCooldown = true;
            rewindBuff.name = "TKSATRewind";
            rewindBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/rewindBuffIcon.png");
            ContentAddition.AddBuffDef(rewindBuff);

            if(Compat_ClassicItems.enabled) {
                LanguageAPI.Add("TKSAT_REWIND_CI_EMBRYO_APPEND", "\n<style=cStack>Beating Embryo: 50% chance to not consume stock.</style>");
                Compat_ClassicItems.RegisterEmbryoHook(equipmentDef, "TKSAT_REWIND_CI_EMBRYO_APPEND", () => "TKSAT.CausalCamera");
            }
        }

        public override void Install() {
            base.Install();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        public override void Uninstall() {
            base.Uninstall();
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
        }



        ////// Hooks //////


        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(!body || !body.inventory) return;
            if(EquipmentCatalog.GetEquipmentDef(body.inventory.currentEquipmentIndex) == this.equipmentDef && !body.gameObject.GetComponent<RewindComponent>()) {
                body.gameObject.AddComponent<RewindComponent>();
            }
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            if(!slot || !slot.characterBody)
                return false;
            var cpt = slot.characterBody.GetComponent<RewindComponent>();
            if(!cpt || cpt.frames.Count < Mathf.CeilToInt(minDuration / frameInterval))
                return false;
            var esm = EntityStateMachine.FindByCustomName(slot.characterBody.gameObject, "Body");
            if(esm == null || esm.state is RewindState) {
                return false;
            }
            new MsgRewind(slot.characterBody).Send(R2API.Networking.NetworkDestination.Clients);
            if(Compat_ClassicItems.enabled) {
                if(Util.CheckRoll(Mathf.Pow(0.5f, Compat_ClassicItems.CheckEmbryoProc(slot, equipmentDef)) * 100f))
                    return false;
            }
            return true;
        }

        public struct MsgRewind : INetMessage {
            CharacterBody _target;

            public MsgRewind(CharacterBody target) {
                _target = target;
            }

            public void Deserialize(NetworkReader reader) {
                var tgto = reader.ReadGameObject();
                if(tgto)
                    _target = tgto.GetComponent<CharacterBody>();
                else {
                    TinkersSatchelPlugin._logger.LogError("Received MsgRewind for nonexistent or non-networked GameObject");
                }
            }

            public void Serialize(NetworkWriter writer) {
                writer.Write(_target.gameObject);
            }

            public void OnReceived() {
                if(!_target) return;
                var cpt = _target.GetComponent<RewindComponent>();
                if(!cpt || cpt.frames.Count == 0)
                    return;
                var esm = EntityStateMachine.FindByCustomName(_target.gameObject, "Body");
                if(esm == null || esm.state is RewindState) return;
                esm.SetState(EntityStateCatalog.InstantiateState(Rewind.instance.rewindStateType));
            }
        }
    }

    public class RewindState : EntityStates.Huntress.BlinkState {
        RewindComponent cpt;
        int currFrame;
        public override void OnEnter() {
            speedCoefficient = 0f;
            duration = Rewind.instance.phaseDuration;
            this.beginSoundString = "Play_huntress_shift_start";
            this.endSoundString = "Play_huntress_shift_end";
            base.OnEnter();
            cpt = outer.commonComponents.characterBody.GetComponent<RewindComponent>();
            currFrame = cpt.frames.Count;
            if(characterModel) {
                characterModel.invisibilityCount--;
            }
            cpt.isRewinding = true;

            modelTransform = base.GetModelTransform();
            if(modelTransform) {
                var ovl = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                ovl.duration = duration;
                ovl.animateShaderAlpha = true;
                ovl.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                ovl.destroyComponentOnEnd = true;
                ovl.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded");
                ovl.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }
        }

        public override void FixedUpdate() {
            base.FixedUpdate();

            int interpolatedFrame = Mathf.Clamp(Mathf.FloorToInt(SteepSigmoid01(1f - stopwatch / duration, 1.5f) * cpt.frames.Count), 0, cpt.frames.Count - 1);
            if(interpolatedFrame != currFrame) {
                currFrame = interpolatedFrame;
                cpt.frames[interpolatedFrame].ApplyTo(outer.commonComponents.characterBody);
                outer.commonComponents.characterMotor.velocity = -outer.commonComponents.characterMotor.velocity;
                Util.PlaySound(this.beginSoundString, base.gameObject);
                outer.commonComponents.characterBody.SetBuffCount(Rewind.instance.rewindBuff.buffIndex, interpolatedFrame);
            }
        }
        public override void OnExit() {
            if(characterModel) {
                characterModel.invisibilityCount++;
            }
            base.OnExit();
            cpt.frames.Clear();
            cpt.isRewinding = false;
            cpt.icd = Rewind.instance.icd;
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class RewindComponent : MonoBehaviour {
        public class RewindFrame {
            public float health;
            public float shield;
            public float barrier;
            public Vector3 position;
            public Vector3 velocity;
            public Vector3 moveVec;
            public Vector3 targVec;
            public (int slot, float cd, int stock)[] skillStates;

            internal RewindFrame() { }

            public RewindFrame(CharacterBody body) {
                if(body.characterDirection) {
                    moveVec = body.characterDirection.moveVector;
                    targVec = body.characterDirection.targetVector;
                } else {
                    moveVec = Vector3.zero;
                    targVec = Vector3.zero;
                }
                if(body.characterMotor) {
                    position = body.characterMotor.previousPosition;
                    velocity = body.characterMotor.velocity;
                } else {
                    position = Vector3.zero;
                    velocity = Vector3.zero;
                }

                if(body.skillLocator)
                    skillStates = body.skillLocator.allSkills.Select(x => (body.skillLocator.GetSkillSlotIndex(x), x.rechargeStopwatch, x.stock)).ToArray();
                else
                    skillStates = new (int, float, int)[0];

                if(NetworkServer.active) {
                    if(body.healthComponent) {
                        health = body.healthComponent.health;
                        shield = body.healthComponent.shield;
                        barrier = body.healthComponent.barrier;
                    } else {
                        health = 0;
                        shield = 0;
                        barrier = 0;
                    }
                }
            }

            public void ApplyTo(CharacterBody body) {
                if(body.characterMotor) {
                    body.characterMotor.rootMotion = (position - body.characterMotor.previousPosition);
                    body.characterMotor.velocity = velocity;
                }
                if(body.characterDirection) {
                    body.characterDirection.moveVector = moveVec;
                    body.characterDirection.targetVector = targVec;
                }

                if(Util.HasEffectiveAuthority(body.networkIdentity)) {
                    foreach(var skill in body.skillLocator.allSkills) {
                        if(Rewind.instance.blacklistedSkills.Contains(skill.skillDef)) continue;
                        
                        var thisSlot = body.skillLocator.GetSkillSlotIndex(skill);
                        var stored = skillStates.Where(x => x.slot == thisSlot);
                        if(stored.Count() != 1) {
                            TinkersSatchelPlugin._logger.LogError($"RewindState.ApplyTo: skillslot {thisSlot} went missing or had duplicates!");
                            continue;
                        }
                        var (_, cd, stock) = stored.First();
                        skill.rechargeStopwatch = cd;
                        skill.stock = stock;
                    }
                }

                if(NetworkServer.active) {
                    if(body.healthComponent) {
                        body.healthComponent.Networkhealth = health;
                        body.healthComponent.Networkshield = shield;
                        body.healthComponent.Networkbarrier = barrier;
                    }
                }
            }
        }

        public List<RewindFrame> frames = new List<RewindFrame>();

        float stopwatch = 0f;
        public float icd = 0f;
        CharacterBody body;
        public bool isRewinding = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void OnDisable() {
            frames.Clear();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(!body || isRewinding) return;
            if(EquipmentCatalog.GetEquipmentDef(body.inventory.currentEquipmentIndex) != Rewind.instance.equipmentDef) {
                if(frames.Count > 0) {
                    frames.Clear();
                    body.SetBuffCount(Rewind.instance.rewindBuff.buffIndex, 0);
                }
                return;
            }
            if(icd > 0f) {
                icd -= Time.fixedDeltaTime;
                return;
            }
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                stopwatch = Rewind.instance.frameInterval;
                frames.Add(new RewindFrame(body));
                if(frames.Count > Rewind.instance.rewindDuration / Rewind.instance.frameInterval) {
                    frames.RemoveAt(0);
                }
                body.SetBuffCount(Rewind.instance.rewindBuff.buffIndex, frames.Count);
            }
        }
    }
}