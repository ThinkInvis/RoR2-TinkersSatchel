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