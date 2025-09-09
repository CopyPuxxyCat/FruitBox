
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FruitPrefabEntry
    {
        public FruitType fruitType;
        public GameObject prefab;
        public int initialPoolSize = 10;
    }

    [Header("Prefabs")]
    public List<FruitPrefabEntry> fruitPrefabs;

    private Dictionary<FruitType, Queue<GameObject>> poolDict;
    private BeatMap currentMap;
    private FDNAudioController audioController;

    void Awake()
    {
        audioController = FindObjectOfType<FDNAudioController>();
        InitPool();
    }

    private void InitPool()
    {
        poolDict = new Dictionary<FruitType, Queue<GameObject>>();

        foreach (var entry in fruitPrefabs)
        {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < entry.initialPoolSize; i++)
            {
                var go = Instantiate(entry.prefab, transform);
                go.SetActive(false);
                queue.Enqueue(go);
            }
            poolDict[entry.fruitType] = queue;
        }
    }

    public void LoadBeatmap(BeatMap map)
    {
        currentMap = map;
        StopAllCoroutines();
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        foreach (var note in currentMap.beatNotes)
        {
            yield return new WaitUntil(() => audioController.GetSongTime() >= note.spawnTimeSec);
            SpawnFruit(note);
        }
    }

    private void SpawnFruit(BeatNote note)
    {
        var obj = GetFromPool(note.fruitType);
        if (obj == null)
        {
            Debug.LogWarning($"No prefab for {note.fruitType}");
            return;
        }

        obj.transform.position = note.spawnPosition;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);

        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            Vector2 dir = Quaternion.Euler(0, 0, note.shootAngle) * Vector2.right;
            rb.AddForce(dir * note.shootSpeed, ForceMode2D.Impulse);
        }
    }

    private GameObject GetFromPool(FruitType type)
    {
        if (!poolDict.ContainsKey(type)) return null;

        var queue = poolDict[type];
        GameObject obj = null;

        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else
        {
            // nếu pool thiếu thì instantiate thêm (hoặc tuỳ bạn quyết định)
            var entry = fruitPrefabs.Find(e => e.fruitType == type);
            obj = Instantiate(entry.prefab, transform);
        }

        return obj;
    }

    public void ReturnToPool(FruitType type, GameObject obj)
    {
        obj.SetActive(false);
        if (poolDict.ContainsKey(type))
            poolDict[type].Enqueue(obj);
        else
            Destroy(obj);
    }
}

