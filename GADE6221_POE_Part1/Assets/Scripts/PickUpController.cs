using UnityEngine;

public class PickupController : MonoBehaviour
{
    [Header("Pickup Type")]
    [SerializeField] public PickupType pickupType = PickupType.SpeedBoost;

    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 90f;

    private void Start()
    {
        gameObject.tag = "Pickup";

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
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