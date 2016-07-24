using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {
	public class SyncAnimationDiffCompact : SyncAnimationBase {
		protected SyncVector3List _syncPositionList;
		protected SyncQuaternionList _syncRotationList;
		protected SyncVector3List _syncScaleList;

		Skelton _tmpdata0, _tmpdata1;
		List<Skelton> _datastream;
		int _updateCount;

		protected override void Awake () {
			base.Awake ();
			_syncPositionList = new SyncVector3List ();
			_syncRotationList = new SyncQuaternionList ();
			_syncScaleList = new SyncVector3List ();
			_datastream = new List<Skelton> ();
			_tmpdata0 = new Skelton(){ bones = new Bone[_bones.Length] };
			_tmpdata1 = new Skelton (){ bones = new Bone[_bones.Length] };
		}
		public override void OnStartServer () {
			base.OnStartServer ();
			_tmpdata0.Save (Time.timeSinceLevelLoad, transform, _bones);
			InitData(_tmpdata0);
		}
		public override void OnStartClient () {
			_datastream.Add (CreateSkeltonFromData(Time.timeSinceLevelLoad));
			_updateCount = 0;
			_syncPositionList.Callback = (op, i) => _updateCount++;
			_syncRotationList.Callback = (op, i) => _updateCount++;
			_syncScaleList.Callback = (op, i) => _updateCount++;
		}
		protected override void NotifyData () {
			_tmpdata1.Save (Time.timeSinceLevelLoad, transform, _bones);
			SaveDataChange();
			SwapTemp();
		}
		protected override void ApplyData () {
			if (_updateCount > 0) {
				_updateCount = 0;
				_datastream.Add (CreateSkeltonFromData(Time.timeSinceLevelLoad));
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

		void InitData (Skelton sk) {
			_syncPositionList.Clear ();
			_syncRotationList.Clear ();
			_syncScaleList.Clear ();
			AddBoneOnData (sk.root);
			var boneCount = sk.bones.Length;
			for (var i = 0; i < boneCount; i++)
				AddBoneOnData(sk.bones [i]);
		}
		Bone CreateBoneFromDataAt (int i) {
			return new Bone () {
				position = _syncPositionList [i],
				rotation = _syncRotationList [i],
				scale = _syncScaleList [i]
			};
		}
		Skelton CreateSkeltonFromData (float time) {
			var boneCount = _syncPositionList.Count;
			var bones = new Bone[boneCount - 1];
			for (var i = 1; i < boneCount; i++)
				bones [i - 1] = CreateBoneFromDataAt (i);
			var sk = new Skelton () {
				time = time,
				root = CreateBoneFromDataAt (0),
				bones = bones
			};
			return sk;
		}
		void SwapTemp () {
			var tmp = _tmpdata0;
			_tmpdata0 = _tmpdata1;
			_tmpdata1 = tmp;
		}

		void AddBoneOnData(Bone bone) {
			_syncPositionList.Add (bone.position);
			_syncRotationList.Add (bone.rotation);
			_syncScaleList.Add (bone.scale);
		}
		void SaveDataChange () {
			SaveDataChangeAt (0, _tmpdata0.root, _tmpdata1.root);
			var boneCount = _syncPositionList.Count;
			for (var i = 1; i < boneCount; i++)
				SaveDataChangeAt (i, _tmpdata0.bones [i - 1], _tmpdata1.bones [i - 1]);
		}
		void SaveDataChangeAt(int i, Bone prev, Bone next) {
			if (prev.position != next.position)
				_syncPositionList [i] = next.position;
			if (prev.rotation != next.rotation)
				_syncRotationList [i] = next.rotation;
			if (prev.scale != next.scale)
				_syncScaleList [i] = next.scale;
		}

		public class SyncVector3List : SyncListStruct<Vector3> {}
		public class SyncQuaternionList : SyncListStruct<Quaternion> {}
	}
}
