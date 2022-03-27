using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
	public class TargetSpinnerAnim : MonoBehaviour {
		public float rotateTime = 0.5f;
		public float delayTime = 1f;
		public Vector3 rotateAxis;

		private float targPos = -1f;
		private float currVel;
		private float currPos;
		private float stopwatch;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void Update() {
			stopwatch -= Time.deltaTime;
			if(stopwatch < 0f) {
				targPos = Random.value * Mathf.PI * 2;
				stopwatch = rotateTime + delayTime;
			}
			currPos = Mathf.SmoothDampAngle(currPos, targPos, ref currVel, rotateTime);
			this.gameObject.transform.Rotate(rotateAxis, currVel);
		}
	}

	public class WobbleSpinnerAnim : MonoBehaviour {
		public float rotateSpeed = 0.5f;
		public float wobbleBaseMin = -0.2f;
		public float wobbleBaseMax = 0.2f;
		public float wobbleEccentricity = 0.15f;
		public float wobbleStability = 2f;
		public Vector3 rotateAxis;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
		void Update() {
			this.gameObject.transform.Rotate(rotateAxis, rotateSpeed);
			Vector3 rb = Vector3.one * Random.Range(wobbleBaseMin, wobbleBaseMax);
			Vector3 rx = new Vector3(
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2),
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2),
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2)
				);
			this.gameObject.transform.localScale = (rb + rx) * (1f / wobbleStability) + Vector3.one;
		}
	}

	public class PixieFuseFlicker : MonoBehaviour {
		[SerializeField]
		public GameObject[] targets;
		public float switchIntervalMin;
		public float switchIntervalMax;
		public float flickerInterval;
		public int flickersMin;
		public int flickersMax;

		private float stopwatch = 0f;
		private int flickerCount = 0;
		private int currIndex = 0;

		void Update() {
			stopwatch -= Time.deltaTime;
			if(stopwatch < 0f) {
				if(flickerCount <= 0) {
					currIndex = Random.Range(0, targets.Length);
					flickerCount = Random.Range(flickersMin, flickersMax + 1) * 2;
					for(var i = 0; i < targets.Length; i++) {
						targets[i].SetActive(currIndex == i);
					}
				}

				stopwatch = (flickerCount > 1 ? flickerInterval : Random.Range(switchIntervalMin, switchIntervalMax));
				flickerCount--;
				targets[currIndex].SetActive(!targets[currIndex].activeSelf);
			}
		}
	}
}