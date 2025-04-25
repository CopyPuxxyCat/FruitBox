using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicSettingsManager : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Slider slider;
    [SerializeField] private RawImage outputImage;
    [SerializeField] private Camera mainCamera;

    [Header("Popup")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private RenderTexture renderTexture;
    private int pendingIndex = 1;
    private int currentIndex = 1;

    private readonly Vector2Int[] internalResolutions = new Vector2Int[]
    {
        new Vector2Int(640, 360),   // Low
        new Vector2Int(1280, 720),  // Medium
        new Vector2Int(1920, 1080)  // High
    };

    private readonly string[] labels = { "Low", "Medium", "High" };

    private void Start()
    {
        slider.onValueChanged.AddListener(OnSliderChanged);
        slider.wholeNumbers = true;

        int saved = PlayerPrefs.GetInt("GraphicsSetting", 1);
        slider.value = saved;
        currentIndex = saved;
        pendingIndex = saved;

        UpdateLabel(saved);
        ApplyGraphicsSettings(saved);

        confirmButton.onClick.AddListener(ConfirmApply);
        cancelButton.onClick.AddListener(ClosePopup);

        popupPanel.SetActive(false);
    }

    private void OnSliderChanged(float value)
    {
        pendingIndex = Mathf.RoundToInt(value);
        UpdateLabel(pendingIndex);
        if (pendingIndex != currentIndex)
            popupPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        slider.value = currentIndex;
        popupPanel.SetActive(false);
    }

    private void ConfirmApply()
    {
        ApplyGraphicsSettings(pendingIndex);
        PlayerPrefs.SetInt("GraphicsSetting", pendingIndex);
        currentIndex = pendingIndex;
        popupPanel.SetActive(false);
    }

    private void ApplyGraphicsSettings(int index)
    {
        // Release cũ trước khi tạo mới
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        // Set internal resolution
        Vector2Int res = internalResolutions[index];
        renderTexture = new RenderTexture(res.x, res.y, 24);
        renderTexture.filterMode = FilterMode.Bilinear;

        if (mainCamera != null)
            mainCamera.targetTexture = renderTexture;

        if (outputImage != null)
        {
            outputImage.texture = renderTexture;

            // Đảm bảo RawImage scale đúng full màn hình
            outputImage.rectTransform.anchorMin = Vector2.zero;
            outputImage.rectTransform.anchorMax = Vector2.one;
            outputImage.rectTransform.offsetMin = Vector2.zero;
            outputImage.rectTransform.offsetMax = Vector2.zero;
        }

        // Set Quality Settings tương ứng
        switch (index)
        {
            case 0: // Low
                QualitySettings.shadowDistance = 0f;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.globalTextureMipmapLimit = 2;
                QualitySettings.antiAliasing = 0;
                break;
            case 1: // Medium
                QualitySettings.shadowDistance = 50f;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.globalTextureMipmapLimit = 1;
                QualitySettings.antiAliasing = 2;
                break;
            case 2: // High
                QualitySettings.shadowDistance = 100f;
                QualitySettings.shadowResolution = ShadowResolution.High;
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.antiAliasing = 4;
                break;
        }
    }

    private void UpdateLabel(int index)
    {
        if (label != null)
            label.text = labels[index];
    }
}

