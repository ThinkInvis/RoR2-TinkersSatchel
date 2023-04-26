using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using RoR2.Orbs;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class HurdyGurdy : Item<HurdyGurdy> {

        ////// Item Data //////
        
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Damage});

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            projDamage.ToString("P0"), windupTime.ToString("N0"), baseCharges.ToString("N0"), stackCharges.ToString("N0")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Base damage of this item's projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float projDamage { get; private set; } = 3f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 180f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Forward cone angle for acquiring projectile targets.", AutoConfigFlags.PreventNetMismatch, 0f, 180f)]
        public float projAngle { get; private set; } = 15f;

        [AutoConfigRoOIntSlider("{0:N0}", 1, 100)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Uninterrupted secondary skill activations required to start firing.", AutoConfigFlags.PreventNetMismatch, 1, int.MaxValue)]
        public int windupTime { get; private set; } = 3;

        [AutoConfigRoOIntSlider("{0:N0}", 0, 10)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Secondary skill charges provided by the first stack of this item.", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int baseCharges { get; private set; } = 2;

        [AutoConfigRoOIntSlider("{0:N0}", 0, 10)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Secondary skill charges provided per additional first stack of this item.", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int stackCharges { get; private set; } = 1;



        ////// Other Fields/Properties //////




        ////// TILER2 Module Setup //////

        public HurdyGurdy() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/HurdyGurdy.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/hurdyGurdyIcon.png");
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
            if(!self || !self.skillLocator) return;
            var count = GetCount(self);
            if(count == 0) return;

            if(!self.TryGetComponent<HurdyGurdyTracker>(out var hgt)) hgt = self.gameObject.AddComponent<HurdyGurdyTracker>();

            if(self.skillLocator.FindSkillSlot(skill) != SkillSlot.Secondary) {
                hgt.consecutiveCasts = 0;
            } else {
                hgt.consecutiveCasts++;
            }

            if(hgt.consecutiveCasts >= windupTime) {
                var bs = new BullseyeSearch {
                    maxAngleFilter = 15f,
                    maxDistanceFilter = 1000f,
                    teamMaskFilter = TeamMask.allButNeutral,
                    filterByLoS = true,
                    searchOrigin = self.corePosition,
                    searchDirection = self.characterDirection.forward,
                    sortMode = BullseyeSearch.SortMode.None
                };
                bs.teamMaskFilter.RemoveTeam(self.teamComponent.teamIndex);
                bs.RefreshCandidates();
                var res = bs.GetResults().ToArray();
                if(res.Any()) {
                    for(var i = 0; i < count; i++) {
                        var target = rng.NextElementUniform(res);

                        OrbManager.instance.AddOrb(new HurdyGurdyOrb {
                            origin = self.corePosition,
                            damageValue = self.damage * projDamage,
                            damageType = DamageType.IgniteOnHit,
                            isCrit = self.RollCrit(),
                            bouncesRemaining = 0,
                            teamIndex = self.teamComponent.teamIndex,
                            attacker = self.gameObject,
                            target = target,
                            procCoefficient = 0.5f,
                            procChainMask = default,
                            range = 1000f,
                            damageColorIndex = DamageColorIndex.Item
                        });
                    }
                }
            }
        }
    }

    public class HurdyGurdyOrb : LightningOrb {
        public override void Begin() {
            lightningType = LightningType.Count; //invalid type
            duration = 0.5f;
            var effectData = new EffectData {
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/VoidLightningOrbEffect"), effectData, true); //TODO replace with custom effect
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class HurdyGurdyTracker : MonoBehaviour {
        public int consecutiveCasts = 0;
    }
}