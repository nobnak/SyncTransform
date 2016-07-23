using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {
	[RequireComponent(typeof(Animator))]
	[NetworkSettings(channel=Channels.DefaultUnreliable)]
    public class SyncAnimation : NetworkBehaviour {
		public float latency = 2f;
        public SkinnedMeshRenderer skin;

		[SyncVar(hook="OnDataChanged")]
        protected Skelton _syncData;

		Animator _animator;
		Transform[] _bones;
		float _nextUpdateTime;
		List<Skelton> _dataStream;
		Skelton _interpolatedData;

		protected void Awake() {
			_animator = GetComponent<Animator> ();
			_animator.enabled = false;

			_nextUpdateTime = -1f;
			_bones = skin.bones;
			Debug.LogFormat ("Bone count={0}", _bones.Length);
			_dataStream = new List<Skelton> ();
			_interpolatedData = new Skelton (){ bones = new Bone[_bones.Length] };
		}
		public override void OnStartServer () {
			_animator.enabled = true;
		}
        protected void Update() {
            if (isServer) {
				var t = Time.timeSinceLevelLoad;
				if (_nextUpdateTime <= t) {
					_nextUpdateTime += GetNetworkSendInterval ();
					UpdateData ();
				}
            }
			if (isClient) {
				var count = _dataStream.Count;
				if (count <= 0)
					return;

				var tnow = Time.timeSinceLevelLoad;
				var tinterp = -latency * GetNetworkSendInterval () + tnow;
				while (_dataStream.Count >= 2 && _dataStream [1].time <= tinterp)
					_dataStream.RemoveAt (0);

				var d0 = _dataStream [0];
				var d1 = d0;
				if (_dataStream.Count >= 2)
					d1 = _dataStream [1];

				var dt = d1.time - d0.time;
				if (dt > Mathf.Epsilon) {
					var t = (tinterp - d0.time) / dt;
					_interpolatedData.Interpolate (d0, d1, t);
					d0 = _interpolatedData;
				}
				Load (d0);
            }
        }

        #region Server
		protected void UpdateData() {
			var bones = new Bone[_bones.Length];
			for (var i = 0; i < _bones.Length; i++)
				bones [i] = new Bone (_bones[i]);
			_syncData = new Skelton () { 
				time = Time.timeSinceLevelLoad,
				root = new Bone(transform),
				bones = bones
			};
		}
        #endregion

        #region Client
		protected void OnDataChanged(Skelton sk) {
			sk.time = Time.timeSinceLevelLoad;
			_dataStream.Add (sk);
		}
		protected void Load(Skelton sk) {
			sk.root.Load (transform);
			var boneCount = sk.bones.Length;
			for (var i = 0; i < boneCount; i++)
				sk.bones [i].Load (_bones [i]);
		}
        #endregion

        public struct Bone {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

			public Bone(Transform tr) {
				position = tr.localPosition;
				rotation = tr.localRotation;
				scale = tr.localScale;
			}
			public void Load(Transform tr) {
				tr.localPosition = position;
				tr.localRotation = rotation;
				tr.localScale = scale;
			}
			public void Interpolate(Bone d0, Bone d1, float t) {
				position = Vector3.Lerp (d0.position, d1.position, t);
				rotation = Quaternion.Lerp (d0.rotation, d1.rotation, t);
				scale = Vector3.Lerp (d0.scale, d1.scale, t);
			}
        }
        public struct Skelton {
            public float time;
            public Bone root;
            public Bone[] bones;

			public void Interpolate(Skelton d0, Skelton d1, float t) {
				time = Mathf.Lerp (d0.time, d1.time, t);
				root.Interpolate (d0.root, d1.root, t);
				var boneCount = bones.Length;
				for (var i = 0; i < boneCount; i++)
					bones [i].Interpolate (d0.bones [i], d1.bones [i], t);
			}
        }
    }
}
