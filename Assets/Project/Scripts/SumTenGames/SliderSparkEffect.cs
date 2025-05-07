using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderSparkEffect : MonoBehaviour
{
    [SerializeField] private UISparkSpawner sparkSpawner;
    private Slider slider;
    private float lastValue;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (slider == null || sparkSpawner == null) return;

        sparkSpawner.SetSpawnPoint(slider.handleRect);
        lastValue = slider.value;
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        if (Mathf.Abs(value - lastValue) > 0.01f)
        {
            sparkSpawner.SpawnSparks();
            lastValue = value;
        }
    }
}




