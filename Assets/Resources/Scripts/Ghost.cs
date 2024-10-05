using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ghost : MonoBehaviour
{
    private const int WIDTH = 720, HEIGHT = 480;
    private const float MIN_WHITENESS = .2f;
    private static readonly Color DARK_COLOR = new(MIN_WHITENESS, MIN_WHITENESS, MIN_WHITENESS);

    private RectTransform rectTransform;
    private GraphicRaycaster raycaster;
    private Image outerImage;
    private System.Random random;
    private Player player;
    [SerializeField] private Image cover;

    private Vector2 targetPos;
    private bool playingDeathAnimation;
    
    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        raycaster = GameObject.FindWithTag("GameController").GetComponent<GraphicRaycaster>();
        outerImage = GetComponent<Image>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        random = new();
        outerImage.alphaHitTestMinimumThreshold = .8f;
    }

    void Start() {
        targetPos = rectTransform.anchoredPosition;
        playingDeathAnimation = false;
    }

    private Vector2 GetTargetPos() {
        Vector2 myPos = rectTransform.anchoredPosition;
        if ((targetPos - myPos).sqrMagnitude < .1f) {
            targetPos = new Vector2(random.Next(0, WIDTH), random.Next(0, HEIGHT));
        }
        float radius = 50f * player.Scale;
        targetPos.x = Mathf.Clamp(targetPos.x, radius, WIDTH - radius);
        targetPos.y = Mathf.Clamp(targetPos.y, radius, HEIGHT - radius);
        return targetPos;
    }

    void Update() {
        if (playingDeathAnimation || player.GhostDead)
            return;

        transform.localScale = Vector3.one * player.Scale;
        float scalePercent = player.ScalePercent, safePercent = player.SafePercent;
        Color color = Color.Lerp(DARK_COLOR, Color.white, Mathf.Sqrt(scalePercent));
        if (player.SafePercent > 0f)
            color = Color.Lerp(color, Color.green, safePercent * .25f);
        float alpha = Mathf.Min(1f, scalePercent * 1.5f + .6f);
        color.a = alpha;
        outerImage.color = color;

        // Don't do any logic if the game is not active
        if (!player.GameStarted)
            return;
        if (!player.IsAlive) {
            if (!playingDeathAnimation) {
                playingDeathAnimation = true;
                StartCoroutine(StartDeathAnimation());
            }
            return;
        }

        // Move this ghost towards the target position
        Vector2 target = GetTargetPos();
        Vector2 myPos = rectTransform.anchoredPosition;
        Vector2 direction = target - myPos;
        float travelDistance = player.GhostSpeed * Time.deltaTime;
        if (direction.magnitude > travelDistance)
            direction = direction.normalized * travelDistance;
        rectTransform.anchoredPosition = myPos + direction;
    }

    private IEnumerator StartDeathAnimation() {
        LeanTween.move(rectTransform, new Vector2(WIDTH / 2, HEIGHT / 2), 1f).setEaseOutQuad();
        LeanTween.color(outerImage.rectTransform, new Color(1f, 0, 0f, 1f), 1f).setEaseOutQuad();
        transform.LeanScale(Vector3.one * 10f, 1f).setEaseOutQuad();
        yield return new WaitForSeconds(.5f);
        
        TextMeshProUGUI loseText = cover.transform.Find("LoseText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI loseRoundText = cover.transform.Find("LoseRound").GetComponent<TextMeshProUGUI>();
        loseText.color = Color.clear;
        loseRoundText.color = Color.clear;
        loseRoundText.text = $"Lost on\nWave {player.bulletHellSystem.RoundIndex}/{player.bulletHellSystem.roundCount}";
        Button button = cover.transform.Find("Button").GetComponent<Button>();
        button.gameObject.SetActive(false);
        cover.gameObject.SetActive(true);
        LeanTween.value(cover.gameObject, alpha => cover.color = new Color(0, 0, 0, alpha), 0, 1, 1f).setEaseOutQuad();
        yield return new WaitForSeconds(.5f);

        LeanTween.value(loseText.gameObject, alpha => loseText.color = new Color(1, 1, 1, alpha), 0, 1, 1f).setEaseOutQuad();
        yield return new WaitForSeconds(1f);

        LeanTween.value(loseText.gameObject, alpha => loseRoundText.color = new Color(1, 1, 1, alpha), 0, 1, 1f).setEaseOutQuad();
        yield return new WaitForSeconds(1f);

        button.gameObject.SetActive(true);
    }

    public bool IsOverUI() {
        PointerEventData pointerData = new(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new();
        raycaster.Raycast(pointerData, results);
        return results.FindIndex(result => result.gameObject == gameObject) >= 0;
    }

    public void RestartScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NoMoreGhost() {
        player.GhostDead = true;
        LeanTween.scale(rectTransform, Vector3.zero, .7f).setEaseInBack();
    }
}
