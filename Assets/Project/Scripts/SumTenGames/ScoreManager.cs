using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TMP_Text scoreText;
    private int score = 0;

    private void Awake()
    {
        // Đảm bảo chỉ có một instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Nếu cần giữ lại giữa các scene:
        DontDestroyOnLoad(gameObject);
    }

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

