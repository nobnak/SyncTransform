using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SyncTransformSystem {
	
	public class SyncPhysicsTest : NetworkBehaviour {
		public GameObject fab;

		List<GameObject> _spawned = new List<GameObject>();

		void Update () {
			if (isServer) {
				if (Input.GetMouseButton(1)) {
					var instance = Instantiate (fab);
					//instance.transform.SetParent (transform, false);
					instance.transform.position = transform.position + 0.5f * Random.insideUnitSphere;
					_spawned.Add (instance);
					NetworkServer.Spawn (instance);
				}
			}
		}

		public void OnStopServer(NetworkManager nm) {
			foreach (var go in _spawned)
				Destroy (go);
			_spawned.Clear ();
		}
	}
}