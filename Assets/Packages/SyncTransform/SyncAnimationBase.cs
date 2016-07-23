using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {
	[RequireComponent(typeof(Animator))]
	[NetworkSettings(channel=1,sendInterval=0.1f)]
	public abstract class SyncAnimationBase : NetworkBehaviour {
		public float latency = 2f;
        public SkinnedMeshRenderer skin;

		protected Animator _animator;
		protected Transform[] _bones;
		protected float _nextUpdateTime;

		protected virtual void Awake() {
			_animator = GetComponent<Animator> ();
			_animator.enabled = false;

			_nextUpdateTime = -1f;
			_bones = skin.bones;
		}
		public override void OnStartServer () {
			base.OnStartServer ();
			_animator.enabled = true;
		}
        protected void Update() {
            if (isServer) {
				var t = Time.timeSinceLevelLoad;
				if (_nextUpdateTime <= t) {
					_nextUpdateTime += GetNetworkSendInterval ();
					NotifyData ();
				}
            }
			if (isClient) {
				ApplyData();
            }
        }

        #region Server
		protected abstract void NotifyData();
        #endregion

        #region Client
		protected abstract void ApplyData ();
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
			public int Changed(Bone prev) {
				return (position != prev.position ? (1 << 0) : 0)
				| (rotation != prev.rotation ? (1 << 1) : 0)
				| (scale != prev.scale ? (1 << 2) : 0);
			}
        }
        public struct Skelton {
            public float time;
            public Bone root;
            public Bone[] bones;

			public Skelton Save(float time, Transform root, Transform[] bones) {
				for (var i = 0; i < bones.Length; i++)
					this.bones [i] = new Bone (bones[i]);
				this.time = time;
				this.root = new Bone (root);
				return this;
			}
			public void Interpolate(Skelton d0, Skelton d1, float t) {
				time = Mathf.Lerp (d0.time, d1.time, t);
				root.Interpolate (d0.root, d1.root, t);
				var boneCount = bones.Length;
				for (var i = 0; i < boneCount; i++)
					bones [i].Interpolate (d0.bones [i], d1.bones [i], t);
			}
			public void Clone(Skelton org) {
				time = org.time;
				root = org.root;
				var boneCount = bones.Length;
				for (var i = 0; i < boneCount; i++)
					bones [i] = org.bones [i];
			}
        }
    }
}
