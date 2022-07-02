using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ThinkInvisible.TinkersSatchel {
    public class ExtendoArms : Item<ExtendoArms> {

        ////// Item Data //////
        
        public override string displayName => "Extendo-Arms";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Damage});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Close-range attacks reach farther and deal slightly more damage.";
        protected override string GetDescString(string langid = null) => $"All your melee attacks, as well as AoEs centered within {pbaoeRange:N1} meters of you, gain <style=cIsUtility>{Pct(resizeAmount)} range <style=cStack>(+{Pct(resizeAmount)} per stack)</style></style> and <style=cIsDamage>{Pct(damageAmount)} damage <style=cStack>(+{Pct(damageAmount)} per stack)</style></style>.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////
        
        [AutoConfigRoOSlider("{0:P0}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Hitbox scale increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float resizeAmount { get; private set; } = 0.125f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 3f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Melee attack damage increase per stack, linear.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageAmount { get; private set; } = 0.0625f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum distance between character and center of an AoE to count as a PBAoE.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float pbaoeRange { get; private set; } = 5f;



        ////// Other Fields/Properties //////

        public GameObject idrPrefab { get; private set; }



        ////// TILER2 Module Setup //////

        public ExtendoArms() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/ExtendoArms.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/extendoArmsIcon.png");
            idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/ExtendoArms.prefab");
        }

        public override void SetupModifyItemDef() {
            base.SetupModifyItemDef();

            CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

            #region ItemDisplayRule Definitions

            /// Survivors ///
            displayRules.Add("Bandit2Body", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "SideWeapon",
                localPos = new Vector3(0.04871F, -0.0517F, -0.05809F),
                localAngles = new Vector3(339.1259F, 332.7074F, 338.0398F),
                localScale = new Vector3(0.3F, 0.3F, 0.3F)
            });
            displayRules.Add("CaptainBody", new ItemDisplayRule {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = idrPrefab,
                childName = "HandL",
                localPos = new Vector3(0.1149F, -0.0598F, -0.09023F),
                localAngles = new Vector3(359.4513F, 311.727F, 0.74839F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
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
        }

        public override void Install() {
            base.Install();
            IL.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
            On.RoR2.BlastAttack.Fire += BlastAttack_Fire;
        }

        public override void Uninstall() {
            base.Uninstall();
            IL.RoR2.OverlapAttack.Fire -= OverlapAttack_Fire;
            On.RoR2.BlastAttack.Fire -= BlastAttack_Fire;
        }



        ////// Public API //////

        public static float GetRangeMultiplier(CharacterBody body) {
            if(!ExtendoArms.instance.enabled || !body) return 1f;
            var count = ExtendoArms.instance.GetCount(body.inventory);
            return 1f + count * ExtendoArms.instance.resizeAmount;
        }

        public static float GetDamageMultiplier(CharacterBody body) {
            if(!ExtendoArms.instance.enabled || !body) return 1f;
            var count = ExtendoArms.instance.GetCount(body.inventory);
            return 1f + count * ExtendoArms.instance.damageAmount;
        }



        ////// Hooks //////

        private BlastAttack.Result BlastAttack_Fire(On.RoR2.BlastAttack.orig_Fire orig, BlastAttack self) {
            var origDamage = self.baseDamage;
            var origRadius = self.radius;
            if(self.attacker && self.attacker.TryGetComponent<CharacterBody>(out var attackerBody)) {
                if(Vector3.Distance(attackerBody.corePosition, self.position) < pbaoeRange) {
                    var count = GetCount(attackerBody);
                    self.baseDamage *= 1f + count * damageAmount;
                    self.radius *= 1f + count * resizeAmount;
                }
            }
            var retv = orig(self);
            self.baseDamage = origDamage;
            self.radius = origRadius;
            return retv;
        }

        private void OverlapAttack_Fire(ILContext il) {
            var c = new ILCursor(il);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<OverlapAttack>>((self) => {
                if(self.attacker) {
                    var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                    self.damage *= 1f + count * damageAmount;
                }
            });
            if(c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Transform>("get_lossyScale"))) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Vector3, OverlapAttack, Vector3>>((origFullExtents, self) => {
                    if(!self.attacker) return origFullExtents;
                    var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                    return origFullExtents * (1f + resizeAmount * count);
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("ExtendoArms: failed to apply IL hook (OverlapAttack_Fire), target instructions not found. Item will not apply a hitbox scale bonus.");
            }
            c.Index = 0;
            while(c.TryGotoNext(MoveType.Before, x => x.MatchRet())) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<OverlapAttack>>((self) => {
                    if(self.attacker) {
                        var count = GetCount(self.attacker.GetComponent<CharacterBody>());
                        self.damage /= 1f + count * damageAmount;
                    }
                });
                c.Index++;
            }
        }
    }
}