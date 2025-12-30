using UnityEngine;

public class TileSpawner : MonoBehaviour {

    TileManager tileManager;

    void Start() {
        tileManager = FindFirstObjectByType<TileManager>();
    }
}
