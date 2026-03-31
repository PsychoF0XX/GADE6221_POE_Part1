using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs (assign at least 1 in Inspector)")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;    // seconds between spawns
    [SerializeField] private float spawnDistAhead = 40f;   // how far ahead to spawn
    [SerializeField] private float destroyDistBehind = 15f;  // how far behind before destroy

    [Header("Lane Settings")]
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private int totalLanes = 3;

    private float spawnTimer = 0f;
    private Transform playerTransform;
    private List<GameObject> activeObstacles = new List<GameObject>();
    // Tracks which obstacles have already been counted as passed
    private HashSet<GameObject> passedObstacles = new HashSet<GameObject>();

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Spawn timer — runs non-stop
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnObstacle();
        }

        HandleObstacles();
    }

    private void SpawnObstacle()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        int lane = Random.Range(0, totalLanes);
        float xPos = (lane - 1) * laneWidth;

        Vector3 spawnPos = new Vector3(
            xPos,
            playerTransform.position.y,
            playerTransform.position.z + spawnDistAhead
        );

        GameObject obs = Instantiate(prefab, spawnPos, Quaternion.identity);
        obs.tag = "Obstacle";
        activeObstacles.Add(obs);
    }

    private void HandleObstacles()
    {
        float playerZ = playerTransform.position.z;

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obs = activeObstacles[i];

            // Clean up destroyed entries
            if (obs == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            float obsZ = obs.transform.position.z;

            // Obstacle is behind the player and hasn't been counted yet
            if (obsZ < playerZ && !passedObstacles.Contains(obs))
            {
                passedObstacles.Add(obs);
                GameManager.Instance?.AddScore(1);
            }

            // Obstacle is far enough behind — destroy it
            if (playerZ - obsZ > destroyDistBehind)
            {
                passedObstacles.Remove(obs);
                Destroy(obs);
                activeObstacles.RemoveAt(i);
            }
        }
    }
}