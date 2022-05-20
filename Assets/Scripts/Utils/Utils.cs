using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    public static Vector3 GetRandomSpawnPoint() {
        return new Vector3(Random.Range(-5, 5), 4, Random.Range(-5, 5));
    }
    public static void SetRenderLayerDeep(Transform transform, int layerNumber) {
        transform.gameObject.layer = layerNumber;
        foreach (Transform trans in transform.GetComponentsInChildren<Transform>(true)) {
            trans.gameObject.layer = layerNumber;
        }
    }
}
