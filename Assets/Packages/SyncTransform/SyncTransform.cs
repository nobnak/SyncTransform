using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {
	[NetworkSettings(channel=Channels.DefaultUnreliable)]
    public class SyncTransform : NetworkBehaviour {
		public enum SyncModeEnum { ClientRPC = 0, SyncVars }
		public const float EPSILON = 1e-5f;

		public SyncModeEnum syncMode;
        public float latency = 2f;

		#pragma warning disable 0414
		[SyncVar(hook="ChangeCurrentTransform")]
		TransformData syncCurrentTransform;
		#pragma warning restore 0414

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
				switch (syncMode) {
				case SyncModeEnum.SyncVars:
					syncCurrentTransform = currentTransform;
					break;
				default:
					RpcChangeCurrentTransform (currentTransform);
					break;
				}
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
			while (_recievedData.Count >= 2 && _recievedData [1].time <= tinterp)
				_recievedData.RemoveAt (0);
			
            var d0 = _recievedData [0];
            var d1 = d0;
			if (_recievedData.Count >= 2)
				d1 = _recievedData [1];
			
			TransformData.Load (transform, d0, d1, tinterp);
            return true;
        }
        void CheckInitData() {
            if (_recievedData == null)
                _recievedData = new List<TransformData> ();
        }

		void ChangeCurrentTransform(TransformData v) {
			var t = Time.timeSinceLevelLoad;
			v.time = t;
			_recievedData.Add (v);
		}

		[ClientRpc(channel=Channels.DefaultUnreliable)]
        void RpcChangeCurrentTransform(TransformData v) {
            if (!isServer)
	            ChangeCurrentTransform (v);
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