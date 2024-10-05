using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Killbrick
{
    public delegate Vector2 PositionFunc(float t);
    public float lifetime;

    public PositionFunc GetPosition;

    private float aliveT;
    
    void Start() {
        transform.position = GetPosition(0);
        aliveT = 0;
    }

    void FixedUpdate() {
        transform.position = GetPosition(aliveT += Time.fixedDeltaTime);
        if (aliveT > lifetime) {
            Destroy(gameObject);
        }
    }
}
