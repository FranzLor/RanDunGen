using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum DungeonTypes {
    Cave,
    Rooms
}

public class TileManager : MonoBehaviour
{

    [HideInInspector] public float minX, maxX, minY, maxY;

    [SerializeField] public GameObject wallPrefab, floorPrefab, doorwayPrefab;
    [SerializeField] public GameObject tileSpawnerPrefab;
    [SerializeField] public int totalFloorCount = 750;
    [SerializeField] public GameObject[] spawnRandomObjects, spawnRandomEnemies;

    [SerializeField, UnityEngine.Range(0, 100)] int randomObjectSpawnChance;
    [SerializeField, UnityEngine.Range(0, 100)] int randomEnemySpawnChance;

    public DungeonTypes dungeonType;

    List<Vector3> floorList = new List<Vector3>();
    LayerMask floorMask, wallMask;
    Vector2 hitSize;

    private void Awake() {
        floorMask = LayerMask.GetMask("Floor");
        wallMask = LayerMask.GetMask("Wall");
    }

    private void Start() {
        hitSize = Vector2.one * 0.8f;

        switch (dungeonType) {
            case DungeonTypes.Cave:
                RandomCaveWalker();
                break;

            case DungeonTypes.Rooms:
                RandomRoomWalker();
                break;
        }

        
    }

    private void Update() {
        // editor reset scene shortcut BACKSPACE
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Backspace)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void RandomCaveWalker() {
        Vector3 currentPos = Vector3.zero;

        // gets initial tile spawner at pos 0,0
        floorList.Add(currentPos);

        while (floorList.Count < totalFloorCount) {
            currentPos += GetRandomStep();
            if (!DoesFloorExistAtPos(currentPos)) {
                floorList.Add(currentPos);
            }
        }

        StartCoroutine(DelayGeneration());
    }

    void RandomRoomWalker() {
        Vector3 currentPos = Vector3.zero;

        // gets initial tile spawner at pos 0,0
        floorList.Add(currentPos);

        // walks in a direction for a set length to create hallways
        while (floorList.Count < totalFloorCount) {
            Vector3 walkingDirection = GetRandomStep();
            int walkingLength = Random.Range(9, 18);

            for (int i = 0; i < walkingLength; i++) {
                if (!DoesFloorExistAtPos(currentPos + walkingDirection)) {
                    floorList.Add(currentPos + walkingDirection);
                }
                currentPos += walkingDirection;
            }
            // adds a room at the end of the hallway
            int width = Random.Range(1, 5);
            int height = Random.Range(1, 5);

            for (int w = -width; w <= width; w++) {
                for (int h = -height; h <= height; h++) {
                    // skips the center tile since already added
                    Vector3 offset = new Vector3(w, h, 0);
                    if (!DoesFloorExistAtPos(currentPos + offset)) {
                        floorList.Add(currentPos + offset);
                    }
                }
            }
        }

        StartCoroutine(DelayGeneration());
    }

    Vector3 GetRandomStep() {
        switch (Random.Range(1, 5)) {
            case 1:
                return Vector3.up;
            case 2:
                return Vector3.right;
            case 3:
                return Vector3.down;
            case 4:
                return Vector3.left;
        }
        // should never reach here but compiler bitches without it
        return Vector3.zero;
    }

    bool DoesFloorExistAtPos(Vector3 currentPos) {
        // prevents duplicating spawns (stacking)            
        for (int i = 0; i < floorList.Count(); i++) {
            if (Vector3.Equals(currentPos, floorList[i])) {
                return true;
            }
        }
        return false;
    }

    IEnumerator DelayGeneration() {
        // spawns tile spawners in new random pos
        for (int i = 0; i < floorList.Count(); i++) {
            GameObject spawnTile = Instantiate(tileSpawnerPrefab, floorList[i], Quaternion.identity);
            spawnTile.name = tileSpawnerPrefab.name;
            spawnTile.transform.SetParent(this.transform);
        }

        // used to wait for all tile spawners to finish before placing exit doorway
        while (FindObjectsByType<TileSpawner>(FindObjectsSortMode.None).Length > 0) {
            yield return null;
        }

        PostGeneration();
    }

    void PostGeneration() {
        ExitDoorway();
        SpawnRandomObjects();
        SpawnRandomEnemies();
    }

    void ExitDoorway() {
        // places exit doorway at last floor position
        Vector3 doorPos = floorList[floorList.Count - 1];

        GameObject gameObjectDoor = Instantiate(doorwayPrefab, doorPos, Quaternion.identity);
        gameObjectDoor.name = doorwayPrefab.name;
        gameObjectDoor.transform.SetParent(this.transform);
    }

    void SpawnRandomObjects() {
        // spawns random objects by walls on random floor tiles
        for (int x = (int)(minX - 2); x <= (int)maxX + 2; x++) {
            for (int y = (int)minY - 2; y <= (int)maxY + 2; y++) {
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, floorMask);

                if (hitFloor) {
                    // avoids placing objects on the exit doorway floor
                    if (!Vector2.Equals(hitFloor.transform.position, floorList[floorList.Count - 1])) {

                        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);

                        // only places object if there is a wall on one side
                        if ((hitTop || hitBottom || hitRight || hitLeft) && !(hitTop && hitBottom) && !(hitRight && hitLeft)) {
                            // random chance to spawn object
                            int roll = Random.Range(1, 101);
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

    void SpawnRandomEnemies() {
        // spawns random enemies on open random floor tiles
        for (int x = (int)(minX - 2); x <= (int)maxX + 2; x++) {
            for (int y = (int)minY - 2; y <= (int)maxY + 2; y++) {
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, floorMask);

                if (hitFloor) {
                    // avoids placing objects on the exit doorway floor
                    if (!Vector2.Equals(hitFloor.transform.position, floorList[floorList.Count - 1])) {

                        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);

                        // makes sure there are no walls around the tile
                        if (!hitTop && !hitBottom && !hitLeft && !hitRight) {
                            // random chance to spawn object
                            int roll = Random.Range(1, 101);
                            if (roll <= randomObjectSpawnChance) {
                                int enemyIndex = Random.Range(0, spawnRandomEnemies.Length);
                                GameObject gameObjects = Instantiate(spawnRandomEnemies[enemyIndex], hitFloor.transform.position, Quaternion.identity);
                                gameObjects.name = spawnRandomEnemies[enemyIndex].name;
                                gameObjects.transform.SetParent(this.transform);

                            }
                        }
                    }
                }
            }
        }
    }

}
