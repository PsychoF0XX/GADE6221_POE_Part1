using System.Collections.Generic;
using UnityEngine;

// Tracks claimed spawn slots so obstacles and pickups can't share the same lane/Z position.
// ObstacleSpawner claims slots when spawning; PickupSpawner checks before spawning.
// Slots are released when the object is destroyed.
public static class SpawnRegistry
{
    private static readonly List<(int lane, float z)> claimed = new();
    private const float ZTolerance = 3f;

    public static bool TryClaim(int lane, float z)
    {
        foreach (var c in claimed)
            if (c.lane == lane && Mathf.Abs(c.z - z) < ZTolerance)
                return false;
        claimed.Add((lane, z));
        return true;
    }

    public static void Release(int lane, float z)
    {
        for (int i = claimed.Count - 1; i >= 0; i--)
            if (claimed[i].lane == lane && Mathf.Abs(claimed[i].z - z) < ZTolerance)
            { claimed.RemoveAt(i); return; }
    }

    public static void Clear() => claimed.Clear();
}
