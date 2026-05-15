using UnityEngine;

public class PickupController : MonoBehaviour
{
    [Header("Pickup Type")]
    [SerializeField] public PickupType pickupType = PickupType.SpeedBoost;

    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private Material poisonMaterial;  // assign a purple material in Inspector

    [Header("Magnet Attraction")]
    [SerializeField] private float magnetRange = 15f;
    [SerializeField] private float magnetSpeed = 50f;

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

    // Called by PickupSpawner after instantiation so OnDestroy can release the registry slot
    public void RegisterSlot(int lane, float z)
    {
        registeredLane = lane;
        registeredZ = z;
    }

    public void Poison()
    {
        isPoisoned = true;

        // Apply poison material to every renderer in the pickup (handles multi-mesh models)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (poisonMaterial != null)
            {
                // Replace all material slots with the poison material
                Material[] slots = new Material[rend.materials.Length];
                for (int i = 0; i < slots.Length; i++) slots[i] = poisonMaterial;
                rend.materials = slots;
            }
            else
            {
                // Fallback: enable emission for a purple glow visible over any texture
                Material mat = rend.material;
                Color purple = new Color(0.55f, 0f, 1f);
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", purple * 2f);
            }
        }
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
}
