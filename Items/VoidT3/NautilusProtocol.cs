using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using System.Linq;
using static R2API.RecalculateStatsAPI;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
	public class NautilusProtocol : Item<NautilusProtocol> {

		////// Item Data //////

		public override ItemTier itemTier => ItemTier.VoidTier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage, ItemTag.Utility });
        public override bool itemIsAIBlacklisted { get; protected set; } = true;

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            armorBuff.ToString("N0"), regenBuff.ToString("N0"), damageBuff.ToString("P0"), detRange.ToString("N0"), detDamage.ToString("P0"), detIcd.ToString("N0")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:N0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Armor applied per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float armorBuff { get; private set; } = 25f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Regen applied per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float regenBuff { get; private set; } = 2f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Damage bonus multiplier applied per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageBuff { get; private set; } = 0.2f;

        [AutoConfigRoOSlider("{0:N0} m", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Range (m) of the ping explosion.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float detRange { get; private set; } = 80f;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 300f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Relative damage of the ping explosion.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float detDamage { get; private set; } = 1f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 60f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Minimum time between detonations on a single drone.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float detIcd { get; private set; } = 5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Proc coefficient of the void detonation attack.", AutoConfigFlags.None, 0f, 1f)]
        public float procCoefficient { get; private set; } = 1f;



        ////// Other Fields/Properties //////

        private readonly string[] validBodyNames = new[] {
            "Drone1Body(Clone)",
            "BackupDroneBody(Clone)",
            "FlameDroneBody(Clone)",
            "MegaDroneBody(Clone)",
            "MissileDroneBody(Clone)",
            "Turret1Body(Clone)",
            "EngiTurretBody(Clone)",
            "SquidTurretBody(Clone)",
            "RoboBallGreenBuddyBody(Clone)",
            "RoboBallRedBuddyBody(Clone)"
        };

        public GameObject hitEffectPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public NautilusProtocol() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Nautilus.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/nautilusIcon.png");
            hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ElementalRingVoid/ElementalRingVoidImplodeEffect.prefab").WaitForCompletion();
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            R2API.Networking.NetworkingAPI.RegisterMessageType<MsgDetonateNautilus>();

            On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
                var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                isp.relationships = new[] {new ItemDef.Pair {
                    itemDef1 = Wrangler.instance.itemDef,
                    itemDef2 = itemDef
                }};
                orig(providers.Concat(new[] { isp }).ToArray());
            };
        }

        public override void Install() {
            base.Install();

            On.RoR2.PingerController.SetCurrentPing += PingerController_SetCurrentPing;
            On.RoR2.CharacterAI.BaseAI.EvaluateSkillDrivers += BaseAI_EvaluateSkillDrivers;

            GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        public override void Uninstall() {
            base.Uninstall();

            On.RoR2.PingerController.SetCurrentPing -= PingerController_SetCurrentPing;
            On.RoR2.CharacterAI.BaseAI.EvaluateSkillDrivers -= BaseAI_EvaluateSkillDrivers;

            GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }



        ////// Hooks //////
        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            if(!sender || !validBodyNames.Contains(sender.name)) return;
            var cpt = sender.GetComponent<NautilusTrackerComponent>();
            if(cpt && cpt.cachedWranglerCount > 0) {
                args.damageMultAdd += damageBuff * cpt.cachedWranglerCount;
                args.armorAdd += armorBuff * cpt.cachedWranglerCount;
                args.baseRegenAdd += regenBuff * cpt.cachedWranglerCount;
            }
        }

        private void PingerController_SetCurrentPing(On.RoR2.PingerController.orig_SetCurrentPing orig, PingerController self, PingerController.PingInfo newPingInfo) {
            orig(self, newPingInfo);
            if(
                self.TryGetComponent<PlayerCharacterMasterController>(out var pcmc) && pcmc.body && GetCount(pcmc.body) > 0
                && newPingInfo.targetGameObject && newPingInfo.targetGameObject.TryGetComponent<CharacterBody>(out var cb)) {
                new MsgDetonateNautilus(cb).Send(R2API.Networking.NetworkDestination.Server);
            }
        }

        private RoR2.CharacterAI.BaseAI.SkillDriverEvaluation BaseAI_EvaluateSkillDrivers(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSkillDrivers orig, RoR2.CharacterAI.BaseAI self) {
            var retv = orig(self);

            if(!self.body) return retv;

            var cpt = self.body.GetComponent<NautilusTrackerComponent>();
            if(!cpt) {
                if(validBodyNames.Contains(self.body.name))
                    cpt = self.body.gameObject.AddComponent<NautilusTrackerComponent>();
            }
            if(!cpt) return retv;

            if(self.leader == null || !self.leader.characterBody)
                cpt.SetWranglerCount(0);
            else
                cpt.SetWranglerCount(GetCount(self.leader.characterBody));

            return retv;
        }



        ////// Networking //////

        public struct MsgDetonateNautilus : INetMessage {
            CharacterBody _target;

            public MsgDetonateNautilus(CharacterBody target) {
                _target = target;
            }

            public void Deserialize(NetworkReader reader) {
                var tgto = reader.ReadGameObject();
                if(tgto)
                    _target = tgto.GetComponent<CharacterBody>();
                else {
                    TinkersSatchelPlugin._logger.LogError("Received MsgDetonateNautilus for nonexistent or non-networked GameObject");
                }
            }

            public void Serialize(NetworkWriter writer) {
                writer.Write(_target.gameObject);
            }

            public void OnReceived() {
                if(!_target || !_target.TryGetComponent<NautilusTrackerComponent>(out var ntc)) return;
                ntc.Detonate();
            }
        }
    }


    [RequireComponent(typeof(CharacterBody))]
    public class NautilusTrackerComponent : MonoBehaviour {
        CharacterBody body;

        public int cachedWranglerCount { get; private set; } = 0;
        float detCooldown = 0f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            body = GetComponent<CharacterBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(detCooldown > 0f) detCooldown -= Time.fixedDeltaTime;
        }

        public void SetWranglerCount(int count) {
            if(cachedWranglerCount != count)
                body.MarkAllStatsDirty();
            cachedWranglerCount = count;
        }

        public void Detonate() {
            if(detCooldown > 0f) return;
            detCooldown = NautilusProtocol.instance.detIcd;
            var detHealth = body.healthComponent.fullCombinedHealth / 2f;
            body.healthComponent.TakeDamage(new DamageInfo {
                damage = detHealth * 2f, //TODO: find out why this is necessary
                attacker = body.gameObject,
                position = body.corePosition,
                damageType = DamageType.BypassArmor | DamageType.BypassBlock
            });
            new BlastAttack {
                attacker = body.gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseDamage = detHealth * NautilusProtocol.instance.detDamage,
                baseForce = 0f,
                crit = false,
                damageColorIndex = DamageColorIndex.Void,
                damageType = DamageType.Generic,
                falloffModel = BlastAttack.FalloffModel.None,
                inflictor = body.gameObject,
                position = body.corePosition,
                procChainMask = default,
                procCoefficient = NautilusProtocol.instance.procCoefficient,
                radius = NautilusProtocol.instance.detRange,
                teamIndex = body.teamComponent.teamIndex
            }.Fire();
            EffectManager.SpawnEffect(NautilusProtocol.instance.hitEffectPrefab, new EffectData {
                scale = NautilusProtocol.instance.detRange,
                origin = body.corePosition,
                start = body.corePosition
            }, true);
        }
    }
}