using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrightnessSlider : MonoBehaviour
{
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Text brightnessLabel;
    [SerializeField] private Image brightnessOverlay; 

    void Start()
    {
        float savedBrightness = PlayerPrefs.GetFloat("Brightness", 1f);
        brightnessSlider.value = savedBrightness;
        ApplyBrightness(savedBrightness);
        OnBrightnessChanged(savedBrightness);
    }

    public void OnBrightnessChanged(float value)
    {
        ApplyBrightness(value);
        PlayerPrefs.SetFloat("Brightness", value);
    }

    private void ApplyBrightness(float value)
    {
        float alpha = Mathf.Lerp(0.7f, 0f, Mathf.Clamp01(value));

        if (brightnessOverlay != null)
        {
            Color overlayColor = brightnessOverlay.color;
            overlayColor.a = alpha;
            brightnessOverlay.color = overlayColor;
        }

        if (brightnessLabel != null)
        {
            brightnessLabel.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }
}


