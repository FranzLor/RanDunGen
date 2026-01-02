using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]

public class Doorway : MonoBehaviour
{

    private void Reset() {
        Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.bodyType = RigidbodyType2D.Kinematic;

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.size = Vector2.one * 0.1f;
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        // reloads the current scene
        if (collision.tag == "Player") {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
