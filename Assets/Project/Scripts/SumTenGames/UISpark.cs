using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UISpark : MonoBehaviour
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Animator animator;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        animator = GetComponent<Animator>();
    }

    public void ResetSpark(Vector2 anchoredPosition)
    {
        // Reset transform
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;

        // Reset alpha
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // Reset scale
        transform.localScale = Vector3.one;

        // Reset Animator nếu có
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 0f);
        }

        // Reset LeanTween (rất quan trọng!)
        LeanTween.cancel(gameObject);
    }
}



