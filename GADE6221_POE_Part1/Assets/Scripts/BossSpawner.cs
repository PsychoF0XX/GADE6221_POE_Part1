using System.Collections;
using UnityEngine;
using TMPro;

public class BossSpawner : MonoBehaviour
{
    [Header("Drone")]
    [SerializeField] private GameObject droneObject;    // cop drone (child of Player in hierarchy)

    [Header("Score Thresholds")]
    [SerializeField] private int spawnAtScore = 500;
    [SerializeField] private int despawnAtScore = 1000;

    [Header("Alert UI")]
    [SerializeField] private TMP_Text alertText;        // fullscreen warning text on Canvas
    [SerializeField] private float alertDuration = 4f;
    [SerializeField] private float flashInterval = 0.3f;

    private PickupSpawner pickupSpawner;
    private bool bossActive = false;
    private bool bossDefeated = false;

    private void Start()
    {
        pickupSpawner = FindFirstObjectByType<PickupSpawner>();
        if (droneObject != null) droneObject.SetActive(false);
        if (alertText != null) alertText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        int score = GameManager.Instance.Score;

        if (!bossActive && !bossDefeated && score >= spawnAtScore)
            ActivateBoss();
        else if (bossActive && score >= despawnAtScore)
            DeactivateBoss();
    }

    private void ActivateBoss()
    {
        bossActive = true;
        if (droneObject != null) droneObject.SetActive(true);
        if (pickupSpawner != null) pickupSpawner.BossActive = true;
        StartCoroutine(ShowAlert(
            "DRONE DETECTED!\n<size=75%>1 in 5 obstacles is now POISONED!\nSurvive to 1000 points to escape.</size>"));
    }

    private void DeactivateBoss()
    {
        bossActive = false;
        bossDefeated = true;
        if (droneObject != null) droneObject.SetActive(false);
        if (pickupSpawner != null) pickupSpawner.BossActive = false;
        StartCoroutine(ShowAlert("<size=90%>Drone lost! Good escape!</size>"));
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
