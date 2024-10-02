using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const float MOVEMENT_SPEED = 5f;
    private const float GHOST_SPEED = 180f;
    private const float STARTING_SCALE = .4f;
    private const float MAX_SCALE = 1.5f;
    private const float MAX_TIME_NO_HOVER = 2.5f;
    // Not actually the time but ok
    private const float MAX_TIME_SAFE = .5f;
    private static readonly float STARTING_SIZE = Mathf.Sqrt(STARTING_SCALE) - MAX_TIME_SAFE;
    private static readonly float MAX_SIZE = Mathf.Sqrt(MAX_SCALE);
    private static readonly float SIZE_RANGE = MAX_SIZE - STARTING_SIZE;
    private static readonly float SIZE_DELTA = SIZE_RANGE / MAX_TIME_NO_HOVER;

    public bool GameStarted { get; private set; }
    public bool IsAlive { get; private set; }
    public float Scale => Mathf.Max(STARTING_SCALE, size * size);
    public float ScalePercent => (Scale - STARTING_SCALE) / (MAX_SCALE - STARTING_SCALE);
    public float SafePercent => Mathf.Clamp(1 - (size - STARTING_SIZE) / MAX_TIME_SAFE, 0f, 1f);
    public float GhostSpeed => GHOST_SPEED / Mathf.Sqrt(Scale) * (timeElapsed * timeElapsed / (100 * (timeElapsed + 60)) + 1);

    private new Camera camera;
    private new Rigidbody2D rigidbody;
    [SerializeField] private Ghost ghost;
    [SerializeField] private GameObject gate;
    [SerializeField] private GameObject tutorial;

    // Internal variable that Scale is based off of
    private float size;
    private float timeElapsed;

    void Awake() {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start() {
        size = STARTING_SIZE + MAX_TIME_SAFE;
        GameStarted = false;
        IsAlive = true;
        timeElapsed = 0f;
    }

    void Update() {
        camera.transform.position = new(transform.position.x, transform.position.y, camera.transform.position.z);
        bool isHoveringGhost = ghost.IsOverUI();

        // Don't do any logic if the game is not active
        if (!GameStarted) {
            if (!isHoveringGhost)
                return;
            GameStarted = true;
            Destroy(gate);
            tutorial.SetActive(false);
        }
        if (!IsAlive)
            return;
        
        transform.localScale = Vector3.one * Scale;
        if (isHoveringGhost) {
            size -= Time.deltaTime * SIZE_DELTA;
        }
        else {
            size += Time.deltaTime * SIZE_DELTA;
        }
        if (size > MAX_SIZE) {
            IsAlive = false;
            size = MAX_SIZE;
        }
        else if (size < STARTING_SIZE)
            size = STARTING_SIZE;
        timeElapsed += Time.deltaTime;
    }

    void FixedUpdate() {
        if (!IsAlive)
            return;
        Vector2 movement = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (movement.magnitude > 1)
            movement.Normalize();
        transform.localScale = Vector3.one * Scale;
        rigidbody.MovePosition(rigidbody.position + MOVEMENT_SPEED * Time.fixedDeltaTime * movement);
    }

    public void KillPlayer() {
        IsAlive = false;
    }

    public void CollectPowerup(Powerup powerup) {
        
        Destroy(powerup.gameObject);
    }
}
