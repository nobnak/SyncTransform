using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SyncTransformSystem {
	
	public class Sphere : NetworkBehaviour {
		public bool removeComponents;

		public override void OnStartServer () {
			
		}
		public override void OnStartClient () {
			if (!isServer) {
				if (removeComponents)
					RemoveComponents ();
			}
		}

		void RemoveComponents () {
			foreach (var r in GetComponentsInChildren<Rigidbody> ())
				Destroy (r);
			foreach (var c in GetComponentsInChildren<Collider> ())
				Destroy (c);
		}

		[ClientRpc]
		void RpcSetParent(int parentNetId) {
			
		}
	}
}