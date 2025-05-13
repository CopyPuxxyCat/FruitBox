using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Coins { get; private set; }
    public GameState CurrentState { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetState(GameState.Playing);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}
