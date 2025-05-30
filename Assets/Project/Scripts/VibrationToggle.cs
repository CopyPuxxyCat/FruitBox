
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;

#if UNITY_IOS || UNITY_ANDROID
public class VibrationToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private TMP_Text onOffLabel;

    [Header("Localized Text")]
    [SerializeField] private LocalizedString onText;
    [SerializeField] private LocalizedString offText;

    [Header("Settings")]
    [SerializeField] private bool isVibrateOn = true;

    private Vector2 onPos = new Vector2(-180f, 0f);
    private Vector2 offPos = new Vector2(20f, 0f);

    public void ToggleVibration()
    {
        isVibrateOn = !isVibrateOn;

        UpdateUI();
        PlayerPrefs.SetInt("Vibration", isVibrateOn ? 1 : 0);

        if (isVibrateOn)
            Handheld.Vibrate();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("Vibration"))
            isVibrateOn = PlayerPrefs.GetInt("Vibration") == 1;

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        UpdateUI();
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale obj)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (onOffLabel != null)
        {
            var localized = isVibrateOn ? onText : offText;
            onOffLabel.text = localized.GetLocalizedString();
        }

        if (buttonRect != null)
        {
            Vector2 targetPos = isVibrateOn ? onPos : offPos;
            LeanTween.moveLocal(buttonRect.gameObject, targetPos, 0.3f).setEaseOutExpo();
        }
    }
}
#else
// Phiên bản rỗng nếu không phải mobile
public class VibrationToggle : MonoBehaviour
{
    // Không làm gì trên PC/WebGL
}
#endif




