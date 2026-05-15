using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private float defaultDuration  = 0.3f;
    [SerializeField] private float defaultMagnitude = 0.2f;

    private float magnitude;
    private float duration;
    private float elapsed;
    private bool isShaking;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Shake() => Shake(defaultDuration, defaultMagnitude);

    public void Shake(float duration, float magnitude)
    {
        this.duration  = duration;
        this.magnitude = magnitude;
        elapsed   = 0f;
        isShaking = true;
    }

    // LateUpdate runs after all camera follow scripts, so the offset always wins
    private void LateUpdate()
    {
        if (!isShaking) return;

        elapsed += Time.deltaTime;
        if (elapsed >= duration) { isShaking = false; return; }

        // Fade strength toward zero so the shake eases out rather than snapping
        float strength = magnitude * (1f - elapsed / duration);
        transform.position += (Vector3)Random.insideUnitCircle * strength;
    }
}
