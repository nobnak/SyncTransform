using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SyncTransformSystem {

	public class Cage : NetworkBehaviour {
		public float rotationSpeed = 1f;

		Vector3 _prevMousePos;

		public override void OnStartServer () {
			NetworkServer.Spawn (gameObject);
		}

		void Update() {
			if (isServer) {
				MouseRotation();
			}
		}

		void MouseRotation () {
			if (Input.GetMouseButtonDown (0))
				_prevMousePos = Input.mousePosition;
			if (Input.GetMouseButton (0)) {
				var dx = Input.mousePosition - _prevMousePos;
				_prevMousePos = Input.mousePosition;

				transform.rotation *= Quaternion.Euler (rotationSpeed * dx.x, 0f, rotationSpeed * dx.y);
			}
		}
	}
}