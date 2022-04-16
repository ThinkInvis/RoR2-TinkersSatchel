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

		public override AutoConfigFlags enabledConfigFlags => AutoConfigFlags.DeferForever | AutoConfigFlags.PreventNetMismatch;



		////// Other Fields/Properties //////

		public SkillDef skillDef { get; private set; }
		bool setupSucceeded = false;
		SkillFamily targetSkillFamily;



		////// TILER2 Module Setup //////

		public CommandoUtilityJinkJet() {
		}

		public override void RefreshPermanentLanguage() {
			permanentGenericLanguageTokens.Add("TKSAT_COMMANDO_UTILITY_JINKJET_NAME", "Jink Jet");
			permanentGenericLanguageTokens.Add("TKSAT_COMMANDO_UTILITY_JINKJET_DESCRIPTION", "Perform a very short dodge in your <style=cIsUtility>aim direction</style>. Hold up to 3 charges.");
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
				ChildLocator component = animator.GetComponent<ChildLocator>();
				var skillForward = GetAimRay().direction.normalized;
				var animForward = characterDirection ? characterDirection.forward : skillForward;
				var forwardAdjust = Vector3.Dot(skillForward, animForward);
				var rightAdjust = Vector3.Dot(skillForward, Vector3.Cross(Vector3.up, animForward));
				animator.SetFloat("forwardSpeed", forwardAdjust, 0.1f, Time.fixedDeltaTime);
				animator.SetFloat("rightSpeed", rightAdjust, 0.1f, Time.fixedDeltaTime);
				if(Mathf.Abs(forwardAdjust) > Mathf.Abs(rightAdjust))
					PlayAnimation("Body", (forwardAdjust > 0f) ? "DodgeForward" : "DodgeBackward", "Dodge.playbackRate", duration);
				else
					PlayAnimation("Body", (rightAdjust > 0f) ? "DodgeRight" : "DodgeLeft", "Dodge.playbackRate", duration);
				var je = EntityStates.Commando.DodgeState.jetEffect;
				if(je) {
					var tsfL = component.FindChild("LeftJet");
					var tsfR = component.FindChild("RightJet");
					if(tsfL) UnityEngine.Object.Instantiate<GameObject>(je, tsfL);
					if(tsfR) UnityEngine.Object.Instantiate<GameObject>(je, tsfR);
				}
				float sprintMult = 1;
				if(characterBody)
					sprintMult = characterBody.sprintingSpeedMultiplier;
				if(characterMotor)
					characterMotor.velocity = skillForward * moveSpeedStat * sprintMult * 3;
				if(isAuthority)
					outer.SetNextStateToMain();
			}

			[SerializeField]
			public float duration = 0.15f;
		}
	}
}