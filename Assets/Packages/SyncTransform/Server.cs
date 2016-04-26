using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SyncTransformSystem {

    public class Server : NetworkBehaviour {
        public GameObject characterfab;

        Rect _window = new Rect(10, 10, 200, 200);

        void OnGUI() {
            if (isServer)
                _window = GUILayout.Window (GetInstanceID (), _window, Window, "Server");
        }

        void Window(int id) {
            GUILayout.BeginVertical ();

            if (GUILayout.Button ("Spawn")) {
                var c = Instantiate (characterfab);
                c.transform.SetParent (transform, false);
                NetworkServer.Spawn (c);
            }

            GUILayout.EndVertical ();
            GUI.DragWindow ();
        }
    }
}