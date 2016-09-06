using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

namespace SyncTransformSystem {
	[RequireComponent(typeof(Animator))]
	[NetworkSettings(channel=1)]
	public abstract class SyncAnimationBase : NetworkBehaviour {
		[SyncVar]
		public float interval = 0.1f;
		[SyncVar]
		public float latency = 2f;

		protected Animator _animator;
		protected Transform[] _bones;
		protected float _nextUpdateTime;

		protected virtual void Awake() {
			_animator = GetComponent<Animator> ();
			_animator.enabled = false;

			_nextUpdateTime = -1f;
            _bones = Bones (transform);
		}
		public override void OnStartServer () {
			base.OnStartServer ();
			_animator.enabled = true;
		}
		public override float GetNetworkSendInterval () { return interval; }

        protected void Update() {
            if (isServer) {
				var t = Time.timeSinceLevelLoad;
				if (_nextUpdateTime <= t) {
					_nextUpdateTime += GetNetworkSendInterval ();
					NotifyData ();
				}
			} else if (isClient) {
				ApplyData();
            }
        }

        #region Bones
        public static IEnumerable<Transform> Listup(Transform root, bool includingRoot = true) {
            if (includingRoot)
                yield return root;
            for (var i = 0; i < root.childCount; i++)
                foreach (var tr in Listup (root.GetChild (i)))
                    yield return tr;
        }
        public static Transform[] Bones(Transform root, bool includingRoot = true) {
            return Listup (root, includingRoot).ToArray ();
        }
        #endregion

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

        #region Classes
        public struct Bone {
            [System.Flags]
            public enum ChangeFlags { 
                None = 0, Position = 1 << 0, Rotation = 1 << 1, Scale = 1 << 2,
                All = Position | Rotation | Scale
            }
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
			public ChangeFlags Changed(Bone prev) {
                return (position != prev.position ? ChangeFlags.Position : 0)
                    | (rotation != prev.rotation ? ChangeFlags.Rotation : 0)
                    | (scale != prev.scale ? ChangeFlags.Scale : 0);
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
        #endregion
    }
}
