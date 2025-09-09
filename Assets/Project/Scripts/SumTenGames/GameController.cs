using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public SelectionManager selectionManager;
    public GridManager gridManager;
    public ScoreManager scoreManager;

    private bool isGameOver = false;

    private void Awake()
    {
        isGameOver = false;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        gameOverPanel.SetActive(true);
        finalScoreText.text = $"{scoreManager.GetScore()}";
    }

    public void ResetGame()
    {
        isGameOver = false;
        gameOverPanel.SetActive(false);
        scoreManager.ResetScore();
        selectionManager.ResetTimer();

        gridManager.SpawnNewGrid(); 
    }

    public void ReturnToMenu()
    {
        isGameOver = true;
        scoreManager.ResetScore();
        selectionManager.ResetTimer();
        SceneManager.LoadScene("MenuScene"); 
    }
}

