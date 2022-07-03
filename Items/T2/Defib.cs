using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using System.Collections.Generic;
using System;
using RoR2.Orbs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine.Networking;
using R2API;

namespace ThinkInvisible.TinkersSatchel {
    public class Defib : Item<Defib> {

        ////// Item Data //////
        
        public override string displayName => "Defibrillator";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new(new[] {ItemTag.Healing});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Your heals can crit.";
        protected override string GetDescString(string langid = null) => $"Your <style=cIsDamage>crit chance</style> applies to all <style=cIsHealing>healing</style> that you cause, for <style=cIsHealth>+{Pct(critFracBase)} <style=cStack>(+{Pct(critFracStack)} per stack)</style> HP healed</style> when a crit occurs.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplier for extra healing at first stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float critFracBase { get; private set; } = 0.5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplier for extra healing per additional stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float critFracStack { get; private set; } = 0.25f;



        ////// Other Fields/Properties //////

        public Stack<CharacterBody> healingSourceStack = new();
        Color origColorValue;
        internal static UnlockableDef unlockable;



        ////// TILER2 Module Setup //////
        
        public Defib() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Defib.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/defibIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
            var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
            unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockable.cachedName = $"TkSat_{name}Unlockable";
            unlockable.sortScore = 200;
            unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/defibIcon.png");
            ContentAddition.AddUnlockableDef(unlockable);
            LanguageAPI.Add(achiNameToken, "Medic!");
            LanguageAPI.Add(achiDescToken, "Item Set: Shareable healing. Have 4 or more (of 7) at once.");
            itemDef.unlockableDef = unlockable;
        }

        public override void Install() {
            base.Install();

            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;

            On.EntityStates.SiphonItem.DetonateState.FixedUpdate += DetonateState_FixedUpdate;
            On.EntityStates.Toolbot.DroneProjectileHoverHeal.HealOccupants += DroneProjectileHoverHeal_HealOccupants;
            On.EntityStates.VoidSurvivor.Vent.VentCorruption.FixedUpdate += VentCorruption_FixedUpdate;
            On.RoR2.CharacterBody.RemoveBuff_BuffIndex += CharacterBody_RemoveBuff_BuffIndex;
            On.RoR2.GlobalEventManager.OnCrit += GlobalEventManager_OnCrit;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.HealBeamController.OnTickServer += HealBeamController_OnTickServer;
            On.RoR2.HealNearbyController.Tick += HealNearbyController_Tick;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.UsePotion += HealthComponent_UsePotion;
            On.RoR2.Orbs.DamageOrb.OnArrival += DamageOrb_OnArrival;
            On.RoR2.SiphonNearbyController.Tick += SiphonNearbyController_Tick;
            On.RoR2.TarTetherController.DoDamageTick += TarTetherController_DoDamageTick;
            On.EntityStates.GhostUtilitySkillState.FixedUpdate += GhostUtilitySkillState_FixedUpdate;
            On.EntityStates.Treebot.BurrowDash.FixedUpdate += BurrowDash_FixedUpdate;
            On.EntityStates.Treebot.FlowerProjectileHover.HealPulse += FlowerProjectileHover_HealPulse;
            On.EntityStates.VoidSurvivor.Weapon.CrushBase.OnEnter += CrushBase_OnEnter;
            On.RoR2.EquipmentSlot.FireFruit += EquipmentSlot_FireFruit;
            On.RoR2.EquipmentSlot.FirePassiveHealing += EquipmentSlot_FirePassiveHealing;
            On.RoR2.HealingFollowerController.DoHeal += HealingFollowerController_DoHeal;
            On.RoR2.HealthComponent.UpdateLastHitTime += HealthComponent_UpdateLastHitTime;
            On.RoR2.MushroomVoidBehavior.FixedUpdate += MushroomVoidBehavior_FixedUpdate;

            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;

            IL.RoR2.HealthComponent.HandleHeal += HealthComponent_HandleHeal;

            //not instantiated by default
            origColorValue = DamageColor.colors[(int)DamageColorIndex.CritHeal];
            DamageColor.colors[(int)DamageColorIndex.CritHeal] = new Color(0.45f, 1f, 0.7f);
        }

        public override void Uninstall() {
            base.Uninstall();

            IL.RoR2.HealthComponent.Heal -= HealthComponent_Heal;

            On.EntityStates.SiphonItem.DetonateState.FixedUpdate -= DetonateState_FixedUpdate;
            On.EntityStates.Toolbot.DroneProjectileHoverHeal.HealOccupants -= DroneProjectileHoverHeal_HealOccupants;
            On.EntityStates.VoidSurvivor.Vent.VentCorruption.FixedUpdate -= VentCorruption_FixedUpdate;
            On.RoR2.CharacterBody.RemoveBuff_BuffIndex -= CharacterBody_RemoveBuff_BuffIndex;
            On.RoR2.GlobalEventManager.OnCrit -= GlobalEventManager_OnCrit;
            On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
            On.RoR2.HealBeamController.OnTickServer -= HealBeamController_OnTickServer;
            On.RoR2.HealNearbyController.Tick -= HealNearbyController_Tick;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.UsePotion -= HealthComponent_UsePotion;
            On.RoR2.Orbs.DamageOrb.OnArrival -= DamageOrb_OnArrival;
            On.RoR2.SiphonNearbyController.Tick -= SiphonNearbyController_Tick;
            On.RoR2.TarTetherController.DoDamageTick -= TarTetherController_DoDamageTick;
            On.EntityStates.GhostUtilitySkillState.FixedUpdate -= GhostUtilitySkillState_FixedUpdate;
            On.EntityStates.Treebot.BurrowDash.FixedUpdate -= BurrowDash_FixedUpdate;
            On.EntityStates.Treebot.FlowerProjectileHover.HealPulse -= FlowerProjectileHover_HealPulse;
            On.EntityStates.VoidSurvivor.Weapon.CrushBase.OnEnter -= CrushBase_OnEnter;
            On.RoR2.EquipmentSlot.FireFruit -= EquipmentSlot_FireFruit;
            On.RoR2.EquipmentSlot.FirePassiveHealing -= EquipmentSlot_FirePassiveHealing;
            On.RoR2.HealingFollowerController.DoHeal -= HealingFollowerController_DoHeal;
            On.RoR2.HealthComponent.UpdateLastHitTime -= HealthComponent_UpdateLastHitTime;
            On.RoR2.MushroomVoidBehavior.FixedUpdate -= MushroomVoidBehavior_FixedUpdate;

            On.RoR2.Run.FixedUpdate -= Run_FixedUpdate;

            DamageColor.colors[(int)DamageColorIndex.CritHeal] = origColorValue;
        }



        ////// Hooks //////

        private void HealthComponent_HandleHeal(ILContext il) {
            var c = new ILCursor(il);

            bool wasCrit = false;

            if(c.TryGotoNext(MoveType.After, x => x.MatchLdfld<HealthComponent.HealMessage>(nameof(HealthComponent.HealMessage.amount)))) {
                c.EmitDelegate<Func<float, float>>(hv => {
                        wasCrit = hv < 0;
                        return Mathf.Abs(hv);
                    });
            } else {
                TinkersSatchelPlugin._logger.LogError("Defib: failed to apply IL hook (HealthComponent_HandleHeal), target instructions not found (amount). Heal numbers may appear as 1s.");
            }

            if(c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt<DamageNumberManager>(nameof(DamageNumberManager.SpawnDamageNumber)))) {
                c.EmitDelegate<Func<DamageColorIndex, DamageColorIndex>>(dci => {
                    if(wasCrit) return DamageColorIndex.CritHeal;
                    return dci;
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("Defib: failed to apply IL hook (HealthComponent_HandleHeal), target instructions not found (damage number). Crit heals will have the same color as normal heals.");
            }
        }

        private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self) {
            orig(self);
            if(healingSourceStack.Count != 0) {
                TinkersSatchelPlugin._logger.LogError("Defib: HealingSourceStack was not empty on new frame, clearing. May be a cascading effect of another error, or a mod may be misusing HealingSourceStack.");
                healingSourceStack.Clear();
            }
        }

        private void HealthComponent_Heal(ILContext il) {
            var c = new ILCursor(il);

            bool wasCrit = false;
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Func<float, float>>((origAmount) => {
                wasCrit = false;
                if(NetworkServer.active && healingSourceStack.Count > 0) {
                    var sourceBody = healingSourceStack.Peek();
                    var sourceCount = GetCount(sourceBody);
                    if(sourceCount > 0 && sourceBody.RollCrit()) {
                        wasCrit = true;
                        return origAmount * (1f + critFracBase + critFracStack * (sourceCount - 1));
                    }
                }
                return origAmount;
            });
            c.Emit(OpCodes.Starg, 1);

            if(c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt<HealthComponent>(nameof(HealthComponent.SendHeal)))) {
                c.EmitDelegate<Func<bool, bool>>(oldIsCritPCM => wasCrit);
            } else {
                TinkersSatchelPlugin._logger.LogError("Defib: failed to apply IL hook (HealthComponent_Heal), target instructions not found. Heal numbers will not have a crit effect.");
            }
        }

        private void DetonateState_FixedUpdate(On.EntityStates.SiphonItem.DetonateState.orig_FixedUpdate orig, EntityStates.SiphonItem.DetonateState self) {
            bool hasBody = self.attachedBody;
            if(hasBody) healingSourceStack.Push(self.attachedBody);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private void DroneProjectileHoverHeal_HealOccupants(On.EntityStates.Toolbot.DroneProjectileHoverHeal.orig_HealOccupants orig, EntityStates.Toolbot.DroneProjectileHoverHeal self, float radius, float healPoints, float healFraction) {
            //todo: this probably doesn't work
            bool hasBody = self.outer.commonComponents.characterBody;
            if(hasBody) healingSourceStack.Push(self.outer.commonComponents.characterBody);
            orig(self, radius, healPoints, healFraction);
            if(hasBody) healingSourceStack.Pop();
        }


        private void VentCorruption_FixedUpdate(On.EntityStates.VoidSurvivor.Vent.VentCorruption.orig_FixedUpdate orig, EntityStates.VoidSurvivor.Vent.VentCorruption self) {
            bool hasBody = self.outer.commonComponents.characterBody;
            if(hasBody) healingSourceStack.Push(self.outer.commonComponents.characterBody);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private void CharacterBody_RemoveBuff_BuffIndex(On.RoR2.CharacterBody.orig_RemoveBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType) {
            bool hasBody = self;
            if(hasBody) healingSourceStack.Push(self);
            orig(self, buffType);
            if(hasBody) healingSourceStack.Pop();
        }

        private void GlobalEventManager_OnCrit(On.RoR2.GlobalEventManager.orig_OnCrit orig, GlobalEventManager self, CharacterBody body, DamageInfo damageInfo, CharacterMaster master, float procCoefficient, ProcChainMask procChainMask) {
            bool hasBody = body;
            if(hasBody) healingSourceStack.Push(body);
            orig(self, body, damageInfo, master, procCoefficient, procChainMask);
            if(hasBody) healingSourceStack.Pop();
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            bool hasBody = damageInfo != null && damageInfo.attacker;
            CharacterBody body = null;
            if(hasBody) {
                body = damageInfo.attacker.GetComponent<CharacterBody>();
                hasBody = body;
            }
            if(hasBody) healingSourceStack.Push(body);
            orig(self, damageInfo, victim);
            if(hasBody) healingSourceStack.Pop();
        }

        private void HealBeamController_OnTickServer(On.RoR2.HealBeamController.orig_OnTickServer orig, HealBeamController self) {
            bool hasBody = self && self.ownership && self.ownership.ownerObject;
            CharacterBody body = null;
            if(hasBody) {
                body = self.ownership.ownerObject.GetComponent<CharacterBody>();
                hasBody = body;
            }
            if(hasBody) healingSourceStack.Push(body);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private void HealNearbyController_Tick(On.RoR2.HealNearbyController.orig_Tick orig, HealNearbyController self) {
            bool hasBody = self && self.networkedBodyAttachment && self.networkedBodyAttachment.attachedBody;
            if(hasBody) healingSourceStack.Push(self.networkedBodyAttachment.attachedBody);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            bool hasBody = self.body;
            if(hasBody) healingSourceStack.Push(self.body);
            orig(self, damageInfo);
            if(hasBody) healingSourceStack.Pop();
        }

        private void HealthComponent_UsePotion(On.RoR2.HealthComponent.orig_UsePotion orig, HealthComponent self) {
            bool hasBody = self.body;
            if(hasBody) healingSourceStack.Push(self.body);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        //tar healing tar-immune enemies
        private void DamageOrb_OnArrival(On.RoR2.Orbs.DamageOrb.orig_OnArrival orig, DamageOrb self) {
            bool hasBody = self != null && self.attacker;
            CharacterBody body = null;
            if(hasBody) {
                body = self.attacker.GetComponent<CharacterBody>();
                hasBody = body;
            }
            if(hasBody) healingSourceStack.Push(body);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private void SiphonNearbyController_Tick(On.RoR2.SiphonNearbyController.orig_Tick orig, SiphonNearbyController self) {
            bool hasBody = self && self.networkedBodyAttachment && self.networkedBodyAttachment.attachedBody;
            if(hasBody) healingSourceStack.Push(self.networkedBodyAttachment.attachedBody);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private void TarTetherController_DoDamageTick(On.RoR2.TarTetherController.orig_DoDamageTick orig, TarTetherController self, bool mulch) {
            bool hasBody = self && self.ownerBody;
            if(hasBody) healingSourceStack.Push(self.ownerBody);
            orig(self, mulch);
            if(hasBody) healingSourceStack.Pop();
        }

        private void GhostUtilitySkillState_FixedUpdate(On.EntityStates.GhostUtilitySkillState.orig_FixedUpdate orig, EntityStates.GhostUtilitySkillState self) {
            bool hasBody = self.outer.commonComponents.characterBody;
            if(hasBody) healingSourceStack.Push(self.outer.commonComponents.characterBody);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private void BurrowDash_FixedUpdate(On.EntityStates.Treebot.BurrowDash.orig_FixedUpdate orig, EntityStates.Treebot.BurrowDash self) {
            bool hasBody = self.outer.commonComponents.characterBody;
            if(hasBody) healingSourceStack.Push(self.outer.commonComponents.characterBody);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private void FlowerProjectileHover_HealPulse(On.EntityStates.Treebot.FlowerProjectileHover.orig_HealPulse orig, EntityStates.Treebot.FlowerProjectileHover self) {
            bool hasBody = self.owner;
            CharacterBody body = null;
            if(hasBody) {
                body = self.owner.GetComponent<CharacterBody>();
                hasBody = body;
            }
            if(hasBody) healingSourceStack.Push(body);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private void CrushBase_OnEnter(On.EntityStates.VoidSurvivor.Weapon.CrushBase.orig_OnEnter orig, EntityStates.VoidSurvivor.Weapon.CrushBase self) {
            bool hasBody = self.outer.commonComponents.characterBody;
            if(hasBody) healingSourceStack.Push(self.outer.commonComponents.characterBody);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }

        private bool EquipmentSlot_FireFruit(On.RoR2.EquipmentSlot.orig_FireFruit orig, EquipmentSlot self) {
            bool hasBody = self && self.characterBody;
            if(hasBody) healingSourceStack.Push(self.characterBody);
            var retv = orig(self);
            if(hasBody) healingSourceStack.Pop();
            return retv;
        }

        private bool EquipmentSlot_FirePassiveHealing(On.RoR2.EquipmentSlot.orig_FirePassiveHealing orig, EquipmentSlot self) {
            bool hasBody = self && self.characterBody;
            if(hasBody) healingSourceStack.Push(self.characterBody);
            var retv = orig(self);
            if(hasBody) healingSourceStack.Pop();
            return retv;
        }

        private void HealingFollowerController_DoHeal(On.RoR2.HealingFollowerController.orig_DoHeal orig, HealingFollowerController self, float healFraction) {
            bool hasBody = self && self.ownerBodyObject;
            CharacterBody body = null;
            if(hasBody) {
                body = self.ownerBodyObject.GetComponent<CharacterBody>();
                hasBody = body;
            }
            if(hasBody) healingSourceStack.Push(body);
            orig(self, healFraction);
            if(hasBody) healingSourceStack.Pop();
        }

        private void HealthComponent_UpdateLastHitTime(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker) {
            bool hasBody = self && self.body;
            if(hasBody) healingSourceStack.Push(self.body);
            orig(self, damageValue, damagePosition, damageIsSilent, attacker);
            if(hasBody) healingSourceStack.Pop();
        }

        private void MushroomVoidBehavior_FixedUpdate(On.RoR2.MushroomVoidBehavior.orig_FixedUpdate orig, MushroomVoidBehavior self) {
            bool hasBody = self && self.body;
            if(hasBody) healingSourceStack.Push(self.body);
            orig(self);
            if(hasBody) healingSourceStack.Pop();
        }
        //TODO:
        //find users of HealFraction, RepeatHealComponent
        //attach owner info to HealthPickup, HealOrb, VendingMachineOrb
        //calc player to find crit chance from randomly weighted by count on Lepton Daisy
    }

    [RegisterAchievement("TkSat_Defib", "TkSat_DefibUnlockable", "")]
    public class TkSatDefibAchievement : RoR2.Achievements.BaseAchievement {
        public override void OnInstall() {
            base.OnInstall();
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
        }

        public override void OnUninstall() {
            base.OnUninstall();
            On.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
        }

        private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self) {
            orig(self);
            if(localUser.cachedMaster != self) return;
            int matches = 0;
            if(self.inventory.GetItemCount(RoR2Content.Items.Mushroom) > 0) matches++;
            if(self.inventory.GetItemCount(RoR2Content.Items.Tooth) > 0) matches++;
            if(self.inventory.GetItemCount(RoR2Content.Items.TPHealingNova) > 0) matches++;
            if(self.inventory.GetItemCount(RoR2Content.Items.Plant) > 0) matches++;
            if(ShootToHeal.instance.GetCount(self.inventory) > 0) matches++;
            if(self.inventory.currentEquipmentIndex == RoR2Content.Equipment.PassiveHealing.equipmentIndex
                || self.inventory.alternateEquipmentIndex == RoR2Content.Equipment.PassiveHealing.equipmentIndex)
                matches++;
            if(self.inventory.currentEquipmentIndex == DLC1Content.Equipment.VendingMachine.equipmentIndex
                || self.inventory.alternateEquipmentIndex == DLC1Content.Equipment.VendingMachine.equipmentIndex)
                matches++;
            if(matches >= 4)
                Grant();
        }
    }
}