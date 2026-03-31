using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TMP_Text hudScoreText;

    [Header("Death Screen (set panel inactive by default)")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TMP_Text deathScoreText;

    [Header("Pause Menu (set panel inactive by default)")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    private int score = 0;
    private bool isGameOver = false;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        deathPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        RefreshHUD();
    }

    private void Update()
    {
        // Allow pause/unpause at any point while alive
        if (!isGameOver && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }


    public void AddScore(int amount)
    {
        score += amount;
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (hudScoreText != null)
            hudScoreText.text = "Score: " + score;
    }

    public void OnPlayerDied()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;

        if (deathScoreText != null)
            deathScoreText.text = "Score: " + score;

        deathPanel?.SetActive(true);
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