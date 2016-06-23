using UnityEngine;
using System.Collections;

namespace SyncTransformSystem {
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class MirrorBones : MonoBehaviour {
        public SkinnedMeshRenderer sourceSkin;

        SkinnedMeshRenderer _attachedSkin;

        void Awake () {
            _attachedSkin = GetComponent<SkinnedMeshRenderer> ();
        }
    	void Update () {
            if (sourceSkin == null || _attachedSkin == null) {
                Debug.LogError ("Null");
                return;
            }            
            var s = sourceSkin.bones;
            var d = _attachedSkin.bones;
            if (s == null || d == null || s.Length != d.Length) {
                Debug.LogError ("Incompatible bones");
                return;
            }

            for (var i = 0; i < s.Length; i++) {
                var str = s [i];
                var dtr = d [i];
                dtr.localPosition = str.localPosition;
                dtr.localRotation = str.localRotation;
                dtr.localScale = str.localScale;
            }
    	}
    }
}