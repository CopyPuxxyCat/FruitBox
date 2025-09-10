using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComboData
{
    public int comboId;
    public List<FruitType> fruitSequence; // sequence of types (order requirement)
    public int activeIndex; // index to match next
    public float startTime;
    public bool isCompleted;
    public List<GameObject> activeFruits; // spawned GameObjects belonging to this combo (for clearing)

    public ComboData(int id, List<FruitType> sequence)
    {
        comboId = id;
        fruitSequence = new List<FruitType>(sequence);
        activeIndex = 0;
        isCompleted = false;
        activeFruits = new List<GameObject>();
    }

    // TryAdvance returns true if this slice counts toward this combo.
    // it only matches the next required type (respecting order),
    // duplicates in sequence are handled because next required may equal same type.
    public bool TryAdvance(FruitType slicedType)
    {
        if (isCompleted || activeIndex >= fruitSequence.Count) return false;

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

    // Used to force-complete (when submit), or clear tracked objects
    public void ForceComplete()
    {
        isCompleted = true;
        activeIndex = fruitSequence.Count;
    }
}
