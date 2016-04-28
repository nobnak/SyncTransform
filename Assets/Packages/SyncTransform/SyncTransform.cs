using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {

    public class SyncTransform : NetworkBehaviour {
        public float latency = 2f;

        #pragma warning disable 0414
        [SyncVar(hook="OnCurrentTransformChange")]
        TransformData currentTransform;
        #pragma warning restore

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

            if (isClient)
                UpdateClient ();
        }

        void UpdateServer() {
            UpdateCurrentTransform ();
        }
        void UpdateCurrentTransform() {
            var t = Time.timeSinceLevelLoad;
            if (_nextTransformUpdateTime < t) {
                _nextTransformUpdateTime = t + GetNetworkSendInterval ();
                currentTransform = new TransformData () {
                    time = t,
                    position = transform.localPosition,
                    rotation = transform.localRotation,
                    scale = transform.localScale
                };
            }
        }

        void UpdateClient() {
            InterpolrateTransform ();
        }
        void InterpolrateTransform () {
            var count = _recievedData.Count;
            if (count < 2)
                return;

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
            var t = Mathf.Clamp01 ((tinterp - d0.time) / (d1.time - d0.time));
            Interpolate (transform, ref d0, ref d1, t);
        }
        void OnCurrentTransformChange(TransformData v) {
            var t = Time.timeSinceLevelLoad;
            v.time = t;
            _recievedData.Add (v);
        }
        void CheckInitData() {
            if (_recievedData == null)
                _recievedData = new List<TransformData> ();
        }

        public static void Interpolate (Transform transform, ref TransformData d0, ref TransformData d1, float t) {
            transform.localPosition = Vector3.Lerp (d0.position, d1.position, t);
            transform.localRotation = Quaternion.Lerp (d0.rotation, d1.rotation, t);
            transform.localScale = Vector3.Lerp (d0.scale, d1.scale, t);
        }

        public struct TransformData {
            public float time;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public override string ToString () {
                return string.Format ("[TransformData] t={0} p={1} r={2} s={3}", time, position, rotation, scale);
            }
        }
    }
}