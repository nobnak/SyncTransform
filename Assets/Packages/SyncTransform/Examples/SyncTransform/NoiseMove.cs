using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SyncTransformSystem {

    public class NoiseMove : NetworkBehaviour {
        public const float SEED_SIZE = 1000f;
        public const float ROTATION_DEG = 360f;

        public float posFreq = 1f;
        public float posSize = 10f;

        public float rotFreq = 1f;

        Vector3 _seed;

        public override void OnStartServer () {
            _seed = new Vector3 (
                Random.Range (-SEED_SIZE, SEED_SIZE),
                Random.Range (-SEED_SIZE, SEED_SIZE),
                Random.Range (-SEED_SIZE, SEED_SIZE));
        }
        void Update() {
            if (isServer) {
                UpdateServer();

            }
        }

        void UpdateServer () {
            var t = Time.timeSinceLevelLoad;
            var tpos = t * posFreq;
            var trot = t * rotFreq;
            transform.localPosition = new Vector3 (
                posSize * Noise (tpos + _seed.x, _seed.y), 
                posSize * Noise (tpos + _seed.y, _seed.z), 
                posSize * Noise (tpos + _seed.z, _seed.x));
            transform.localRotation = Quaternion.Euler (
                ROTATION_DEG * Noise (trot + _seed.x, _seed.z), 
                ROTATION_DEG * Noise (trot + _seed.z, _seed.y), 
                ROTATION_DEG * Noise (trot + _seed.y, _seed.x));
        }

        float Noise(float x, float y) {
            return 2f * Mathf.PerlinNoise (x, y) - 1f;
        }
    }
}