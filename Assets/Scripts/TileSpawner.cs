using UnityEngine;

public class TileSpawner : MonoBehaviour {

    TileManager tileManager;

    void Awake() {
        tileManager = FindFirstObjectByType<TileManager>();

        GameObject gameObjectFloor = Instantiate(tileManager.floorPrefab, transform.position, Quaternion.identity);
        gameObjectFloor.name = tileManager.floorPrefab.name;
        gameObjectFloor.transform.SetParent(tileManager.transform);

        if (transform.position.x > tileManager.maxX) {
            tileManager.maxX = transform.position.x;
        }
        if (transform.position.x < tileManager.minX) {
            tileManager.minX = transform.position.x;
        }
        if (transform.position.y > tileManager.maxY) {
            tileManager.maxY = transform.position.y;
        }
        if (transform.position.y < tileManager.minY) {
            tileManager.minY = transform.position.y;
        }
    }

    void Start() {
        LayerMask environmentMask = LayerMask.GetMask("Wall", "Floor");
        Vector2 hitSize = Vector2.one * 0.8f;

        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {

                // removes wall spawning on the tile itself, overlap doesnt detect it for some reason
                if (x == 0 && y == 0) {
                    continue;
                }

                Vector2 targetPos = new Vector2(transform.position.x + x, transform.position.y + y);
                Collider2D collisionHit = Physics2D.OverlapBox(targetPos, hitSize, 0, environmentMask);

                if (!collisionHit) {
                    GameObject gameObjectWall = Instantiate(tileManager.wallPrefab, targetPos, Quaternion.identity);
                    gameObjectWall.name = tileManager.wallPrefab.name;
                    gameObjectWall.transform.SetParent(tileManager.transform);
                }
            }
        }

        Destroy(this.gameObject);
    }

    // visual helper
    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1f, 1));
    }
}
