using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class ComboPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private List<Image> comboIcons;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0f);
    [SerializeField] private Color completedColor = new Color(0.3f, 1f, 0.3f);

    [Header("References")]
    public FruitSpriteData fruitSpriteData;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float phase2Duration = 1f;

    private Dictionary<FruitType, Sprite> fruitSprites;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    // Current combo data reference
    private ComboData currentComboData;

    // Pool management
    private bool isInUse;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        if (fruitSpriteData != null)
        {
            fruitSprites = new Dictionary<FruitType, Sprite>
            {
                { FruitType.Watermelon, fruitSpriteData.watermelonSprite },
                { FruitType.Apple, fruitSpriteData.appleSprite },
                { FruitType.Orange, fruitSpriteData.orangeSprite },
                { FruitType.Banana, fruitSpriteData.bananaSprite },
                { FruitType.Grape, fruitSpriteData.grapeSprite }
            };
        }

        ResetPanelImmediate();
    }

    public void InitializeWithCombo(ComboData comboData)
    {
        currentComboData = comboData;
        comboData.uiPanel = this;
        isInUse = true;

        // Set to phase 1 position
        SetPhase1Position();

        // Update display
        UpdateComboDisplay();

        // Fade in
        FadeTo(1f, 0.15f);

        // Start phase 1
        StartPhase1();
    }

    private void SetPhase1Position()
    {
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void SetPhase2Position()
    {
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void StartPhase1()
    {
        if (currentComboData == null) return;

        currentComboData.currentPhase = ComboPhase.Phase1_Active;
        currentComboData.phaseStartTime = Time.time;

        StartCoroutine(Phase1Coroutine());
    }

    private IEnumerator Phase1Coroutine()
    {
        float phase1Duration = currentComboData.GetPhase1Duration();
        float startTime = currentComboData.phaseStartTime;

        while (Time.time - startTime < phase1Duration && currentComboData.IsActive())
        {
            // Update progress bar
            float progress = 1f - (Time.time - startTime) / phase1Duration;
            if (progressBar) progressBar.value = Mathf.Clamp01(progress);

            // Update combo display
            UpdateComboDisplay();

            yield return null;
        }

        // Check if combo is completed or expired
        if (currentComboData.isCompleted)
        {
            // Combo completed, return to pool immediately
            ReturnToPool();
        }
        else if (currentComboData.IsActive())
        {
            // Time's up, start phase 2
            StartPhase2();
        }
    }

    private void StartPhase2()
    {
        if (currentComboData == null) return;

        currentComboData.currentPhase = ComboPhase.Phase2_Sliding;
        currentComboData.phaseStartTime = Time.time;

        // Notify manager that this combo is entering phase 2
        ComboPanelManager.Instance.OnComboEnterPhase2(currentComboData);

        StartCoroutine(Phase2SlideCoroutine());
    }

    private IEnumerator Phase2SlideCoroutine()
    {
        // Slide up animation
        Vector2 fromMin = new Vector2(0, 0);
        Vector2 fromMax = new Vector2(1, 0.5f);
        Vector2 toMin = new Vector2(0, 0.5f);
        Vector2 toMax = new Vector2(1, 1);

        float elapsedTime = 0f;
        while (elapsedTime < slideDuration)
        {
            float t = elapsedTime / slideDuration;
            t = Mathf.SmoothStep(0, 1, t); // Smooth animation curve

            rectTransform.anchorMin = Vector2.Lerp(fromMin, toMin, t);
            rectTransform.anchorMax = Vector2.Lerp(fromMax, toMax, t);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position
        SetPhase2Position();

        // Start phase 2 final
        currentComboData.currentPhase = ComboPhase.Phase2_Final;
        currentComboData.phaseStartTime = Time.time;

        StartCoroutine(Phase2FinalCoroutine());
    }

    private IEnumerator Phase2FinalCoroutine()
    {
        float startTime = currentComboData.phaseStartTime;

        // Blink effect during last 0.5s
        bool isBlinking = false;

        while (Time.time - startTime < phase2Duration && currentComboData.IsActive())
        {
            float remainingTime = phase2Duration - (Time.time - startTime);

            // Start blinking in last 0.5s
            if (remainingTime <= 0.5f && !isBlinking)
            {
                isBlinking = true;
                Blink(3);
            }

            // Update combo display
            UpdateComboDisplay();

            yield return null;
        }

        // Check if still active (not completed)
        if (currentComboData.IsActive())
        {
            // Force expire the combo
            currentComboData.ForceExpire();
            currentComboData.currentPhase = ComboPhase.Expired;

            // Clear fruits and notify manager
            ComboPanelManager.Instance.ClearComboActiveFruits(currentComboData);
        }

        // Return to pool
        ReturnToPool();
    }

    private void UpdateComboDisplay()
    {
        if (currentComboData == null) return;

        int count = currentComboData.fruitSequence.Count;
        for (int i = 0; i < comboIcons.Count; i++)
        {
            if (i < count)
            {
                comboIcons[i].enabled = true;
                var type = currentComboData.fruitSequence[i];
                comboIcons[i].sprite = fruitSprites.ContainsKey(type) ? fruitSprites[type] : null;

                if (i < currentComboData.activeIndex)
                    comboIcons[i].color = completedColor;
                else if (i == currentComboData.activeIndex)
                    comboIcons[i].color = activeColor;
                else
                    comboIcons[i].color = normalColor;

                // Active slot pulse
                if (i == currentComboData.activeIndex && currentComboData.IsActive())
                {
                    LeanTween.cancel(comboIcons[i].gameObject);
                    LeanTween.scale(comboIcons[i].gameObject, Vector3.one * 1.18f, 0.18f).setEasePunch();
                }
                else
                {
                    comboIcons[i].transform.localScale = Vector3.one;
                }
            }
            else
            {
                comboIcons[i].enabled = false;
            }
        }
    }

    public void OnComboCompleted()
    {
        if (currentComboData != null)
        {
            currentComboData.ForceComplete();
            StopAllCoroutines();
            ReturnToPool();
        }
    }

    public void OnComboSubmitted()
    {
        OnComboCompleted();
    }

    private void ReturnToPool()
    {
        if (!isInUse) return;

        StopAllCoroutines();

        // Clean up
        if (currentComboData != null)
        {
            currentComboData.uiPanel = null;
        }

        ResetPanelImmediate();
        currentComboData = null;
        isInUse = false;

        // Return to pool
        ComboPanelManager.Instance.ReturnPanelToPool(this);
    }

    private void ResetPanelImmediate()
    {
        // Reset UI elements
        foreach (var icon in comboIcons)
        {
            if (icon)
            {
                icon.enabled = false;
                icon.transform.localScale = Vector3.one;
                LeanTween.cancel(icon.gameObject);
            }
        }

        if (progressBar) progressBar.value = 0f;

        // Reset transform
        SetPhase1Position();

        // Reset canvas group
        if (canvasGroup)
        {
            LeanTween.cancel(gameObject);
            canvasGroup.alpha = 0f;
        }

        // Deactivate
        gameObject.SetActive(false);
    }

    private void FadeTo(float alpha, float duration)
    {
        if (canvasGroup == null) return;
        LeanTween.alphaCanvas(canvasGroup, alpha, duration).setEaseInOutQuad();
    }

    public void Blink(int repeat = 3)
    {
        if (canvasGroup == null) return;
        LeanTween.alphaCanvas(canvasGroup, 0.3f, 0.2f).setLoopPingPong(repeat).setOnComplete(() =>
        {
            if (canvasGroup != null) canvasGroup.alpha = 0.7f;
        });
    }

    // Public getters
    public bool IsInUse => isInUse;
    public ComboData GetComboData() => currentComboData;
}