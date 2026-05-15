using UnityEngine;

public class PickupController : MonoBehaviour
{
    [Header("Pickup Type")]
    [SerializeField] public PickupType pickupType = PickupType.SpeedBoost;

    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private ParticleSystem glowEffect;  // assign a particle system child in Inspector

    [Header("Magnet Attraction")]
    [SerializeField] private float magnetRange = 15f;
    [SerializeField] private float magnetSpeed = 50f;

    private static readonly Color GlowGreen  = new Color(0f,   1f,   0.2f);
    private static readonly Color GlowPurple = new Color(0.55f, 0f,  1f);

    private Transform playerTransform;
    private bool isPoisoned = false;
    private bool collected = false;
    private int registeredLane = -1;
    private float registeredZ = 0f;

    private void Start()
    {
        gameObject.tag = "Pickup";

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        transform.rotation = Quaternion.Euler(0f, 0f, -60f);

        SetGlowColour(GlowGreen);
    }

    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);

        if (!isPoisoned
            && playerTransform != null
            && PickupManager.Instance != null
            && PickupManager.Instance.IsMagnetActive)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist < magnetRange)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    playerTransform.position,
                    magnetSpeed * Time.deltaTime);
            }
        }
    }

    public void RegisterSlot(int lane, float z)
    {
        registeredLane = lane;
        registeredZ = z;
    }

    public void Poison()
    {
        isPoisoned = true;
        SetGlowColour(GlowPurple);
    }

    public void Collect()
    {
        if (collected) return;
        collected = true;

        if (isPoisoned)
            GameManager.Instance?.TakeDamage();
        else
            PickupManager.Instance?.ActivatePickup(pickupType);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (registeredLane >= 0)
            SpawnRegistry.Release(registeredLane, registeredZ);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Collect();
    }

    private void SetGlowColour(Color colour)
    {
        if (glowEffect == null) return;
        var main = glowEffect.main;
        main.startColor = colour;
        if (!glowEffect.isPlaying) glowEffect.Play();
    }
}
