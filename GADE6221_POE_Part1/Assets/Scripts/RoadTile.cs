using UnityEngine;

// Attach to the root of every road prefab.
// If the mesh pivot is offset, set lengthOverride manually in the Inspector.
public class RoadTile : MonoBehaviour
{
    [Tooltip("Leave at 0 to auto-detect from renderer bounds. Set manually if the prefab pivot is offset.")]
    [SerializeField] private float lengthOverride = 0f;

    public float length { get; private set; }
    public float startOffset { get; private set; }

    private void Awake()
    {
        if (lengthOverride > 0f)
        {
            length = lengthOverride;
            startOffset = 0f;
            return;
        }

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
