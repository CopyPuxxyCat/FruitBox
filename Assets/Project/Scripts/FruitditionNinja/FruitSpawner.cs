using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FruitPrefabEntry
    {
        public FruitType fruitType;
        public GameObject prefab; // Prefab phải có FruitBehavior script
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

                // Ensure fruit has FruitBehavior component
                var fruitBehavior = go.GetComponent<FruitBehavior>();
                if (fruitBehavior == null)
                {
                    Debug.LogError($"Fruit prefab {entry.prefab.name} missing FruitBehavior component!");
                }

                go.SetActive(false);
                queue.Enqueue(go);
            }
            poolDict[entry.fruitType] = queue;
        }
    }

    public void LoadBeatmap(BeatMap map)
    {
        currentMap = map;
        foreach (var note in map.beatNotes)
        {
            Debug.Log($"[Combo {note.comboId}] Glow at {note.glowTimeSec:F2}s " +
                      $"Spawn at {note.spawnTimeSec:F2}s " +
                      $"Peak {note.peakPosition} " +
                      $"Spawn {note.spawnPosition} " +
                      $"Fruit {note.fruitType}");
        }
        StopAllCoroutines();
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        foreach (var note in currentMap.beatNotes)
        {
            // Wait until it's time to spawn this fruit
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

        // Position and activate the fruit
        obj.transform.position = note.spawnPosition;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);

        // Initialize fruit behavior with the beat note data
        var fruitBehavior = obj.GetComponent<FruitBehavior>();
        if (fruitBehavior != null)
        {
            fruitBehavior.Initialize(note, this);
        }
        else
        {
            Debug.LogError($"Fruit {obj.name} missing FruitBehavior component!");

            // Fallback: use old physics method
            var rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                Vector2 dir = Quaternion.Euler(0, 0, note.shootAngle) * Vector2.right;
                rb.AddForce(dir * note.shootSpeed, ForceMode2D.Impulse);
            }
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
            // If pool is empty, instantiate more
            var entry = fruitPrefabs.Find(e => e.fruitType == type);
            if (entry != null)
            {
                obj = Instantiate(entry.prefab, transform);

                // Ensure new object has FruitBehavior
                var fruitBehavior = obj.GetComponent<FruitBehavior>();
                if (fruitBehavior == null)
                {
                    Debug.LogError($"New fruit prefab {entry.prefab.name} missing FruitBehavior component!");
                }
            }
        }

        return obj;
    }

    public void ReturnToPool(FruitType type, GameObject obj)
    {
        obj.SetActive(false);
        if (poolDict.ContainsKey(type))
        {
            poolDict[type].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    // Test method to manually spawn a fruit
    [ContextMenu("Test Spawn Random Fruit")]
    public void TestSpawnFruit()
    {
        var testNote = new BeatNote
        {
            comboId = 0,
            glowFrame = 60,
            glowTimeSec = 1f,
            spawnTimeSec = 0f,
            spawnPosition = new Vector2(0, -5),
            peakPosition = new Vector2(0, 5),
            shootSpeed = 10f,
            shootAngle = 90f,
            fruitType = (FruitType)Random.Range(0, System.Enum.GetValues(typeof(FruitType)).Length)
        };

        SpawnFruit(testNote);
    }
}