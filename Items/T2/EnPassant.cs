using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

namespace ThinkInvisible.TinkersSatchel {
    public class EnPassant : Item<EnPassant> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            attackDamage.ToString("P0"), attackTime.ToString("N0"), cdrPerHit.ToString("N3")
        };



        ////// Config ///////

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Base damage of the attack, per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float attackDamage { get; private set; } = 0.7f;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the attack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float attackTime { get; private set; } = 1.5f;

        [AutoConfigRoOSlider("{0:N2} s", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fixed cooldown reduction per attack hit, per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float cdrPerHit { get; private set; } = 0.025f;



        ////// Other Fields/Properties //////

        public GameObject attackPrefab { get; private set; }
        public GameObject hitEffectPrefab { get; private set; }
        public GameObject swingEffectPrefab { get; private set; }



        ////// TILER2 Module Setup //////
        public EnPassant() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/EnPassant.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/enPassantIcon.png");
            attackPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Misc/EnPassantAttack.prefab");

            hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXSlash.prefab").WaitForCompletion();

            var origSwingEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("TkSatTempSetupPrefab", false);
            var ch1 = origSwingEffectPrefab.transform.Find("SwingTrail");
            var ch2 = origSwingEffectPrefab.transform.Find("Distortion");
            ch1.rotation = Quaternion.Euler(90f, 0, 0);
            ch2.rotation = Quaternion.Euler(90f, 0, 0);
            swingEffectPrefab = origSwingEffectPrefab.InstantiateClone("TkSatEnPassantSwingEffect", false);

            ContentAddition.AddEffect(swingEffectPrefab);
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
        }



        ////// Hooks //////
        
        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill) {
            orig(self, skill);
            if(!NetworkServer.active) return;
            if(self && self.skillLocator
                && self.skillLocator.FindSkillSlot(skill) == SkillSlot.Utility) {
                var count = GetCount(self);
                if(count > 0) {
                    var hitbox = Object.Instantiate(attackPrefab);
                    hitbox.GetComponent<DestroyOnTimer>().duration = attackTime;
                    NetworkServer.Spawn(hitbox);
                    hitbox.GetComponent<EnPassantAttack>().Begin(self);
                }
            }
        }
    }

    [RequireComponent(typeof(HitBoxGroup))]
    public class EnPassantAttack : MonoBehaviour {
        OverlapAttack attack = null;
        CharacterBody attackerBody = null;
        float stopwatch = 0f;
        float visualStopwatch = 0f;
        const float TICK_RATE = 0.1f;
        const float VISUAL_TICK_RATE = 0.2f;
        List<HurtBox> results;

        public void Begin(CharacterBody attackerBody) {
            this.attackerBody = attackerBody;
            attack = new OverlapAttack {
                attacker = attackerBody.gameObject,
                inflictor = attackerBody.gameObject,
                teamIndex = TeamComponent.GetObjectTeam(attackerBody.gameObject),
                damage = attackerBody.damage * EnPassant.instance.attackDamage * EnPassant.instance.GetCount(attackerBody),
                hitEffectPrefab = EnPassant.instance.hitEffectPrefab,
                isCrit = attackerBody.RollCrit(),
                hitBoxGroup = GetComponent<HitBoxGroup>()
            };
        }

        void Awake() {
            results = new();
        }

        void FixedUpdate() {
            if(attack == null || !NetworkServer.active) return;
            if(!attackerBody || !attackerBody.healthComponent.alive) { //f
                Destroy(this);
                return;
            }
            transform.position = attackerBody.corePosition;
            stopwatch += Time.fixedDeltaTime;
            visualStopwatch += Time.fixedDeltaTime;
            if(stopwatch > TICK_RATE) {
                stopwatch %= TICK_RATE;
                results.Clear();
                attack.Fire(results);
                var utilSlot = attackerBody.skillLocator.GetSkill(SkillSlot.Utility);
                utilSlot.RunRecharge(EnPassant.instance.cdrPerHit * results.Count * EnPassant.instance.GetCount(attackerBody));
            }
            if(visualStopwatch >= VISUAL_TICK_RATE) {
                visualStopwatch %= VISUAL_TICK_RATE;
                EffectManager.SpawnEffect(EnPassant.instance.swingEffectPrefab, new EffectData {
                    rootObject = gameObject
                }, true);
            }
        }
    }
}