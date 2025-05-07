using System.Collections.Generic;
using UnityEngine;

public class SparkPoolManager : MonoBehaviour
{
    [SerializeField] private GameObject sparkPrefab;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private Transform poolParent;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (sparkPrefab == null)
        {
            Debug.LogError("Spark prefab is not assigned.");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject spark = Instantiate(sparkPrefab, poolParent);
            spark.SetActive(false);
            pool.Enqueue(spark);
        }
    }

    public GameObject GetSpark()
    {
        if (pool.Count > 0)
        {
            GameObject spark = pool.Dequeue();
            spark.SetActive(true);
            spark.transform.localScale = Vector3.one;
            return spark;
        }
        else
        {
            Debug.LogWarning("Spark pool is empty. Consider increasing pool size.");
            return null;
        }
    }

    public void ReturnSpark(GameObject spark)
    {
        if (spark == null) return;
        spark.transform.localScale = Vector3.one;
        if (spark.TryGetComponent(out CanvasGroup cg))
            cg.alpha = 1f;

        spark.SetActive(false);
        pool.Enqueue(spark);
    }
}


