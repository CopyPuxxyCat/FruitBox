using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int rows = 12;
    public int cols = 7;
    public Transform gridHolder; // phải là RectTransform
    public GridCell cellPrefab;
    public CellPool cellPool;
    public BacktrackingSolver solver;

    public Vector2 cellSize = new Vector2(100, 100); // kích thước mỗi ô
    public Vector2 spacing = new Vector2(5, 5);      // khoảng cách giữa các ô

    private List<GridCell> gridCells = new List<GridCell>();

    void Awake()
    {
        LeanTween.init(5000); // Tăng giới hạn lên (tùy theo nhu cầu, ví dụ 1000 hoặc hơn)
    }

    private void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        ClearGrid();
        solver.GenerateValidGrid(rows, cols); // đảm bảo có đường đi tổng = 10

        for (int i = 0; i < rows * cols; i++)
        {
            GridCell cell = cellPool.Get();
            cell.transform.SetParent(gridHolder, false);

            int number = solver.GetNumberAt(i);
            cell.SetNumber(number);
            cell.SetIndex(i);
            cell.gameObject.SetActive(true);

            PositionCell(cell, i); // gán vị trí thủ công
            gridCells.Add(cell);
        }
    }

    private void PositionCell(GridCell cell, int index)
    {
        int row = index / cols;
        int col = index % cols;

        RectTransform rect = cell.GetComponent<RectTransform>();
        float x = col * (cellSize.x + spacing.x);
        float y = -row * (cellSize.y + spacing.y); // trục y đi xuống

        rect.anchoredPosition = new Vector2(x, y);
    }

    public void ClearGrid()
    {
        foreach (var cell in gridCells)
        {
            cellPool.Return(cell);
        }
        gridCells.Clear();
    }

    public GridCell GetCellAtIndex(int index)
    {
        return index >= 0 && index < gridCells.Count ? gridCells[index] : null;
    }

    public List<GridCell> GetAllCells() => gridCells;
}
