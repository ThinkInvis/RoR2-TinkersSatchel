using RoR2;
using UnityEngine;
using TILER2;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates;
using RoR2.Skills;

namespace ThinkInvisible.TinkersSatchel {
	public class CommandoUtilityJinkJet : T2Module<CommandoUtilityJinkJet> {

		////// Module Data //////

		public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferUntilEndGame | AutoConfigFlags.PreventNetMismatch;



		////// Config //////

		[AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
		[AutoConfig("Multiplier to GoFaster BuffFrac for this skill: multiplies launch speed.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float goFasterBuffFrac { get; private set; } = 0.5f;



		////// Other Fields/Properties //////

		public SkillDef skillDef { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public CommandoUtilityJinkJet() {
		}

		public override void RefreshPermanentLanguage() {
			permanentGenericLanguageTokens.Add("TKSAT_COMMANDO_UTILITY_JINKJET_NAME", "Jink Jet");
			permanentGenericLanguageTokens.Add("TKSAT_COMMANDO_UTILITY_JINKJET_DESCRIPTION", "Perform a small jet-assisted horizontal jump in your <style=cIsUtility>aim direction</style>. Hold up to 3.");
			base.RefreshPermanentLanguage();
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			//load custom assets
			skillDef = TinkersSatchelPlugin.resources.LoadAsset<SkillDef>("Assets/TinkersSatchel/SkillDefs/CommandoUtilityJinkJet.asset");

			//load vanilla assets
			targetSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Commando/CommandoBodyUtilityFamily.asset")
				.WaitForCompletion();

			//R2API catalog reg
			skillDef.activationState = ContentAddition.AddEntityState<QuickDodge>(out bool entStateDidSucceed);

			if(!entStateDidSucceed) {
				TinkersSatchelPlugin._logger.LogError("EntityState setup failed on CommandoUtilityJinkJet! Skill will not appear nor function.");
			} else if(!ContentAddition.AddSkillDef(skillDef)) {
				TinkersSatchelPlugin._logger.LogError("SkillDef setup failed on CommandoUtilityJinkJet! Skill will not appear nor function.");
			} else {
				setupSucceeded = true;
			}

			GoFaster.instance.handledSkillDefs.Add(skillDef);
		}

		public override void Install() {
			base.Install();
			if(setupSucceeded) {
				targetSkillFamily.AddVariant(skillDef);
			}
		}

		public override void Uninstall() {
			base.Uninstall();
			if(setupSucceeded) {
				targetSkillFamily.RemoveVariant(skillDef);
			}
		}



		////// Hooks //////



		////// Skill States //////

		public class QuickDodge : BaseState {
			public override void OnEnter() {
				base.OnEnter();
				Util.PlaySound("Play_commando_M2_grenade_explo", gameObject);
				var animator = GetModelAnimator();
				var cLoc = animator.GetComponent<ChildLocator>();
				var skillForward = GetAimRay().direction;
				if(characterMotor && characterMotor.isGrounded)
					skillForward.y = 0.3f;
				else
					skillForward.y = Mathf.Min(skillForward.y, 0.15f);
				skillForward.Normalize();
				var je = EntityStates.Commando.DodgeState.jetEffect;
				if(je) {
					var tsfL = cLoc.FindChild("LeftJet");
					var tsfR = cLoc.FindChild("RightJet");
					if(tsfL) UnityEngine.Object.Instantiate<GameObject>(je, tsfL);
					if(tsfR) UnityEngine.Object.Instantiate<GameObject>(je, tsfR);
				}
				float sprintMult = 1;
				if(characterBody)
					sprintMult = characterBody.sprintingSpeedMultiplier;
				if(characterMotor) {
					characterMotor.Motor.ForceUnground();
					characterMotor.velocity = Vector3.zero;
					float boost = 1f;
					if(GoFaster.instance.enabled)
						boost += (float)GoFaster.instance.GetCount(characterBody) * GoFaster.instance.buffFrac * CommandoUtilityJinkJet.instance.goFasterBuffFrac;
					characterMotor.velocity = skillForward * moveSpeedStat * sprintMult * 2.35f * boost;
				}
				if(isAuthority)
					outer.SetNextStateToMain();
			}
		}
	}
}