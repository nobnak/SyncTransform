using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class HeavyLoading : NetworkBehaviour {
    public GameObject fab;
    public int count = 1000;
    public float size = 1f;

    public override void OnStartServer () {
        var width = Mathf.CeilToInt (Mathf.Sqrt (count));
        for (var i = 0; i < count; i++) {
            var y = i / width;
            var x = i - y * width;
            var instance = Instantiate (fab);
            instance.transform.SetParent (transform);
            var pos = Vector3.Scale (
                size * fab.transform.localScale, 
                new Vector3 (x - 0.5f * width, y - 0.5f * width, 0f));
            instance.transform.localPosition = pos;
            NetworkServer.Spawn (instance);
        }
    }
}
