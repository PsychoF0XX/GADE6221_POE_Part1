using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Terrain Section Prefabs (assign at least 3 in Inspector)")]
    [SerializeField] private GameObject[] sectionPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float fallbackSectionLength = 30f;
    [SerializeField] private int sectionsAhead = 5;

    [Header("Destroy Settings")]
    [SerializeField] private float destroyBufferBehind = 20f;  // how far past the section's END before destroying

    private Transform playerTransform;
    private float nextSpawnZ;
    private List<GameObject> activeSections = new List<GameObject>();
    // Stores the world Z where each section's mesh actually ends
    private Dictionary<GameObject, float> sectionEndZs = new Dictionary<GameObject, float>();

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
        if (sectionPrefabs == null || sectionPrefabs.Length == 0) return;

        GameObject prefab = sectionPrefabs[Random.Range(0, sectionPrefabs.Length)];
        // Instantiate at origin first so Awake runs and RoadTile can measure bounds
        GameObject section = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        RoadTile tile = section.GetComponent<RoadTile>();
        float length = tile != null ? tile.length : fallbackSectionLength;

        // Correct for pivot offset so the mesh front edge sits exactly at nextSpawnZ
        float spawnZ = tile != null ? nextSpawnZ - tile.startOffset : nextSpawnZ;
        section.transform.position = new Vector3(0f, 0f, spawnZ);

        // Record where this section's mesh actually ends in world space
        float meshEndZ = nextSpawnZ + length;
        sectionEndZs[section] = meshEndZ;
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

            // Only destroy once the player is destroyBufferBehind past the section's END
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
