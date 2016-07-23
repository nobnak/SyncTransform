using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {
	public class SyncAnimationDiff : SyncAnimationBase {
		protected SyncBoneList _syncBones;

		Skelton _tmpdata0, _tmpdata1;
		List<Skelton> _datastream;
		int _updateCount;

		protected override void Awake () {
			base.Awake ();
			_syncBones = new SyncBoneList();
			_datastream = new List<Skelton> ();
			_tmpdata0 = new Skelton(){ bones = new Bone[_bones.Length] };
			_tmpdata1 = new Skelton (){ bones = new Bone[_bones.Length] };
		}
		public override void OnStartServer () {
			base.OnStartServer ();
			_tmpdata0.Save (Time.timeSinceLevelLoad, transform, _bones);
			_syncBones.Init (_tmpdata0);
		}
		public override void OnStartClient () {
			_datastream.Add (_syncBones.Create (Time.timeSinceLevelLoad));
			_updateCount = 0;
			_syncBones.Callback = (op, i) => _updateCount++;
		}
		protected override void NotifyData () {
			_tmpdata1.Save (Time.timeSinceLevelLoad, transform, _bones);
			_syncBones.Save (_tmpdata0, _tmpdata1, true);
			_tmpdata0.Clone (_tmpdata1);
		}
		protected override void ApplyData () {
			if (_updateCount > 0) {
				_updateCount = 0;
				_datastream.Add (_syncBones.Create (Time.timeSinceLevelLoad));
			}

			var count = _datastream.Count;
			if (count <= 0)
				return;

			var tnow = Time.timeSinceLevelLoad;
			var tinterp = -latency * GetNetworkSendInterval () + tnow;
			while (_datastream.Count >= 2 && _datastream [1].time <= tinterp)
				_datastream.RemoveAt (0);

			var d0 = _datastream [0];
			var d1 = d0;
			if (_datastream.Count >= 2)
				d1 = _datastream [1];

			var dt = d1.time - d0.time;
			if (dt > Mathf.Epsilon) {
				var t = (tinterp - d0.time) / dt;
				_tmpdata0.Interpolate (d0, d1, t);
				d0 = _tmpdata0;
			}
			Load (d0);
		}

		public class SyncBoneList : SyncListStruct<Bone> {
			public void Init(Skelton sk) {
				Clear ();
				Add (sk.root);
				var boneCount = sk.bones.Length;
				for (var i = 0; i < boneCount; i++)
					Add (sk.bones [i]);
			}
			public void Save(Skelton current, Skelton next, bool diff) {
				if (!diff || next.root.Changed (current.root) != 0)
					this [0] = next.root;
				var boneCount = next.bones.Length;
				for (var i = 0; i < boneCount; i++)
					if (!diff || next.bones [i].Changed (current.bones [i]) != 0)
						this [i + 1] = next.bones [i];
			}
			public Skelton Create(float time) {
				var boneCount = Count;
				var bones = new Bone[boneCount - 1];
				for (var i = 1; i < boneCount; i++)
					bones [i - 1] = this [i];
				return new Skelton (){ time = time, root = this[0], bones = bones };
			}
		}
	}
}
