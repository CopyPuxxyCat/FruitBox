using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubmitButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button submitButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image buttonImage;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pressedColor = Color.yellow;
    [SerializeField] private Color disabledColor = Color.gray;

    [Header("Animation Settings")]
    [SerializeField] private float pulseDuration = 0.5f;
    [SerializeField] private Vector3 pressScale = new Vector3(0.95f, 0.95f, 0.95f);

    [Header("Auto Hide Settings")]
    [SerializeField] private bool autoHideWhenNoActiveCombos = true;
    [SerializeField] private float hideDelay = 1f;

    private bool isEnabled = true;
    private ComboPanelManager comboPanelManager;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Get references
        if (submitButton == null)
            submitButton = GetComponent<Button>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        // Add CanvasGroup for fade effects
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Setup button
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitPressed);
        }

        // Set initial state
        UpdateVisualState();
    }

    private void Start()
    {
        comboPanelManager = ComboPanelManager.Instance;

        // Set initial button text
        if (buttonText != null)
        {
            buttonText.text = "SUBMIT\nCOMBO";
        }

        // Start monitoring active combos
        if (autoHideWhenNoActiveCombos)
        {
            InvokeRepeating(nameof(CheckActiveCombos), 0.5f, 0.5f);
        }
    }

    private void OnSubmitPressed()
    {
        if (!isEnabled || comboPanelManager == null) return;

        // Visual feedback
        PlayPressAnimation();

        // FIXED: Use correct method name
        comboPanelManager.SubmitComboManually();

        Debug.Log("Submit button pressed - clearing active combos");

        // Provide haptic feedback on mobile
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    private void PlayPressAnimation()
    {
        // Scale animation for press feedback
        LeanTween.cancel(gameObject);

        transform.localScale = Vector3.one;
        LeanTween.scale(gameObject, pressScale, 0.1f)
            .setEaseOutQuad()
            .setOnComplete(() =>
            {
                LeanTween.scale(gameObject, Vector3.one, 0.1f)
                    .setEaseOutQuad();
            });

        // Color flash
        if (buttonImage != null)
        {
            var originalColor = buttonImage.color;
            LeanTween.color(buttonImage.rectTransform, pressedColor, 0.1f)
                .setOnComplete(() =>
                {
                    LeanTween.color(buttonImage.rectTransform, originalColor, 0.2f);
                });
        }
    }

    private void CheckActiveCombos()
    {
        if (comboPanelManager == null) return;

        // Check if there are any active combos
        bool hasActiveCombos = HasActiveCombos();

        if (hasActiveCombos && canvasGroup.alpha < 1f)
        {
            // Show button when there are active combos
            ShowButton();
        }
        else if (!hasActiveCombos && canvasGroup.alpha > 0f)
        {
            // Hide button when no active combos (with delay)
            Invoke(nameof(HideButtonIfStillNoActiveCombos), hideDelay);
        }
    }

    private bool HasActiveCombos()
    {
        // Since we don't have direct access to activeCombos list,
        // we'll use a simple approach - check if the manager exists and is active
        // You might want to add a public method to ComboPanelManager to check this
        return comboPanelManager != null && comboPanelManager.gameObject.activeInHierarchy;
    }

    private void HideButtonIfStillNoActiveCombos()
    {
        if (!HasActiveCombos())
        {
            HideButton();
        }
    }

    private void ShowButton()
    {
        CancelInvoke(nameof(HideButtonIfStillNoActiveCombos));

        if (canvasGroup != null)
        {
            LeanTween.alphaCanvas(canvasGroup, 1f, 0.3f).setEaseOutQuad();
        }

        SetEnabled(true);
    }

    private void HideButton()
    {
        if (canvasGroup != null)
        {
            LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f).setEaseInQuad();
        }
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;

        if (submitButton != null)
            submitButton.interactable = enabled;

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (buttonImage == null) return;

        Color targetColor = isEnabled ? normalColor : disabledColor;
        buttonImage.color = targetColor;

        // Update text alpha
        if (buttonText != null)
        {
            var textColor = buttonText.color;
            textColor.a = isEnabled ? 1f : 0.5f;
            buttonText.color = textColor;
        }
    }

    public void StartUrgentPulse()
    {
        if (!isEnabled) return;

        LeanTween.cancel(gameObject);

        // Pulse animation to indicate urgency
        LeanTween.scale(gameObject, Vector3.one * 1.1f, pulseDuration)
            .setEaseInOutSine()
            .setLoopPingPong();

        // Also add color pulse for more visibility
        if (buttonImage != null)
        {
            var originalColor = buttonImage.color;
            LeanTween.color(buttonImage.rectTransform, pressedColor, pulseDuration)
                .setEaseInOutSine()
                .setLoopPingPong()
                .setOnComplete(() =>
                {
                    if (buttonImage != null)
                        buttonImage.color = originalColor;
                });
        }
    }

    public void StopUrgentPulse()
    {
        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one;

        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }

    private void Update()
    {
        // Keyboard shortcuts (for testing on PC)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (isEnabled)
                OnSubmitPressed();
        }
    }

    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
        CancelInvoke();
    }

    // Public methods for external control
    public void ForceShow()
    {
        ShowButton();
    }

    public void ForceHide()
    {
        HideButton();
    }

    public bool IsVisible()
    {
        return canvasGroup != null && canvasGroup.alpha > 0f;
    }
}