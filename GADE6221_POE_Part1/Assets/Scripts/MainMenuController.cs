using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Attach to a GameObject in the Main Menu scene.
// Wire up buttons in the Inspector:
//   Play Button  -> OnPlayClicked()
//   Quit Button  -> OnQuitClicked()
// Assign highScoreText TMP_Text to display the saved best score.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "EndlessRunner";
    [SerializeField] private TMP_Text highScoreText;

    private void Start()
    {
        if (highScoreText != null)
        {
            int best = HighScoreManager.Instance != null ? HighScoreManager.Instance.HighScore : PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = best > 0 ? "Best: " + best : "Your future high score";
        }
    }

    public void OnPlayClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
