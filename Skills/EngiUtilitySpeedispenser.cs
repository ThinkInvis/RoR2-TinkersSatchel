using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using EntityStates;
using RoR2.Skills;
using RoR2.Projectile;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace ThinkInvisible.TinkersSatchel {
    public class EngiUtilitySpeedispenser : T2Module<EngiUtilitySpeedispenser> {

        ////// Module Data //////

        public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferForever | AutoConfigFlags.PreventNetMismatch;



        ////// Other Fields/Properties //////
        
		public SkillDef skillDef { get; private set; }
		public GameObject deployableBody { get; private set; }
		public GameObject deployableMaster { get; private set; }
		public GameObject deployableBlueprint { get; private set; }
		public BuffDef speedBuff { get; private set; }
		public DeployableSlot deployableSlot { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;


		////// TILER2 Module Setup //////

		public EngiUtilitySpeedispenser() {
        }

        public override void RefreshPermanentLanguage() {
			permanentGenericLanguageTokens.Add("TKSAT_ENGI_SPEEDISPENSER_BODY_NAME", "Speed Dispenser");
			permanentGenericLanguageTokens.Add("TKSAT_ENGI_SPEEDISPENSER_CONTEXT", "Take speed boost");
			permanentGenericLanguageTokens.Add("TKSAT_ENGI_SPEEDISPENSER_CONTEXT_ALREADY_HAVE", "Can't take speed boost\n(Don't be greedy!)");
			permanentGenericLanguageTokens.Add("TKSAT_ENGI_SPEEDISPENSER_CONTEXT_CHARGING", "Can't take speed boost\n(Still brewing...)");
			permanentGenericLanguageTokens.Add("TKSAT_ENGI_UTILITY_SPEEDISPENSER_NAME", "Speed Dispenser");
            permanentGenericLanguageTokens.Add("TKSAT_ENGI_UTILITY_SPEEDISPENSER_DESCRIPTION", "Deploy a <style=cIsUtility>stationary decanter</style> that stores up to 4 delicious, caffeinated, precision-brewed charges of <style=cIsUtility>sprint speed</style> and <style=cIsUtility>jump height</style>. <style=cIsUtility>Inherits all your items</style>.");
            base.RefreshPermanentLanguage();
			//todo: allow interact to consume partial charge and renew buff
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

			//load custom assets
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/EngiUtilitySpeedispenser.asset");
			var tmpDeployableBody = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Characters/EngiSpeedispenser/EngiSpeedispenserBody.prefab")
				.InstantiateClone("TkSatTempSetupPrefab", false);
			deployableMaster = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Characters/EngiSpeedispenser/EngiSpeedispenserMaster.prefab");
			deployableBlueprint = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Characters/EngiSpeedispenser/EngiSpeedispenserBlueprints.prefab");

			//load vanilla assets
			targetSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Engi/EngiBodyUtilityFamily.asset")
				.WaitForCompletion();
			var captainSupply = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainSupplyDrop, EquipmentRestock.prefab")
				.WaitForCompletion()
				.InstantiateClone("TkSatTempSetupPrefab2", false);
			var buffIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/AttackSpeedAndMoveSpeed/texCoffeeIcon.png")
				.WaitForCompletion();
			var mainMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Engi/matEngiTurret.mat")
				.WaitForCompletion();
			var bpOkMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Engi/matBlueprintsOk.mat")
				.WaitForCompletion();
			var bpNokMtl = Addressables.LoadAssetAsync<Material>("RoR2/Base/Engi/matBlueprintsInvalid.mat")
				.WaitForCompletion();
			var ctp = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandard.asset")
				.WaitForCompletion();
			var turretBp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiTurretBlueprints.prefab")
				.WaitForCompletion();
			var turretObj = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiTurretBody.prefab")
				.WaitForCompletion();
			var bpProjMtl = turretBp.transform.Find("BlobLightProjector").GetComponent<Projector>().material;

			//modify
			speedBuff = ScriptableObject.CreateInstance<BuffDef>();
			speedBuff.buffColor = Color.white;
			speedBuff.canStack = false;
			speedBuff.isDebuff = false;
			speedBuff.name = "TKSATEngiSpeedispenserBuff";
			speedBuff.iconSprite = buffIcon;

			tmpDeployableBody.GetComponent<CameraTargetParams>().cameraParams = ctp;
			tmpDeployableBody.GetComponent<ModelLocator>().modelTransform.GetComponent<MeshRenderer>().material = mainMtl;
			var eic = captainSupply.transform.Find("ModelBase/captain supply drop/EnergyIndicatorContainer");
			eic.SetParent(tmpDeployableBody.transform);
			eic.localScale *= 0.5f;
			eic.position -= new Vector3(0f, 1.5f, 0f);
			var cl = tmpDeployableBody.GetComponent<ModelLocator>().modelTransform.GetComponent<ChildLocator>();
			cl.transformPairs[0].transform = eic;
			cl.transformPairs[1].transform = eic.Find("Canvas/EnergyIndicatorBackgroundPanel/EnergyIndicator");

			GameObject.Destroy(captainSupply);
			deployableBody = tmpDeployableBody.InstantiateClone("EngiSpeedispenserBody", true);
			GameObject.Destroy(tmpDeployableBody);
			deployableMaster.GetComponent<CharacterMaster>().bodyPrefab = deployableBody;

			deployableBlueprint.transform.Find("BlobLight").GetComponent<Projector>().material = bpProjMtl;
			deployableBlueprint.transform.Find("Model Base/SpeedispenserModel").GetComponent<MeshRenderer>().material = bpOkMtl;
			var bp = deployableBlueprint.GetComponent<BlueprintController>();
			bp.okMaterial = bpOkMtl;
			bp.invalidMaterial = bpNokMtl;

			foreach(var akEvent in turretBp.GetComponents<AkEvent>()) {
				var newEvent = deployableBlueprint.AddComponent<AkEvent>();
				newEvent.triggerList = akEvent.triggerList.ToArray().ToList();
				newEvent.useOtherObject = akEvent.useOtherObject;
				newEvent.actionOnEventType = akEvent.actionOnEventType;
				newEvent.curveInterpolation = akEvent.curveInterpolation;
				newEvent.enableActionOnEvent = akEvent.enableActionOnEvent;
				newEvent.data = akEvent.data;
				newEvent.useCallbacks = akEvent.useCallbacks;
				newEvent.Callbacks = akEvent.Callbacks.ToArray().ToList();
				newEvent.playingId = akEvent.playingId;
				newEvent.soundEmitterObject = deployableBlueprint;
				newEvent.transitionDuration = akEvent.transitionDuration;
			}

			foreach(var akEvent in turretObj.GetComponents<AkEvent>()) {
				var newEvent = deployableBody.AddComponent<AkEvent>();
				newEvent.triggerList = akEvent.triggerList.ToArray().ToList();
				newEvent.useOtherObject = akEvent.useOtherObject;
				newEvent.actionOnEventType = akEvent.actionOnEventType;
				newEvent.curveInterpolation = akEvent.curveInterpolation;
				newEvent.enableActionOnEvent = akEvent.enableActionOnEvent;
				newEvent.data = akEvent.data;
				newEvent.useCallbacks = akEvent.useCallbacks;
				newEvent.Callbacks = akEvent.Callbacks.ToArray().ToList();
				newEvent.playingId = akEvent.playingId;
				newEvent.soundEmitterObject = deployableBody;
				newEvent.transitionDuration = akEvent.transitionDuration;
			}

			//R2API catalog reg

			var dmsSerializable = ContentAddition.AddEntityState<DispenserMainState>(out bool entStateDidSucceed);

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on EngiUtilitySpeedispenser (DispenserMainState)! Deployable will be unusable.");
			} else {
				var esm = deployableBody.GetComponent<EntityStateMachine>();
				esm.initialStateType = dmsSerializable;
				esm.mainStateType = dmsSerializable;
			}

			R2API.Networking.NetworkingAPI.RegisterMessageType<PlaceDispenser.MsgConstructDispenser>();
			ContentAddition.AddBuffDef(speedBuff);
			ContentAddition.AddBody(deployableBody);
			ContentAddition.AddMaster(deployableMaster);
			deployableSlot = DeployableAPI.RegisterDeployableSlot((master, countMult) => {
				return 1;
			});

			skillDef.activationState = ContentAddition.AddEntityState<PlaceDispenser>(out entStateDidSucceed);

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on EngiUtilitySpeedispenser! Skill will not appear nor function.");
			} else if(!ContentAddition.AddSkillDef(skillDef)) {
				TinkersSatchelPlugin._logger.LogError("SkillDef setup failed on EngiUtilitySpeedispenser! Skill will not appear nor function.");
			} else {
				setupSucceeded = true;
			}
		}

		public override void Install() {
            base.Install();

			if(setupSucceeded) {
				targetSkillFamily.AddVariant(skillDef);
				RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
			}
        }

        public override void Uninstall() {
            base.Uninstall();

			if(setupSucceeded) {
				targetSkillFamily.RemoveVariant(skillDef);
				RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
			}
		}



		////// Hooks //////

		private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
			if(!sender) return;
			if(sender.HasBuff(speedBuff) && sender.isSprinting) {
				args.moveSpeedMultAdd += 0.5f;
				args.jumpPowerMultAdd += 0.25f;
			}
		}



		////// Skill States //////

		public class PlaceDispenser : EntityStates.Engi.EngiWeapon.PlaceTurret {
			public override void OnEnter() {
				this.wristDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiTurretWristDisplay.prefab").WaitForCompletion();
				this.blueprintPrefab = EngiUtilitySpeedispenser.instance.deployableBlueprint;
				this.turretMasterPrefab = EngiUtilitySpeedispenser.instance.deployableMaster;
				base.OnEnter();
			}
            public override void FixedUpdate() {
				fixedAge += Time.fixedDeltaTime;
				if(!isAuthority) return;
				entryCountdown -= Time.fixedDeltaTime;
				if(exitPending) {
					exitCountdown -= Time.fixedDeltaTime;
					if(exitCountdown <= 0f) outer.SetNextStateToMain();
					return;
				}
				if(inputBank && entryCountdown <= 0f) {
					var deploy = (inputBank.skill1.down || inputBank.skill4.justPressed) && currentPlacementInfo.ok;
					var cancel = inputBank.skill2.justPressed;
					if(deploy || cancel) {
						if(characterBody && !cancel) {
							new MsgConstructDispenser(characterBody, currentPlacementInfo.position, currentPlacementInfo.rotation, MasterCatalog.FindMasterIndex(turretMasterPrefab))
								.Send(R2API.Networking.NetworkDestination.Server);
							if(base.skillLocator) {
								var skill = skillLocator.GetSkill(SkillSlot.Utility);
								if(skill)
									skill.DeductStock(1);
							}
							Util.PlaySound(placeSoundString, gameObject);
						}
						DestroyBlueprints();
						exitPending = true;
					}
				}
			}
            public struct MsgConstructDispenser : INetMessage {
				CharacterBody _body;
				Vector3 _pos;
				Quaternion _rot;
				MasterCatalog.MasterIndex _masterIndex;

                public MsgConstructDispenser(CharacterBody body, Vector3 pos, Quaternion rot, MasterCatalog.MasterIndex masterIndex) {
                    _body = body;
                    _pos = pos;
                    _rot = rot;
                    _masterIndex = masterIndex;
                }

                public void Deserialize(NetworkReader reader) {
					var obj = reader.ReadGameObject();
					if(obj) _body = obj.GetComponent<CharacterBody>();
					_pos = reader.ReadVector3();
					_rot = reader.ReadQuaternion();
					_masterIndex = (MasterCatalog.MasterIndex)reader.ReadInt32();
                }

                public void Serialize(NetworkWriter writer) {
					writer.Write(_body.gameObject);
					writer.Write(_pos);
					writer.Write(_rot);
					writer.Write((int)_masterIndex);
				}

				public void OnReceived() {
					if(_body && _body.master) {
						var dispMaster = new MasterSummon {
							masterPrefab = MasterCatalog.GetMasterPrefab(_masterIndex),
							position = _pos,
							rotation = _rot,
							summonerBodyObject = _body.gameObject,
							ignoreTeamMemberLimit = true,
							inventoryToCopy = _body.inventory
						}.Perform();
						Deployable deployable = dispMaster.gameObject.AddComponent<Deployable>();
						deployable.onUndeploy = new UnityEvent();
						deployable.onUndeploy.AddListener(new UnityAction(dispMaster.TrueKill));
						_body.master.AddDeployable(deployable, EngiUtilitySpeedispenser.instance.deployableSlot);
					}
				}
			}
        }

		public class DispenserMainState : EntityStates.CaptainSupplyDrop.BaseMainState {
			public override bool shouldShowEnergy => true;

            public override string GetContextString(Interactor activator) {
				if(GetInteractability(activator) == Interactability.ConditionsNotMet)
					if(energyComponent.energy < 1f)
						return Language.GetString("TKSAT_ENGI_SPEEDISPENSER_CONTEXT_CHARGING");
					else
						return Language.GetString("TKSAT_ENGI_SPEEDISPENSER_CONTEXT_ALREADY_HAVE");
				return Language.GetString("TKSAT_ENGI_SPEEDISPENSER_CONTEXT");
			}

            public override Interactability GetInteractability(Interactor activator) {
				var acbody = activator.GetComponent<CharacterBody>();
				if(!acbody) return Interactability.Disabled;
				if(energyComponent.energy < 1f || acbody.HasBuff(EngiUtilitySpeedispenser.instance.speedBuff))
					return Interactability.ConditionsNotMet;
				return Interactability.Available;
			}

			public override void OnInteractionBegin(Interactor activator) {
				var acbody = activator.GetComponent<CharacterBody>();
				if(!acbody) return;
				energyComponent.TakeEnergy(1f);
				acbody.AddTimedBuff(EngiUtilitySpeedispenser.instance.speedBuff, 15f);
			}
		}
	}
}