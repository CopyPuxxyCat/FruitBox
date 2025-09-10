using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboPanelManager : MonoBehaviour
{
    public static ComboPanelManager Instance;

    [Header("Panels (0=top/future,1=middle/next,2=bottom/current)")]
    [SerializeField] private List<ComboPanel> panels; // size==3 expected, assign in inspector
    [Header("Timing")]
    [SerializeField] private float timerDuration = 1.5f;     // time until slide
    [SerializeField] private float slideDuration = 0.5f;     // slide animation time
    [SerializeField] private Vector2 slideOffset = new Vector2(0, 170f); // target anchored position delta for slide
    //[SerializeField] private float oldPanelOpacityOnOverlap = 0.7f;

    private Queue<ComboData> comboQueue;
    private ComboData currentCombo;
    private ComboData oldCombo;
    private Coroutine currentTimerCoroutine;
    private FruitSpawner spawner;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        comboQueue = new Queue<ComboData>();
        spawner = FindObjectOfType<FruitSpawner>();
    }

    private void OnEnable()
    {
        // subscribe to fruit slice event
        FruitBehavior.OnFruitSliced += OnFruitSliced;
    }

    private void OnDisable()
    {
        FruitBehavior.OnFruitSliced -= OnFruitSliced;
    }

    public void InitializeCombos(BeatMap beatMap)
    {
        comboQueue.Clear();

        // group beatNotes into combos (ordered by spawnTimeSec within same comboId)
        var grouped = beatMap.beatNotes.GroupBy(n => n.comboId).OrderBy(g => g.Min(n => n.spawnTimeSec));
        foreach (var group in grouped)
        {
            var seq = group.OrderBy(n => n.spawnTimeSec).Select(n => n.fruitType).ToList();
            comboQueue.Enqueue(new ComboData(group.Key, seq));
        }

        // Reset panels visually
        foreach (var p in panels) p.ResetPanel();

        // preload first up to 3 combos
        SpawnNextAsCurrent();
        PreloadNextPanels();
    }

    private void SpawnNextAsCurrent()
    {
        if (comboQueue.Count == 0) { currentCombo = null; return; }
        currentCombo = comboQueue.Dequeue();
        currentCombo.startTime = Time.time;

        // update bottom panel (index 2)
        panels[2].UpdateDisplay(currentCombo, 1f);

        // start timer for current
        StopCurrentTimerCoroutine();
        currentTimerCoroutine = StartCoroutine(CurrentTimerCoroutine(currentCombo));
    }

    private IEnumerator CurrentTimerCoroutine(ComboData combo)
    {
        float remaining = timerDuration;
        while (remaining > 0f && !combo.isCompleted)
        {
            remaining -= Time.deltaTime;
            float progress = remaining / timerDuration;
            panels[2].UpdateDisplay(combo, progress);
            yield return null;
        }

        if (!combo.isCompleted)
        {
            // slide up current into old slot
            SlideCurrentUp();
        }
    }

    private void StopCurrentTimerCoroutine()
    {
        if (currentTimerCoroutine != null) { StopCoroutine(currentTimerCoroutine); currentTimerCoroutine = null; }
    }

    private void PreloadNextPanels()
    {
        // middle = next, top = future
        if (comboQueue.Count > 0)
        {
            var next = comboQueue.Peek();
            panels[1].UpdateDisplay(new ComboData(next.comboId, next.fruitSequence), 0f);
        }
        if (comboQueue.Count > 1)
        {
            var future = comboQueue.ElementAt(1);
            panels[0].UpdateDisplay(new ComboData(future.comboId, future.fruitSequence), 0f);
        }
    }

    private void SlideCurrentUp()
    {
        if (currentCombo == null) return;

        oldCombo = currentCombo;
        StopCurrentTimerCoroutine();

        RectTransform rt = panels[2].GetComponent<RectTransform>();

        // slide anchors thay vì anchoredPosition
        Vector2 fromMin = new Vector2(0, 0);
        Vector2 fromMax = new Vector2(1, 0.5f);
        Vector2 toMin = new Vector2(0, 0.5f);
        Vector2 toMax = new Vector2(1, 1);

        // đảm bảo về state ban đầu
        rt.anchorMin = fromMin;
        rt.anchorMax = fromMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        LeanTween.value(rt.gameObject, 0f, 1f, slideDuration)
            .setEaseInOutQuad()
            .setOnUpdate((float t) =>
            {
                rt.anchorMin = Vector2.Lerp(fromMin, toMin, t);
                rt.anchorMax = Vector2.Lerp(fromMax, toMax, t);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            })
            .setOnComplete(() =>
            {
                SwapPanelReferencesAfterSlide();
            });

        StartCoroutine(OldComboTimerCoroutine(oldCombo));
    }


    private IEnumerator OldComboTimerCoroutine(ComboData old)
    {
        float remaining = timerDuration; // old gets same duration after slide
        // blink at last 0.5s
        while (remaining > 0f && !old.isCompleted)
        {
            if (remaining <= 0.5f)
            {
                panels[1].Blink();
            }
            remaining -= Time.deltaTime;
            yield return null;
        }

        if (!old.isCompleted)
        {
            // auto-submit
            AutoSubmitOld();
        }
    }

    private void SwapPanelReferencesAfterSlide()
    {
        // panels list corresponds to visual stack: [0]=topfuture, [1]=middle (old), [2]=bottom (current)
        // after sliding bottom->middle, we want to rotate references so that panels[1] now shows oldCombo,
        // and the freed panel (previously bottom) is reassigned as new bottom ready for next combo.
        // We'll do a circular rotate: temp = panels[0]; panels[0]=panels[1]; panels[1]=panels[2]; panels[2]=temp;
        var temp = panels[0];
        panels[0] = panels[1];
        panels[1] = panels[2];
        panels[2] = temp;

        // Update UI: middle (panels[1]) should show oldCombo state (progress continues handled in OldComboTimerCoroutine)
        panels[1].UpdateDisplay(oldCombo, 1f);

        // Now spawn next combo as new current
        SpawnNextAsCurrent();
        PreloadNextPanels();
    }

    private void AutoSubmitOld()
    {
        if (oldCombo == null) return;
        // clear fruits for old and finalize
        ClearComboActiveFruits(oldCombo);
        oldCombo.ForceComplete();
        panels[1].ResetPanel();
        oldCombo = null;
    }

    // Called when fruit is spawned to track active fruit objects
    public void AddFruitToCombo(int comboId, GameObject fruit)
    {
        if (currentCombo != null && currentCombo.comboId == comboId)
        {
            currentCombo.activeFruits.Add(fruit);
            return;
        }
        if (oldCombo != null && oldCombo.comboId == comboId)
        {
            oldCombo.activeFruits.Add(fruit);
            return;
        }
    }

    private void ClearComboActiveFruits(ComboData combo)
    {
        if (combo == null) return;

        foreach (var go in combo.activeFruits.ToList())
        {
            if (go == null) continue;
            var fb = go.GetComponent<FruitBehavior>();
            if (fb != null)
            {
                // deactivate and return to pool
                spawner.ReturnToPool(fb.GetFruitType(), go);
            }
            else
            {
                Destroy(go);
            }
        }
        combo.activeFruits.Clear();
    }

    // Called on player pressing manual submit
    public void SubmitComboManually()
    {
        // Clear current and old
        if (currentCombo != null && !currentCombo.isCompleted)
        {
            ClearComboActiveFruits(currentCombo);
            currentCombo.ForceComplete();
            panels[2].ResetPanel();
        }
        if (oldCombo != null && !oldCombo.isCompleted)
        {
            ClearComboActiveFruits(oldCombo);
            oldCombo.ForceComplete();
            panels[1].ResetPanel();
            oldCombo = null;
        }

        // Cancel timers and spawn next combos appropriately
        StopCurrentTimerCoroutine();
        SpawnNextAsCurrent();
        PreloadNextPanels();
    }

    // Score reaction when a fruit is sliced
    private void OnFruitSliced(FruitType type, GameObject fruit)
    {
        // First, try current
        bool advanced = false;
        if (currentCombo != null && !currentCombo.isCompleted)
        {
            advanced = currentCombo.TryAdvance(type);
            if (advanced)
            {
                // Update bottom panel
                panels[2].UpdateDisplay(currentCombo, Mathf.Clamp01((timerDuration - (Time.time - currentCombo.startTime)) / timerDuration));
                // if completed -> finalize
                if (currentCombo.isCompleted)
                {
                    OnComboCompleted(currentCombo);
                }
                return;
            }
        }

        // Then try old combo (overlap)
        if (oldCombo != null && !oldCombo.isCompleted)
        {
            bool advOld = oldCombo.TryAdvance(type);
            if (advOld)
            {
                panels[1].UpdateDisplay(oldCombo, 1f);
                if (oldCombo.isCompleted)
                {
                    // finalize old combo
                    ClearComboActiveFruits(oldCombo);
                    panels[1].ResetPanel();
                    oldCombo = null;
                }
            }
        }
    }

    private void OnComboCompleted(ComboData completed)
    {
        // award score (handled elsewhere, hook here if needed)
        // Clear active fruits
        ClearComboActiveFruits(completed);
        completed.ForceComplete();

        // Reset bottom panel and rotate
        panels[2].ResetPanel();

        // Spawn next combo immediately
        StopCurrentTimerCoroutine();
        SpawnNextAsCurrent();
        PreloadNextPanels();
    }
}
