using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BulletHellSystem : MonoBehaviour
{
    public Player player;
    public Ghost ghost;
    public TextMeshProUGUI roundText;
    public Image winCover;
    public RectTransform winScreen;
    public int roundCount;
    public float roundDelay;
    
    public int RoundIndex { get; private set; }
    public bool RoundActive { get; private set; }
    public const float BASE_BULLET_SPEED = 2.5f;
    public float BulletSpeed => BASE_BULLET_SPEED * (1 + RoundIndex * .08f);

    public float spawnInterval;
    public float spawnerDelay;
    public Bullet bulletPrefab;
    public Spawner spawnerPrefab;
    public AudioClip bulletSound;
    public AudioSource winSound;
    
    private BoxCollider2D boxCollider;
    private RectTransform roundTextRect;
    private Vector2 roundTextPos, roundTextHiddenPos;
    private float timeUntilNextRound;
    private bool playedWin;
    [SerializeField] private int debugRoundIndex;

    void Awake() {
        boxCollider = GetComponent<BoxCollider2D>();
        roundTextRect = roundText.rectTransform;
        roundTextPos = roundTextRect.anchoredPosition;
        roundTextHiddenPos = new(0, roundTextRect.sizeDelta.y + 10);
        roundTextRect.anchoredPosition = roundTextHiddenPos;
    }

    void Start() {
        RoundIndex = debugRoundIndex;
        RoundActive = false;
        timeUntilNextRound = 1f;
        playedWin = false;
    }

    void Update() {
        if (!player.GameStarted || !player.IsAlive || RoundActive)
            return;
        
        if (RoundIndex >= roundCount) {
            if (playedWin)
                return;
            playedWin = true;
            StartCoroutine(PlayOnWin());
        }
        else
            TryStartRound();
    }

    private IEnumerator PlayOnWin() {
        yield return new WaitForSeconds(roundDelay);
        winCover.color = new(1, 1, 1, 0);
        winCover.gameObject.SetActive(true);
        ghost.NoMoreGhost();
        float duration = 1f;
        LeanTween.alpha(winCover.rectTransform, 1f, duration);
        yield return new WaitForSeconds(duration);
        winScreen.gameObject.SetActive(true);
        winSound.Play();
        LeanTween.alpha(winCover.rectTransform, 0f, duration);
        yield return new WaitForSeconds(duration);
        winCover.gameObject.SetActive(false);
    }

    public void SpawnBullet(Vector3 position, Vector3 direction) {
        Bullet bullet = Instantiate(bulletPrefab, position, Quaternion.FromToRotation(Vector3.right, direction), transform);
        bullet.GetPosition = time => position + time * BulletSpeed * direction;
        bullet.lifetime = 10;
        AudioSource.PlayClipAtPoint(bulletSound, position, .4f);
    }

    private (Vector3, Vector3) GetRandomPositionOnEdge() {
        bool bottom = Random.value < .5f;
        if (Random.value < .5f) {
            float x = Random.Range(0, boxCollider.bounds.size.x) + boxCollider.bounds.min.x;
            float y = bottom ? boxCollider.bounds.min.y : boxCollider.bounds.max.y;
            return (new(x, y), bottom ? Vector3.up : Vector3.down);
        }
        else {
            float x = bottom ? boxCollider.bounds.min.x : boxCollider.bounds.max.x;
            float y = Random.Range(0, boxCollider.bounds.size.y) + boxCollider.bounds.min.y;
            return (new(x, y), bottom ? Vector3.right : Vector3.left);
        }
    }

    public void TryStartRound() {
        if ((timeUntilNextRound -= Time.deltaTime) > 0)
            return;
        
        RoundIndex++;
        RoundActive = true;
        timeUntilNextRound = roundDelay;
        StartCoroutine(StartRoundCoroutine());
    }

    private IEnumerator StartRoundCoroutine() {
        roundText.text = $"Wave {RoundIndex}/{roundCount}";
        roundTextRect.anchoredPosition = roundTextHiddenPos;
        float duration = .8f;
        // Tween round text
        LeanTween.move(roundTextRect, roundTextPos, duration).setEaseOutBack();
        yield return new WaitForSeconds(duration + 1.5f);
        LeanTween.move(roundTextRect, roundTextHiddenPos, duration).setEaseInBack();
        yield return new WaitForSeconds(duration);
        // Do random sequence based on round number
        void callback() => RoundActive = false;
        
        if (RoundIndex == 1)
            StartCoroutine(DoGrid(6, 5, callback));
        else if (RoundIndex == 2)
            StartCoroutine(DoDiagonal(6, 5, callback));
        else if (RoundIndex == 3)
            StartCoroutine(DoSpawner(3, 6, 2, SpawnerFireMode.Single, callback));
        else if (RoundIndex == 4)
            StartCoroutine(DoGrid(8, 5, callback));
        else if (RoundIndex == 5)
            StartCoroutine(DoGrid(10, 6, callback));
        else if (RoundIndex == 6)
            StartCoroutine(DoDiagonal(8, 6, callback));
        else if (RoundIndex == 7)
            StartCoroutine(DoSpawner(3, 6, 2.5f, SpawnerFireMode.Spread, callback));
        else if (RoundIndex == 8)
            StartCoroutine(DoDiagonal(9, 8, callback));
        else if (RoundIndex == 9)
            StartCoroutine(DoGrid(11, 8, callback));
        else if (RoundIndex == 10)
            StartCoroutine(DoSpawner(4, 8, 2.5f, SpawnerFireMode.Spread, callback));
        else {
            Debug.LogWarning($"No round sequence for round {RoundIndex}, waiting 1 second");
            LeanTween.delayedCall(1f, callback);
        }
    }

    private IEnumerator DoGrid(int perSecond, int duration, UnityAction callback) {
        int total = perSecond * duration;
        float t = 0;
        for (int i = 0; i < total; i++) {
            while (i > t * perSecond) {
                t += spawnInterval;
                yield return new WaitForSeconds(spawnInterval);
            }
            (Vector3 pos, Vector3 direction) = GetRandomPositionOnEdge();
            SpawnBullet(pos, direction);
        }
        callback();
    }

    private IEnumerator DoDiagonal(int perSecond, int duration, UnityAction callback) {
        int total = perSecond * duration;
        float t = 0;
        for (int i = 0; i < total; i++) {
            while (i > t * perSecond) {
                t += spawnInterval;
                yield return new WaitForSeconds(spawnInterval);
            }
            (Vector3 pos, Vector3 direction) = GetRandomPositionOnEdge();
            if (pos.x == boxCollider.bounds.min.x && pos.y <= boxCollider.bounds.center.y || pos.x <= boxCollider.bounds.center.x && pos.y == boxCollider.bounds.min.y)
                direction = Vector3.right + Vector3.up;
            else if (pos.x == boxCollider.bounds.max.x && pos.y > boxCollider.bounds.center.y || pos.x > boxCollider.bounds.center.x && pos.y == boxCollider.bounds.max.y)
                direction = Vector3.left + Vector3.down;
            else if (pos.x == boxCollider.bounds.min.x && pos.y > boxCollider.bounds.center.y || pos.x <= boxCollider.bounds.center.x && pos.y == boxCollider.bounds.max.y)
                direction = Vector3.right + Vector3.down;
            else if (pos.x == boxCollider.bounds.max.x && pos.y <= boxCollider.bounds.center.y || pos.x > boxCollider.bounds.center.x && pos.y == boxCollider.bounds.min.y)
                direction = Vector3.left + Vector3.up;
            direction.Normalize();
            SpawnBullet(pos, direction);
        }
        callback();
    }

    private IEnumerator DoSpawner(int numSpawners, int duration, float fireRate, SpawnerFireMode fireMode, UnityAction callback) {
        for (int i = 0; i < numSpawners; i++) {
            yield return new WaitForSeconds(spawnerDelay);
            (Vector3 pos, Vector3 _) = GetRandomPositionOnEdge();
            Spawner spawner = Instantiate(spawnerPrefab, pos, Quaternion.identity, transform);
            spawner.openPosition = Vector3.MoveTowards(pos, boxCollider.bounds.center, 2f);
            spawner.bulletHellSystem = this;
            spawner.lifetime = duration;
            spawner.target = player.transform;
            spawner.fireMode = fireMode;
            spawner.fireRate = fireRate;
        }
        yield return new WaitForSeconds(duration);
        callback();
    }
}
