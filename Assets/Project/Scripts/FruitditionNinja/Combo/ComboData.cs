using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComboData
{
    public int comboId;
    public List<FruitType> fruitSequence;
    public int activeIndex; // index to match next
    public float spawnTime; // thời gian spawn combo này
    public bool isCompleted;
    public bool isExpired; // combo đã hết hạn (không thể hoàn thành nữa)
    public List<GameObject> activeFruits;

    // UI Panel reference
    public ComboPanel uiPanel;

    // Phase timing
    public ComboPhase currentPhase;
    public float phaseStartTime;

    public ComboData(int id, List<FruitType> sequence, float spawnTime)
    {
        comboId = id;
        fruitSequence = new List<FruitType>(sequence);
        activeIndex = 0;
        this.spawnTime = spawnTime;
        isCompleted = false;
        isExpired = false;
        activeFruits = new List<GameObject>();
        uiPanel = null;
        currentPhase = ComboPhase.None;
        phaseStartTime = 0f;
    }

    public bool TryAdvance(FruitType slicedType)
    {
        if (isCompleted || isExpired || activeIndex >= fruitSequence.Count) return false;

        if (fruitSequence[activeIndex] == slicedType)
        {
            activeIndex++;
            if (activeIndex >= fruitSequence.Count)
            {
                isCompleted = true;
            }
            return true;
        }

        return false;
    }

    public void ForceComplete()
    {
        isCompleted = true;
        activeIndex = fruitSequence.Count;
    }

    public void ForceExpire()
    {
        isExpired = true;
    }

    // Calculate phase 1 duration based on fruit count
    public float GetPhase1Duration()
    {
        int fruitCount = fruitSequence.Count;
        switch (fruitCount)
        {
            case 1: return 2.3f;
            case 2: return 2.6f;
            case 3: return 2.9f;
            case 4: return 3.2f;
            case 5: return 3.5f;
            default: return 2.3f + (fruitCount - 1) * 0.3f; // fallback formula
        }
    }

    public bool IsActive()
    {
        return !isCompleted && !isExpired;
    }

    public void Reset(int newId, List<FruitType> newSequence, float newSpawnTime)
    {
        comboId = newId;
        fruitSequence = new List<FruitType>(newSequence);
        activeIndex = 0;
        spawnTime = newSpawnTime;
        isCompleted = false;
        isExpired = false;
        activeFruits.Clear();
        uiPanel = null;
        currentPhase = ComboPhase.None;
        phaseStartTime = 0f;
    }
}

public enum ComboPhase
{
    None,
    Phase1_Active,    // Giai đoạn 1: hiển thị và chờ
    Phase2_Sliding,   // Đang trượt lên
    Phase2_Final,     // Giai đoạn 2: ở trên và chờ biến mất
    Expired           // Đã hết hạn
}