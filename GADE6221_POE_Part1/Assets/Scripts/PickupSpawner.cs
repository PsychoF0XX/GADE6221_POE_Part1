using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [Header("Pickup Prefabs (assign in Inspector — each needs PickupController)")]
    [SerializeField] private GameObject[] pickupPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistAhead = 160f;
    [SerializeField] private float destroyDistBehind = 20f;
    [SerializeField] private float spawnHeight = 1f;

    [Header("Spawn Precision")]
    [SerializeField] private float overlapCheckRadius = 1.5f;

    [Header("Lane Settings")]
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private int totalLanes = 3;

    [Header("Boss Phase")]
    [SerializeField] private int poisonEveryN = 5;

    // Set by BossSpawner when the drone phase begins/ends
    private bool _bossActive = false;
    public bool BossActive
    {
        get => _bossActive;
        set { if (value != _bossActive) { _bossActive = value; poisonCounter = 0; } }
    }

    private Transform playerTransform;
    private ObstacleSpawner obstacleSpawner;
    private List<GameObject> activePickups = new List<GameObject>();
    private int poisonCounter = 0;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();
    }

    private void Update()
    {
        CleanupPickups();
    }

    // Called by ObstacleSpawner every N obstacles
    public void SpawnPickupNow()
    {
        if (playerTransform == null || pickupPrefabs == null || pickupPrefabs.Length == 0) return;

        float spawnZ = playerTransform.position.z + spawnDistAhead;
        float spawnY = playerTransform.position.y + spawnHeight;

        List<int> lanes = new List<int>();
        for (int i = 0; i < totalLanes; i++) lanes.Add(i);
        ShuffleList(lanes);

        foreach (int lane in lanes)
        {
            float xPos = (lane - 1) * laneWidth;
            Vector3 spawnPos = new Vector3(xPos, spawnY, spawnZ);

            // SpawnRegistry blocks any lane/Z already claimed by an obstacle this frame
            if (!SpawnRegistry.TryClaim(lane, spawnZ)) continue;

            if (IsPositionClear(spawnPos))
            {
                GameObject prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];
                if (prefab == null) { SpawnRegistry.Release(lane, spawnZ); continue; }

                GameObject pickup = Instantiate(prefab, spawnPos, Quaternion.identity);
                PickupController pc = pickup.GetComponent<PickupController>();
                pc?.RegisterSlot(lane, spawnZ);

                if (_bossActive)
                {
                    poisonCounter++;
                    if (poisonCounter >= poisonEveryN)
                    {
                        poisonCounter = 0;
                        // Only poison if fewer than 2 obstacles share this row —
                        // guarantees at least one safe unpoisoned lane exists
                        int obstaclesInRow = obstacleSpawner != null
                            ? obstacleSpawner.CountObstaclesAtZ(spawnZ)
                            : 0;
                        if (obstaclesInRow < 2)
                            pc?.Poison();
                    }
                }

                activePickups.Add(pickup);
                return;
            }

            SpawnRegistry.Release(lane, spawnZ);
        }
    }

    private bool IsPositionClear(Vector3 position)
    {
        if (obstacleSpawner != null && obstacleSpawner.IsObstacleNear(position, overlapCheckRadius))
            return false;

        Collider[] hits = Physics.OverlapSphere(position, overlapCheckRadius);
        foreach (Collider hit in hits)
            if (hit.CompareTag("Pickup")) return false;

        return true;
    }

    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void CleanupPickups()
    {
        if (playerTransform == null) return;
        float playerZ = playerTransform.position.z;
        for (int i = activePickups.Count - 1; i >= 0; i--)
        {
            if (activePickups[i] == null) { activePickups.RemoveAt(i); continue; }
            if (playerZ - activePickups[i].transform.position.z > destroyDistBehind)
            {
                Destroy(activePickups[i]);
                activePickups.RemoveAt(i);
            }
        }
    }
}
