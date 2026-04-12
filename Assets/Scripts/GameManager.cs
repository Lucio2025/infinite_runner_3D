using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int startingLives = 3;

    private int _lives;
    private int _coins;
    private bool _isGameOver;

    public int Lives => _lives;
    public int Coins => _coins;
    public bool IsGameOver => _isGameOver;

    public event Action<int> OnLivesChanged;
    public event Action<int> OnCoinsChanged;
    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _lives = startingLives;
        _coins = 0;
        _isGameOver = false;
    }

    public void TakeDamage()
    {
        if (_isGameOver) return;
        _lives = Mathf.Max(0, _lives - 1);
        OnLivesChanged?.Invoke(_lives);
        if (_lives <= 0)
        {
            _isGameOver = true;
            OnGameOver?.Invoke();
        }
    }

    public void AddCoins(int amount)
    {
        if (_isGameOver) return;
        _coins += amount;
        OnCoinsChanged?.Invoke(_coins);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
