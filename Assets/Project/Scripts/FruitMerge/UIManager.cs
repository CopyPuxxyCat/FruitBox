using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI coinText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject skinPanel;

    private void OnEnable()
    {
        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnCoinsChanged += UpdateCoins;
        GameManager.Instance.OnGameStateChanged += HandleStateChange;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnCoinsChanged -= UpdateCoins;
        GameManager.Instance.OnGameStateChanged -= HandleStateChange;
    }

    private void UpdateScore(int score) => scoreText.text = $"Score: {score}";
    private void UpdateCoins(int coins) => coinText.text = $"Coins: {coins}";

    private void HandleStateChange(GameState state)
    {
        gameOverPanel.SetActive(state == GameState.GameOver);
    }

    public void ToggleSkinPanel() => skinPanel.SetActive(!skinPanel.activeSelf);

    public void OnSelectSkin(int index)
    {
        SkinManager.Instance.SelectSkin(index);
    }
}

