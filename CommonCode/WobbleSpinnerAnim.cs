using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
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
			Vector3 rx = new(
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2),
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2),
				Random.Range(-wobbleEccentricity / 2, wobbleEccentricity / 2)
				);
			this.gameObject.transform.localScale = (rb + rx) * (1f / wobbleStability) + Vector3.one;
		}
	}
}