using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Attach to any GameObject in the Main Menu scene.
// Assign scoreTexts (up to 10 TMP_Text fields) in the Inspector — one per leaderboard row.
// Populates automatically on scene load.
public class LeaderboardDisplay : MonoBehaviour
{
    [Header("Score Text Fields (assign in order, top to bottom)")]
    [SerializeField] private TMP_Text[] scoreTexts;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (DatabaseManager.Instance == null)
        {
            FillEmpty();
            return;
        }

        List<ScoreEntry> top = DatabaseManager.Instance.GetTopScores(scoreTexts.Length);

        for (int i = 0; i < scoreTexts.Length; i++)
        {
            if (scoreTexts[i] == null) continue;
            scoreTexts[i].text = i < top.Count
                ? $"{i + 1}.  {top[i].playerName}   {top[i].score}"
                : $"{i + 1}.  ---";
        }
    }

    private void FillEmpty()
    {
        for (int i = 0; i < scoreTexts.Length; i++)
            if (scoreTexts[i] != null)
                scoreTexts[i].text = $"{i + 1}.  ---";
    }
}
