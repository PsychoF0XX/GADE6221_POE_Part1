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
    [SerializeField] private float minObstacleSpacing = 8f;

    [Header("Wave Settings")]
    [SerializeField] [Range(0f, 1f)] private float doubleSpawnChance = 0.4f;

    [Header("Pickup Ratio")]
    [SerializeField] private int obstaclesPerPickup = 3;

    [Header("Lane Settings")]
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private int totalLanes = 3;

    [Header("Height")]
    [SerializeField] private float spawnHeightOffset = -2f;  // offset below the drone player


    private float spawnTimer = 0f;
    private int obstaclesSinceLastPickup = 0;
    private Transform playerTransform;
    private PickupSpawner pickupSpawner;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private HashSet<GameObject> passedObstacles = new HashSet<GameObject>();
    private Dictionary<GameObject, (int lane, float z)> obstacleSlots = new();

    private void Start()
    {
        SpawnRegistry.Clear();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
        pickupSpawner = FindFirstObjectByType<PickupSpawner>();
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
        if (IsZoneOccupied(spawnZ)) return;

        int waveSize = (Random.value < doubleSpawnChance) ? 2 : 1;

        List<int> lanes = new List<int> { 0, 1, 2 };
        ShuffleList(lanes);

        int spawned = 0;
        bool triggerPickup = false;

        for (int i = 0; i < lanes.Count && spawned < waveSize; i++)
        {
            if (totalLanes - spawned <= 1) break;
            if (!SpawnRegistry.TryClaim(lanes[i], spawnZ)) continue;

            SpawnObstacleAt(lanes[i], spawnZ);
            spawned++;

            obstaclesSinceLastPickup++;
            if (obstaclesSinceLastPickup >= obstaclesPerPickup)
            {
                obstaclesSinceLastPickup = 0;
                triggerPickup = true;
            }
        }

        // Spawn pickup AFTER all obstacles so CountObstaclesAtZ sees the full wave count
        if (triggerPickup)
            pickupSpawner?.SpawnPickupNow();
    }

    private void SpawnObstacleAt(int lane, float spawnZ)
    {
        float xPos = (lane - 1) * laneWidth;
        Vector3 spawnPos = new Vector3(xPos, playerTransform.position.y + spawnHeightOffset, spawnZ);
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        GameObject obs = Instantiate(prefab, spawnPos, Quaternion.identity);
        obs.tag = "Obstacle";
        activeObstacles.Add(obs);
        obstacleSlots[obs] = (lane, spawnZ);

    }

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

    public int CountObstaclesAtZ(float z, float tolerance = 4f)
    {
        int count = 0;
        foreach (GameObject obs in activeObstacles)
        {
            if (obs == null) continue;
            if (Mathf.Abs(obs.transform.position.z - z) < tolerance)
                count++;
        }
        return count;
    }

    public bool IsObstacleNear(Vector3 position, float radius)
    {
        foreach (GameObject obs in activeObstacles)
        {
            if (obs == null) continue;
            if (Vector3.Distance(obs.transform.position, position) < radius)
                return true;
        }
        return false;
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
                if (obstacleSlots.TryGetValue(obs, out var slot))
                {
                    SpawnRegistry.Release(slot.lane, slot.z);
                    obstacleSlots.Remove(obs);
                }
                passedObstacles.Remove(obs);
                Destroy(obs);
                activeObstacles.RemoveAt(i);
            }
        }
    }
}
