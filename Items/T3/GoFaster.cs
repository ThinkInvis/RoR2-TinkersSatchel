using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;
using RoR2.Projectile;
using System.Linq;

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
		public float banditCloakSpeedFrac { get; private set; } = 20f;

		[AutoConfig("Multiplier to BuffFrac for MUL-T dash: multiplies move speed during reactivation boosts (1 per skill use per stack).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float toolbotReacBoostFrac { get; private set; } = 0.5f;

		[AutoConfig("MUL-T dash: directly specify duration of each boost. Does not stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float toolbotReacTime { get; private set; } = 1f;

		[AutoConfig("Multiplier to BuffFrac for Engineer shield: shield provides a stacking, 5-second speed buff at a rate increased by item stacks.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float engiSharedBuffFrac { get; private set; } = 0.35f;

		[AutoConfig("Multiplier to BuffFrac for Engineer missiles: controls launch force per consumed missile.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float engiBoostFrac { get; private set; } = 35f;

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

		//todo: IDR as overlay on character model?



		////// Other Fields/Properties //////

		BuffDef engiSpeedBoostBuff;



		////// TILER2 Module Setup //////

		public GoFaster() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/GoFaster.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/goFasterIcon.png");
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
		}



		////// Hooks //////

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
				self.characterBody.characterMotor.ApplyForce(mainSpeed * massAdjust * dir, false, false);
			}
		}

		private void ToolbotDash_OnEnter(On.EntityStates.Toolbot.ToolbotDash.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDash self) {
			orig(self);
			var count = GetCount(self.characterBody);
			if(count > 0) {
				var cpt = self.GetComponent<ToolbotDashBoostTracker>();
				if(!cpt) cpt = self.gameObject.AddComponent<ToolbotDashBoostTracker>();
				cpt.maxBoosts = count;
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
				self.characterBody.characterMotor.ApplyForce(mainSpeed * massAdjust * dir, false, false);
				self.fireIndex++;
				var muzzle = (self.fireIndex % 2 == 0) ? "MuzzleLeft" : "MuzzleRight";
				var pos = self.inputBank.aimOrigin;
				var tsf = self.FindModelChild(muzzle);
				if(tsf != null) pos = tsf.position;
				EffectManager.SimpleMuzzleFlash(EntityStates.Engi.EngiMissilePainter.Fire.muzzleflashEffectPrefab, self.gameObject, muzzle, true);
				self.PlayAnimation((self.fireIndex % 2 == 0) ? "Gesture Left Cannon, Additive" : "Gesture Right Cannon, Additive", "FireHarpoon");

				ProjectileManager.instance.FireProjectile(new FireProjectileInfo {
					projectilePrefab = EntityStates.Engi.EngiMissilePainter.Fire.projectilePrefab,
					position = self.characterBody.footPosition,
					rotation = Util.QuaternionSafeLookRotation(Vector3.up),
					procChainMask = default,
					target = self.gameObject,
					owner = self.characterBody.gameObject,
					damage = 0,
					crit = false,
					force = mainSpeed * massAdjust,
					damageColorIndex = DamageColorIndex.Item
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
}