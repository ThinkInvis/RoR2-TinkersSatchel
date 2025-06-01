﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class PixieTube : Item<PixieTube> {

        ////// Item Data //////

        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage, ItemTag.Utility });

        protected override string[] GetDescStringArgs(string langID = null) => new[] {
            perSkillCooldown.ToString("N0"), primaryCooldown.ToString("N0"), buffDuration.ToString("N0"), buffDamageAmt.ToString("0%"), buffMoveAmt.ToString("0%"), buffAttackAmt.ToString("0%"), buffArmorAmt.ToString("N0")
        };



        ////// Config ///////

        [AutoConfigRoOSlider("{0:N0} s", 0f, 60f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of all Pixie Tube buffs.", AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever, 0f, float.MaxValue)]
        public float buffDuration { get; private set; } = 10f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fractional move speed bonus from the Water buff.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffMoveAmt { get; private set; } = 0.05f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fractional attack speed bonus from the Air buff.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffAttackAmt { get; private set; } = 0.05f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Fractional damage bonus from the Fire buff.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffDamageAmt { get; private set; } = 0.03f;

        [AutoConfigRoOSlider("{0:N0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Flat armor bonus from the Earth buff.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float buffArmorAmt { get; private set; } = 10f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Internal cooldown on each non-primary skill, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float perSkillCooldown { get; private set; } = 3f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Internal cooldown on primary skill, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float primaryCooldown { get; private set; } = 6f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, stacking spawns stronger wisps instead of more.", AutoConfigFlags.PreventNetMismatch)]
        public bool performanceMerge { get; private set; } = true;



        ////// Other Fields/Properties //////

        GameObject[] prefabs;
        BuffDef moveBuff;
        BuffDef attackBuff;
        BuffDef damageBuff;
        BuffDef armorBuff;
        RoR2.Skills.SkillDef[] blacklistedSkills;
        const float PICKUP_ARMING_DELAY = 1.5f;
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public PixieTube() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/PixieTube.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/pixieTubeIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/PixieTube.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MainWeapon",
                localPos = new Vector3(-0.06131F, 0.23459F, -0.10051F),
                localAngles = new Vector3(64.21507F, 224.2983F, 93.09216F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MuzzleGun",
                localPos = new Vector3(0.00103F, -0.07124F, -0.31367F),
                localAngles = new Vector3(16.75661F, 12.69404F, 225.5789F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "GunL",
                localPos = new Vector3(0.18255F, 0.13195F, -0.09012F),
                localAngles = new Vector3(348.5039F, 239.2215F, 139.2646F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "LowerArmR",
                localPos = new Vector3(1.40126F, 4.0343F, -1.41909F),
                localAngles = new Vector3(293.1907F, 91.10483F, 276.6844F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "CannonHeadL",
                localPos = new Vector3(0.21842F, 0.23697F, 0.22945F),
                localAngles = new Vector3(298.1622F, 14.17452F, 257.4075F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "BowHinge1L",
                localPos = new Vector3(-0.1164F, 0.28435F, 0.00236F),
                localAngles = new Vector3(20.09913F, 328.6948F, 121.9962F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechUpperArmL",
                localPos = new Vector3(0.19321F, 0.07457F, 0.00612F),
                localAngles = new Vector3(24.69925F, 22.04997F, 326.4116F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(-0.17263F, -0.06813F, -0.06435F),
                localAngles = new Vector3(291.7245F, 249.2733F, 226.4265F),
                localScale = new Vector3(0.25F, 0.25F, 0.25F)
            });
            displayRules.Add("MercBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "UpperArmL",
                localPos = new Vector3(0.22202F, -0.0177F, 0.0294F),
                localAngles = new Vector3(359.0609F, 138.6153F, 156.2666F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("ToolbotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "UpperArmL",
                localPos = new Vector3(0.03897F, 1.91933F, -1.69502F),
                localAngles = new Vector3(300.506F, 179.1773F, 221.359F),
                localScale = new Vector3(3F, 3F, 3F)
            });
            displayRules.Add("TreebotBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "WeaponPlatform",
                localPos = new Vector3(0.09565F, -0.95396F, 0.22819F),
                localAngles = new Vector3(299.7137F, 50.52557F, 274.8567F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });
            displayRules.Add("RailgunnerBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Backpack",
                localPos = new Vector3(0.071F, 0.49013F, -0.01937F),
                localAngles = new Vector3(343.5707F, 7.39626F, 43.74519F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "CannonEnd",
                localPos = new Vector3(0.35103F, -0.3362F, 0.02411F),
                localAngles = new Vector3(32.0267F, 14.99465F, 307.135F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            #endregion
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            blacklistedSkills = new[] {
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiCancelTargetingDummy"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiConfirmTargetDummy"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceTurret"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceWalkerTurret"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiHarpoons"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/CaptainCancelDummy"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/PrepSupplyDrop"),
            };

            var colors = new[] { new Color(0.15f, 0.25f, 1f), new Color(1f, 1f, 0.4f), new Color(1f, 0.25f, 0.1f), new Color(0.5f, 0.35f, 0.2f) };
            var sharedBuffIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texBarrelIcon.png")
                .WaitForCompletion();
            var rampTex = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampDefault.png")
                .WaitForCompletion();

            moveBuff = ScriptableObject.CreateInstance<BuffDef>();
            moveBuff.buffColor = colors[0];
            moveBuff.canStack = true;
            moveBuff.isDebuff = false;
            moveBuff.name = modInfo.shortIdentifier + "PixieMoveSpeed";
            moveBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(moveBuff);

            attackBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackBuff.buffColor = colors[1];
            attackBuff.canStack = true;
            attackBuff.isDebuff = false;
            attackBuff.name = modInfo.shortIdentifier + "PixieAttackSpeed";
            attackBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(attackBuff);

            damageBuff = ScriptableObject.CreateInstance<BuffDef>();
            damageBuff.buffColor = colors[2];
            damageBuff.canStack = true;
            damageBuff.isDebuff = false;
            damageBuff.name = modInfo.shortIdentifier + "PixieDamage";
            damageBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(damageBuff);

            armorBuff = ScriptableObject.CreateInstance<BuffDef>();
            armorBuff.buffColor = colors[3];
            armorBuff.canStack = true;
            armorBuff.isDebuff = false;
            armorBuff.name = modInfo.shortIdentifier + "PixieArmor";
            armorBuff.iconSprite = sharedBuffIcon;
            ContentAddition.AddBuffDef(armorBuff);

            var origPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HealPack");
            prefabs = new[] {
                origPrefab.InstantiateClone("TkSatTempSetupPrefab1", false),
                origPrefab.InstantiateClone("TkSatTempSetupPrefab2", false),
                origPrefab.InstantiateClone("TkSatTempSetupPrefab3", false),
                origPrefab.InstantiateClone("TkSatTempSetupPrefab4", false) };
            var bufftypes = new[] { moveBuff, attackBuff, damageBuff, armorBuff };
            var finalNames = new[] { "TkSatPixieMovePack", "TkSatPixieAttackPack", "TkSatPixieDamagePack", "TkSatPixieArmorPack" };
            for(var i = 0; i < prefabs.Length; i++) {
                var prefab = prefabs[i];
                
                var vros = prefab.GetComponent<VelocityRandomOnStart>();
                vros.enabled = false;

                var dstroy = prefab.GetComponent<DestroyOnTimer>();
                dstroy.duration += PICKUP_ARMING_DELAY;

                var blinker = prefab.GetComponent<BeginRapidlyActivatingAndDeactivating>();
                blinker.delayBeforeBeginningBlinking = dstroy.duration - 1f;

                var trail = prefab.transform.Find("HealthOrbEffect/TrailParent/Trail").gameObject;
                var tren = trail.GetComponent<TrailRenderer>();
                tren.material.SetTexture("_RemapTex", rampTex);
                tren.material.SetColor("_TintColor", colors[i]);

                var core = prefab.transform.Find("HealthOrbEffect/VFX/Core").gameObject;
                var cren = core.GetComponent<ParticleSystem>();
                var ccol = cren.colorOverLifetime;
                ccol.color = new ParticleSystem.MinMaxGradient(colors[i], colors[i].AlphaMultiplied(0f));
                core.transform.localScale *= 0.5f;

                var pulse = prefab.transform.Find("HealthOrbEffect/VFX/PulseGlow").gameObject;
                var pren = pulse.GetComponent<ParticleSystem>();
                var pcol = pren.colorOverLifetime;
                pcol.color = new ParticleSystem.MinMaxGradient(colors[i], colors[i].AlphaMultiplied(0f));
                pulse.transform.localScale *= 0.5f;

                var pickup = prefab.transform.Find("PickupTrigger").gameObject;
                pickup.GetComponent<HealthPickup>().enabled = false;
                var bpkp = pickup.AddComponent<EffectlessBuffPickup>();
                bpkp.buffDef = bufftypes[i];
                bpkp.teamFilter = prefab.GetComponent<TeamFilter>();
                bpkp.buffDuration = buffDuration;
                bpkp.baseObject = prefabs[i];

                var grav = prefab.transform.Find("GravitationController").gameObject;
                var delay = prefab.AddComponent<ActivateAfterDelay>();
                pickup.SetActive(false);
                core.SetActive(false);
                delay.targets.Add(pickup);
                delay.targets.Add(core);
                delay.delay = PICKUP_ARMING_DELAY;

                grav.GetComponent<GravitatePickup>().enabled = false;

                var gravramp = grav.AddComponent<WispAnimAndGravitate>();
                gravramp.armingDelay = PICKUP_ARMING_DELAY;
                gravramp.duration = dstroy.duration - PICKUP_ARMING_DELAY;
                gravramp.parentRigidbody = prefab.GetComponent<Rigidbody>();
                gravramp.teamFilter = prefab.GetComponent<TeamFilter>();
                gravramp.playerOnlyDuration = gravramp.duration - 2f;

                prefabs[i] = prefabs[i].InstantiateClone(finalNames[i], true);
            }
        }

        public override void Install() {
            base.Install();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.EntityStates.Engi.EngiMissilePainter.Fire.FireMissile += Fire_FireMissile;
            On.EntityStates.Engi.EngiWeapon.PlaceTurret.FixedUpdate += PlaceTurret_FixedUpdate;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        public override void Uninstall() {
            base.Uninstall();
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
            On.EntityStates.Engi.EngiMissilePainter.Fire.FireMissile -= Fire_FireMissile;
            On.EntityStates.Engi.EngiWeapon.PlaceTurret.FixedUpdate -= PlaceTurret_FixedUpdate;
            On.RoR2.EquipmentSlot.PerformEquipmentAction -= EquipmentSlot_PerformEquipmentAction;
        }



        ////// Private Methods //////

        void SpawnWisp(Vector3 pos, TeamIndex team, int stacks = 1) {
            if(!NetworkServer.active) {
                TinkersSatchelPlugin._logger.LogWarning("Server-only function TinkersSatchel.PixieTube.SpawnWisp called on client");
                return;
            }
            var vvec = Quaternion.AngleAxis(UnityEngine.Random.value * 360f, Vector3.up) * (new Vector3(1f, 1f, 0f).normalized * 15f);
            var orb = Object.Instantiate(rng.NextElementUniform(prefabs), pos, UnityEngine.Random.rotation);
            orb.GetComponent<TeamFilter>().teamIndex = team;
            orb.GetComponent<Rigidbody>().velocity = vvec;
            orb.transform.Find("PickupTrigger").gameObject.GetComponent<EffectlessBuffPickup>().stacks = stacks;
            NetworkServer.Spawn(orb);
        }



        ////// Hooks //////

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill) {
            orig(self, skill);
            if(!NetworkServer.active) return;
            
            if(self && self.skillLocator
                && !blacklistedSkills.Contains(skill.skillDef)) {
                var count = GetCount(self);
                if(count <= 0) return;
                var pts = self.gameObject.GetComponent<PixieTubeStopwatch>();
                if(!pts)
                    pts = self.gameObject.AddComponent<PixieTubeStopwatch>();
                if(!pts.CheckProc(self.skillLocator.FindSkillSlot(skill))) return;

                if(performanceMerge) {
                    SpawnWisp(self.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None, count);
                } else {
                    for(var i = 0; i < count; i++) {
                        SpawnWisp(self.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None);
                    }
                }
            }
        }

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef) {
            var retv = orig(self, equipmentDef);
            if(NetworkServer.active && self && self.characterBody) {
                var count = GetCount(self.characterBody);
                if(count <= 0) return retv;
                var pts = self.characterBody.gameObject.GetComponent<PixieTubeStopwatch>();
                if(!pts)
                    pts = self.characterBody.gameObject.AddComponent<PixieTubeStopwatch>();
                if(pts.CheckProcEquipment()) {
                    for(var i = 0; i < count; i++) {
                        SpawnWisp(self.characterBody.corePosition, self.characterBody.teamComponent ? self.characterBody.teamComponent.teamIndex : TeamIndex.None);
                    }
                }
            }
            return retv;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            if(!sender) return;
            args.armorAdd += sender.GetBuffCount(armorBuff) * buffArmorAmt;
            args.attackSpeedMultAdd += sender.GetBuffCount(attackBuff) * buffAttackAmt;
            args.damageMultAdd += sender.GetBuffCount(damageBuff) * buffDamageAmt;
            args.moveSpeedMultAdd += sender.GetBuffCount(moveBuff) * buffMoveAmt;
        }

        private void Fire_FireMissile(On.EntityStates.Engi.EngiMissilePainter.Fire.orig_FireMissile orig, EntityStates.Engi.EngiMissilePainter.Fire self, HurtBox target, Vector3 position) {
            orig(self, target, position);
            var count = GetCount(self.characterBody);
            if(count <= 0) return;
            var pts = self.characterBody.gameObject.GetComponent<PixieTubeStopwatch>();
            if(!pts)
                pts = self.characterBody.gameObject.AddComponent<PixieTubeStopwatch>();
            if(!pts.CheckProc(SkillSlot.Utility)) return;
            for(var i = 0; i < count; i++) {
                SpawnWisp(self.characterBody.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None);
            }
        }

        private void PlaceTurret_FixedUpdate(On.EntityStates.Engi.EngiWeapon.PlaceTurret.orig_FixedUpdate orig, EntityStates.Engi.EngiWeapon.PlaceTurret self) {
            orig(self);
            if((self.inputBank.skill1.down || self.inputBank.skill4.justPressed) && self.currentPlacementInfo.ok && self.exitCountdown == 0.25f && self.exitPending) {
                var count = GetCount(self.characterBody);
                if(count <= 0) return;
                var pts = self.characterBody.gameObject.GetComponent<PixieTubeStopwatch>();
                if(!pts)
                    pts = self.characterBody.gameObject.AddComponent<PixieTubeStopwatch>();
                if(!pts.CheckProc(SkillSlot.Special)) return;
                for(var i = 0; i < count; i++) {
                    SpawnWisp(self.characterBody.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None);
                }
            }
        }
    }

    public class ActivateAfterDelay : MonoBehaviour {
        public float delay;
        public List<GameObject> targets = new();

        float stopwatch = 0f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(!isActiveAndEnabled) return;
            if(stopwatch < delay) {
                stopwatch += Time.fixedDeltaTime;
                if(stopwatch >= delay) {
                    foreach(var target in targets) {
                        target.SetActive(true);
                    }
                }
            }
        }

        public void Reset() {
            foreach(var target in targets) {
                target.SetActive(false);
            }
            stopwatch = 0f;
        }
    }

    public class PixieTubeStopwatch : MonoBehaviour {
        readonly float[] stopwatches = new[] { 0f, 0f, 0f, 0f, 0f };

        public bool CheckProc(SkillSlot slot) {
            if(slot == SkillSlot.None || slot > SkillSlot.Special) return false;
            if(stopwatches[(int)slot] <= 0f) {
                stopwatches[(int)slot] = (slot == SkillSlot.Primary) ? PixieTube.instance.primaryCooldown : PixieTube.instance.perSkillCooldown;
                return true;
            }
            return false;
        }

        public bool CheckProcEquipment() {
            if(stopwatches[4] <= 0f) {
                stopwatches[4] = PixieTube.instance.perSkillCooldown;
                return true;
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            for(int i = 0; i < stopwatches.Length; i++) {
                if(stopwatches[i] > 0f)
                    stopwatches[i] -= Time.fixedDeltaTime;
            }
        }
    }

    [RequireComponent(typeof(SphereCollider))]
    public class WispAnimAndGravitate : MonoBehaviour {
        public float rangeStart = 6f;
        public float rangeEnd = 36f;
        public float duration = 10f;
        public float playerOnlyDuration = 8f;
        public float dragDelay = 0.5f;
        public float dragStrength = 0.01f;
        public float maxSpeed = 60f;
        public float acceleration = 5f;
        public float armingDelay = 0f;

        public float zipDelayMin = 0.2f;
        public float zipDelayMax = 1f;
        public float zipStrengthMin = 5f;
        public float zipStrengthMax = 15f;

        public TeamFilter teamFilter;
        public Rigidbody parentRigidbody;

        float stopwatch = 0f;
        float zipStopwatch = 0f;

        Transform target;

        SphereCollider coll;

        bool GetCanPerformTargetingOps() { return NetworkServer.active && !target && stopwatch > armingDelay && teamFilter && teamFilter.teamIndex != TeamIndex.None; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            coll = GetComponent<SphereCollider>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(stopwatch < duration)
                stopwatch += Time.fixedDeltaTime;

            coll.enabled = stopwatch > armingDelay;
            coll.radius = Mathf.Lerp(rangeStart, rangeEnd, stopwatch / duration);

            if(target) {
                parentRigidbody.velocity = Vector3.MoveTowards(parentRigidbody.velocity, (target.position - transform.position).normalized * maxSpeed, this.acceleration);
            } else {
                parentRigidbody.velocity *= 1f - Mathf.Clamp01(stopwatch / dragDelay) * dragStrength;
                if(stopwatch > armingDelay) {
                    zipStopwatch -= Time.fixedDeltaTime;
                    if(zipStopwatch <= 0f) {
                        zipStopwatch = UnityEngine.Random.Range(zipDelayMin, zipDelayMax);
                        parentRigidbody.velocity += UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(zipStrengthMin, zipStrengthMax);
                    }
                }
            }

            parentRigidbody.velocity -= Physics.gravity * Time.fixedDeltaTime;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void OnTriggerStay(Collider other) {
            if(GetCanPerformTargetingOps()) {
                var tgtTeam = TeamComponent.GetObjectTeam(other.gameObject);
                if(tgtTeam == teamFilter.teamIndex) {
                    var tgtBody = other.gameObject.GetComponent<CharacterBody>();
                    if(stopwatch > playerOnlyDuration
                        || tgtTeam != TeamIndex.Player
                        || (tgtBody && tgtBody.isPlayerControlled)) {
                        target = other.gameObject.transform;
                    }
                }
            }
        }
    }

    public class PixieFuseFlicker : MonoBehaviour {
        [SerializeField]
        public GameObject[] targets;
        public float switchIntervalMin;
        public float switchIntervalMax;
        public float flickerInterval;
        public int flickersMin;
        public int flickersMax;

        private float stopwatch = 0f;
        private int flickerCount = 0;
        private int currIndex = 0;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Update() {
            stopwatch -= Time.deltaTime;
            if(stopwatch < 0f) {
                if(flickerCount <= 0) {
                    currIndex = Random.Range(0, targets.Length);
                    flickerCount = Random.Range(flickersMin, flickersMax + 1) * 2;
                    for(var i = 0; i < targets.Length; i++) {
                        targets[i].SetActive(currIndex == i);
                    }
                }

                stopwatch = (flickerCount > 1 ? Random.Range(0.02f, 0.1f) : Random.Range(switchIntervalMin, switchIntervalMax));
                flickerCount--;
                targets[currIndex].SetActive(!targets[currIndex].activeSelf);
            }
        }
    }

    public class EffectlessBuffPickup : MonoBehaviour {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        private void OnTriggerStay(Collider other) {
            if(!NetworkServer.active) return;
            if(TeamComponent.GetObjectTeam(other.gameObject) == teamFilter.teamIndex) {
                var tgtBody = other.GetComponent<CharacterBody>();
                if(!tgtBody) return;
                for(var i = 0; i < stacks; i++) {
                    tgtBody.AddTimedBuff(buffDef.buffIndex, buffDuration);
                }
                UnityEngine.Object.Destroy(baseObject);
            }
        }
        public GameObject baseObject;
        public TeamFilter teamFilter;
        public BuffDef buffDef;
        public float buffDuration;
        public int stacks = 1;
    }
}