using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    internal class CatalogBoilerplateModule : Module<CatalogBoilerplateModule> {
        public override bool managedEnable => false;

        internal static readonly FilingDictionary<CatalogBoilerplate> allInstances = new();
        internal static readonly Dictionary<ItemIndex, Item> itemInstances = new();
        internal static readonly Dictionary<EquipmentIndex, Equipment> equipmentInstances = new();
        internal static readonly Dictionary<ArtifactIndex, Artifact> artifactInstances = new();

        public static Sprite lockIcon { get; private set; }

        public override void SetupConfig() {
            base.SetupConfig();
            lockIcon = LegacyResourcesAPI.Load<Sprite>("Textures/MiscIcons/texUnlockIcon");
            On.RoR2.UI.LogBook.LogBookController.BuildPickupEntries += On_LogbookBuildPickupEntries;
            On.RoR2.Run.BuildDropTable += On_RunBuildDropTable;
            On.RoR2.PickupPickerController.GetOptionsFromPickupState += PickupPickerController_GetOptionsFromPickupState;
            On.RoR2.UI.LogBook.LogBookController.CanSelectItemEntry += LogBookController_CanSelectItemEntry;
            On.RoR2.UI.LogBook.LogBookController.CanSelectEquipmentEntry += LogBookController_CanSelectEquipmentEntry;
            On.RoR2.RuleDef.FromItem += RuleDef_FromItem;
            On.RoR2.RuleDef.FromEquipment += RuleDef_FromEquipment;
            On.RoR2.RuleDef.FromArtifact += RuleDef_FromArtifact;
            IL.RoR2.PreGameController.ResolveChoiceMask += PreGameController_ResolveChoiceMask;
            RoR2.Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private void Run_onRunStartGlobal(Run obj) {
            UpdateEnigmaEquipmentTable();
            UpdateRandomTriggerEquipmentTable();
        }

        internal void UpdateEnigmaEquipmentTable() {
            var toRemove = equipmentInstances.Where(x => !x.Value.enabled).Select(x => x.Key);
            var toAdd = equipmentInstances.Where(x => x.Value.enabled && x.Value.isEnigmaCompatible).Select(x => x.Key);
            foreach(var eqp in toRemove) {
                EquipmentCatalog.enigmaEquipmentList.Remove(eqp);
                RoR2.Artifacts.EnigmaArtifactManager.validEquipment.Remove(eqp);
            }
            foreach(var eqp in toAdd) {
                if(!EquipmentCatalog.enigmaEquipmentList.Contains(eqp))
                    EquipmentCatalog.enigmaEquipmentList.Add(eqp);
                if(!RoR2.Artifacts.EnigmaArtifactManager.validEquipment.Contains(eqp))
                    RoR2.Artifacts.EnigmaArtifactManager.validEquipment.Add(eqp);
            }
        }

        internal void UpdateRandomTriggerEquipmentTable() {
            var toRemove = equipmentInstances.Where(x => !x.Value.enabled).Select(x => x.Key);
            var toAdd = equipmentInstances.Where(x => x.Value.enabled && x.Value.canBeRandomlyTriggered).Select(x => x.Key);
            foreach(var eqp in toRemove) {
                EquipmentCatalog.randomTriggerEquipmentList.Remove(eqp);
            }
            foreach(var eqp in toAdd) {
                if(!EquipmentCatalog.randomTriggerEquipmentList.Contains(eqp))
                    EquipmentCatalog.randomTriggerEquipmentList.Add(eqp);
            }
        }

        private void PreGameController_ResolveChoiceMask(ILContext il) {
            ILCursor c = new(il);

            if(c.TryGotoNext(MoveType.Before,
                x => x.MatchLdfld<PreGameController>(nameof(PreGameController.choiceMaskBuffer)),
                x => x.MatchCallOrCallvirt<NetworkRuleChoiceMask>(nameof(NetworkRuleChoiceMask.SetRuleChoiceMask))
                )) {
                c.Index++;
                c.EmitDelegate<Func<RuleChoiceMask, RuleChoiceMask>>((origMask) => {
                    for(var i = 0; i < origMask.length; i++) {
                        var cdef = RuleCatalog.GetChoiceDef(i);
                        if(cdef.artifactIndex != ArtifactIndex.None
                            && artifactInstances.ContainsKey(cdef.artifactIndex)
                            && !artifactInstances[cdef.artifactIndex].enabled)
                            origMask[i] = false;
                        if(cdef.equipmentIndex != EquipmentIndex.None
                            && equipmentInstances.ContainsKey(cdef.equipmentIndex)
                            && !equipmentInstances[cdef.equipmentIndex].enabled)
                            origMask[i] = false;
                        if(cdef.itemIndex != ItemIndex.None
                            && itemInstances.ContainsKey(cdef.itemIndex)
                            && !itemInstances[cdef.itemIndex].enabled)
                            origMask[i] = false;
                    }
                    return origMask;
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("CatalogBoilerplateModule: Failed to apply IL hook (PreGameController.ResolveChoiceMask), target instructions not found. Disabled items will be erroneously selectable if using a pregame item rulebook unhider mod.");
            }
        }

        private RuleDef RuleDef_FromArtifact(On.RoR2.RuleDef.orig_FromArtifact orig, ArtifactIndex artifactIndex) {
            var retv = orig(artifactIndex);
            foreach(CatalogBoilerplate bpl in allInstances) {
                if(bpl is Artifact artifact && artifact.catalogIndex == artifactIndex) {
                    artifactInstances[artifactIndex] = artifact;
                    artifact.ruleDef = retv;
                    break;
                }
            }
            return retv;
        }

        private RuleDef RuleDef_FromEquipment(On.RoR2.RuleDef.orig_FromEquipment orig, EquipmentIndex equipmentIndex) {
            var retv = orig(equipmentIndex);
            foreach(CatalogBoilerplate bpl in allInstances) {
                if(bpl is Equipment equipment && equipment.catalogIndex == equipmentIndex) {
                    equipmentInstances[equipmentIndex] = equipment;
                    equipment.ruleDef = retv;
                    break;
                }
            }
            return retv;
        }

        private RuleDef RuleDef_FromItem(On.RoR2.RuleDef.orig_FromItem orig, ItemIndex itemIndex) {
            var retv = orig(itemIndex);
            foreach(CatalogBoilerplate bpl in allInstances) {
                if(bpl is Item item && item.catalogIndex == itemIndex) {
                    itemInstances[itemIndex] = item;
                    item.ruleDef = retv;
                    break;
                }
            }
            return retv;
        }

        private bool LogBookController_CanSelectEquipmentEntry(On.RoR2.UI.LogBook.LogBookController.orig_CanSelectEquipmentEntry orig, EquipmentDef equipmentDef, Dictionary<RoR2.ExpansionManagement.ExpansionDef, bool> expansionAvailability) {
            var retv = orig(equipmentDef, expansionAvailability);
            if(equipmentDef != null && allInstances.Any(x => !x.enabled && x is Equipment eqp && eqp.equipmentDef == equipmentDef))
                return false;
            return retv;
        }

        private bool LogBookController_CanSelectItemEntry(On.RoR2.UI.LogBook.LogBookController.orig_CanSelectItemEntry orig, ItemDef itemDef, Dictionary<RoR2.ExpansionManagement.ExpansionDef, bool> expansionAvailability) {
            var retv = orig(itemDef, expansionAvailability);
            if(itemDef != null && allInstances.Any(x => !x.enabled && x is Item item && item.itemDef == itemDef))
                return false;
            return retv;
        }

        private PickupPickerController.Option[] PickupPickerController_GetOptionsFromPickupState(On.RoR2.PickupPickerController.orig_GetOptionsFromPickupState orig, UniquePickup pickupState) {
            var origv = orig(pickupState);
            var remv = new HashSet<PickupIndex>();
            foreach(CatalogBoilerplate bpl in allInstances) {
                if((bpl is Item || bpl is Equipment) && !bpl.enabled && bpl.pickupIndex != PickupIndex.none) remv.Add(bpl.pickupIndex);
            }
            return origv.Where(x => !remv.Contains(x.pickup.pickupIndex)).ToArray();
        }

        private void On_RunBuildDropTable(On.RoR2.Run.orig_BuildDropTable orig, Run self) {
            var newItemMask = self.availableItems;
            var newEqpMask = self.availableEquipment;
            foreach(CatalogBoilerplate bpl in allInstances) {
                if(bpl is Item item && !item.enabled)
                    newItemMask.Remove(item.catalogIndex);
                else if(bpl is Equipment equipment && !equipment.enabled)
                    newEqpMask.Remove(equipment.catalogIndex);
            }
            self.availableItems = newItemMask;
            self.availableEquipment = newEqpMask;
            orig(self);
            //should force-update most cached drop tables
            PickupDropTable.RegenerateAll(Run.instance);
            UpdateEnigmaEquipmentTable(); //todo: trigger enigma if player has newly disabled equipment
            UpdateRandomTriggerEquipmentTable();
        }

        [SystemInitializer(new Type[] {typeof(PickupCatalog)})]
        private static void PostCachePickups() {
            foreach(CatalogBoilerplate bpl in allInstances) {
                PickupIndex pind;
                if(bpl is Equipment equipment) pind = PickupCatalog.FindPickupIndex(equipment.catalogIndex);
                else if(bpl is Item item) pind = PickupCatalog.FindPickupIndex(item.catalogIndex);
                else continue;
                var pickup = PickupCatalog.GetPickupDef(pind);

                bpl.pickupDef = pickup;
                bpl.pickupIndex = pind;
            }
        }

        private RoR2.UI.LogBook.Entry[] On_LogbookBuildPickupEntries(On.RoR2.UI.LogBook.LogBookController.orig_BuildPickupEntries orig, Dictionary<RoR2.ExpansionManagement.ExpansionDef, bool> expansionAvailability) {
            var retv = orig(expansionAvailability);
            var bplsLeft = allInstances.ToList();
            foreach(var entry in retv) {
                if(entry.extraData is not PickupIndex) continue;
                CatalogBoilerplate matchedBpl = null;
                foreach(CatalogBoilerplate bpl in bplsLeft) {
                    if((PickupIndex)entry.extraData == bpl.pickupIndex) {
                        matchedBpl = bpl;
                        break;
                    }
                }
                if(matchedBpl != null) {
                    matchedBpl.logbookEntry = entry;
                    bplsLeft.Remove(matchedBpl);
                }
            }
            return retv;
        }
    }
}
