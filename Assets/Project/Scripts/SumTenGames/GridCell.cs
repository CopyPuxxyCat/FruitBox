using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GridCell : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private Image background;

    private Color normalColor = Color.white;
    private Color highlightColor = Color.green;

    private int number;
    private int index;

    public void SetNumber(int value)
    {
        number = value;
        numberText.text = value.ToString();
    }

    public int GetNumber() => number;

    public void SetIndex(int idx) => index = idx;

    public int GetIndex() => index;

    public void Highlight(bool active)
    {
        if (background == null) return;

        background.color = active ? highlightColor : normalColor;
    }

    public void Hide()
    {
        StartCoroutine(HideWithFade());
    }

    private IEnumerator HideWithFade()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        float duration = 0.3f;
        float elapsed = 0f;
        float startAlpha = cg.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }

        cg.alpha = 1f;
        gameObject.SetActive(false);
    }
}
