using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using RoR2.Orbs;
using RoR2.ExpansionManagement;

namespace ThinkInvisible.TinkersSatchel {
    public class VoidwispHive : Item<VoidwispHive> {

        ////// Item Data //////

        public override string displayName => "Voidwisp Hive";
        public override ItemTier itemTier => ItemTier.VoidTier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Damage });

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Drop damaging wisp allies on using skills. <style=cIsVoid>Corrupts all Pixie Tubes</style>.";
        protected override string GetDescString(string langid = null) => $"You drop a <style=cIsDamage>voidwisp</style> when you <style=cIsUtility>use any skill</style> <style=cStack>({perSkillCooldown:N0} s individual cooldown on each skill, {primaryCooldown:N0} s on primary skill)</style>. <style=cIsDamage>Voidwisps</style> live for {wispDuration:N0} seconds, tracking nearby enemies and zapping them for <style=cIsDamage>{1f/damageRate:N1}x{Pct(damageAmt)} damage per second <style=cStack>(damage increases linearly per stack)</style></style>. <style=cIsVoid>Corrupts all Pixie Tubes</style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config ///////

        [AutoConfigRoOSlider("{0:N1} s", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Lifetime of Voidwisp Hive wisps.", AutoConfigFlags.PreventNetMismatch | AutoConfigFlags.DeferForever, 0f, float.MaxValue)]
        public float wispDuration { get; private set; } = 10f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Damage fraction dealt by Voidwisp attacks per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageAmt { get; private set; } = 0.08f;

        [AutoConfigRoOSlider("{0:N2} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Time, in seconds, between attacks.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageRate { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Internal cooldown on each non-primary skill, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float perSkillCooldown { get; private set; } = 3f;

        [AutoConfigRoOSlider("{0:N0} s", 0f, 30f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Internal cooldown on primary skill, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float primaryCooldown { get; private set; } = 6f;



        ////// Other Fields/Properties //////

        GameObject wispPrefab;
        RoR2.Skills.SkillDef[] blacklistedSkills;
        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public VoidwispHive() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/VoidwispHive.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/voidwispHiveIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/VoidwispHive.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.06965F, -0.00208F, -0.17908F),
                localAngles = new Vector3(11.42411F, 356.2912F, 14.84471F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.36759F, -0.0275F, -0.1511F),
                localAngles = new Vector3(31.87035F, 332.9695F, 3.18838F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CommandoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(0.08343F, 0.03755F, -0.13804F),
                localAngles = new Vector3(27.00084F, 326.5775F, 4.93487F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("CrocoBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(1.06242F, -2.34785F, 2.19338F),
                localAngles = new Vector3(34.82076F, 356.4073F, 8.73201F),
                localScale = new Vector3(2F, 2F, 2F)
            });
            displayRules.Add("EngiBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(-0.18945F, 0.06303F, 0.14275F),
                localAngles = new Vector3(19.74273F, 338.7649F, 343.2596F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
            });
            displayRules.Add("HuntressBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Stomach",
                localPos = new Vector3(-0.02239F, 0.15988F, -0.08744F),
                localAngles = new Vector3(14.62809F, 338.0782F, 18.2589F),
                localScale = new Vector3(0.15F, 0.15F, 0.15F)
            });
            displayRules.Add("LoaderBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "MechBase",
                localPos = new Vector3(0.2564F, -0.07702F, -0.11248F),
                localAngles = new Vector3(42.98969F, 110.4701F, 5.24458F),
                localScale = new Vector3(0.15F, 0.15F, 0.15F)
            });
            displayRules.Add("MageBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "Pelvis",
                localPos = new Vector3(-0.1685F, -0.14564F, 0.12029F),
                localAngles = new Vector3(357.5521F, 355.006F, 105.9485F),
                localScale = new Vector3(0.2F, 0.2F, 0.2F)
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

            blacklistedSkills = new[] {
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiCancelTargetingDummy"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiConfirmTargetDummy"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceTurret"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceWalkerTurret"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/EngiBody/EngiHarpoons"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/CaptainCancelDummy"),
                LegacyResourcesAPI.Load<RoR2.Skills.SkillDef>("SkillDefs/CaptainBody/PrepSupplyDrop"),
            };

            var rampTex = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampDefault.png")
                .WaitForCompletion();

            var tmpPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HealPack").InstantiateClone("TkSatTempSetupPrefab", false);

            var wispColor = new Color(0.3f, 0.05f, 0.4f);

            var vros = tmpPrefab.GetComponent<VelocityRandomOnStart>();
            vros.enabled = false;

            var dstroy = tmpPrefab.GetComponent<DestroyOnTimer>();
            dstroy.duration = wispDuration;

            var blinker = tmpPrefab.GetComponent<BeginRapidlyActivatingAndDeactivating>();
            blinker.delayBeforeBeginningBlinking = dstroy.duration - 1f;

            var trail = tmpPrefab.transform.Find("HealthOrbEffect/TrailParent/Trail").gameObject;
            var tren = trail.GetComponent<TrailRenderer>();
            tren.material.SetTexture("_RemapTex", rampTex);
            tren.material.SetColor("_TintColor", wispColor);

            var core = tmpPrefab.transform.Find("HealthOrbEffect/VFX/Core").gameObject;
            var cren = core.GetComponent<ParticleSystem>();
            var ccol = cren.colorOverLifetime;
            ccol.color = new ParticleSystem.MinMaxGradient(wispColor, wispColor.AlphaMultiplied(0f));
            core.transform.localScale *= 0.5f;

            var pulse = tmpPrefab.transform.Find("HealthOrbEffect/VFX/PulseGlow").gameObject;
            var pren = pulse.GetComponent<ParticleSystem>();
            var pcol = pren.colorOverLifetime;
            pcol.color = new ParticleSystem.MinMaxGradient(wispColor, wispColor.AlphaMultiplied(0f));
            pulse.transform.localScale *= 0.5f;

            var pickup = tmpPrefab.transform.Find("PickupTrigger").gameObject;
            pickup.transform.parent = null;
            GameObject.DestroyImmediate(pickup);

            var grav = tmpPrefab.transform.Find("GravitationController").gameObject;
            MonoBehaviour.Destroy(grav.GetComponent<GravitatePickup>());

            var gravramp = grav.AddComponent<VoidwispController>();
            gravramp.duration = dstroy.duration;
            gravramp.parentRigidbody = tmpPrefab.GetComponent<Rigidbody>();
            gravramp.teamFilter = tmpPrefab.GetComponent<TeamFilter>();

            wispPrefab = tmpPrefab.InstantiateClone("TkSatVoidWisp", true);
            GameObject.Destroy(tmpPrefab);

            itemDef.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset")
                .WaitForCompletion();

            On.RoR2.ItemCatalog.SetItemRelationships += (orig, providers) => {
                var isp = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                isp.relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                isp.relationships = new[] {new ItemDef.Pair {
                    itemDef1 = PixieTube.instance.itemDef,
                    itemDef2 = itemDef
                }};
                orig(providers.Concat(new[] { isp }).ToArray());
            };
        }

        public override void Install() {
            base.Install();
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.EntityStates.Engi.EngiMissilePainter.Fire.FireMissile += Fire_FireMissile;
            On.EntityStates.Engi.EngiWeapon.PlaceTurret.FixedUpdate += PlaceTurret_FixedUpdate;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
            On.EntityStates.Engi.EngiMissilePainter.Fire.FireMissile -= Fire_FireMissile;
            On.EntityStates.Engi.EngiWeapon.PlaceTurret.FixedUpdate -= PlaceTurret_FixedUpdate;
            On.RoR2.EquipmentSlot.PerformEquipmentAction -= EquipmentSlot_PerformEquipmentAction;
        }



        ////// Private Methods //////

        void SpawnWisp(Vector3 pos, TeamIndex team, CharacterBody ownerBody) {
            var vvec = Quaternion.AngleAxis(UnityEngine.Random.value * 360f, Vector3.up) * (new Vector3(1f, 1f, 0f).normalized * 15f);
            var orb = Object.Instantiate(wispPrefab, pos, UnityEngine.Random.rotation);
            orb.GetComponent<TeamFilter>().teamIndex = team;
            orb.GetComponent<Rigidbody>().velocity = vvec;
            orb.transform.Find("GravitationController").GetComponent<VoidwispController>().owner = ownerBody;
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
                var pts = self.gameObject.GetComponent<VoidwispHiveStopwatch>();
                if(!pts)
                    pts = self.gameObject.AddComponent<VoidwispHiveStopwatch>();
                if(!pts.CheckProc(self.skillLocator.FindSkillSlot(skill))) return;
                SpawnWisp(self.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None, self);
            }
        }

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef) {
            var retv = orig(self, equipmentDef);
            if(self && self.characterBody) {
                var count = GetCount(self.characterBody);
                if(count <= 0) return retv;
                var pts = self.characterBody.gameObject.GetComponent<VoidwispHiveStopwatch>();
                if(!pts)
                    pts = self.characterBody.gameObject.AddComponent<VoidwispHiveStopwatch>();
                if(pts.CheckProcEquipment()) {
                     SpawnWisp(self.characterBody.corePosition, self.characterBody.teamComponent ? self.characterBody.teamComponent.teamIndex : TeamIndex.None, self.characterBody);
                }
            }
            return retv;
        }

        private void Fire_FireMissile(On.EntityStates.Engi.EngiMissilePainter.Fire.orig_FireMissile orig, EntityStates.Engi.EngiMissilePainter.Fire self, HurtBox target, Vector3 position) {
            orig(self, target, position);
            var count = GetCount(self.characterBody);
            if(count <= 0) return;
            var pts = self.characterBody.gameObject.GetComponent<PixieTubeStopwatch>();
            if(!pts)
                pts = self.characterBody.gameObject.AddComponent<PixieTubeStopwatch>();
            if(!pts.CheckProc(SkillSlot.Utility)) return;
             SpawnWisp(self.characterBody.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None, self.characterBody);
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
                 SpawnWisp(self.characterBody.corePosition, self.teamComponent ? self.teamComponent.teamIndex : TeamIndex.None, self.characterBody);
            }
        }
    }

    public class VoidwispHiveStopwatch : MonoBehaviour {
        readonly float[] stopwatches = new[] { 0f, 0f, 0f, 0f, 0f };

        public bool CheckProc(SkillSlot slot) {
            if(slot == SkillSlot.None || slot > SkillSlot.Special) return false;
            if(stopwatches[(int)slot] <= 0f) {
                stopwatches[(int)slot] = (slot == SkillSlot.Primary) ? VoidwispHive.instance.primaryCooldown : VoidwispHive.instance.perSkillCooldown;
                return true;
            }
            return false;
        }

        public bool CheckProcEquipment() {
            if(stopwatches[4] <= 0f) {
                stopwatches[4] = VoidwispHive.instance.perSkillCooldown;
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
    public class VoidwispController : MonoBehaviour {
        public float attackRange {
            get { return _attackRange; }
            set { _attackRange = value; _attackRangeSq = value * value; }
        }
        float _attackRange = 15f;
        float _attackRangeSq = 225f;
        public float seekRange {
            get { return _seekRange; }
            set { _seekRange = value; _seekRangeSq = value * value; }
        }
        float _seekRange = 50f;
        float _seekRangeSq = 2500f;
        public float idealOrbitRange = 8f;
        public float duration = 10f;
        public float dragDelay = 0.5f;
        public float dragStrength = 0.01f;

        public float zipDelayMin = 0.2f;
        public float zipDelayMax = 1f;
        public float zipStrengthMin = 5f;
        public float zipStrengthMax = 15f;
        public float zipDelayMultWithTarget = 0.5f;

        public TeamFilter teamFilter;
        public Rigidbody parentRigidbody;
        public CharacterBody owner;

        float lifeStopwatch = 0f;
        float zipStopwatch = 0.5f;
        float attackStopwatch = 0f;

        Transform target;
        CharacterBody tgtBody;

        SphereCollider coll;

        bool GetCanPerformTargetingOps() { return NetworkServer.active && !target && teamFilter && teamFilter.teamIndex != TeamIndex.None; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void Awake() {
            coll = GetComponent<SphereCollider>();
            coll.radius = seekRange;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void FixedUpdate() {
            if(lifeStopwatch < duration)
                lifeStopwatch += Time.fixedDeltaTime;

            bool hasTarget = target && tgtBody && tgtBody.healthComponent && tgtBody.healthComponent.alive;
            if(!hasTarget) target = null; //allow retargeting immediately if current target dies
            bool targetIsInSeekRange = false;
            bool targetIsInAttackRange = false;
            var targetVec = Vector3.zero;
            if(hasTarget) {
                targetVec = target.position - transform.position;
                var mag = targetVec.sqrMagnitude;
                targetIsInSeekRange = mag < _seekRangeSq;
                targetIsInAttackRange = mag < _attackRangeSq;
            }

            parentRigidbody.velocity *= 1f - Mathf.Clamp01(lifeStopwatch / dragDelay) * dragStrength;
            zipStopwatch -= Time.fixedDeltaTime;
            if(zipStopwatch <= 0f) {
                zipStopwatch = UnityEngine.Random.Range(zipDelayMin, zipDelayMax);
                if(targetIsInSeekRange) {
                    var targetPos = target.position - targetVec.normalized * idealOrbitRange;
                    zipStopwatch *= zipDelayMultWithTarget;
                    parentRigidbody.velocity += ((targetPos - transform.position).normalized * 0.7f + UnityEngine.Random.onUnitSphere * 0.3f) * UnityEngine.Random.Range(zipStrengthMin, zipStrengthMax);
                } else {
                    target = null;
                    parentRigidbody.velocity += UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(zipStrengthMin, zipStrengthMax);
                }
            }

            attackStopwatch -= Time.fixedDeltaTime;
            if(attackStopwatch < 0f) attackStopwatch = 0f;
            if(targetIsInAttackRange && attackStopwatch <= 0f) {
                attackStopwatch = VoidwispHive.instance.damageRate;
                var baseDamage = 10f;
                var count = VoidwispHive.instance.GetCount(owner);
                if(count > 0)
                    baseDamage = owner.damage * count;

                OrbManager.instance.AddOrb(new VoidwispLightningOrb {
                    origin = transform.position,
                    damageValue = baseDamage * VoidwispHive.instance.damageAmt,
                    isCrit = owner ? owner.RollCrit() : false,
                    bouncesRemaining = 0,
                    teamIndex = teamFilter.teamIndex,
                    attacker = owner ? owner.gameObject : gameObject,
                    target = Util.FindBodyMainHurtBox(tgtBody),
                    procCoefficient = 0.1f,
                    procChainMask = default,
                    range = _attackRange + 5f,
                    damageColorIndex = DamageColorIndex.Item
                });
            }

            parentRigidbody.velocity -= Physics.gravity * Time.fixedDeltaTime;
        }

        public class VoidwispLightningOrb : LightningOrb {
            public override void Begin() {
                lightningType = LightningType.Count; //invalid type
                duration = 0.06f;
                var effectData = new EffectData {
                    origin = origin,
                    genericFloat = duration
                };
                effectData.SetHurtBoxReference(target);
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/VoidLightningOrbEffect"), effectData, true);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
        void OnTriggerStay(Collider other) {
            if(GetCanPerformTargetingOps()) {
                var tgtb = other.GetComponentInParent<CharacterBody>();
                if(tgtb && tgtb.healthComponent && tgtb.healthComponent.alive && tgtb.teamComponent && tgtb.teamComponent.teamIndex != teamFilter.teamIndex && tgtb.teamComponent.teamIndex != TeamIndex.Neutral) {
                    target = other.gameObject.transform;
                    tgtBody = tgtb;
                }
            }
        }
    }
}