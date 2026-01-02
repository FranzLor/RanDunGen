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
    [SerializeField] public GameObject[] spawnRandomObjects;

    [SerializeField, UnityEngine.Range(0, 100)] int randomObjectSpawnChance;

    List<Vector3> floorList = new List<Vector3>();
    LayerMask floorMask, wallMask;

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

        // spawns random objects on random floor tiles
        Vector2 floorSize = Vector2.one * 0.8f;
        floorMask = LayerMask.GetMask("Floor");
        wallMask = LayerMask.GetMask("Wall");

        for (int x = (int)(minX - 2); x <= (int)maxX + 2; x++) {
            for (int y = (int)minY - 2; y <= (int)maxY + 2; y++) {
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), floorSize, 0, floorMask);

                if (hitFloor) {
                    // avoids placing objects on the exit doorway floor
                    if (!Vector2.Equals(hitFloor.transform.position, floorList[floorList.Count - 1])) {

                        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), floorSize, 0, wallMask);
                        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), floorSize, 0, wallMask);
                        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), floorSize, 0, wallMask);
                        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), floorSize, 0, wallMask);

                        // only places object if there is a wall on one side
                        if ((hitTop || hitBottom || hitRight || hitLeft) && !(hitTop && hitBottom) && !(hitRight && hitLeft)) {
                            // random chance to spawn object
                            int roll = Random.Range(0, 101);
                            if (roll <= randomObjectSpawnChance) {
                                int objectIndex = Random.Range(0, spawnRandomObjects.Length);
                                GameObject gameObjects = Instantiate(spawnRandomObjects[objectIndex], hitFloor.transform.position, Quaternion.identity);
                                gameObjects.name = spawnRandomObjects[objectIndex].name;
                                gameObjects.transform.SetParent(this.transform);

                            }
                        }
                    }
                }
            }
        }
    }

    void ExitDoorway() {
        // places exit doorway at last floor position
        Vector3 doorPos = floorList[floorList.Count - 1];

        GameObject gameObjectDoor = Instantiate(doorwayPrefab,  doorPos, Quaternion.identity);
        gameObjectDoor.name = doorwayPrefab.name;
        gameObjectDoor.transform.SetParent(this.transform);
    }
}
