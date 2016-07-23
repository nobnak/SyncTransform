using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

public class TestSyncArray : NetworkBehaviour {
    [SyncVar(hook="OnSkinningChanged")]
    public Skinning _skinning;

    void OnSkinningChanged(Skinning skinning) {
        var bones = skinning.bones;
        var buf = new StringBuilder ();
        buf.AppendFormat("Observe Skinning w count={0}", bones.Length);
        for (var i = 0; i < bones.Length; i++)
            buf.AppendFormat (" i={0} p={1}", i, bones [i].position);
        Debug.Log (buf);
    }

    void Update() {
        if (isServer) {
            if (Input.GetMouseButtonDown (0)) {
                var boneCount = Random.Range (0, 10);
                var bones = new Bone[boneCount];
                for (var i = 0; i < boneCount; i++)
                    bones [i] = new Bone (){ position = i * Vector3.one };
                _skinning = new Skinning (){ bones = bones };
                Debug.LogFormat ("Update Skinning w count={0}", boneCount);
            }
        } else if (isClient) {
        }
    }

    public struct Bone {
        public Vector3 position;
    }

    public struct Skinning {
        public Bone[] bones;
    }
}
