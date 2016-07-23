using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {
    [NetworkSettings(channel=Channels.DefaultUnreliable)]
    public class SyncAnimation : NetworkBehaviour {
        public SkinnedMeshRenderer skin;

		[SyncVar(hook="OnDataChanged")]
        protected Skelton _syncData;

		Transform[] _bones;
		float _nextUpdateTime;
		List<Bone> _dataStream;

		protected void Awake() {
			_nextUpdateTime = -1f;
			_bones = skin.bones;
			_dataStream = new List<Bone> ();
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
				InterpolateData ();
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
		protected void InterpolateData() {
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
        }
        public struct Skelton {
            public float time;
            public Bone root;
            public Bone[] bones;
        }
    }
}
