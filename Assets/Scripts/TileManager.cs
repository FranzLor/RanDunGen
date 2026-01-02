using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileManager : MonoBehaviour {

    [HideInInspector] public float minX, maxX, minY, maxY;

    [SerializeField] public GameObject wallPrefab, floorPrefab, doorwayPrefab;
    [SerializeField] public GameObject tileSpawnerPrefab;
    [SerializeField] public int totalFloorCount = 750;

    List<Vector3> floorList = new List<Vector3>();

    private void Start() {
        RandomWalker();
    }

    private void Update() {
        // editor reset scene shortcut BACKSPACE
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Backspace)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void RandomWalker() {
        Vector3 currentPos = Vector3.zero;

        // gets initial tile spawner at pos 0,0
        floorList.Add(currentPos);

        while (floorList.Count < totalFloorCount) {
            switch (Random.Range(1, 5)) {
                case 1:
                    currentPos += Vector3.up;
                    break;
                case 2:
                    currentPos += Vector3.down;
                    break;
                case 3:
                    currentPos += Vector3.right;
                    break;
                case 4:
                    currentPos += Vector3.left;
                    break;
                default:
                    break;
            }
            // prevents duplicating spawns (stacking)
            bool floorExists = false;
            for (int i = 0; i < floorList.Count(); i++) {
                if (Vector3.Equals(currentPos, floorList[i])) {
                    floorExists = true;
                    break;
                }
            }
            if (!floorExists) {
                floorList.Add(currentPos);
            }
        }

        // spawns tile spawners in new random pos
        for (int i = 0; i < floorList.Count(); i++) {
            GameObject spawnTile = Instantiate(tileSpawnerPrefab, floorList[i], Quaternion.identity);
            spawnTile.name = tileSpawnerPrefab.name;
            spawnTile.transform.SetParent(this.transform);
        }

        StartCoroutine(DelayGeneration());
    }

    IEnumerator DelayGeneration() {
        // used to wait for all tile spawners to finish before placing exit doorway
        while (FindObjectsByType<TileSpawner>(FindObjectsSortMode.None).Length > 0) {
            yield return null;
        }

        ExitDoorway();
    }
    
    void ExitDoorway() {
        // places exit doorway at last floor position
        Vector3 doorPos = floorList[floorList.Count - 1];

        GameObject gameObjectDoor = Instantiate(doorwayPrefab,  doorPos, Quaternion.identity);
        gameObjectDoor.name = doorwayPrefab.name;
        gameObjectDoor.transform.SetParent(this.transform);
    }
}
