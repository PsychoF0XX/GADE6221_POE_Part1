using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Attach to your leaderboard panel.
// Assign the 5 score text fields in the Inspector (scoreTexts array).
// Call Refresh() when the panel opens.
public class LeaderboardDisplay : MonoBehaviour
{
    [Header("Score Text Fields (assign 5 in order)")]
    [SerializeField] private TMP_Text[] scoreTexts;   // drag in 5 TMP texts

    [Header("Panel")]
    [SerializeField] private GameObject leaderboardPanel;

    public void Show()
    {
        leaderboardPanel?.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        leaderboardPanel?.SetActive(false);
    }

    public void Refresh()
    {
        if (DatabaseManager.Instance == null) return;

        List<ScoreEntry> top = DatabaseManager.Instance.GetTopScores(5);

        for (int i = 0; i < scoreTexts.Length; i++)
        {
            if (scoreTexts[i] == null) continue;

            if (i < top.Count)
                scoreTexts[i].text = $"{i + 1}. {top[i].playerName}  {top[i].score}";
            else
                scoreTexts[i].text = $"{i + 1}. ---";
        }
    }
}
