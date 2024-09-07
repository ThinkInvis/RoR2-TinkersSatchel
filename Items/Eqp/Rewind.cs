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

        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered { get; protected set; } = true;
        public override float cooldown {get; protected set;} = 90f;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            phaseDuration.ToString("N1"), rewindDuration.ToString("N1"), minDuration.ToString("N0")
        };
        protected override string[] GetLoreStringArgs(string langID = null) => new[] {
            rewindDuration.ToString("N0")
        };



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



        ////// TILER2 Module Setup //////

        public Rewind() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Rewind.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/rewindIcon.png");
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
                esm.SetState(EntityStateCatalog.InstantiateState(ref Rewind.instance.rewindStateType));
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
                var toi = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                toi.duration = duration;
                toi.animateShaderAlpha = true;
                toi.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                toi.destroyComponentOnEnd = true;
                toi.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded");
                toi.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
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

        public List<RewindFrame> frames = new();

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