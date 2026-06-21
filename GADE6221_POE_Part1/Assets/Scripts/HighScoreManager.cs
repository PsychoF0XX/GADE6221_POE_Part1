using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance { get; private set; }

    private const string HighScoreKey = "HighScore";

    public int HighScore { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    public bool TrySetHighScore(int score)
    {
        if (score > HighScore)
        {
            HighScore = score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    public void ResetHighScore()
    {
        HighScore = 0;
        PlayerPrefs.DeleteKey(HighScoreKey);
    }
}
