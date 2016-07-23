using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SyncTransformSystem {
    [NetworkSettings(channel=Channels.DefaultUnreliable)]
    public class SyncAnimation : NetworkBehaviour {
        public SkinnedMeshRenderer skin;

        [SyncVar]
        Skelton _skelton;

        void Update() {
            if (isServer) {
                
            } else if (isClient) {
                
            }
        }

        #region Server
        #endregion

        #region Client
        #endregion

        public struct Bone {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }
        public struct Skelton {
            public float time;
            public Bone root;
            public Bone[] bones;
        }
    }
}
