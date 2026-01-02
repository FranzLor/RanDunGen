using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] float movementSpeed = 5.0f;

    Transform spriteComponent;
    float flipX;
    Vector2 targetPos;
    bool isMoving = false;
    LayerMask colliderMasks;

    void Start() {
        spriteComponent = GetComponentInChildren<SpriteRenderer>().transform;
        flipX = spriteComponent.localScale.x;

        colliderMasks = LayerMask.GetMask("Wall", "Enemy", "NPC");
    }

    void Update() {
        // grid-like movement input
        float horizontal = System.Math.Sign(Input.GetAxisRaw("Horizontal"));
        float vertical = System.Math.Sign(Input.GetAxisRaw("Vertical"));


        // meanwhile with input
        if (Mathf.Abs(horizontal) > 0 || Mathf.Abs(vertical) > 0) {

            // flips sprite based on movement direction
            if (Mathf.Abs(horizontal) > 0) {
                spriteComponent.localScale = new Vector2(flipX * horizontal, spriteComponent.localScale.y);
            }

            if (!isMoving) {
                // sets target position based on input
                if (Mathf.Abs(horizontal) > 0) {
                    targetPos = new Vector2(transform.position.x + horizontal, transform.position.y);
                }
                else if (Mathf.Abs(vertical) > 0) {
                    targetPos = new Vector2(transform.position.x, transform.position.y + vertical);
                }


                // collision check
                Vector2 colliderSize = Vector2.one * 0.8f;
                Collider2D collisionHit = Physics2D.OverlapBox(targetPos, colliderSize, 0.0f, colliderMasks);

                if (!collisionHit) {
                    StartCoroutine(Movement());
                }
            }
        }
    }

    IEnumerator Movement() {
        isMoving = true;
        // move towards target pos
        while (Vector2.Distance(transform.position, targetPos) > 0.01f) {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, movementSpeed * Time.deltaTime);
            yield return null;
        }
        // snap to target pos
        transform.position = targetPos;
        isMoving = false;
    }

}
