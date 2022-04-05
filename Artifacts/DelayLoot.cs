using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
    public class DelayLoot : Artifact<DelayLoot> {

        ////// Artifact Data //////

        public override string displayName => "Artifact of Safekeeping";

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetDescString(string langid = null) => "All item drops are taken and guarded by the teleporter boss, which will explode in a shower of loot when killed.";



        ////// Config //////

        public enum AnnounceItemsMode {
            Nothing, Vague, ItemTier, ItemName
        }
        [AutoConfig("What to display in chat when an item is taken for safekeeping.")]
        public AnnounceItemsMode announceItems { get; private set; } = AnnounceItemsMode.ItemName;

        public enum AnnounceDropMode {
            Nothing, TotalItemCount, ItemTierCounts
        }
        [AutoConfig("What to display in chat when the teleporter boss is killed.")]
        public AnnounceDropMode announceDrop { get; private set; } = AnnounceDropMode.ItemTierCounts;



        ////// Other Fields/Properties //////

        bool shouldDeferDrops = true;
        Vector3 lootShowerLoc = Vector3.zero;
        float lootShowerTimer = 0f;
        List<GameObject> deferredDrops = new List<GameObject>();
        List<Vector3> launchVelocities = new List<Vector3>();



        ////// TILER2 Module Setup //////

        public DelayLoot() {
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/delayitems_on.png");
            iconResourceDisabled = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/delayitems_off.png");
        }

        public override void Install() {
            base.Install();
            Stage.onServerStageBegin += Stage_onServerStageBegin;
            IL.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 += PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;
        }

        public override void Uninstall() {
            base.Uninstall();
            Stage.onServerStageBegin -= Stage_onServerStageBegin;
            IL.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 -= PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3;
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.Run.FixedUpdate -= Run_FixedUpdate;
        }



        ////// Hooks //////

        private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self) {
            orig(self);
            if(NetworkServer.active && IsActiveAndEnabled() && !shouldDeferDrops && deferredDrops.Count > 0) {
                lootShowerTimer -= Time.fixedDeltaTime;
                if(lootShowerTimer <= 0f) {
                    var tgtNode = launchVelocities[0];
                    launchVelocities.RemoveAt(0);
                    launchVelocities.Add(tgtNode);
                    var rbody = deferredDrops[0].GetComponent<Rigidbody>();
                    rbody.velocity = tgtNode;
                    rbody.drag = 0f;
                    deferredDrops[0].GetComponent<ConstantForce>().enabled = false;
                    deferredDrops[0].transform.position = lootShowerLoc;
                    deferredDrops[0].SetActive(true);
                    deferredDrops.RemoveAt(0);
                    lootShowerTimer = 0.125f;
                }
            }
        }

        private void Stage_onServerStageBegin(Stage obj) {
            shouldDeferDrops = true;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report) {
            if(IsActiveAndEnabled() && report.victimIsBoss && TeleporterInteraction.instance && TeleporterInteraction.instance.bossGroup.combatSquad.memberCount <= 1) {
                shouldDeferDrops = false;
                lootShowerLoc = report.victim.transform.position + Vector3.up * 3f;
                var nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);

                launchVelocities = CollectNearestNodeLaunchVelocities(nodeGraph, deferredDrops.Count, 10f, 1000f,
                    lootShowerLoc, 5f, 0.625f, 3f, 10, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore);

                if(launchVelocities.Count < deferredDrops.Count) {
                    var ls2 = TeleporterInteraction.instance.transform.position + Vector3.up * 3f;
                    var lvel2 = CollectNearestNodeLaunchVelocities(nodeGraph, deferredDrops.Count, 10f, 1000f,
                    ls2, 5f, 0.625f, 3f, 10, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore);
                    if(launchVelocities.Count == 0 && lvel2.Count == 0) {
                        TinkersSatchelPlugin._logger.LogWarning("DelayLoot: found no free navnodes to drop items at, using fallback circle");
                        lootShowerLoc = ls2;
                        for(var i = 0; i < deferredDrops.Count; i++) {
                            var theta = (float)i / (float)deferredDrops.Count * Mathf.PI * 2f;
                            launchVelocities.Add(new Vector3(Mathf.Cos(theta) * 10f, 20f, Mathf.Sin(theta) * 10f));
                        }
                    } else if(lvel2.Count > launchVelocities.Count) {
                        TinkersSatchelPlugin._logger.LogWarning("DelayLoot: couldn't find enough free navnodes to drop items at from boss, falling back to TP");
                        launchVelocities = lvel2;
                        lootShowerLoc = ls2;
                    }
                    if(launchVelocities.Count < deferredDrops.Count) {
                        TinkersSatchelPlugin._logger.LogWarning("DelayLoot: couldn't find enough free navnodes to drop items at from any source, some items will stack");
                    }
                }

                if(announceDrop == AnnounceDropMode.ItemTierCounts) {
                    Dictionary<ItemTier, int> totalItemTiers = new Dictionary<ItemTier, int>();
                    int totalEquipments = 0;
                    int totalLunarEquipments = 0;
                    int totalOther = 0;
                    string lunarColorHex = "";
                    string equipmentColorHex = "";
                    foreach(var drop in deferredDrops) {
                        if(drop && drop.TryGetComponent<PickupDropletController>(out var pickup)) {
                            var pdef = PickupCatalog.GetPickupDef(pickup.pickupIndex);
                            if(pdef != null) {
                                if(pdef.itemIndex != ItemIndex.None && pdef.itemTier != ItemTier.NoTier) {
                                    if(!totalItemTiers.ContainsKey(pdef.itemTier)) {
                                        totalItemTiers[pdef.itemTier] = 0;
                                    }
                                    totalItemTiers[pdef.itemTier]++;
                                } else if(pdef.equipmentIndex != EquipmentIndex.None) {
                                    if(pdef.isLunar) {
                                        totalLunarEquipments++;
                                        lunarColorHex = Util.RGBToHex(pdef.baseColor);
                                    } else {
                                        totalEquipments++;
                                        equipmentColorHex = Util.RGBToHex(pdef.baseColor);
                                    }
                                } else totalOther++;
                            } else totalOther++;
                        } else totalOther++;
                    }
                    List<string> displays = new List<string>();
                    if(totalItemTiers.ContainsKey(ItemTier.Tier1)) displays.Add($"<color=#{ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Tier1Item)}>{totalItemTiers[ItemTier.Tier1]} tier-1 item{NPlur(totalItemTiers[ItemTier.Tier1])}</color>");
                    if(totalItemTiers.ContainsKey(ItemTier.Tier2)) displays.Add($"<color=#{ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Tier2Item)}>{totalItemTiers[ItemTier.Tier2]} tier-2 item{NPlur(totalItemTiers[ItemTier.Tier2])}</color>");
                    if(totalItemTiers.ContainsKey(ItemTier.Tier3)) displays.Add($"<color=#{ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Tier3Item)}>{totalItemTiers[ItemTier.Tier3]} tier-3 item{NPlur(totalItemTiers[ItemTier.Tier3])}</color>");
                    if(totalItemTiers.ContainsKey(ItemTier.Lunar)) displays.Add($"<color=#{ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.LunarItem)}>{totalItemTiers[ItemTier.Lunar]} lunar item{NPlur(totalItemTiers[ItemTier.Lunar])}</color>");
                    if(totalItemTiers.ContainsKey(ItemTier.Boss)) displays.Add($"<color=#{ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.BossItem)}>{totalItemTiers[ItemTier.Boss]} boss item{NPlur(totalItemTiers[ItemTier.Boss])}</color>");
                    if(totalEquipments > 0) displays.Add($"<color=#{ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Equipment)}>{totalEquipments} equipment{NPlur(totalEquipments)}</color>");
                    if(totalLunarEquipments > 0) displays.Add($"<color=#{ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.LunarItem)}>{totalLunarEquipments} lunar equipment{NPlur(totalLunarEquipments)}</color>");
                    int totalVoidCount = 0;
                    if(totalItemTiers.ContainsKey(ItemTier.VoidTier1)) { totalVoidCount += totalItemTiers[ItemTier.VoidTier1]; }
                    if(totalItemTiers.ContainsKey(ItemTier.VoidTier2)) { totalVoidCount += totalItemTiers[ItemTier.VoidTier2]; }
                    if(totalItemTiers.ContainsKey(ItemTier.VoidTier3)) { totalVoidCount += totalItemTiers[ItemTier.VoidTier3]; }
                    if(totalItemTiers.ContainsKey(ItemTier.VoidBoss)) { totalVoidCount += totalItemTiers[ItemTier.VoidBoss]; }
                    if(totalVoidCount > 0) displays.Add($"<color=#{ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.VoidItem)}>{totalVoidCount} void item{NPlur(totalVoidCount)}</color>");
                    if(totalOther > 0) displays.Add($"{totalOther} other drop{NPlur(totalOther)}");

                    if(displays.Count == 0) {
                    } else if(displays.Count == 1) {
                        NetUtil.ServerSendGlobalChatMsg($"The boss's hoard of {displays[0]} is yours.");
                    } else if(displays.Count == 2) {
                        NetUtil.ServerSendGlobalChatMsg($"The boss's hoard of {String.Join(" and ", displays)} is yours.");
                    } else {
                        displays[displays.Count - 1] = "and " + displays[displays.Count - 1];
                        NetUtil.ServerSendGlobalChatMsg($"The boss's hoard of {String.Join(", ", displays)} is yours.");
                    }
                } else if(announceDrop == AnnounceDropMode.TotalItemCount && deferredDrops.Count > 0) {
                    NetUtil.ServerSendGlobalChatMsg($"The boss's hoard of {deferredDrops.Count} item{NPlur(deferredDrops.Count)} is yours.");
                }
            }
        }

        private void PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3(ILContext il) {
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<NetworkServer>(nameof(NetworkServer.Spawn)))) {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate<Action<GameObject>>(obj => {
                    if(IsActiveAndEnabled() && shouldDeferDrops && TeleporterInteraction.instance)
                        DeferDroplet(obj);
                });
            } else {
                TinkersSatchelPlugin._logger.LogError("DelayLoot failed to apply IL hook (PickupDropletController_CreatePickupDroplet): couldn't find target instructions");
            }
        }


        void DeferDroplet(GameObject droplet) {
            if(!droplet) return;
            deferredDrops.Add(droplet);
            droplet.SetActive(false);
            var pctrl = droplet.GetComponent<PickupDropletController>();
            if(!pctrl) return;
            var pdef = PickupCatalog.GetPickupDef(pctrl.pickupIndex);

            if(pdef != null && announceItems != AnnounceItemsMode.Nothing) {
                string displayName = "Something";
                if(announceItems == AnnounceItemsMode.ItemName) {
                    displayName = $"<color=#{Util.RGBToHex(pdef.baseColor)}>{Language.GetString(pdef.nameToken)}</color>";
                } else if(announceItems == AnnounceItemsMode.ItemTier) {
                    var tierString = "Something strange";
                    if(pdef.itemTier == ItemTier.Tier1)
                        tierString = "A tier-1 item";
                    if(pdef.itemTier == ItemTier.Tier2)
                        tierString = "A tier-2 item";
                    if(pdef.itemTier == ItemTier.Tier3)
                        tierString = "A tier-3 item";
                    if(pdef.itemTier == ItemTier.Lunar)
                        tierString = "A lunar item";
                    if(pdef.itemTier == ItemTier.Boss)
                        tierString = "A boss item";
                    if(pdef.itemTier == ItemTier.VoidTier1 || pdef.itemTier == ItemTier.VoidTier2 || pdef.itemTier == ItemTier.VoidTier3 || pdef.itemTier == ItemTier.VoidBoss)
                        tierString = "A void item";
                    if(pdef.equipmentIndex != EquipmentIndex.None)
                        tierString = pdef.isLunar ? "A lunar equipment" : "An equipment";
                    displayName = $"<color=#{Util.RGBToHex(pdef.baseColor)}>{tierString}</color>";
                }
                NetUtil.ServerSendGlobalChatMsg($"{displayName} has been taken for safekeeping.");
                /*var effectData = new EffectData {
                    origin = droplet.transform.position,
                    genericFloat = 1f,
                    genericUInt = (uint)(pdef.itemIndex + 1)
                };
                effectData.SetNetworkedObjectReference(TeleporterInteraction.instance.gameObject);
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/ItemTakenOrbEffect"), effectData, true);*/
            }
        }
    }
}