using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SyncTransformSystem {
    public class RootMotionTester : NetworkBehaviour {
        public const float TWO_PI = 2f * Mathf.PI;
        public float freq = 0.1f;
        public float scale = 1f;

    	void Update () {
            if (isServer) {
                var t = freq * Time.timeSinceLevelLoad;
                var angle = TWO_PI * t;
                transform.localPosition = scale * new Vector3 (Mathf.Cos (angle), Mathf.Sin (angle), 0f);
            }
    	}
    }
}
