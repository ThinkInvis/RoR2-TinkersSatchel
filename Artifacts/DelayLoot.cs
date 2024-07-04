using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
    public class DelayLoot : Artifact<DelayLoot> {

        ////// Config //////

        public enum AnnounceItemsMode {
            Nothing, Vague, ItemTier, ItemName
        }
        [AutoConfig("What to display in chat when an item is taken for safekeeping.")]
        public AnnounceItemsMode announceItems { get; private set; } = AnnounceItemsMode.ItemName;

        public enum AnnounceDropMode {
            Nothing, TotalItemCount
        }
        [AutoConfig("What to display in chat when the teleporter boss is killed.")]
        public AnnounceDropMode announceDrop { get; private set; } = AnnounceDropMode.TotalItemCount;



        ////// Other Fields/Properties //////

        bool shouldDeferDrops = true;
        Vector3 lootShowerLoc = Vector3.zero;
        float lootShowerTimer = 0f;
        readonly List<GameObject> deferredDrops = new();
        List<Vector3> launchVelocities = new();



        ////// TILER2 Module Setup //////

        public DelayLoot() {
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/delayitems_on.png");
            iconResourceDisabled = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ArtifactIcons/delayitems_off.png");
        }

        public override void Install() {
            base.Install();
            Stage.onServerStageBegin += Stage_onServerStageBegin;
            IL.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3 += PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;
        }

        public override void Uninstall() {
            base.Uninstall();
            Stage.onServerStageBegin -= Stage_onServerStageBegin;
            IL.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3 -= PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3;
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.Run.FixedUpdate -= Run_FixedUpdate;
        }



        ////// Hooks //////

        private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self) {
            orig(self);
            if(NetworkServer.active && IsActiveAndEnabled() && !shouldDeferDrops && deferredDrops.Count > 0) {
                lootShowerTimer -= Time.fixedDeltaTime;
                if(lootShowerTimer <= 0f) {
                    if(!deferredDrops[0]) {
                        deferredDrops.RemoveAt(0);
                        return;
                    }
                    var tgtNode = launchVelocities[0];
                    launchVelocities.RemoveAt(0);
                    launchVelocities.Add(tgtNode);
                    var rbody = deferredDrops[0].GetComponent<Rigidbody>();
                    if(rbody) {
                        rbody.velocity = tgtNode;
                        rbody.drag = 0f;
                    }
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
                if(announceDrop == AnnounceDropMode.TotalItemCount && deferredDrops.Count > 0) {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage {
                        paramTokens = new[] { deferredDrops.Count.ToString() },
                        baseToken = "TKSAT_DELAYLOOT_MSG_KILL"
                    });
                }
            }
        }

        private void PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3(ILContext il) {
            ILCursor c = new(il);
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
                string displayName = Language.GetString((announceItems == AnnounceItemsMode.ItemName) ? pdef.nameToken : "TKSAT_DELAYLOOT_MSG_DELAY_VAGUE");
                if(announceItems != AnnounceItemsMode.Vague)
                    displayName = $"<color=#{Util.RGBToHex(pdef.baseColor)}>{displayName}</color>";
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage {
                    paramTokens = new[] { displayName },
                    baseToken = "TKSAT_DELAYLOOT_MSG_DELAY"
                });
            }
        }
    }
}