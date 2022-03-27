using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using System.Linq;
using System.Collections.Generic;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using EntityStates;

namespace ThinkInvisible.TinkersSatchel {
    public class Rewind : Equipment<Rewind> {

        ////// Equipment Data //////

        public override string displayName => "Causal Camera";
        public override bool isLunar => false;
        public override bool canBeRandomlyTriggered => false;
        public override float cooldown {get; protected set;} = 90f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Phase briefly and rewind yourself 10 seconds.";
        protected override string GetDescString(string langid = null) =>
            $"Phase out of existence for <style=cIsUtility>{phaseDuration:N1} seconds</style>. <style=cIsUtility>Rewind</style> your <style=cIsUtility>position</style>, <style=cIsHealth>health</style>, <style=cIsDamage>buffs/debuffs</style>, and <style=cIsUtility>skill cooldowns <style=cStack>(except equipment)</style></style> to their states from up to <style=cIsUtility>{rewindDuration:N1} seconds ago</style>.";
        protected override string GetLoreString(string langid = null) => $"Order: TIL-1.8c Temporal Imaging Lens Prototype\r\nTracking Number: 88***********\r\nEstimated Delivery: {rewindDuration:N0} seconds from now\r\nShipping Method:  Closed Timelike Curve\r\nShipping Address: Petrichor V\r\nShipping Details: \r\n\r\nFixed the problem with the slow-moving anomalies (< 0.01c) coming up blurry in photos. Hopefully we'll get better results on those now.\r\n\r\nDO NOT SHAKE -- I had to sacrifice some stasis field integrity. Already called UES logistics twice to correct the manifest when their equipment auto-detected localized time travel.\r\n";



        ////// Config //////

        [AutoConfig("Duration of the phasing effect, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float phaseDuration { get; private set; } = 2f;

        [AutoConfig("Maximum rewind time, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float rewindDuration { get; private set; } = 10f;

        [AutoConfig("Time between saved player states, in seconds.", AutoConfigFlags.PreventNetMismatch, 0.05f, float.MaxValue)]
        public float frameInterval { get; private set; } = 0.5f;



        ////// Other Fields/Properties //////

        SerializableEntityStateType rewindStateType;



        ////// TILER2 Module Setup //////

        public Rewind() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Rewind.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/rewindIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
            rewindStateType = ContentAddition.AddEntityState<RewindState>(out _);
            R2API.Networking.NetworkingAPI.RegisterMessageType<MsgRewind>();
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
            if(!cpt || cpt.frames.Count == 0)
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
                _target = reader.ReadGameObject()?.GetComponent<CharacterBody>();
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
            Debug.Log($"Entering RewindState with {cpt.frames.Count} frames");
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

        //Sigmoid-like curve as a function of x with fixed points at (0, 0), (0.5, 0.5), and (1, 1). Has flatter ends and steeper midpoint as b increases.
        float SteepSigmoid01(float x, float b) {
            return 0.5f - (float)System.Math.Tanh(2*b*(x-0.5f))/(2f*(float)System.Math.Tanh(-b));
        }

        public override void FixedUpdate() {
            base.FixedUpdate();

            int interpolatedFrame = Mathf.Clamp(Mathf.FloorToInt(SteepSigmoid01(1f - stopwatch / duration, 1.5f) * cpt.frames.Count), 0, cpt.frames.Count - 1);
            if(interpolatedFrame != currFrame) {
                currFrame = interpolatedFrame;
                cpt.frames[interpolatedFrame].ApplyTo(outer.commonComponents.characterBody);
                outer.commonComponents.characterMotor.velocity = -outer.commonComponents.characterMotor.velocity;
                Util.PlaySound(this.beginSoundString, base.gameObject);
            }
        }
        public override void OnExit() {
            if(characterModel) {
                characterModel.invisibilityCount++;
            }
            base.OnExit();
            cpt.frames.Clear();
            cpt.isRewinding = false;
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
            public int[] buffs;
            public List<CharacterBody.TimedBuff> timedBuffs;
            public List<DotController.DotStack> dotStacks;

            internal RewindFrame() { }

            public RewindFrame(CharacterBody body) {
                position = body.characterMotor.previousPosition;
                moveVec = body.characterDirection.moveVector;
                targVec = body.characterDirection.targetVector;
                velocity = body.characterMotor.velocity;

                skillStates = body.skillLocator.allSkills.Select(x => (body.skillLocator.GetSkillSlotIndex(x), x.rechargeStopwatch, x.stock)).ToArray();

                if(NetworkServer.active) {
                    health = body.healthComponent.health;
                    shield = body.healthComponent.shield;
                    barrier = body.healthComponent.barrier;
                    buffs = (int[])body.buffs.Clone();
                    timedBuffs = new List<CharacterBody.TimedBuff>();
                    foreach(var tb in body.timedBuffs) {
                        timedBuffs.Add(new CharacterBody.TimedBuff {
                            buffIndex = tb.buffIndex,
                            timer = tb.timer
                        });
                    }
                    if(DotController.dotControllerLocator.TryGetValue(body.gameObject.GetInstanceID(), out var dotController)) {
                        foreach(var dot in dotController.dotStackList) {
                            dotStacks.Add(new DotController.DotStack {
                                attackerObject = dot.attackerObject,
                                attackerTeam = dot.attackerTeam,
                                damage = dot.damage,
                                damageType = dot.damageType,
                                dotDef = dot.dotDef,
                                dotIndex = dot.dotIndex,
                                timer = dot.timer
                            });
                        }
                    }
                }
            }

            public void ApplyTo(CharacterBody body) {
                body.characterMotor.rootMotion = (position - body.characterMotor.previousPosition);
                body.characterDirection.moveVector = moveVec;
                body.characterDirection.targetVector = targVec;
                body.characterMotor.velocity = velocity;

                if(Util.HasEffectiveAuthority(body.networkIdentity)) {
                    foreach(var skill in body.skillLocator.allSkills) {
                        var thisSlot = body.skillLocator.GetSkillSlotIndex(skill);
                        var stored = skillStates.Where(x => x.slot == thisSlot);
                        if(stored.Count() != 1) {
                            TinkersSatchelPlugin._logger.LogError($"RewindState.ApplyTo: skillslot {thisSlot} went missing or had duplicates!");
                            continue;
                        }
                        var ovr = stored.First();
                        skill.rechargeStopwatch = ovr.cd;
                        skill.stock = ovr.stock;
                    }
                }

                if(NetworkServer.active) {
                    body.healthComponent.Networkhealth = health;
                    body.healthComponent.Networkshield = shield;
                    body.healthComponent.Networkbarrier = barrier;

                    for(var i = 0; i < buffs.Length; i++) {
                        body.ClearTimedBuffs((BuffIndex)i);
                    }
                    foreach(var tb in timedBuffs) {
                        body.AddTimedBuff(tb.buffIndex, tb.timer);
                    }
                    for(var i = 0; i < buffs.Length; i++) { //undoes erroneous buff index changes from AddTimedBuff
                        body.SetBuffCount((BuffIndex)i, buffs[i]);
                    }

                    if(DotController.dotControllerLocator.TryGetValue(body.gameObject.GetInstanceID(), out var dotController)) {
                        DotController.RemoveAllDots(body.gameObject);
                        foreach(var dot in dotStacks) {
                            DotController.InflictDot(body.gameObject, dot.attackerObject, dot.dotIndex, dot.timer);
                        }
                    }
                }
            }
        }

        public List<RewindFrame> frames = new List<RewindFrame>();

        float stopwatch = 0f;
        CharacterBody body;
        public bool isRewinding = false;

        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        void OnDisable() {
            frames.Clear();
        }

        void FixedUpdate() {
            if(isRewinding) return;
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                stopwatch = Rewind.instance.frameInterval;
                frames.Add(new RewindFrame(body));
                if(frames.Count > Rewind.instance.rewindDuration / Rewind.instance.frameInterval) {
                    frames.RemoveAt(0);
                }
            }
        }
    }
}