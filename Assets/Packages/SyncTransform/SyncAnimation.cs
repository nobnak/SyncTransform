using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {
	public class SyncAnimation : SyncAnimationBase {
		[SyncVar(hook="OnDataChanged")]
        protected Skelton _syncData;

		List<Skelton> _datastream;
		Skelton _tmpData;

		protected override void Awake() {
			base.Awake ();
			_datastream = new List<Skelton> ();
			_tmpData = new Skelton (){ bones = new Bone[_bones.Length] };
		}

        #region Server
		protected override void NotifyData() {
			_tmpData.Save (Time.timeSinceLevelLoad, transform, _bones);
			_syncData = _tmpData;
		}
        #endregion

        #region Client
		protected void OnDataChanged(Skelton sk) {
			sk.time = Time.timeSinceLevelLoad;
			_datastream.Add (sk);
		}
		protected override void ApplyData () {
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
				_tmpData.Interpolate (d0, d1, t);
				d0 = _tmpData;
			}
			Load (d0);
		}
        #endregion
    }
}
