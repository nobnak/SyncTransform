using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SyncTransformSystem {

    public class Server : NetworkBehaviour {
        public const string PROP_COLOR = "_Color";

        public GameObject characterfab;

        int _counter = 0;
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
                var rend = c.GetComponent<Renderer> ();
                if (rend != null) {
                    var block = new MaterialPropertyBlock ();
                    block.SetColor (PROP_COLOR, Color.HSVToRGB (_counter / 6f, 1f, 1f));
                    rend.SetPropertyBlock (block);
                }
                NetworkServer.Spawn (c);
                    _counter++;
            }

            GUILayout.EndVertical ();
            GUI.DragWindow ();
        }
    }
}