using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const float MOVEMENT_SPEED = 5f;
    private const float GHOST_SPEED = 100f;
    private const float STARTING_SCALE = .4f;
    private const float MAX_SCALE = 1.5f;
    private const float MAX_TIME_NO_HOVER = 2.5f;
    // Not actually the time but ok
    private const float MAX_TIME_SAFE = .8f;
    private static readonly float STARTING_SIZE = Mathf.Sqrt(STARTING_SCALE) - MAX_TIME_SAFE;
    private static readonly float MAX_SIZE = Mathf.Sqrt(MAX_SCALE);
    private static readonly float SIZE_RANGE = MAX_SIZE - STARTING_SIZE;
    private static readonly float SIZE_DELTA = SIZE_RANGE / MAX_TIME_NO_HOVER;

    public bool GameStarted { get; private set; }
    public bool IsAlive { get; private set; }
    public bool GhostDead;
    public float Scale => Mathf.Max(STARTING_SCALE, size * size);
    public float ScalePercent => (Scale - STARTING_SCALE) / (MAX_SCALE - STARTING_SCALE);
    public float SafePercent => Mathf.Clamp(1 - (size - STARTING_SIZE) / MAX_TIME_SAFE, 0f, 1f);
    public float GhostSpeed => GHOST_SPEED / Mathf.Sqrt(Scale) * (1 + .05f * bulletHellSystem.RoundIndex);

    public Ghost ghost;
    public BulletHellSystem bulletHellSystem;
    // public GameObject gate;
    public GameObject tutorial;
    public AudioSource backgroundMusic, loseSound;

    private new Camera camera;
    private new Rigidbody2D rigidbody;

    // Internal variable that Scale is based off of
    private float size;

    void Awake() {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start() {
        size = STARTING_SIZE + MAX_TIME_SAFE;
        GameStarted = false;
        IsAlive = true;
        GhostDead = false;
    }

    void Update() {
        camera.transform.position = new(transform.position.x, transform.position.y, camera.transform.position.z);
        bool isHoveringGhost = ghost.IsOverUI();

        // Don't do any logic if the game is not active
        if (!GameStarted) {
            if (!isHoveringGhost)
                return;
            GameStarted = true;
            // Destroy(gate);
            tutorial.SetActive(false);
            backgroundMusic.Play();
        }
        if (!IsAlive || GhostDead)
            return;
        
        transform.localScale = Vector3.one * Scale;
        if (isHoveringGhost) {
            size -= Time.deltaTime * SIZE_DELTA;
        }
        else {
            size += Time.deltaTime * SIZE_DELTA;
        }
        if (size > MAX_SIZE) {
            KillPlayer();
            size = MAX_SIZE;
        }
        else if (size < STARTING_SIZE)
            size = STARTING_SIZE;
    }

    void FixedUpdate() {
        if (!IsAlive || !GameStarted)
            return;
        Vector2 movement = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (movement.magnitude > 1)
            movement.Normalize();
        transform.localScale = Vector3.one * Scale;
        rigidbody.MovePosition(rigidbody.position + MOVEMENT_SPEED * Time.fixedDeltaTime * movement);
    }

    public void KillPlayer() {
        if (!IsAlive)
            return;

        IsAlive = false;
        LeanTween.value(backgroundMusic.gameObject, val => backgroundMusic.volume = val, backgroundMusic.volume, 0, .8f)
            .setOnComplete(() => backgroundMusic.Stop());
        loseSound.Play();
    }

    public void CollectPowerup(Powerup powerup) {
        
        Destroy(powerup.gameObject);
    }
}
