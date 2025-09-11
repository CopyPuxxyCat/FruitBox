using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboPanelManager : MonoBehaviour
{
    public static ComboPanelManager Instance;

    [Header("Pool Settings")]
    [SerializeField] private GameObject comboPanelPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Transform panelContainer; // Parent object to hold active panels

    [Header("Timing")]
    [SerializeField] private float spawnOffset = -1f; // Spawn panels 1s before first fruit

    private Queue<ComboPanel> panelPool;
    private List<ComboData> allCombos;
    private Queue<ComboData> pendingCombos;
    private List<ComboData> activeCombos;

    private FruitSpawner spawner;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        InitializePool();

        allCombos = new List<ComboData>();
        pendingCombos = new Queue<ComboData>();
        activeCombos = new List<ComboData>();
        spawner = FindObjectOfType<FruitSpawner>();
    }

    private void OnEnable()
    {
        FruitBehavior.OnFruitSliced += OnFruitSliced;
    }

    private void OnDisable()
    {
        FruitBehavior.OnFruitSliced -= OnFruitSliced;
    }

    private void InitializePool()
    {
        panelPool = new Queue<ComboPanel>();

        // Create panel container if not assigned
        if (panelContainer == null)
        {
            GameObject container = new GameObject("ComboPanelContainer");
            container.transform.SetParent(transform);
            panelContainer = container.transform;
        }

        // Create pool objects
        for (int i = 0; i < poolSize; i++)
        {
            GameObject panelObj = Instantiate(comboPanelPrefab, panelContainer);
            ComboPanel panel = panelObj.GetComponent<ComboPanel>();

            if (panel == null)
            {
                Debug.LogError("ComboPanelPrefab must have ComboPanel component!");
                continue;
            }

            panelObj.SetActive(false);
            panelPool.Enqueue(panel);
        }

        Debug.Log($"Initialized combo panel pool with {panelPool.Count} panels");
    }

    public void InitializeCombos(BeatMap beatMap)
    {
        // Stop any existing spawn coroutine
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        // Clear existing data
        ClearAllCombos();

        // Group beatNotes into combos
        var grouped = beatMap.beatNotes.GroupBy(n => n.comboId).OrderBy(g => g.Min(n => n.spawnTimeSec));

        foreach (var group in grouped)
        {
            var sortedNotes = group.OrderBy(n => n.spawnTimeSec).ToList();
            var sequence = sortedNotes.Select(n => n.fruitType).ToList();
            float firstFruitSpawnTime = sortedNotes.First().spawnTimeSec;

            ComboData combo = new ComboData(group.Key, sequence, firstFruitSpawnTime);
            allCombos.Add(combo);
            pendingCombos.Enqueue(combo);
        }

        Debug.Log($"Initialized {allCombos.Count} combos");

        // Start spawn coroutine
        spawnCoroutine = StartCoroutine(SpawnComboCoroutine());
    }

    private IEnumerator SpawnComboCoroutine()
    {
        var audioController = FindObjectOfType<FDNAudioController>();
        if (audioController == null)
        {
            Debug.LogError("FDNAudioController not found!");
            yield break;
        }

        while (pendingCombos.Count > 0)
        {
            ComboData nextCombo = pendingCombos.Peek();
            float targetSpawnTime = nextCombo.spawnTime + spawnOffset;

            // Wait until it's time to spawn this combo panel
            yield return new WaitUntil(() => audioController.GetSongTime() >= targetSpawnTime);

            // Spawn the combo panel
            SpawnComboPanel(pendingCombos.Dequeue());
        }
    }

    private void SpawnComboPanel(ComboData comboData)
    {
        ComboPanel panel = GetPanelFromPool();
        if (panel == null)
        {
            Debug.LogWarning("No available panels in pool! Consider increasing pool size.");
            return;
        }

        // Initialize panel with combo data
        panel.gameObject.SetActive(true);
        panel.InitializeWithCombo(comboData);

        // Add to active combos
        activeCombos.Add(comboData);

        NotifyActiveComboCountChanged();

        Debug.Log($"Spawned combo panel for combo {comboData.comboId} with {comboData.fruitSequence.Count} fruits");
    }

    private ComboPanel GetPanelFromPool()
    {
        if (panelPool.Count > 0)
        {
            return panelPool.Dequeue();
        }

        // Pool is empty, try to create more
        if (comboPanelPrefab != null && panelContainer != null)
        {
            GameObject panelObj = Instantiate(comboPanelPrefab, panelContainer);
            ComboPanel panel = panelObj.GetComponent<ComboPanel>();

            if (panel != null)
            {
                Debug.Log("Created additional panel due to pool shortage");
                return panel;
            }
        }

        return null;
    }

    public void ReturnPanelToPool(ComboPanel panel)
    {
        if (panel == null) return;

        panel.gameObject.SetActive(false);
        panelPool.Enqueue(panel);

        // Remove from active combos if it's there
        ComboData comboData = panel.GetComboData();
        if (comboData != null)
        {
            activeCombos.Remove(comboData);
            NotifyActiveComboCountChanged();
        }
    }

    public void OnComboEnterPhase2(ComboData comboData)
    {
        // This is called when a combo enters phase 2 (sliding up)
        // This is the perfect time to spawn the next combo panel if available

        if (pendingCombos.Count > 0)
        {
            // Note: The spawn coroutine will handle the timing automatically
            Debug.Log($"Combo {comboData.comboId} entered phase 2");
        }
    }

    public void AddFruitToCombo(int comboId, GameObject fruit)
    {
        ComboData combo = activeCombos.FirstOrDefault(c => c.comboId == comboId);
        if (combo != null)
        {
            combo.activeFruits.Add(fruit);
        }
    }

    public void ClearComboActiveFruits(ComboData combo)
    {
        if (combo == null || spawner == null) return;

        var fruitsToRemove = combo.activeFruits.ToList(); // Create copy to avoid modification during iteration

        foreach (var go in fruitsToRemove)
        {
            if (go == null) continue;

            var fb = go.GetComponent<FruitBehavior>();
            if (fb != null)
            {
                // Force fruit to sliced state to prevent further coroutines
                if (fb.GetCurrentState() != FruitState.Sliced && fb.GetCurrentState() != FruitState.Destroyed)
                {
                    // Stop all coroutines before returning to pool
                    fb.StopAllCoroutines();
                    LeanTween.cancel(go);
                }

                spawner.ReturnToPool(fb.GetFruitType(), go);
            }
            else
            {
                Destroy(go);
            }
        }

        combo.activeFruits.Clear();
    }

    public void SubmitComboManually()
    {
        // Submit all active combos
        var combosToSubmit = activeCombos.Where(c => c.IsActive()).ToList();

        foreach (var combo in combosToSubmit)
        {
            // Clear fruits
            ClearComboActiveFruits(combo);

            // Force complete
            combo.ForceComplete();

            // Notify UI panel
            if (combo.uiPanel != null)
            {
                combo.uiPanel.OnComboSubmitted();
            }
        }

        Debug.Log($"Manually submitted {combosToSubmit.Count} active combos");
    }

    private void OnFruitSliced(FruitType type, GameObject fruit)
    {
        bool foundMatch = false;

        // Try to match with active combos (prioritize phase 1 over phase 2)
        var sortedCombos = activeCombos.Where(c => c.IsActive())
                                     .OrderBy(c => c.currentPhase == ComboPhase.Phase1_Active ? 0 : 1)
                                     .ThenBy(c => c.spawnTime);

        foreach (var combo in sortedCombos)
        {
            if (combo.TryAdvance(type))
            {
                foundMatch = true;

                // Update UI if panel exists
                if (combo.uiPanel != null)
                {
                    // Panel will update itself through its coroutine
                }

                // Check if combo is completed
                if (combo.isCompleted)
                {
                    OnComboCompleted(combo);
                }

                break; // Only match with the first valid combo
            }
        }

        if (!foundMatch)
        {
            Debug.Log($"Sliced {type} but no active combo could advance");
        }
    }

    private void OnComboCompleted(ComboData combo)
    {
        // Clear active fruits
        ClearComboActiveFruits(combo);

        // Notify UI panel
        if (combo.uiPanel != null)
        {
            combo.uiPanel.OnComboCompleted();
        }

        // Remove from active combos (will be removed when panel returns to pool)
        Debug.Log($"Combo {combo.comboId} completed!");
    }

    private void ClearAllCombos()
    {
        // Stop spawn coroutine
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // Clear all active combos and return panels to pool
        foreach (var combo in activeCombos.ToList())
        {
            ClearComboActiveFruits(combo);
            if (combo.uiPanel != null)
            {
                combo.uiPanel.OnComboSubmitted();
            }
        }

        // Clear lists
        allCombos.Clear();
        pendingCombos.Clear();
        activeCombos.Clear();
    }

    private void OnDestroy()
    {
        ClearAllCombos();
    }

    // Debug methods
    [ContextMenu("Debug Print Active Combos")]
    public void DebugPrintActiveCombos()
    {
        Debug.Log($"Active combos: {activeCombos.Count}");
        foreach (var combo in activeCombos)
        {
            Debug.Log($"Combo {combo.comboId}: Phase {combo.currentPhase}, Progress {combo.activeIndex}/{combo.fruitSequence.Count}");
        }
    }

    [ContextMenu("Debug Submit All")]
    public void DebugSubmitAll()
    {
        SubmitComboManually();
    }

    // Public methods for UI integration
    public bool HasActiveCombos()
    {
        return activeCombos.Count > 0 && activeCombos.Any(c => c.IsActive());
    }

    public int GetActiveComboCount()
    {
        return activeCombos.Count(c => c.IsActive());
    }

    public List<ComboData> GetActiveCombos()
    {
        return activeCombos.Where(c => c.IsActive()).ToList();
    }

    // Events for UI integration
    public static event System.Action<int> OnActiveComboCountChanged;

    private void NotifyActiveComboCountChanged()
    {
        OnActiveComboCountChanged?.Invoke(GetActiveComboCount());
    }
}