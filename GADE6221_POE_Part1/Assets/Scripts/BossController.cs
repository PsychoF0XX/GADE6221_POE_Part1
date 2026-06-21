using UnityEngine;

// Attach to the cop drone GameObject.
// BossSpawner calls Activate() when the drone appears and Deactivate() when beaten.
// Car-dropping attack removed — will be reimplemented in Level 2 via poisoned pickups.
public class BossController : MonoBehaviour
{
    private bool isActive = false;

    public void Activate()
    {
        if (isActive) return;
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }
}
