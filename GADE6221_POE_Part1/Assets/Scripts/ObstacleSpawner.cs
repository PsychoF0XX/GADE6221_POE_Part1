using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs (assign at least 1 in Inspector)")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1.2f;
    [SerializeField] private float spawnDistAhead = 160f;
    [SerializeField] private float destroyDistBehind = 20f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float minSpawnInterval = 0.35f;
    [SerializeField] private float difficultyScaleRate = 0.015f;

    [Header("Spawn Precision")]
    [SerializeField] private float minObstacleSpacing = 8f;    // min Z gap between wave rows

    [Header("Wave Settings")]
    [SerializeField] [Range(0f, 1f)] private float doubleSpawnChance = 0.4f;  // chance of 2 obstacles per wave

    [Header("Pickup Ratio")]
    [SerializeField] private int obstaclesPerPickup = 3;

    [Header("Lane Settings")]
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private int totalLanes = 3;

    private float spawnTimer = 0f;
    private int obstaclesSinceLastPickup = 0;
    private Transform playerTransform;
    private PickupSpawner pickupSpawner;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private HashSet<GameObject> passedObstacles = new HashSet<GameObject>();

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
        pickupSpawner = FindObjectOfType<PickupSpawner>();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - difficultyScaleRate * Time.deltaTime);

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnWave();
        }

        HandleObstacles();
    }

    private void SpawnWave()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        float spawnZ = playerTransform.position.z + spawnDistAhead;

        // Skip if another wave row already exists in this zone
        if (IsZoneOccupied(spawnZ)) return;

        // 40% chance of 2 obstacles this wave (leaving 1 lane open), otherwise 1
        int waveSize = (Random.value < doubleSpawnChance) ? 2 : 1;

        // Shuffle lanes so the blocked ones are random
        List<int> lanes = new List<int> { 0, 1, 2 };
        ShuffleList(lanes);

        int spawned = 0;
        for (int i = 0; i < lanes.Count && spawned < waveSize; i++)
        {
            // Safety: always leave at least 1 lane free
            if (totalLanes - spawned <= 1) break;

            SpawnObstacleAt(lanes[i], spawnZ);
            spawned++;

            obstaclesSinceLastPickup++;
            if (obstaclesSinceLastPickup >= obstaclesPerPickup)
            {
                obstaclesSinceLastPickup = 0;
                pickupSpawner?.SpawnPickupNow();
            }
        }
    }

    private void SpawnObstacleAt(int lane, float spawnZ)
    {
        float xPos = (lane - 1) * laneWidth;
        Vector3 spawnPos = new Vector3(xPos, playerTransform.position.y, spawnZ);
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        GameObject obs = Instantiate(prefab, spawnPos, Quaternion.identity);
        obs.tag = "Obstacle";
        activeObstacles.Add(obs);
    }

    // Returns true if any active obstacle is within minObstacleSpacing of this Z — prevents row overlap
    private bool IsZoneOccupied(float spawnZ)
    {
        foreach (GameObject obs in activeObstacles)
        {
            if (obs == null) continue;
            if (Mathf.Abs(obs.transform.position.z - spawnZ) < minObstacleSpacing)
                return true;
        }
        return false;
    }

    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void HandleObstacles()
    {
        float playerZ = playerTransform.position.z;

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obs = activeObstacles[i];
            if (obs == null) { activeObstacles.RemoveAt(i); continue; }

            float obsZ = obs.transform.position.z;

            if (obsZ < playerZ && !passedObstacles.Contains(obs))
            {
                passedObstacles.Add(obs);
                GameManager.Instance?.AddScore(1);
            }

            if (playerZ - obsZ > destroyDistBehind)
            {
                passedObstacles.Remove(obs);
                Destroy(obs);
                activeObstacles.RemoveAt(i);
            }
        }
    }
}
