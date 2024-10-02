using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    private new Collider2D collider;
    private bool collected = false;

    void Awake() {
        collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (collected)
            return;
        if (other.TryGetComponent(out Player player)) {
            collected = true;
            player.CollectPowerup(this);
        }
    }
}
