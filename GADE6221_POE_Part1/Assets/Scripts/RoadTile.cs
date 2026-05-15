using UnityEngine;

// Attach to the root empty parent of every road prefab.
// Measures length and pivot offset from the renderer bounds — no collider needed.
public class RoadTile : MonoBehaviour
{
    public float length { get; private set; }
    public float startOffset { get; private set; }

    private void Awake()
    {
        // Encapsulate all child renderers into one combined bounds
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds combined = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                combined.Encapsulate(renderers[i].bounds);

            length = combined.size.z;
            startOffset = combined.min.z - transform.position.z;
            return;
        }

        length = 30f;
        startOffset = 0f;
        Debug.LogWarning($"RoadTile on '{name}' found no Renderers — defaulting to length 30.");
    }
}
