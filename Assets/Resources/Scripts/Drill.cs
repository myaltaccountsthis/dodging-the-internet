using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drill : MonoBehaviour
{
    private const float DRILL_SPEED = 3f;

    public Vector2 startPosition, endPosition;

    private SpriteRenderer spriteRenderer;
    private bool leftToRight;
    private Vector2 ExtendedOffset => new(.5f * (leftToRight ? 1 : -1), 0);
    private Vector2 Offset => new(.25f * (leftToRight ? 1 : -1), 0);
    private Vector3 direction;
    bool shouldMove;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        leftToRight = !spriteRenderer.flipX;
        direction = new(leftToRight ? 1 : -1, 0);
    }
    
    void Start() {
        shouldMove = false;
        Vector2 extendedPos = startPosition + ExtendedOffset, readyPos = startPosition + Offset;
        transform.position = startPosition;
        transform.LeanMove(extendedPos, .8f).setEaseOutQuad().setOnComplete(() => {
            transform.LeanMove(readyPos, .5f).setEaseInOutQuad().setOnComplete(() => shouldMove = true);
        });
    }

    void FixedUpdate() {
        if (shouldMove) {
            transform.position = transform.position + Time.fixedDeltaTime * DRILL_SPEED * direction;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent(out Player player)) {
            player.KillPlayer();
            Destroy(gameObject);
        }
    }
}
