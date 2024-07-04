using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using RoR2.Orbs;
using System.Linq;
using R2API;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class HurdyGurdy : Item<HurdyGurdy> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            projDamage.ToString("P0"), windupTime.ToString("N0"), baseCharges.ToString("N0"), stackCharges.ToString("N0")
        };



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Base damage of this item's projectiles.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float projDamage { get; private set; } = 2f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 180f)]
        [AutoConfig("Forward cone angle for acquiring projectile targets.", AutoConfigFlags.PreventNetMismatch, 0f, 180f)]
        public float projAngle { get; private set; } = 30f;

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

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfig("Proc coefficient of the item attack.", AutoConfigFlags.None, 0f, 1f)]
        public float procCoefficient { get; private set; } = 0.5f;

        [AutoConfigRoOString()]
        [AutoConfig("Skill names which will always count for consecutive Hurdy-Gurdy activations.", AutoConfigFlags.PreventNetMismatch)]
        public string skillOverridesConfig { get; private set; } = "RailgunnerBodyFireSnipeHeavy";



        ////// Other Fields/Properties //////

        public GameObject orbEffectPrefab { get; private set; }
        readonly HashSet<string> skillOverrides = new();



        ////// TILER2 Module Setup //////

        public HurdyGurdy() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/HurdyGurdy.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/hurdyGurdyIcon.png");

            var tempPfb = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ChainLightningVoid/VoidLightningOrbEffect.prefab").WaitForCompletion().InstantiateClone("temporary setup prefab", false);

            tempPfb.GetComponent<OrbEffect>().endEffect =
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MagmaWorm/MagmaWormImpactExplosion.prefab")
                .WaitForCompletion();

            var effc = tempPfb.GetComponent<EffectComponent>();
            effc.soundName = "Play_fireballsOnHit_impact";

            var lRen = tempPfb.transform.Find("Bezier").GetComponent<LineRenderer>();

            lRen.materials[0] = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/ChainLightningVoid/matLightingLongVoid.mat").WaitForCompletion());
            lRen.materials[0].SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWilloWispExplosion.png").WaitForCompletion());

            var pSysRen = tempPfb.transform.Find("Bezier").Find("HarshGlow, Billboard").GetComponent<ParticleSystemRenderer>();
            pSysRen.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matFirePillarParticle.mat").WaitForCompletion();

            orbEffectPrefab = tempPfb.InstantiateClone("HurdyGurdyOrbPrefab", false);
            UnityEngine.Object.Destroy(tempPfb);
            ContentAddition.AddEffect(orbEffectPrefab);
        }

        public override void SetupConfig() {
            base.SetupConfig();

            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(skillOverridesConfig))
                    UpdateSkillOverrides();
            };
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
        }



        ////// Private Methods //////

        void UpdateSkillOverrides() {
            skillOverrides.Clear();
            skillOverrides.UnionWith(skillOverridesConfig.Split(','));
        }



        ////// Hooks //////

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
            orig(self);
            var count = GetCount(self);
            if(count <= 0 || !self.skillLocator || !self.skillLocator.secondaryBonusStockSkill) return;
            self.skillLocator.secondaryBonusStockSkill.SetBonusStockFromBody(
                self.skillLocator.secondaryBonusStockSkill.bonusStockFromBody
                + baseCharges + (count - 1) * stackCharges);
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill) {
            orig(self, skill);
            if(!NetworkServer.active
                || !self || !self.skillLocator || !skill.skillDef)
                return;
            var count = GetCount(self);
            if(count == 0) return;

            if(!self.TryGetComponent<HurdyGurdyTracker>(out var hgt)) hgt = self.gameObject.AddComponent<HurdyGurdyTracker>();

            if(
                (self.skillLocator.FindSkillSlot(skill) != SkillSlot.Secondary
                    || skill.skillDef.baseRechargeInterval < 1f || skill.skillDef.stockToConsume <= 0
                ) && !skillOverrides.Contains(skill.name.Replace("(Clone)",""))) {
                hgt.consecutiveCasts = 0;
            } else {
                hgt.consecutiveCasts++;
            }

            if(hgt.consecutiveCasts >= windupTime) {
                var bs = new BullseyeSearch {
                    maxAngleFilter = projAngle,
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
                            origin = self.aimOriginTransform ? self.aimOriginTransform.position : self.corePosition,
                            damageValue = self.damage * projDamage,
                            damageType = DamageType.IgniteOnHit,
                            isCrit = self.RollCrit(),
                            bouncesRemaining = 0,
                            teamIndex = self.teamComponent.teamIndex,
                            attacker = self.gameObject,
                            target = target,
                            procCoefficient = procCoefficient,
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
            EffectManager.SpawnEffect(HurdyGurdy.instance.orbEffectPrefab, effectData, true);
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class HurdyGurdyTracker : MonoBehaviour {
        public int consecutiveCasts = 0;
    }
}