using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TestSyncArray : NetworkBehaviour {
    [SyncVar(hook="OnBonesUpdate")]
    Bone[] _bones;

    void OnBonesUpdate(Bone[] bones) {
        Debug.LogFormat ("Update bones on {0}", 0);
    }

    public struct Bone {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}
