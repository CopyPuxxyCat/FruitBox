using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public GridManager gridManager;
    public ScoreManager scoreManager;
    public RectTransform selectionBox;
    public Slider timeSlider;
    public float timeLimit = 30f;

    private Vector2 startPos;
    private Vector2 endPos;
    private List<GridCell> selectedCells = new List<GridCell>();
    private float timeRemaining;

    void Start()
    {
        timeRemaining = timeLimit;
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timeSlider.value = timeRemaining / timeLimit;
        }
        else
        {
            // Game over or time-out logic here
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        selectedCells.Clear();
        selectionBox.gameObject.SetActive(true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out startPos
        );

        UpdateSelectionBox(startPos, startPos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out endPos
        );

        UpdateSelectionBox(startPos, endPos);
        HighlightSelection(GetSelectionRect(startPos, endPos));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        selectionBox.gameObject.SetActive(false);
        Rect selectionRect = GetSelectionRect(startPos, endPos);

        selectedCells.Clear();
        foreach (var cell in gridManager.GetAllCells())
        {
            cell.Highlight(false);

            if (!cell.gameObject.activeSelf) continue; // bỏ qua ô đã bị ẩn

            if (RectTransformUtility.RectangleContainsScreenPoint(selectionBox, cell.transform.position, null))
            {
                selectedCells.Add(cell);
            }
        }

        CheckSumAndClear();
    }

    private void UpdateSelectionBox(Vector2 start, Vector2 end)
    {
        Vector2 center = (start + end) / 2;
        selectionBox.anchoredPosition = center;

        Vector2 size = new Vector2(Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
        selectionBox.sizeDelta = size;
    }

    private Rect GetSelectionRect(Vector2 start, Vector2 end)
    {
        float xMin = Mathf.Min(start.x, end.x);
        float xMax = Mathf.Max(start.x, end.x);
        float yMin = Mathf.Min(start.y, end.y);
        float yMax = Mathf.Max(start.y, end.y);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private void HighlightSelection(Rect selectionRect)
    {
        foreach (var cell in gridManager.GetAllCells())
        {
            bool inside = RectTransformUtility.RectangleContainsScreenPoint(
                selectionBox,
                cell.transform.position,
                null
            );
            cell.Highlight(inside);
        }
    }

    private void CheckSumAndClear()
    {
        int sum = 0;
        foreach (var cell in selectedCells)
        {
            sum += cell.GetNumber();
        }

        if (sum == 10)
        {
            foreach (var cell in selectedCells)
            {
                cell.Hide();
            }
            int bonus = Mathf.Max(0, selectedCells.Count - 2);
            scoreManager.AddScore(10 + bonus);
        }

        selectedCells.Clear();
    }
}


