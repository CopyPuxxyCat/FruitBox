using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    private int score = 0;

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = $"{score}";
    }

    public int GetScore() => score;

    public void ResetScore()
    {
        score = 0;
        scoreText.text = "0";
    }
}

