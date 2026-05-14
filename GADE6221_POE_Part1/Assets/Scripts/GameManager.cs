using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TMP_Text hudScoreText;
    [SerializeField] private TMP_Text hudLivesText;

    [Header("Death Screen (set panel inactive by default)")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TMP_Text deathScoreText;

    [Header("Pause Menu (set panel inactive by default)")]
    [SerializeField] private GameObject pausePanel;

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float invincibilityDuration = 2f;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    public int Score => score;

    private int score = 0;
    private int lives;
    private bool isGameOver = false;
    private bool isPaused = false;
    private PlayerController playerController;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        lives = maxLives;
        deathPanel?.SetActive(false);
        pausePanel?.SetActive(false);

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) playerController = p.GetComponent<PlayerController>();

        RefreshHUD();
    }

    private void Update()
    {
        if (!isGameOver && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        RefreshHUD();
    }

    public void AddLife()
    {
        if (isGameOver) return;
        lives = Mathf.Min(lives + 1, maxLives);
        RefreshHUD();
    }

    public void TakeDamage()
    {
        if (isGameOver) return;
        lives--;
        RefreshHUD();

        if (lives <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            // Grant brief invincibility so the player can recover
            playerController?.StartInvincibility(invincibilityDuration);
        }
    }

    // Legacy support
    public void OnPlayerDied() => TakeDamage();

    private void TriggerGameOver()
    {
        isGameOver = true;
        playerController?.TriggerDeath();
        Time.timeScale = 0f;

        if (deathScoreText != null)
            deathScoreText.text = "Score: " + score;

        deathPanel?.SetActive(true);
    }

    private void RefreshHUD()
    {
        if (hudScoreText != null)
            hudScoreText.text = "Score: " + score;
        if (hudLivesText != null)
            hudLivesText.text = "Lives: " + lives;
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        pausePanel?.SetActive(isPaused);
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnResumeClicked()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel?.SetActive(false);
    }

    public void OnQuitClicked() => Application.Quit();
}
