using System.Collections.Generic;
using UnityEngine;

// Score-based level switching.
// 0-999: Level 1 prefabs. 1000-1999: Level 2 prefabs. Loops every 2000 score.
public class LevelManager : MonoBehaviour
{
    [Header("Level 1 Section Prefabs")]
    [SerializeField] private GameObject[] level1Prefabs;

    [Header("Level 2 Section Prefabs")]
    [SerializeField] private GameObject[] level2Prefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float fallbackSectionLength = 30f;
    [SerializeField] private int sectionsAhead = 5;

    [Header("Destroy Settings")]
    [SerializeField] private float destroyBufferBehind = 20f;

    [Header("Score Thresholds")]
    [SerializeField] private int level2StartScore = 1000;   // score at which Level 2 begins each cycle
    [SerializeField] private int cycleLength = 2000;        // total score before looping back to Level 1

    private Transform playerTransform;
    private float nextSpawnZ;
    private List<GameObject> activeSections = new List<GameObject>();
    private Dictionary<GameObject, float> sectionEndZs = new Dictionary<GameObject, float>();

    private int currentLevel = 1;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        nextSpawnZ = playerTransform != null ? playerTransform.position.z : 0f;

        for (int i = 0; i < sectionsAhead + 2; i++)
            SpawnNextSection();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        int cycleScore = score % cycleLength;
        int newLevel = cycleScore < level2StartScore ? 1 : 2;

        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;
            EventManager.Instance?.RaiseLevelCompleted();
        }

        while (nextSpawnZ < playerTransform.position.z + sectionsAhead * fallbackSectionLength)
            SpawnNextSection();

        DestroyOldSections();
    }

    private void SpawnNextSection()
    {
        GameObject[] pool = currentLevel == 1 ? level1Prefabs : level2Prefabs;
        if (pool == null || pool.Length == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Length)];
        GameObject section = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        RoadTile tile = section.GetComponent<RoadTile>();
        float length = tile != null ? tile.length : fallbackSectionLength;
        float spawnZ = tile != null ? nextSpawnZ - tile.startOffset : nextSpawnZ;
        section.transform.position = new Vector3(0f, 0f, spawnZ);

        sectionEndZs[section] = nextSpawnZ + length;
        activeSections.Add(section);
        nextSpawnZ += length;
    }

    private void DestroyOldSections()
    {
        float playerZ = playerTransform.position.z;

        for (int i = activeSections.Count - 1; i >= 0; i--)
        {
            GameObject section = activeSections[i];
            if (section == null) { activeSections.RemoveAt(i); continue; }

            float endZ = sectionEndZs.TryGetValue(section, out float ez) ? ez : section.transform.position.z + fallbackSectionLength;
            if (playerZ - endZ > destroyBufferBehind)
            {
                sectionEndZs.Remove(section);
                Destroy(section);
                activeSections.RemoveAt(i);
            }
        }
    }
}
