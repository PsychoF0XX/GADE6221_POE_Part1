using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TMP_Text hudScoreText;
    [SerializeField] private TMP_Text hudLivesText;
    [SerializeField] private TMP_Text hudLevelsText;   // optional — shows levels beaten

    [Header("Death Screen (set panel inactive by default)")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TMP_Text deathScoreText;
    [SerializeField] private TMP_Text deathHighScoreText;
    [SerializeField] private TMP_Text newHighScoreLabel;   // "NEW BEST!" label, optional
    [SerializeField] private PlayerNameInput playerNameInput; // shows name prompt before death panel

    [Header("Pause Menu (set panel inactive by default)")]
    [SerializeField] private GameObject pausePanel;

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float invincibilityDuration = 2f;

    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "EndlessRunner";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public int Score => score;
    public int LevelsBeaten => levelsBeaten;

    private int score = 0;
    private int levelsBeaten = 0;
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
        newHighScoreLabel?.gameObject.SetActive(false);

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) playerController = p.GetComponent<PlayerController>();

        // Subscribe to EventManager events
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnObstaclePassed  += () => AddScore(1);
            EventManager.Instance.OnPickupActivated += _ => AddScore(2);
            EventManager.Instance.OnBossSpawned     += _ => { };   // hooked — extend later
            EventManager.Instance.OnBossBeaten      += _ => AddScore(50);
            EventManager.Instance.OnLevelCompleted  += OnLevelCompleted;
            EventManager.Instance.OnPlayerDied      += TakeDamage;
        }

        RefreshHUD();
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnObstaclePassed  -= () => AddScore(1);
            EventManager.Instance.OnPickupActivated -= _ => AddScore(2);
            EventManager.Instance.OnBossBeaten      -= _ => AddScore(50);
            EventManager.Instance.OnLevelCompleted  -= OnLevelCompleted;
            EventManager.Instance.OnPlayerDied      -= TakeDamage;
        }
    }

    private void OnLevelCompleted()
    {
        levelsBeaten++;
        AddScore(20);   // bonus score for completing a level
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
        CameraShake.Instance?.Shake();

        if (lives <= 0)
            TriggerGameOver();
        else
            playerController?.StartInvincibility(invincibilityDuration);
    }

    public void OnPlayerDied() => TakeDamage();

    private void TriggerGameOver()
    {
        isGameOver = true;
        playerController?.TriggerDeath();
        Time.timeScale = 0f;

        bool isNewBest = HighScoreManager.Instance != null && HighScoreManager.Instance.TrySetHighScore(score);

        if (deathScoreText != null)
            deathScoreText.text = "Score: " + score;

        if (deathHighScoreText != null)
        {
            int best = HighScoreManager.Instance != null ? HighScoreManager.Instance.HighScore : score;
            deathHighScoreText.text = "Best: " + best;
        }

        newHighScoreLabel?.gameObject.SetActive(isNewBest);

        // If name input is set up, show it first — it will reveal the death panel after submit
        if (playerNameInput != null)
            playerNameInput.ShowNameInput(score);
        else
            deathPanel?.SetActive(true);
    }

    private void RefreshHUD()
    {
        if (hudScoreText != null)
            hudScoreText.text = "Score: " + score;
        if (hudLivesText != null)
            hudLivesText.text = "Lives: " + lives;
        if (hudLevelsText != null)
            hudLevelsText.text = "Levels: " + levelsBeaten;
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

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitClicked() => Application.Quit();
}
