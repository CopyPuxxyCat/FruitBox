using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResolutionSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Slider slider;

    private readonly Vector2Int[] resolutions = new Vector2Int[]
    {
        new Vector2Int(640, 360),   // Low
        new Vector2Int(1280, 720),  // Medium
        new Vector2Int(1920, 1080)  // High
    };

    private readonly string[] resolutionLabels = { "Low", "Medium", "High" };

    private void Start()
    {
        slider.onValueChanged.AddListener(OnSliderChanged);
        slider.wholeNumbers = true;

        int saved = PlayerPrefs.GetInt("ResolutionIndex", 1);
        slider.value = saved;
        ApplyResolution(saved);
    }

    private void OnSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        ApplyResolution(index);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    private void ApplyResolution(int index)
    {
        Vector2Int res = resolutions[index];
        label.text = resolutionLabels[index];
        Screen.SetResolution(res.x, res.y, FullScreenMode.Windowed);
    }
}

