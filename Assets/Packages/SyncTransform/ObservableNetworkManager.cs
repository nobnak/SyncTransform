using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SyncTransformSystem {
    
    public class ObservableNetworkManager : NetworkManager {
        public NetworkManagerEvent OnStartServerEvent;
        public NetworkManagerEvent OnStopServerEvent;

        public override void OnStartServer () {
            OnStartServerEvent.Invoke (this);
        }
        public override void OnStopServer () {
            OnStopServerEvent.Invoke (this);
        }

        [System.Serializable]
        public class NetworkManagerEvent : UnityEngine.Events.UnityEvent<NetworkManager> {}
    }
}