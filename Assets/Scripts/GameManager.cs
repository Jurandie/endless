using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject startPanel;
    public GameObject gameUIPanel;
    public GameObject gameOverPanel;
    public Text scoreText;
    public Text coinsText;

    [Header("Refs")]
    public PlayerController_NewInput player;
    public TileSpawner tileSpawner;
    public ObstacleSpawnerPooling obstacleSpawner;

    [Header("Gameplay")]
    public float startingSpeed = 8f;

    public bool IsGameOver { get; private set; } = true;

    private float score = 0f;
    private int coins = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        ShowStartMenu();
    }

    private void Update()
    {
        if (IsGameOver || player == null) return;

        score += player.forwardSpeed * Time.deltaTime;
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
    }

    public void ShowStartMenu()
    {
        IsGameOver = true;
        startPanel?.SetActive(true);
        gameUIPanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        Time.timeScale = 1f;

        if (player != null) player.forwardSpeed = 0f;
        tileSpawner?.ResetTiles();
        obstacleSpawner?.ResetSpawner();
        coins = 0;
        score = 0;
        UpdateCoinsText();
        UpdateScoreText();
    }

    // Chamado pelo botão de Start
    public void StartGame()
    {
        IsGameOver = false;
        startPanel?.SetActive(false);
        gameUIPanel?.SetActive(true);
        gameOverPanel?.SetActive(false);
        Time.timeScale = 1f;

        if (player != null)
        {
            player.enabled = true;
            player.ResetPlayer();
            StartCoroutine(player.IncreaseSpeedOverTime());
            player.forwardSpeed = startingSpeed;
        }

        tileSpawner?.StartGameSpawning();
        obstacleSpawner?.ResetSpawner();

        score = 0;
        coins = 0;
        UpdateCoinsText();
        UpdateScoreText();
    }

    public void CollectCoin()
    {
        coins++;
        UpdateCoinsText();
    }

    private void UpdateCoinsText()
    {
        if (coinsText != null) coinsText.text = "Coins: " + coins;
    }

    private void UpdateScoreText()
    {
        if (scoreText != null) scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;

        if (player != null)
        {
            player.forwardSpeed = 0f;
            player.enabled = false;
        }

        StopAllCoroutines();

        gameOverPanel?.SetActive(true);
        gameUIPanel?.SetActive(false);
    }

    public void Restart()
    {
        StartGame();
    }
}