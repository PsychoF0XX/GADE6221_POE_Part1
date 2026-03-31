using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Terrain Section Prefabs (assign at least 3 in Inspector)")]
    [SerializeField] private GameObject[] sectionPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float sectionLength = 30f;   // Z length of one terrain section
    [SerializeField] private int sectionsAhead = 5;     // how many sections to keep spawned
    [SerializeField] private float destroyDistBehind = 35f;  // destroy when this far behind player

    private Transform playerTransform;
    private float nextSpawnZ;
    private List<GameObject> activeSections = new List<GameObject>();

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        nextSpawnZ = playerTransform != null ? playerTransform.position.z : 0f;

        // Pre-generate sections so the level is visible from the start
        for (int i = 0; i < sectionsAhead + 2; i++)
            SpawnNextSection();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Keep spawning sections far enough ahead
        while (nextSpawnZ < playerTransform.position.z + sectionsAhead * sectionLength)
            SpawnNextSection();

        DestroyOldSections();
    }


    private void SpawnNextSection()
    {
        if (sectionPrefabs == null || sectionPrefabs.Length == 0) return;

        GameObject prefab = sectionPrefabs[Random.Range(0, sectionPrefabs.Length)];
        Vector3 pos = new Vector3(0f, 0f, nextSpawnZ);
        GameObject section = Instantiate(prefab, pos, Quaternion.identity);

        activeSections.Add(section);
        nextSpawnZ += sectionLength;
    }

    private void DestroyOldSections()
    {
        float playerZ = playerTransform.position.z;

        for (int i = activeSections.Count - 1; i >= 0; i--)
        {
            GameObject section = activeSections[i];
            if (section == null) { activeSections.RemoveAt(i); continue; }

            // The end of this section is its Z position + section length
            float sectionEndZ = section.transform.position.z + sectionLength;

            if (playerZ - sectionEndZ > destroyDistBehind)
            {
                Destroy(section);
                activeSections.RemoveAt(i);
            }
        }
    }
}