using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnerFireMode {
    Single, Spread
}

public class Spawner : Killbrick
{
    private const float START_OFFSET = .5f;
    private const float SPREAD_ANGLE = 30f;
    private const float SPREAD_PERIOD = 2f;
    private const float ENTER_DURATION = 1f;
    private const float ATTACK_DELAY = 1f;

    // Set by BulletHellSystem when instantiating
    [HideInInspector] public float lifetime;
    [HideInInspector] public Vector3 openPosition;
    [HideInInspector] public BulletHellSystem bulletHellSystem;
    [HideInInspector] public Transform target;
    [HideInInspector] public SpawnerFireMode fireMode;
    [HideInInspector] public float fireRate;
    
    private Vector3 originalPos;
    private float fireDelay;
    private float aliveT;
    private float AngleOffset => fireMode == SpawnerFireMode.Spread ? SPREAD_ANGLE * Mathf.Sin(2 * Mathf.PI * aliveT / SPREAD_PERIOD) : 0f;
    private bool active;

    void Start() {
        aliveT = 0;
        active = false;
        fireDelay = 1 / fireRate;
        originalPos = transform.position;
        StartCoroutine(PlayStartSequence());
    }

    private IEnumerator PlayStartSequence() {
        LeanTween.move(gameObject, openPosition, ENTER_DURATION).setEaseOutSine();
        yield return new WaitForSeconds(ENTER_DURATION + ATTACK_DELAY);
        active = true;
    }

    void Update() {
        if (active) {
            float oldT = aliveT;
            aliveT += Time.deltaTime;
            if (aliveT > lifetime) {
                active = false;
                LeanTween.move(gameObject, originalPos, ENTER_DURATION).setEaseInSine().setOnComplete(() => Destroy(gameObject));
                return;
            }
            if (aliveT % fireDelay < oldT % fireDelay) {
                Fire();
            }
        }
    }

    private void Fire() {
        Vector3 startPos = Vector3.MoveTowards(transform.position, target.position, START_OFFSET);
        Vector3 direction = (target.position - startPos).normalized;
        if (fireMode == SpawnerFireMode.Spread)
            direction = Quaternion.Euler(0, 0, AngleOffset) * direction;
        bulletHellSystem.SpawnBullet(startPos, direction);
    }
}
