using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VibrationToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform buttonRect;  
    [SerializeField] private TMP_Text onOffLabel;        

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

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (onOffLabel != null)
            onOffLabel.text = isVibrateOn ? "ON" : "OFF";

        if (buttonRect != null)
        {
            Vector2 targetPos = isVibrateOn ? onPos : offPos;
            LeanTween.moveLocal(buttonRect.gameObject, targetPos, 0.3f).setEaseOutExpo();
        }
    }
}


