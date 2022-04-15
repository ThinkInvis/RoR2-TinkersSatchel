using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
	public class Skein : Item<Skein> {

		////// Item Data //////

		public override string displayName => "Spacetime Skein";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility, ItemTag.Damage });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) =>
			"Gain mass while stationary. Lose mass while moving.";
		protected override string GetDescString(string langid = null) =>
			$"Standing still reduces the next <style=cIsDamage>damage and knockback</style> you take by up to <style=cIsDamage>{Pct(highMassFrac)} <style=cStack>(+{Pct(highMassFrac)} per stack, hyperbolic)</style></style>. Moving increasing your <style=cIsUtility>move and attack speed</style> by up to <style=cIsUtility>{Pct(lowMassFrac)} <style=cStack>(+{Pct(lowMassFrac)} per stack, linear)</style></style>. Effect ramps up over {massChangeDuration:N0} seconds, and is lost once you start or stop moving (latter has a brief grace period).";
		protected override string GetLoreString(string langid = null) => "";



		////// Config //////

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Maximum damage/knockback to block per stack (hyperbolic).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float highMassFrac { get; private set; } = 0.5f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
		[AutoConfig("Maximum speed to add per stack (linear).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float lowMassFrac { get; private set; } = 0.5f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Time required to reach maximum buff, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float massChangeDuration { get; private set; } = 5f;

		[AutoConfig("Time required to register a movement stop, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float moveGracePeriod { get; private set; } = 0.25f;



		////// Other Fields/Properties //////
		
		public BuffDef speedBuff { get; private set; }
		public BuffDef resistBuff { get; private set; }
		internal static UnlockableDef unlockable;



		////// TILER2 Module Setup //////

		public Skein() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Skein.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/skeinIcon.png");
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			speedBuff = ScriptableObject.CreateInstance<BuffDef>();
			speedBuff.buffColor = Color.white;
			speedBuff.canStack = true;
			speedBuff.isDebuff = false;
			speedBuff.name = "TKSATSkeinSpeed";
			speedBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/skeinSpeedBuffIcon.png");
			ContentAddition.AddBuffDef(speedBuff);

			resistBuff = ScriptableObject.CreateInstance<BuffDef>();
			resistBuff.buffColor = Color.white;
			resistBuff.canStack = true;
			resistBuff.isDebuff = false;
			resistBuff.name = "TKSATSkeinResist";
			resistBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/skeinResistBuffIcon.png");
			ContentAddition.AddBuffDef(resistBuff);

			unlockable = UnlockableAPI.AddUnlockable<TkSatSkeinAchievement>();
			LanguageAPI.Add("TKSAT_SKEIN_ACHIEVEMENT_NAME", "Phenomenal Cosmic Power");
			LanguageAPI.Add("TKSAT_SKEIN_ACHIEVEMENT_DESCRIPTION", "Complete all 4 Item Set achievements from Tinker's Satchel.");

			itemDef.unlockableDef = unlockable;
		}

		public override void Install() {
			base.Install();
			CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
			RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		}

        public override void Uninstall() {
			base.Uninstall();
			CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
			RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
			On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
		}



		////// Hooks //////
		
		private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
			if(GetCount(body) > 0 && !body.GetComponent<SkeinTracker>())
				body.gameObject.AddComponent<SkeinTracker>();
		}

		private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
			if(!sender) return;
			var count = GetCount(sender);
			var cpt = sender.GetComponent<SkeinTracker>();
			if(count > 0 && cpt) {
				var fac = cpt.GetMovementScalar() * count * lowMassFrac;
				args.moveSpeedMultAdd += fac;
				args.attackSpeedMultAdd += fac;
            }
		}

		private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
			if(self && self.body) {
				var count = GetCount(self.body);
				var cpt = self.GetComponent<SkeinTracker>();
				if(count > 0 && cpt) {
					var fac = 1f - (1f - Mathf.Pow(highMassFrac, count)) * cpt.GetResistanceScalar();
					damageInfo.damage *= fac;
					if(damageInfo.canRejectForce)
						damageInfo.force *= fac;
					cpt.ForceResetStopped();
                }
            }
			orig(self, damageInfo);
		}
	}

	[RequireComponent(typeof(CharacterBody))]
	public class SkeinTracker : MonoBehaviour {
		const float RECALC_TICK_RATE = 0.2f;

		float movingStopwatch = 0f;
		float shortNotMovingStopwatch = 0f;
		float tickStopwatch = 0f;
		bool isStopped = false;

		Vector3 prevPos;

		CharacterBody body;

		public float GetMovementScalar() {
			if(isStopped) return 0;
			return Mathf.Clamp01(movingStopwatch / Skein.instance.massChangeDuration);
        }

		public float GetResistanceScalar() {
			if(!isStopped) return 0;
			return Mathf.Clamp01(shortNotMovingStopwatch / Skein.instance.massChangeDuration);
		}

		public void ForceResetStopped() {
			shortNotMovingStopwatch = 0f;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void Awake() {
			body = GetComponent<CharacterBody>();
			prevPos = body.transform.position;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void FixedUpdate() {
			if(!body || !NetworkServer.active) return;
			float minMove = 0.1f * Time.fixedDeltaTime;
			if((body.transform.position - prevPos).sqrMagnitude <= minMove * minMove) {
				shortNotMovingStopwatch += Time.fixedDeltaTime;
				if(!isStopped) {
					if(shortNotMovingStopwatch > Skein.instance.moveGracePeriod) {
						movingStopwatch = 0f;
						isStopped = true;
						body.statsDirty = true;
						body.SetBuffCount(Skein.instance.speedBuff.buffIndex, 0);
					} else movingStopwatch += Time.fixedDeltaTime;
                }
			} else {
				if(isStopped) {
					body.SetBuffCount(Skein.instance.resistBuff.buffIndex, 0);
					isStopped = false;
				}
				movingStopwatch += Time.fixedDeltaTime;
				shortNotMovingStopwatch = 0f;
			}

			prevPos = body.transform.position;

			tickStopwatch -= Time.fixedDeltaTime;
			if(tickStopwatch <= 0f) {
				tickStopwatch = RECALC_TICK_RATE;
				if(!isStopped) body.statsDirty = true;
				body.SetBuffCount((isStopped ? Skein.instance.resistBuff : Skein.instance.speedBuff).buffIndex,
					Mathf.FloorToInt((isStopped ? GetResistanceScalar() : GetMovementScalar()) * 100));
			}
        }
    }

	public class TkSatSkeinAchievement : RoR2.Achievements.BaseAchievement, IModdedUnlockableDataProvider {
		public string AchievementIdentifier => "TKSAT_SKEIN_ACHIEVEMENT_ID";
		public string UnlockableIdentifier => "TKSAT_SKEIN_UNLOCKABLE_ID";
		public string PrerequisiteUnlockableIdentifier => "";
		public string AchievementNameToken => "TKSAT_SKEIN_ACHIEVEMENT_NAME";
		public string AchievementDescToken => "TKSAT_SKEIN_ACHIEVEMENT_DESCRIPTION";
		public string UnlockableNameToken => Skein.instance.nameToken;

		public Sprite Sprite => TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/skeinIcon.png");

		public System.Func<string> GetHowToUnlock => () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new[] {
			Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

		public System.Func<string> GetUnlocked => () => Language.GetStringFormatted("UNLOCKED_FORMAT", new[] {
			Language.GetString(AchievementNameToken), Language.GetString(AchievementDescToken)});

		public override void OnInstall() {
			base.OnInstall();
            On.RoR2.RoR2Application.Update += RoR2Application_Update;
		}

        public override void OnUninstall() {
			base.OnUninstall();
			On.RoR2.RoR2Application.Update -= RoR2Application_Update;
		}

		float stopwatch = 0f;
		private void RoR2Application_Update(On.RoR2.RoR2Application.orig_Update orig, RoR2Application self) {
			orig(self);
			stopwatch -= Time.deltaTime;
			if(stopwatch <= 0f) {
				stopwatch = 1f;
				if(userProfile.HasUnlockable(Defib.unlockable)
					&& userProfile.HasUnlockable(ShootToHeal.unlockable)
					&& userProfile.HasUnlockable(Pinball.unlockable)
					&& userProfile.HasUnlockable(Lodestone.unlockable))
					Grant();
			}
		}
	}
}