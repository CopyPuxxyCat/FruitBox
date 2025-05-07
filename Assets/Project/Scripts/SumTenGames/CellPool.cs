using UnityEngine;
using System.Collections.Generic;

public class CellPool : MonoBehaviour
{
    public GridCell cellPrefab;
    public Transform parent;

    private Queue<GridCell> pool = new Queue<GridCell>();

    public GridCell Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        return Instantiate(cellPrefab, parent);
    }

    public void Return(GridCell cell)
    {
        cell.gameObject.SetActive(false);
        pool.Enqueue(cell);
    }
}

