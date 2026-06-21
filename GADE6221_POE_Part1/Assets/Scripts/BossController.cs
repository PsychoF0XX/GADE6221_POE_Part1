using System.Collections;
using UnityEngine;
using TMPro;

// Attach to the cop drone GameObject.
// Assign obstaclePrefab (any obstacle prefab), warningText, and ObstacleSpawner reference in Inspector.
// The drone fires a targeted attack at the player's current lane every attackInterval seconds.
public class BossController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 6f;       // seconds between attacks
    [SerializeField] private float warningDuration = 2f;      // time player has to dodge
    [SerializeField] private float attackSpawnDist = 30f;     // how far ahead to spawn attack obstacle

    [Header("Attack Obstacle")]
    [SerializeField] private GameObject obstaclePrefab;       // prefab to spawn as boss attack

    [Header("Warning UI")]
    [SerializeField] private TMP_Text warningText;            // assign an existing or new Canvas text

    [Header("Height")]
    [SerializeField] private float spawnHeightOffset = -2f;

    private Transform playerTransform;
    private PlayerController playerController;
    private bool isActive = false;
    private Coroutine attackCoroutine;

    private static readonly string[] LaneNames = { "LEFT", "CENTRE", "RIGHT" };

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    public void Activate()
    {
        if (isActive) return;
        isActive = true;
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
        // Initial delay before first attack
        yield return new WaitForSeconds(attackInterval * 0.5f);

        while (isActive)
        {
            yield return StartCoroutine(FireAttack());
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator FireAttack()
    {
        if (playerController == null || obstaclePrefab == null) yield break;

        // Sample the lane the player is currently in
        int targetLane = playerController.CurrentLane;
        string laneName = LaneNames[Mathf.Clamp(targetLane, 0, 2)];

        // Show warning for warningDuration seconds so player can dodge
        if (warningText != null)
        {
            warningText.text = $"DRONE ATTACK!\n<size=80%>Move out of {laneName} lane!</size>";
            warningText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(warningDuration);

        if (warningText != null) warningText.gameObject.SetActive(false);

        // Spawn the attack obstacle in the warned lane slightly ahead of the player
        if (playerTransform != null)
        {
            float xPos = (targetLane - 1) * 3f; // assumes laneWidth = 3
            float spawnZ = playerTransform.position.z + attackSpawnDist;
            float spawnY = playerTransform.position.y + spawnHeightOffset;
            Vector3 spawnPos = new Vector3(xPos, spawnY, spawnZ);

            GameObject obs = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obs.tag = "Obstacle";

            // Auto-destroy after the player has passed it
            Destroy(obs, 8f);
        }
    }
}
