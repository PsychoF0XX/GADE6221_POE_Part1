using System.Collections;
using UnityEngine;
using TMPro;

// Handles boss behaviour per level type.
// Level 1: passive — PickupSpawner handles poisoning.
// Level 2: actively drops cars with a lane warning.
public class BossController : MonoBehaviour
{
    [Header("Level 2 Attack Settings")]
    [SerializeField] private float attackInterval = 6f;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private float attackSpawnDist = 40f;

    [Header("Warning UI (same text field as BossSpawner alert)")]
    [SerializeField] private TMP_Text warningText;

    private ObstacleSpawner obstacleSpawner;
    private PlayerController playerController;
    private bool isActive = false;
    private Coroutine attackCoroutine;

    private static readonly string[] LaneNames = { "LEFT", "CENTRE", "RIGHT" };

    private void Start()
    {
        obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerController = player.GetComponent<PlayerController>();

        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    public void Activate(bool isLevel2)
    {
        if (isActive) return;
        isActive = true;
        if (warningText != null) warningText.gameObject.SetActive(false);

        if (isLevel2)
            attackCoroutine = StartCoroutine(AttackLoop());
    }

    public void Deactivate()
    {
        isActive = false;
        if (attackCoroutine != null) { StopCoroutine(attackCoroutine); attackCoroutine = null; }
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(attackInterval * 0.5f);

        while (isActive)
        {
            yield return StartCoroutine(FireAttack());
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator FireAttack()
    {
        if (playerController == null || obstacleSpawner == null) yield break;

        int targetLane = playerController.CurrentLane;
        string laneName = LaneNames[Mathf.Clamp(targetLane, 0, 2)];

        if (warningText != null)
        {
            warningText.text = $"INCOMING!\n<size=80%>Move out of {laneName} lane!</size>";
            warningText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(warningDuration);

        if (warningText != null) warningText.gameObject.SetActive(false);

        obstacleSpawner.SpawnBossObstacle(targetLane, attackSpawnDist);
    }
}
