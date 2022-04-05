using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;
using RoR2.Projectile;

namespace ThinkInvisible.TinkersSatchel {
	public class GoFaster : Item<GoFaster> {

		////// Item Data //////

		public override string displayName => "Go-Faster Stripes";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) =>
			"Your Utility skill gains more mobility.";
		protected override string GetDescString(string langid = null) =>
			$"Upgrades your <style=cIsUtility>Utility skill</style>, greatly increasing its <style=cIsUtility>mobility</style>.";
		protected override string GetLoreString(string langid = null) => "<style=cMono>//--AUTO-TRANSCRIPTION FROM UES [Redacted] --//</style>\n\n\"...What are you doing?\"\n\n\"Painting my armor.\"\n\n\"Why!?\"\n\n\"To make it go faster.\"\n\n\"What!? That\u2019s just a myth! And-- and it\u2019s supposed to be for cars! And in case you haven\u2019t noticed, we are -in the middle of a warzone-! We don\u2019t have -time- for--\"\n\n\"Red stripes make things go faster. More speed means more time. You\u2019ll see.\"";



		////// Config //////

		[AutoConfig("Controls general power of this item (multiplies all other Frac configs). newSpeed ~ baseSpeed * (1 + buffFrac * specificFrac * stack count).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float buffFrac { get; private set; } = 1f;

		[AutoConfig("Multiplier to BuffFrac for Commando dodge: multiplies movement speed during dodge.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float commandoDodgeFrac { get; private set; } = 0.5f;

		[AutoConfig("Multiplier to BuffFrac for Commando slide: multiplies movement speed during slide.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float commandoSlideFrac { get; private set; } = 0.5f;

		[AutoConfig("Multiplier to BuffFrac for both Huntress blink variants: divides time spent in blink animation.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float huntressBlinkTimeFac { get; private set; } = 0.5f;

		[AutoConfig("Multiplier to BuffFrac for Huntress blink: multiplies distance travelled.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float huntressBlinkRangeFac { get; private set; } = 0.25f;

		[AutoConfig("Multiplier to BuffFrac for Huntress mini-blink: multiplies distance travelled.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float huntressBlink2RangeFac { get; private set; } = 0.35f;

		[AutoConfig("Multiplier to BuffFrac for Bandit smokebomb: controls launch force.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float banditCloakSpeedFrac { get; private set; } = 40f;

		[AutoConfig("Multiplier to BuffFrac for MUL-T dash: multiplies move speed during reactivation boosts.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float toolbotReacBoostFrac { get; private set; } = 0.5f;

		[AutoConfig("MUL-T dash: directly specify number of reactivations per cast. Stacks.", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
		public int toolbotReacBoostCount { get; private set; } = 3;

		[AutoConfig("MUL-T dash: directly specify duration of each boost. Does not stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float toolbotReacTime { get; private set; } = 1f;

		[AutoConfig("Multiplier to BuffFrac for Engineer shield: shield provides a stacking, 5-second speed buff at a rate increased by item stacks.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float engiSharedBuffFrac { get; private set; } = 0.35f;

		[AutoConfig("Multiplier to BuffFrac for Engineer missiles: controls launch force per consumed missile.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float engiBoostFrac { get; private set; } = 50f;

		/*[AutoConfig("Multiplier to BuffFrac for Artificer wall: NYI. Current effect is teleport to top of wall, which cannot be meaningfully boosted.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float artiSpeedPadFrac { get; private set; } = 20f;*/

		[AutoConfig("Multiplier to BuffFrac for Mercenary Blinding Assault: multiplies movement speed during dash.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float mercDashFrac { get; private set; } = 0.5f;

		[AutoConfig("Multiplier to BuffFrac for Mercenary Focused Assault: multiplies movement speed during dash.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float mercDash2Frac { get; private set; } = 0.5f;

		[AutoConfig("Multiplier to BuffFrac for REX DIRECTIVE: Disperse: multiplies launch force.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float treebotSonicBoomFrac { get; private set; } = 1f;

		[AutoConfig("Multiplier to BuffFrac for REX Bramble Volley: multiplies launch force.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float treebotSonicBoom2Frac { get; private set; } = 1f;

		[AutoConfig("Multiplier to BuffFrac for Loader Charged Gauntlet: multiplies lunge velocity.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float loaderChargeFistFrac { get; private set; } = 0.5f;

		[AutoConfig("Multiplier to BuffFrac for Loader Thunder Gauntlet: multiplies lunge velocity.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float loaderChargeFist2Frac { get; private set; } = 0.5f;

		[AutoConfig("Multiplier to BuffFrac for Acrid Caustic Leap: multiplies jump velocity.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float crocoLeapFrac { get; private set; } = 0.5f;

		[AutoConfig("Multiplier to BuffFrac for Acrid Frenzied Leap: multiplies jump velocity.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float crocoLeap2Frac { get; private set; } = 0.5f;

		[AutoConfig("Multiplier to BuffFrac for Captain Airstrike: controls launch force of projectile (NOT adjusted for mass!).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float captainAirstrikeFrac { get; private set; } = 6000f;

		[AutoConfig("Multiplier to BuffFrac for Captain Nuke: controls launch velocity of projectile at 1 stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float captainAirstrikeAltFracBase { get; private set; } = 80f;

		[AutoConfig("Multiplier to BuffFrac for Captain Nuke: controls launch velocity of projectile per additional stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float captainAirstrikeAltFracStack { get; private set; } = 20f;

		//todo: IDR as overlay on character model?



		////// Other Fields/Properties //////

		BuffDef engiSpeedBoostBuff;
		GameObject captainStrikeJumperAltProjectile;
		UnlockableDef unlockable;



		////// TILER2 Module Setup //////

		public GoFaster() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/GoFaster.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/goFasterIcon.png");
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			engiSpeedBoostBuff = ScriptableObject.CreateInstance<BuffDef>();
			engiSpeedBoostBuff.buffColor = Color.green;
			engiSpeedBoostBuff.canStack = true;
			engiSpeedBoostBuff.isDebuff = false;
			engiSpeedBoostBuff.name = modInfo.shortIdentifier + "GoFasterEngi";
			engiSpeedBoostBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texMovespeedBuffIcon.tif")
				.WaitForCompletion();
			ContentAddition.AddBuffDef(engiSpeedBoostBuff);

			var tmpPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainAirstrikeAltProjectile.prefab")
				.WaitForCompletion()
				.InstantiateClone("TkSatTempSetupPrefab", false);

			/*var ghost = tmpPrefab.GetComponent<ProjectileController>().ghostPrefab.InstantiateClone("TkSatTempSetupPrefab2", false);

			ghost.AddComponent<SwooceTrajectoryPredictor>();

			tmpPrefab.GetComponent<ProjectileController>().ghostPrefab = ghost.InstantiateClone("TkSatCaptainStrikeJumperAltGhost", false);
			GameObject.Destroy(ghost);*/

			tmpPrefab.AddComponent<SwooceTrajectoryPredictor>();

			captainStrikeJumperAltProjectile = tmpPrefab.InstantiateClone("TkSatCaptainStrikeJumperAltProjectile", true);

			GameObject.Destroy(tmpPrefab);

			unlockable = UnlockableAPI.AddUnlockable<TkSatGoFasterAchievement>();
			LanguageAPI.Add("TKSAT_GOFASTER_ACHIEVEMENT_NAME", "Faster Than Recommended");
			LanguageAPI.Add("TKSAT_GOFASTER_ACHIEVEMENT_DESCRIPTION", "Trimp.");

			itemDef.unlockableDef = unlockable;
		}

		public override void Install() {
			base.Install();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.EntityStates.Commando.DodgeState.RecalculateRollSpeed += DodgeState_RecalculateRollSpeed;
            On.EntityStates.Commando.SlideState.FixedUpdate += SlideState_FixedUpdate;
            On.EntityStates.Huntress.BlinkState.OnEnter += BlinkState_OnEnter;
            On.EntityStates.Bandit2.StealthMode.OnEnter += StealthMode_OnEnter;
            On.EntityStates.Toolbot.ToolbotDash.OnEnter += ToolbotDash_OnEnter;
            On.EntityStates.Toolbot.ToolbotDash.FixedUpdate += ToolbotDash_FixedUpdate;
            On.EntityStates.Toolbot.ToolbotDash.GetIdealVelocity += ToolbotDash_GetIdealVelocity;
            On.EntityStates.Engi.EngiMissilePainter.Fire.OnEnter += Fire_OnEnter;
            On.EntityStates.Engi.EngiBubbleShield.Deployed.OnEnter += Deployed_OnEnter;
            On.EntityStates.Mage.Weapon.PrepWall.OnExit += PrepWall_OnExit;
            On.EntityStates.Merc.Assaulter2.OnEnter += Assaulter2_OnEnter;
            On.EntityStates.Merc.FocusedAssaultDash.OnEnter += FocusedAssaultDash_OnEnter;
            On.EntityStates.Treebot.Weapon.FireSonicBoom.OnEnter += FireSonicBoom_OnEnter;
            On.EntityStates.Loader.BaseSwingChargedFist.OnEnter += BaseSwingChargedFist_OnEnter;
            On.EntityStates.Croco.BaseLeap.OnEnter += BaseLeap_OnEnter;
            On.RoR2.Projectile.ProjectileExplosion.DetonateServer += ProjectileExplosion_DetonateServer;
            On.EntityStates.Captain.Weapon.CallAirstrikeAlt.ModifyProjectile += CallAirstrikeAlt_ModifyProjectile;
		}

        public override void Uninstall() {
			base.Uninstall();
			RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
			On.EntityStates.Commando.DodgeState.RecalculateRollSpeed -= DodgeState_RecalculateRollSpeed;
			On.EntityStates.Commando.SlideState.FixedUpdate -= SlideState_FixedUpdate;
			On.EntityStates.Huntress.BlinkState.OnEnter -= BlinkState_OnEnter;
			On.EntityStates.Bandit2.StealthMode.OnEnter -= StealthMode_OnEnter;
			On.EntityStates.Toolbot.ToolbotDash.OnEnter -= ToolbotDash_OnEnter;
			On.EntityStates.Toolbot.ToolbotDash.FixedUpdate -= ToolbotDash_FixedUpdate;
			On.EntityStates.Toolbot.ToolbotDash.GetIdealVelocity -= ToolbotDash_GetIdealVelocity;
			On.EntityStates.Engi.EngiMissilePainter.Fire.OnEnter -= Fire_OnEnter;
			On.EntityStates.Engi.EngiBubbleShield.Deployed.OnEnter -= Deployed_OnEnter;
			On.EntityStates.Mage.Weapon.PrepWall.OnExit -= PrepWall_OnExit;
			On.EntityStates.Merc.Assaulter2.OnEnter -= Assaulter2_OnEnter;
			On.EntityStates.Merc.FocusedAssaultDash.OnEnter -= FocusedAssaultDash_OnEnter;
			On.EntityStates.Treebot.Weapon.FireSonicBoom.OnEnter -= FireSonicBoom_OnEnter;
			On.EntityStates.Loader.BaseSwingChargedFist.OnEnter -= BaseSwingChargedFist_OnEnter;
			On.EntityStates.Croco.BaseLeap.OnEnter -= BaseLeap_OnEnter;
			On.RoR2.Projectile.ProjectileExplosion.DetonateServer -= ProjectileExplosion_DetonateServer;
			On.EntityStates.Captain.Weapon.CallAirstrikeAlt.ModifyProjectile -= CallAirstrikeAlt_ModifyProjectile;
		}



        ////// Hooks //////
        #region Hooks
        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
			if(!sender) return;
			args.moveSpeedMultAdd += sender.GetBuffCount(engiSpeedBoostBuff) * engiSharedBuffFrac * buffFrac;
		}

		private void DodgeState_RecalculateRollSpeed(On.EntityStates.Commando.DodgeState.orig_RecalculateRollSpeed orig, EntityStates.Commando.DodgeState self) {
			orig(self);
			var count = GetCount(self.characterBody);
			if(count > 0) {
				self.rollSpeed *= 1f + buffFrac * commandoDodgeFrac * count;
            }
		}

		private void SlideState_FixedUpdate(On.EntityStates.Commando.SlideState.orig_FixedUpdate orig, EntityStates.Commando.SlideState self) {
			var count = GetCount(self.characterBody);
			var origMoveSpeed = self.moveSpeedStat;
			if(count > 0) self.moveSpeedStat *= 1f + buffFrac * commandoSlideFrac * count;
			orig(self);
			self.moveSpeedStat = origMoveSpeed;
		}

		private void BlinkState_OnEnter(On.EntityStates.Huntress.BlinkState.orig_OnEnter orig, EntityStates.Huntress.BlinkState self) {
			orig(self);
			var count = GetCount(self.characterBody);
			if(count > 0) {
				self.duration /= 1f + buffFrac * huntressBlinkTimeFac * count;
				self.speedCoefficient *= (1f + buffFrac * (self is EntityStates.Huntress.MiniBlinkState ? huntressBlink2RangeFac : huntressBlinkRangeFac) * count) * (1f + buffFrac * huntressBlinkTimeFac * count);
			}
		}

		private void StealthMode_OnEnter(On.EntityStates.Bandit2.StealthMode.orig_OnEnter orig, EntityStates.Bandit2.StealthMode self) {
			orig(self);
			var count = GetCount(self.characterBody);
			if(count > 0 && self.isAuthority && self.characterBody.characterMotor) {
				var mainSpeed = count * buffFrac * banditCloakSpeedFrac;
				var massAdjust = (self.characterBody.characterMotor ? self.characterBody.characterMotor.mass : 1f);
				Vector3 dir = self.characterBody.transform.forward;
				if(self.characterBody.characterDirection)
					dir = self.characterBody.characterDirection.forward;
				dir.y += 0.75f;
				dir = dir.normalized;
				var yvelCancel = self.characterBody.characterMotor
					? new Vector3(0f, -self.characterBody.characterMotor.velocity.y, 0f)
					: Vector3.zero;
				self.characterBody.characterMotor.ApplyForce(mainSpeed * massAdjust * dir + yvelCancel * massAdjust, false, false);
			}
		}

		private void ToolbotDash_OnEnter(On.EntityStates.Toolbot.ToolbotDash.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDash self) {
			orig(self);
			var count = GetCount(self.characterBody);
			if(count > 0) {
				var cpt = self.GetComponent<ToolbotDashBoostTracker>();
				if(!cpt) cpt = self.gameObject.AddComponent<ToolbotDashBoostTracker>();
				cpt.maxBoosts = count * toolbotReacBoostCount;
				cpt.boostsUsed = 0;
			}
		}

		private void ToolbotDash_FixedUpdate(On.EntityStates.Toolbot.ToolbotDash.orig_FixedUpdate orig, EntityStates.Toolbot.ToolbotDash self) {
			orig(self);
			var count = GetCount(self.characterBody);
			//todo: add contextual skill override
			if(count > 0) {
				var cpt = self.GetComponent<ToolbotDashBoostTracker>();
				if(!cpt) return;
				var isDown = self.hasInputBank && self.inputBank.skill3.down;
				if(!isDown)
					cpt.retrigProtection = false;
				if(!cpt.boosting && isDown && cpt.TryBoost()) {
					self.duration += toolbotReacTime;
					Util.PlaySound(EntityStates.Toolbot.ToolbotDash.endSoundString, self.gameObject);
				}
            }
		}

		private Vector3 ToolbotDash_GetIdealVelocity(On.EntityStates.Toolbot.ToolbotDash.orig_GetIdealVelocity orig, EntityStates.Toolbot.ToolbotDash self) {
			var retv = orig(self);
			var cpt = self.GetComponent<ToolbotDashBoostTracker>();
			if(cpt && cpt.boosting)
				retv *= 1f + buffFrac * toolbotReacBoostFrac * cpt.maxBoosts;
			return retv;
		}

		private void Deployed_OnEnter(On.EntityStates.Engi.EngiBubbleShield.Deployed.orig_OnEnter orig, EntityStates.Engi.EngiBubbleShield.Deployed self) {
			orig(self);
			var depl = self.GetComponent<Deployable>();
			if(!depl) return;
			var count = GetCount(depl.ownerMaster);
			if(count > 0) {
				var bw = self.gameObject.AddComponent<BuffWard>();
				bw.radius = 10f;
				bw.buffDuration = 5f;
				bw.buffDef = engiSpeedBoostBuff;
				bw.interval = 5f / count;
			}
		}

		private void Fire_OnEnter(On.EntityStates.Engi.EngiMissilePainter.Fire.orig_OnEnter orig, EntityStates.Engi.EngiMissilePainter.Fire self) {
			orig(self);
			var count = GetCount(self.characterBody);
			if(count > 0 && self.targetsList.Count == 0) {
				var mainSpeed = count * buffFrac * engiBoostFrac;
				var massAdjust = (self.characterBody.characterMotor ? self.characterBody.characterMotor.mass : 1f);
				Vector3 dir = self.characterBody.transform.forward;
				if(self.characterBody.characterDirection)
					dir = self.characterBody.characterDirection.forward;
				dir.y += 0.75f;
				dir = dir.normalized;
				var yvelCancel = self.characterBody.characterMotor
					? new Vector3(0f, -self.characterBody.characterMotor.velocity.y, 0f)
					: Vector3.zero;
				self.characterBody.characterMotor.ApplyForce(mainSpeed * massAdjust * dir + yvelCancel * massAdjust, false, false);
				self.fireIndex++;
				var muzzle = (self.fireIndex % 2 == 0) ? "MuzzleLeft" : "MuzzleRight";
				var pos = self.inputBank.aimOrigin;
				var tsf = self.FindModelChild(muzzle);
				if(tsf != null) pos = tsf.position;
				EffectManager.SimpleMuzzleFlash(EntityStates.Engi.EngiMissilePainter.Fire.muzzleflashEffectPrefab, self.gameObject, muzzle, true);
				self.PlayAnimation((self.fireIndex % 2 == 0) ? "Gesture Left Cannon, Additive" : "Gesture Right Cannon, Additive", "FireHarpoon");

				ProjectileManager.instance.FireProjectile(new FireProjectileInfo {
					projectilePrefab = EntityStates.Engi.EngiWeapon.FireGrenades.projectilePrefab,
					position = self.characterBody.footPosition,
					rotation = Util.QuaternionSafeLookRotation(Vector3.up),
					procChainMask = default,
					target = self.gameObject,
					owner = self.characterBody.gameObject,
					fuseOverride = 0f,
					useFuseOverride = true,
					_fuseOverride = 0f
				});

				self.activatorSkillSlot.DeductStock(1);
			}
		}

		//TODO: turn wall into tilted launchpad/add slippery speedboost ground instead
		private void PrepWall_OnExit(On.EntityStates.Mage.Weapon.PrepWall.orig_OnExit orig, EntityStates.Mage.Weapon.PrepWall self) {
			Vector3 tpos = Vector3.zero;
			if(self.goodPlacement)
				tpos = self.areaIndicatorInstance.transform.position + Vector3.up * 3f;
			orig(self);
			var count = GetCount(self.characterBody);
			if(count > 0 && self.goodPlacement && self.characterBody.characterMotor) {
				var dvec = tpos - self.characterBody.characterMotor.previousPosition;
				self.characterBody.characterMotor.rootMotion += dvec;
				var blinkEff = new EffectData();
				blinkEff.rotation = Util.QuaternionSafeLookRotation(dvec.normalized);
				blinkEff.origin = Util.GetCorePosition(self.gameObject);
				EffectManager.SpawnEffect(EntityStates.Huntress.BlinkState.blinkPrefab, blinkEff, false);
				var blinkEff2 = new EffectData();
				blinkEff2.rotation = Util.QuaternionSafeLookRotation(dvec.normalized);
				blinkEff2.origin = Util.GetCorePosition(self.gameObject) + dvec;
				EffectManager.SpawnEffect(EntityStates.Huntress.BlinkState.blinkPrefab, blinkEff2, false);
				Util.PlaySound("Play_huntress_shift_start", self.gameObject);
				Util.PlaySound("Play_huntress_shift_end", self.gameObject);
			}
		}

		private void Assaulter2_OnEnter(On.EntityStates.Merc.Assaulter2.orig_OnEnter orig, EntityStates.Merc.Assaulter2 self) {
			orig(self);
			var count = GetCount(self.characterBody);
			if(count > 0) {
				self.moveSpeedStat *= 1f + buffFrac * mercDashFrac * count;
			}
		}

		private void FocusedAssaultDash_OnEnter(On.EntityStates.Merc.FocusedAssaultDash.orig_OnEnter orig, EntityStates.Merc.FocusedAssaultDash self) {
			orig(self);
			var count = GetCount(self.characterBody);
			if(count > 0) {
				self.speedCoefficient *= 1f + buffFrac * mercDash2Frac * count;
			}
		}

		private void FireSonicBoom_OnEnter(On.EntityStates.Treebot.Weapon.FireSonicBoom.orig_OnEnter orig, EntityStates.Treebot.Weapon.FireSonicBoom self) {
			var count = GetCount(self.characterBody);  //nonfunctional!
			if(count > 0) {
				var amt = 1f + buffFrac * (self is EntityStates.Treebot.Weapon.FirePlantSonicBoom ? treebotSonicBoom2Frac : treebotSonicBoomFrac) * count;
				self.airKnockbackDistance *= amt;
				self.groundKnockbackDistance *= amt;
			}
			orig(self);
		}

		private void BaseSwingChargedFist_OnEnter(On.EntityStates.Loader.BaseSwingChargedFist.orig_OnEnter orig, EntityStates.Loader.BaseSwingChargedFist self) {
			var count = GetCount(self.characterBody);
			if(count > 0) {
				var whichFrac = self is EntityStates.Loader.SwingChargedFist ? loaderChargeFistFrac : loaderChargeFist2Frac;
				self.minLungeSpeed *= 1f + buffFrac * whichFrac * count;
				self.maxLungeSpeed *= 1f + buffFrac * whichFrac * count;
			}
			orig(self);
		}

		private void BaseLeap_OnEnter(On.EntityStates.Croco.BaseLeap.orig_OnEnter orig, EntityStates.Croco.BaseLeap self) {
			var count = GetCount(self.characterBody);
			if(count <= 0) {
				orig(self);
				return;
			}
			var origSpeed = self.characterBody.moveSpeed;
			self.characterBody.moveSpeed *= 1f + buffFrac * (self is EntityStates.Croco.Leap ? crocoLeapFrac : crocoLeap2Frac) * count;
			orig(self);
			self.characterBody.moveSpeed = origSpeed;
		}


		private void CallAirstrikeAlt_ModifyProjectile(On.EntityStates.Captain.Weapon.CallAirstrikeAlt.orig_ModifyProjectile orig, EntityStates.Captain.Weapon.CallAirstrikeAlt self, ref FireProjectileInfo fireProjectileInfo) {
			orig(self, ref fireProjectileInfo);
			var count = GetCount(self.characterBody);
			if(count > 0) {
				fireProjectileInfo.projectilePrefab = captainStrikeJumperAltProjectile;
				fireProjectileInfo.force = buffFrac * (captainAirstrikeAltFracBase + (count - 1) * captainAirstrikeAltFracStack);
			}
		}

		private void ProjectileExplosion_DetonateServer(On.RoR2.Projectile.ProjectileExplosion.orig_DetonateServer orig, ProjectileExplosion self) {
			bool isNuke = self.gameObject.name == "TkSatCaptainStrikeJumperAltProjectile(Clone)";
			if(isNuke && self.projectileController.owner) {
				self.bonusBlastForce = Vector3.zero;
				self.projectileDamage.force = 0f;
			}
			orig(self);
			if(!self.projectileController.owner) return;
			var count = GetCount(self.projectileController.owner.gameObject.GetComponent<CharacterBody>());
			if(count <= 0) return;
			if(isNuke) {
				var result = new BlastAttack {
					position = self.transform.position,
					baseDamage = 0,
					baseForce = 0,
					radius = self.blastRadius,
					attacker = self.projectileController.owner.gameObject,
					inflictor = self.gameObject,
					teamIndex = self.projectileController.teamFilter.teamIndex,
					crit = false,
					procChainMask = default,
					procCoefficient = 0f,
					bonusForce = Vector3.zero,
					falloffModel = BlastAttack.FalloffModel.None,
					damageColorIndex = DamageColorIndex.Item,
					damageType = DamageType.BypassBlock | DamageType.NonLethal | DamageType.Silent,
					attackerFiltering = AttackerFiltering.AlwaysHit,
					canRejectForce = true
				}.Fire();
				foreach(var hit in result.hitPoints) {
					if(!hit.hurtBox || !hit.hurtBox.healthComponent.gameObject) continue;
					var go = hit.hurtBox.healthComponent.gameObject;

					var calc = SwooceTrajectoryPredictor.CalculateLaunch(self.transform.position, self.blastRadius, buffFrac * (captainAirstrikeAltFracBase + (count - 1) * captainAirstrikeAltFracStack), go.transform.position, 15f, 90f);

					var motor = go.GetComponent<CharacterMotor>();
					if(motor)
						motor.ApplyForce(calc * motor.mass, true, true);
					var rigid = go.GetComponent<Rigidbody>();
					if(rigid)
						rigid.AddForce(calc, ForceMode.VelocityChange);

					var fdp = go.GetComponent<TemporaryFallDamageProtection>();
					if(!fdp) fdp = go.AddComponent<TemporaryFallDamageProtection>();
					fdp.Apply();
				}
			} else if(self.gameObject.name == "CaptainAirstrikeProjectile1(Clone)") {
				var result = new BlastAttack {
					position = self.transform.position,
					baseDamage = 0f,
					baseForce = count * buffFrac * captainAirstrikeFrac,
					radius = self.blastRadius,
					attacker = self.projectileController.owner.gameObject,
					inflictor = self.gameObject,
					teamIndex = self.projectileController.teamFilter.teamIndex,
					crit = false,
					procChainMask = default,
					procCoefficient = 0f,
					bonusForce = Vector3.zero,
					falloffModel = BlastAttack.FalloffModel.SweetSpot,
					damageColorIndex = DamageColorIndex.Item,
					damageType = DamageType.BypassBlock,
					attackerFiltering = AttackerFiltering.AlwaysHit,
					canRejectForce = false
				}.Fire();
				foreach(var hit in result.hitPoints) {
					if(!hit.hurtBox || !hit.hurtBox.healthComponent.gameObject) continue;
					var go = hit.hurtBox.healthComponent.gameObject;
					var fdp = go.GetComponent<TemporaryFallDamageProtection>();
					if(!fdp) fdp = go.AddComponent<TemporaryFallDamageProtection>();
					fdp.Apply();
				}
			}
		}
		#endregion
	}

	public class TkSatGoFasterAchievement : RoR2.Achievements.BaseAchievement, IModdedUnlockableDataProvider {
		public string AchievementIdentifier => "TKSAT_GOFASTER_ACHIEVEMENT_ID";
		public string UnlockableIdentifier => "TKSAT_GOFASTER_UNLOCKABLE_ID";
		public string PrerequisiteUnlockableIdentifier => "";
		public string AchievementNameToken => "TKSAT_GOFASTER_ACHIEVEMENT_NAME";
		public string AchievementDescToken => "TKSAT_GOFASTER_ACHIEVEMENT_DESCRIPTION";
		public string UnlockableNameToken => GoFaster.instance.nameToken;

		public Sprite Sprite => TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/goFasterIcon.png");

		public System.Func<string> GetHowToUnlock => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new[] {
			Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

		public System.Func<string> GetUnlocked => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new[] {
			Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

		public override void OnInstall() {
			base.OnInstall();
            On.RoR2.CharacterMotor.OnLanded += CharacterMotor_OnLanded;
		}

        public override void OnUninstall() {
			base.OnUninstall();
			On.RoR2.CharacterMotor.OnLanded -= CharacterMotor_OnLanded;
		}

		private void CharacterMotor_OnLanded(On.RoR2.CharacterMotor.orig_OnLanded orig, CharacterMotor self) {
			orig(self);
			if(self.lastVelocity.magnitude > 30f && self.velocity.magnitude > 30f
				&& self.lastVelocity.y < 5f
				&& (self.velocity.y / self.velocity.magnitude) > 0.25f)
				Grant();
		}
	}

	public class ToolbotDashBoostTracker : MonoBehaviour {
		public int maxBoosts = 0;
		public int boostsUsed = 0;
		float boostStopwatch = 0f;
		public bool boosting => boostStopwatch > 0f;
		public bool retrigProtection = true;

		void FixedUpdate() {
			if(boosting)
				boostStopwatch -= Time.fixedDeltaTime;
        }
		public bool TryBoost() {
			if(retrigProtection || boostsUsed >= maxBoosts) return false;
			boostsUsed++;
			boostStopwatch = GoFaster.instance.toolbotReacTime;
			return true;
        }
    }

	[RequireComponent(typeof(ProjectileDamage), typeof(ProjectileImpactExplosion))]
	public class SwooceTrajectoryPredictor : MonoBehaviour {
		float force;
		float radius;
		public float minPitch = 15f;
		public float maxPitch = 90f;

		LineRenderer line;
		void Awake() {
			line = gameObject.AddComponent<LineRenderer>();
			line.material = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<Material>("materials/matBlueprintsOk"));
			//GameObject.Destroy(lineRenMtlSnagFrom);
			line.material.SetColor("_TintColor", new Color(16f, 0.02f, 0.02f, 9f));
			line.positionCount = 100;
			List<Keyframe> kfmArr = new List<Keyframe>();
			for(int i = 0; i < line.positionCount; i++) {
				kfmArr.Add(new Keyframe(i / 100f, (1f - MiscUtil.Wrap(i / 5f, 0f, 1f)) * 0.875f));
			}
			line.widthCurve = new AnimationCurve {
				keys = kfmArr.ToArray()
			};
		}

		void Start() {
			force = GetComponent<ProjectileDamage>().force;
			radius = GetComponent<ProjectileImpactExplosion>().blastRadius;
        }

		void Update() {
			var body = LocalUserManager.GetFirstLocalUser().cachedBodyObject;
			if(!body || (body.transform.position - transform.position).magnitude > radius) {
				line.enabled = false;
				return;
			}
			line.enabled = true;
			line.SetPositions(CalculatePoints(
				body.transform.position,
				CalculateLaunch(transform.position, radius, force, body.transform.position, minPitch, maxPitch),
				100,
				8f));
		}

		public static Vector3 CalculateLaunch(Vector3 blastOrigin, float blastRadius, float force, Vector3 bodyOrigin, float minPitch, float maxPitch) {
			var blastOriginFlat = new Vector3(blastOrigin.x, 0f, blastOrigin.z);
			var bodyOriginFlat = new Vector3(bodyOrigin.x, 0f, bodyOrigin.z);
			var blastTrajectoryFlat = (bodyOriginFlat - blastOriginFlat);
			var launchPitch = Mathf.Lerp(maxPitch, minPitch, blastTrajectoryFlat.magnitude / blastRadius);
			return Vector3.RotateTowards(blastTrajectoryFlat.normalized, Vector3.up, launchPitch * Mathf.PI / 180f, 100f) * force;
        }

		public static Vector3[] CalculatePoints(Vector3 origin, Vector3 velocity, int displayPointsToGenerate, float duration) {
			//calculate points for display
			var generatedPoints = new Vector3[displayPointsToGenerate];
			var timePerPoint = duration / (displayPointsToGenerate - 1f);
			for(int i = 0; i < displayPointsToGenerate; i++) {
				generatedPoints[i] = Trajectory.CalculatePositionAtTime(origin, velocity, timePerPoint * i);
			}

			return generatedPoints;
		}
	}

	[RequireComponent(typeof(CharacterBody))]
	public class TemporaryFallDamageProtection : NetworkBehaviour {
		private CharacterBody attachedBody;
		bool hasProtection = false;
		bool disableNextFrame = false;
		bool disableN2f = false;
		private void FixedUpdate() {
			if(disableN2f) {
				disableN2f = false;
				disableNextFrame = true;
			} else if(disableNextFrame) {
				disableNextFrame = false;
				hasProtection = false;
				attachedBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
			} else if(hasProtection) {
				if(attachedBody.characterMotor.Motor.GroundingStatus.IsStableOnGround && !attachedBody.characterMotor.Motor.LastGroundingStatus.IsStableOnGround) {
					disableN2f = true;
				}
			}
		}
		private void Awake() {
			attachedBody = GetComponent<CharacterBody>();
			attachedBody.characterMotor.onMovementHit += CharacterMotor_onMovementHit;
		}

		public void Apply() {
			hasProtection = true;
			attachedBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
		}

		private void CharacterMotor_onMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo) {
			if(hasProtection && !disableN2f && !disableNextFrame) {
				disableN2f = true;
			}
		}
	}
}