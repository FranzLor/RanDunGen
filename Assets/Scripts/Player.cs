using UnityEngine;

public class Player : MonoBehaviour {

    Transform spriteComponent;
    float flipX;

    void Start() {
        spriteComponent = GetComponentInChildren<SpriteRenderer>().transform;
        flipX = spriteComponent.localScale.x;
    }

    void Update() {
        // grid-like movement input
        float horizontal = System.Math.Sign(Input.GetAxisRaw("Horizontal"));

        // flips sprite based on movement direction
        if (Mathf.Abs(horizontal) > 0) { 
            spriteComponent.localScale = new Vector2(flipX * horizontal, spriteComponent.localScale.y);
        }
    }

}
