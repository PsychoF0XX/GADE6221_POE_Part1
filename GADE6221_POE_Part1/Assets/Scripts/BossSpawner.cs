using System.Collections;
using UnityEngine;
using TMPro;

// Score-based cyclic boss. Pattern repeats every 2000 score:
//   0-499:    Level 1, no boss
//   500-999:  Level 1, boss (poisons pickups)
//   1000-1499: Level 2, no boss
//   1500-1999: Level 2, boss (drops cars)
public class BossSpawner : MonoBehaviour
{
    [Header("Drone")]
    [SerializeField] private GameObject droneObject;

    [Header("Score Thresholds (must match LevelManager)")]
    [SerializeField] private int bossActivationOffset = 500;  // score into each level when boss spawns
    [SerializeField] private int level2StartScore = 1000;
    [SerializeField] private int cycleLength = 2000;

    [Header("Alert UI")]
    [SerializeField] private TMP_Text alertText;
    [SerializeField] private float alertDuration = 4f;
    [SerializeField] private float flashInterval = 0.3f;

    private BossController bossController;
    private PickupSpawner pickupSpawner;
    private bool bossActive = false;

    private void Start()
    {
        pickupSpawner = FindFirstObjectByType<PickupSpawner>();

        if (droneObject != null)
        {
            bossController = droneObject.GetComponent<BossController>();
            droneObject.SetActive(false);
        }

        if (alertText != null) alertText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        int score = GameManager.Instance.Score;
        int cycleScore = score % cycleLength;
        bool shouldBeActive = cycleScore % level2StartScore >= bossActivationOffset;
        bool isLevel2 = cycleScore >= level2StartScore;

        if (shouldBeActive && !bossActive)
            ActivateBoss(isLevel2);
        else if (!shouldBeActive && bossActive)
            DeactivateBoss(isLevel2);
    }

    private void ActivateBoss(bool isLevel2)
    {
        bossActive = true;
        if (droneObject != null) droneObject.SetActive(true);
        bossController?.Activate(isLevel2);
        if (pickupSpawner != null) pickupSpawner.BossActive = !isLevel2; // only poison in Level 1

        EventManager.Instance?.RaiseBossSpawned(1);

        string msg = isLevel2
            ? "DRONE DETECTED!\n<size=75%>Watch out for falling vehicles!\nReach 2000 points to escape.</size>"
            : "DRONE DETECTED!\n<size=75%>Dodge its attacks & beware POISONED pickups!\nReach 1000 points to escape.</size>";
        StartCoroutine(ShowAlert(msg));
    }

    private void DeactivateBoss(bool isLevel2)
    {
        bossActive = false;
        bossController?.Deactivate();
        if (droneObject != null) droneObject.SetActive(false);
        if (pickupSpawner != null) pickupSpawner.BossActive = false;
        EventManager.Instance?.RaiseBossBeaten(1);

        string msg = isLevel2 ? "<size=90%>Drone lost! Level complete!</size>"
                               : "<size=90%>Drone lost! You escaped!</size>";
        StartCoroutine(ShowAlert(msg));
    }

    private IEnumerator ShowAlert(string message)
    {
        if (alertText == null) yield break;

        alertText.text = message;
        float elapsed = 0f;
        while (elapsed < alertDuration)
        {
            alertText.gameObject.SetActive(true);
            yield return new WaitForSeconds(flashInterval);
            alertText.gameObject.SetActive(false);
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval * 2f;
        }

        alertText.gameObject.SetActive(false);
    }
}
