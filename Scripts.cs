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
}