using UnityEngine;
using System.Collections;

namespace SyncTransformSystem {
    
    public class MirrorBones : MonoBehaviour {
        
        public SkinnedMeshRenderer sourceSkin;
        public SkinnedMeshRenderer targetSkin;

        public static void SyncBones(Transform[] src, Transform[] dst) {
            for (var i = 0; i < src.Length; i++) {
                var str = src [i];
                var dtr = dst [i];
                dtr.localPosition = str.localPosition;
                dtr.localRotation = str.localRotation;
                dtr.localScale = str.localScale;
            }
        }

    	void Update () {
            if (sourceSkin == null || targetSkin == null) {
                Debug.LogError ("Null");
                return;
            }            
            var s = sourceSkin.bones;
            var d = targetSkin.bones;
            if (s == null || d == null || s.Length != d.Length) {
                Debug.LogError ("Incompatible bones");
                return;
            }

            SyncBones (s, d);
    	}
    }
}
