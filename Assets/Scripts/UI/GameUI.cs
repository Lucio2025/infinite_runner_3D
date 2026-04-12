using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI musicText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Button restartButton;

    private void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnCoinsChanged += UpdateCoins;
            GameManager.Instance.OnGameOver += ShowGameOver;
            UpdateLives(GameManager.Instance.Lives);
            UpdateCoins(GameManager.Instance.Coins);
        }

        if (MusicLayerManager.Instance != null)
        {
            MusicLayerManager.Instance.OnTrackChanged += UpdateMusic;
            UpdateMusic(MusicLayerManager.Instance.ActiveTrackIndex);
        }
        else
        {
            UpdateMusic(0);
        }
    }

    private void UpdateLives(int lives)
    {
        if (livesText) livesText.text = $"❤ Vidas: {lives}";
    }

    private void UpdateCoins(int coins)
    {
        if (coinsText) coinsText.text = $"● Monedas: {coins}";
    }

    private void UpdateMusic(int track)
    {
        if (musicText) musicText.text = $"♪ Música: {track + 1}  [1/2/3]";
    }

    private void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }
}