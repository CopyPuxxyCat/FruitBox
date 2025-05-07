using UnityEngine;
using System.Collections.Generic;

public class BacktrackingSolver : MonoBehaviour
{
    private List<int> gridNumbers = new List<int>();
    private System.Random rng = new System.Random();

    public void GenerateValidGrid(int rows, int cols)
    {
        int total = rows * cols;
        gridNumbers.Clear();

        for (int i = 0; i < total; i++)
        {
            gridNumbers.Add(rng.Next(1, 10));
        }

        if (!HasValidSum10Path(rows, cols))
        {
            ForceInjectSum10Path(rows, cols);
        }
    }

    public int GetNumberAt(int index) => gridNumbers[index];

    private bool HasValidSum10Path(int rows, int cols)
    {
        // Dummy check always returns false for now
        return false;
    }

    private void ForceInjectSum10Path(int rows, int cols)
    {
        int length = 3; // số lượng ô trong chuỗi tổng 10
        List<int> values = GenerateRandomSum10(length);

        // Tìm vị trí hợp lệ theo hướng ngang hoặc dọc
        bool horizontal = rng.Next(0, 2) == 0;

        int startRow, startCol;
        int index;
        while (true)
        {
            startRow = rng.Next(0, horizontal ? rows : rows - length + 1);
            startCol = rng.Next(0, horizontal ? cols - length + 1 : cols);

            bool canInject = true;
            for (int i = 0; i < length; i++)
            {
                int r = startRow + (horizontal ? 0 : i);
                int c = startCol + (horizontal ? i : 0);
                index = r * cols + c;
                if (index >= gridNumbers.Count)
                {
                    canInject = false;
                    break;
                }
            }
            if (canInject) break;
        }

        // Tiêm giá trị
        for (int i = 0; i < length; i++)
        {
            int r = startRow + (horizontal ? 0 : i);
            int c = startCol + (horizontal ? i : 0);
            index = r * cols + c;
            gridNumbers[index] = values[i];
        }
    }

    private List<int> GenerateRandomSum10(int count)
    {
        List<int> result = new List<int>();
        int remaining = 10;

        for (int i = 0; i < count - 1; i++)
        {
            int maxVal = Mathf.Min(9, remaining - (count - 1 - i));
            int val = rng.Next(1, maxVal + 1);
            result.Add(val);
            remaining -= val;
        }
        result.Add(remaining); // phần còn lại

        // Trộn thứ tự
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }

    public List<int> GetAllNumbers() => gridNumbers;
}
