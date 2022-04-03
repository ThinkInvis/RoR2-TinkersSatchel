using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using TILER2;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class DelayLoot : Artifact<DelayLoot> {

        ////// Artifact Data //////

        public override string displayName => "Artifact of Safekeeping";

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetDescString(string langid = null) => "All item drops are taken and guarded by the teleporter boss, which will explode in a shower of loot when killed.";



        ////// Other Fields/Properties //////

        bool shouldDeferDrops = true;
        Vector3 lootShowerLoc = Vector3.zero;
        float lootShowerTimer = 0f;
        List<GameObject> deferredDrops = new List<GameObject>();



        ////// TILER2 Module Setup //////

        public DelayLoot() {
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/delayitems_on.png");
            iconResourceDisabled = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/delayitems_off.png");
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
                    var rvel = UnityEngine.Random.onUnitSphere;
                    rvel.y = Mathf.Abs(rvel.y) + 0.75f;
                    var rbody = deferredDrops[0].GetComponent<Rigidbody>();
                    rbody.velocity = rvel * UnityEngine.Random.Range(10f, 40f);
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
                lootShowerLoc = report.victim.transform.position;
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

            if(pdef != null && pdef.itemIndex != ItemIndex.None) {
                var effectData = new EffectData {
                    origin = droplet.transform.position,
                    genericFloat = 1f,
                    genericUInt = (uint)(pdef.itemIndex + 1)
                };
                effectData.SetNetworkedObjectReference(TeleporterInteraction.instance.gameObject);
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/ItemTakenOrbEffect"), effectData, true);
            }
        }
    }
}