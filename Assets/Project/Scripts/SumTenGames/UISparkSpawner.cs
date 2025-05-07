using System.Collections;
using UnityEngine;

public class UISparkSpawner : MonoBehaviour
{
    [SerializeField] private SparkPoolManager poolManager;
    [SerializeField] private int sparksPerBurst = 5;

    private RectTransform spawnFromTransform;

    public void SetSpawnPoint(RectTransform handle)
    {
        spawnFromTransform = handle;
    }

    public void SpawnSparks()
    {
        if (spawnFromTransform == null || poolManager == null)
            return;

        for (int i = 0; i < sparksPerBurst; i++)
        {
            GameObject spark = poolManager.GetSpark();
            if (spark == null) continue;

            RectTransform rt = spark.GetComponent<RectTransform>();
            if (rt == null) continue;

            Vector2 localPos;
            RectTransform parentRect = rt.parent as RectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                RectTransformUtility.WorldToScreenPoint(null, spawnFromTransform.position),
                null,
                out localPos))
            {
                // Reset spark
                if (spark.TryGetComponent(out CanvasGroup cg))
                {
                    cg.alpha = 1f; // Reset alpha về 1

                    // Tween alpha bằng CanvasGroup
                    LeanTween.value(spark, 1f, 0f, 0.6f).setOnUpdate((float val) => {
                        if (cg != null) cg.alpha = val;
                    });
                }

                // Animate
                Vector2 direction = Random.insideUnitCircle.normalized * Random.Range(30f, 80f);
                Vector3 target = localPos + direction;

                LeanTween.move(rt, target, 0.6f).setEaseOutCubic();
                LeanTween.scale(rt, Vector3.zero, 0.6f).setEaseInBack();
                LeanTween.rotateZ(rt.gameObject, Random.Range(180f, 360f), 0.6f).setEaseOutQuad();

                StartCoroutine(DelayedReturn(spark, 0.6f));
            }
        }
    }

    private IEnumerator DelayedReturn(GameObject spark, float delay)
    {
        yield return new WaitForSeconds(delay);
        poolManager.ReturnSpark(spark);
    }
}





