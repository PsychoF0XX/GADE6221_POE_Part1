using System.Collections.Generic;
using UnityEngine;

// Manages infinite terrain spawning across two distinct level types.
// Level 1 prefabs and Level 2 prefabs are assigned separately in the Inspector.
// The game always starts Level 1 → Level 2, then randomises from there.
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

    [Header("Level Length")]
    [SerializeField] private int sectionsPerLevel = 10;   // how many sections before switching levels

    private Transform playerTransform;
    private float nextSpawnZ;
    private List<GameObject> activeSections = new List<GameObject>();
    private Dictionary<GameObject, float> sectionEndZs = new Dictionary<GameObject, float>();

    private int currentLevel = 1;           // 1 or 2
    private int sectionsSpawnedThisLevel = 0;
    private bool pastInitialTwo = false;    // true once both level 1 and level 2 have run once

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

        float meshEndZ = nextSpawnZ + length;
        sectionEndZs[section] = meshEndZ;
        activeSections.Add(section);
        nextSpawnZ += length;

        sectionsSpawnedThisLevel++;
        if (sectionsSpawnedThisLevel >= sectionsPerLevel)
            AdvanceLevel();
    }

    private void AdvanceLevel()
    {
        sectionsSpawnedThisLevel = 0;

        if (!pastInitialTwo)
        {
            // First pass: always go 1 → 2
            if (currentLevel == 1)
            {
                currentLevel = 2;
            }
            else
            {
                // Finished level 2 for the first time — now randomise
                pastInitialTwo = true;
                currentLevel = Random.Range(0, 2) == 0 ? 1 : 2;
            }
        }
        else
        {
            // Random pick after the initial two levels
            currentLevel = Random.Range(0, 2) == 0 ? 1 : 2;
        }

        // Fire the event so GameManager can track levels beaten
        EventManager.Instance?.RaiseLevelCompleted();
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
