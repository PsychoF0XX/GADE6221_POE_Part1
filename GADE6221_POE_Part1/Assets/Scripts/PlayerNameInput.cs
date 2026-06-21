using UnityEngine;
using TMPro;

// Attach to your death panel (or a child of it).
// When the player dies, GameManager calls ShowNameInput(score).
// The player types their name and clicks Submit.
// Their score is saved to the database, then the death panel fully appears.
public class PlayerNameInput : MonoBehaviour
{
    [Header("Name Input UI")]
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameField;

    [Header("What to show AFTER name is submitted")]
    [SerializeField] private GameObject deathPanel;

    private int pendingScore;

    public void ShowNameInput(int score)
    {
        pendingScore = score;
        if (nameField != null) nameField.text = "";
        nameInputPanel?.SetActive(true);
    }

    // Wire this to your Submit button's OnClick
    public void OnSubmitClicked()
    {
        string playerName = nameField != null ? nameField.text.Trim() : "Player";
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";

        // Save to database
        DatabaseManager.Instance?.SaveScore(playerName, pendingScore);

        // Hide name input, show death panel
        nameInputPanel?.SetActive(false);
        deathPanel?.SetActive(true);
    }
}
