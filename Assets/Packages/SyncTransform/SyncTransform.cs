using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {
    [NetworkSettings(channel=1,sendInterval=0.2f)]
    public class SyncTransform : NetworkBehaviour {
		public const float EPSILON = 1e-5f;

        public float latency = 2f;

        float _nextTransformUpdateTime;
        List<TransformData> _recievedData;

        void Awake() {
            _recievedData = new List<TransformData>();
        }
        public override void OnStartServer () {
            UpdateCurrentTransform ();
        }
        void Update() {
            if (isServer)
                UpdateServer ();
            else if (isClient)
                UpdateClient ();
        }

		#region Server
        void UpdateServer() {
            UpdateCurrentTransform ();
        }
        void UpdateCurrentTransform() {
            var t = Time.timeSinceLevelLoad;
            if (_nextTransformUpdateTime < t) {
                _nextTransformUpdateTime = t + GetNetworkSendInterval ();
				var currentTransform = TransformData.Create (transform, t);
                RpcChangeCurrentTransform (currentTransform);
            }
        }
		#endregion

		#region Client
        void UpdateClient() {
            InterpolrateTransform ();
        }
        bool InterpolrateTransform () {
            var count = _recievedData.Count;
            if (count <= 0)
                return false;

            var tnow = Time.timeSinceLevelLoad;
            var tinterp = -latency * GetNetworkSendInterval () + tnow;
            var d0 = _recievedData [0];
            var d1 = d0;
            for (var i = 1; i < count; i++) {
                d0 = d1;
                d1 = _recievedData [i];
                if (d0.time <= tinterp && tinterp <= d1.time)
                    break;
            }
			TransformData.Load (transform, d0, d1, tinterp);
            return true;
        }
        void CheckInitData() {
            if (_recievedData == null)
                _recievedData = new List<TransformData> ();
        }
        [ClientRpc]
        void RpcChangeCurrentTransform(TransformData v) {
            if (isServer)
                return;

            var t = Time.timeSinceLevelLoad;
            v.time = t;
            _recievedData.Add (v);
        }
		#endregion

        public struct TransformData {
            public float time;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public override string ToString () {
                return string.Format ("[TransformData] t={0} p={1} r={2} s={3}", time, position, rotation, scale);
            }

			public static TransformData Create(Transform transform, float time) {
				return new TransformData () {
					time = time,
					position = transform.localPosition,
					rotation = transform.localRotation,
					scale = transform.localScale
				};
			}

			public void Load(Transform transform) {
				Load(transform, position, rotation, scale);
			}
			public static void Load(Transform transform, TransformData d0, TransformData d1, float time) {
				var dt = d1.time - d0.time;
				if (-EPSILON < dt && dt < EPSILON) {
					d0.Load (transform);
					return;
				}

				var t = Mathf.Clamp01 ((time - d0.time) / (d1.time - d0.time));
				Load (transform,
					Vector3.Lerp (d0.position, d1.position, t),
					Quaternion.Lerp (d0.rotation, d1.rotation, t),
					Vector3.Lerp (d0.scale, d1.scale, t));
			}
			public static void Load(Transform transform, Vector3 position, Quaternion rotation, Vector3 scale) {
				transform.localPosition = position;
				transform.localRotation = rotation;
				transform.localScale = scale;
			}
        }
    }
}