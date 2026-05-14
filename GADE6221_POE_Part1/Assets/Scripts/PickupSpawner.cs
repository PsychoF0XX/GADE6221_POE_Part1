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

    private Transform playerTransform;
    private List<GameObject> activePickups = new List<GameObject>();

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
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

        // Try lanes in random order until a clear one is found
        List<int> lanes = new List<int> { 0, 1, 2 };
        ShuffleList(lanes);

        foreach (int lane in lanes)
        {
            float xPos = (lane - 1) * laneWidth;
            Vector3 spawnPos = new Vector3(xPos, spawnY, spawnZ);

            if (IsPositionClear(spawnPos))
            {
                GameObject prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];
                if (prefab == null) continue;

                GameObject pickup = Instantiate(prefab, spawnPos, Quaternion.identity);
                activePickups.Add(pickup);
                return;
            }
        }
        // All lanes occupied — skip this pickup cycle
    }

    private bool IsPositionClear(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, overlapCheckRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Obstacle") || hit.CompareTag("Pickup"))
                return false;
        }
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
            if (activePickups[i] == null)
            {
                activePickups.RemoveAt(i);
                continue;
            }
            if (playerZ - activePickups[i].transform.position.z > destroyDistBehind)
            {
                Destroy(activePickups[i]);
                activePickups.RemoveAt(i);
            }
        }
    }
}
