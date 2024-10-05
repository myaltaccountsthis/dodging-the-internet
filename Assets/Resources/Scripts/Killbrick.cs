using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killbrick : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent(out Player player)) {
            player.KillPlayer();
            Destroy(gameObject);
        }
    }
}
