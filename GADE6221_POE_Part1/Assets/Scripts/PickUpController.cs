using UnityEngine;

public class PickupController : MonoBehaviour
{
    [Header("Pickup Type")]
    [SerializeField] public PickupType pickupType = PickupType.SpeedBoost;

    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Magnet Attraction")]
    [SerializeField] private float magnetRange = 15f;
    [SerializeField] private float magnetSpeed = 20f;

    private Transform playerTransform;

    private void Start()
    {
        gameObject.tag = "Pickup";

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // When magnet is active, pull this pickup toward the player regardless of lane
        if (playerTransform != null
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

    public void Collect()
    {
        PickupManager.Instance?.ActivatePickup(pickupType);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Collect();
    }
}
