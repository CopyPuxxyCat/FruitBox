using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class ComboPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private List<Image> comboIcons; // assign exactly 5 images (or however many)
    [SerializeField] private Slider progressBar;
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0f); // warm yellow
    [SerializeField] private Color completedColor = new Color(0.3f, 1f, 0.3f);

    [Header("References")]
    public FruitSpriteData fruitSpriteData; // assign in inspector (ScriptableObject mapping sprites)

    private Dictionary<FruitType, Sprite> fruitSprites;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
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
        ResetPanelInstant();
    }

    /// <summary>
    /// Update display with combo data and progress 0..1
    /// </summary>
    public void UpdateDisplay(ComboData comboData, float progress)
    {
        if (comboData == null)
        {
            ResetPanel();
            return;
        }

        int count = comboData.fruitSequence.Count;
        for (int i = 0; i < comboIcons.Count; i++)
        {
            if (i < count)
            {
                comboIcons[i].enabled = true;
                var type = comboData.fruitSequence[i];
                comboIcons[i].sprite = fruitSprites.ContainsKey(type) ? fruitSprites[type] : null;

                if (i < comboData.activeIndex)
                    comboIcons[i].color = completedColor;
                else if (i == comboData.activeIndex)
                    comboIcons[i].color = activeColor;
                else
                    comboIcons[i].color = normalColor;

                // active slot pulse
                if (i == comboData.activeIndex)
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

        if (progressBar)
            progressBar.value = Mathf.Clamp01(progress);

        // ensure panel visible
        FadeTo(1f, 0.15f);
    }

    public void ResetPanel()
    {
        ResetPanelInstant();
        FadeTo(1f, 0.15f);
    }

    private void ResetPanelInstant()
    {
        foreach (var icon in comboIcons)
        {
            if (icon) { icon.enabled = false; icon.transform.localScale = Vector3.one; }
        }
        if (progressBar) progressBar.value = 0f;
        if (canvasGroup) canvasGroup.alpha = 1f;
    }

    public void FadeOpacity(float alpha)
    {
        FadeTo(alpha, 0.25f);
    }

    private void FadeTo(float alpha, float duration)
    {
        if (canvasGroup == null) return;
        LeanTween.alphaCanvas(canvasGroup, alpha, duration).setEaseInOutQuad();
    }

    public void Blink(int repeat = 3)
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        LeanTween.alphaCanvas(canvasGroup, 0.3f, 0.2f).setLoopPingPong(repeat).setOnComplete(() =>
        {
            // restore to semi visible if desired
            canvasGroup.alpha = 0.7f;
        });
    }
}
