using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private float laneSwitchSpeed = 10f;

    [Header("Forward Speed")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float speedRange = 40f;       // maxSpeed = forwardSpeed + speedRange
    [SerializeField] private float speedRampRate = 0.4f;
    [SerializeField] private float maxPhysicalSpeed = 55f; // hard cap to prevent phasing through colliders

    private float maxSpeed;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Invincibility Flash")]
    [SerializeField] private float flashInterval = 0.12f;

    private Rigidbody rb;
    private int currentLane = 1;          // 0 = left, 1 = centre, 2 = right
    private float targetX;
    private bool isGrounded = true;
    private bool isDead = false;
    private bool isInvincible = false;
    private float baseSpeed;
    private float currentMultiplier = 1f;
    private Renderer[] renderers;

    public int CurrentLane => currentLane;

    private float LaneToX(int lane) => (lane - 1) * laneWidth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;
        // Continuous collision detection prevents phasing through thin colliders at high speed
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        targetX = LaneToX(currentLane);
        baseSpeed = forwardSpeed;
        maxSpeed = forwardSpeed + speedRange;

        renderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        if (isDead) return;
        HandleLaneInput();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        RampSpeed();
        MoveForward();
        SmoothLaneSwitch();
        CheckGrounded();
    }

    private void RampSpeed()
    {
        baseSpeed = Mathf.Min(baseSpeed + speedRampRate * Time.fixedDeltaTime, maxSpeed);
        // Cap effective speed so the player can't phase through colliders or pickups
        forwardSpeed = Mathf.Min(baseSpeed * currentMultiplier, maxPhysicalSpeed);
    }

    private void HandleLaneInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            SetLane(currentLane - 1);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            SetLane(currentLane + 1);
    }

    private void SetLane(int lane)
    {
        currentLane = Mathf.Clamp(lane, 0, 2);
        targetX = LaneToX(currentLane);
    }

    private void HandleJumpInput()
    {
        if (isGrounded && (Input.GetKeyDown(KeyCode.Space) ||
                           Input.GetKeyDown(KeyCode.W) ||
                           Input.GetKeyDown(KeyCode.UpArrow)))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void MoveForward()
    {
        Vector3 pos = rb.position;
        pos.z += forwardSpeed * Time.fixedDeltaTime;
        rb.MovePosition(pos);
    }

    private void SmoothLaneSwitch()
    {
        Vector3 pos = rb.position;
        pos.x = Mathf.Lerp(pos.x, targetX, laneSwitchSpeed * Time.fixedDeltaTime);
        rb.MovePosition(pos);
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.6f, groundLayer);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentMultiplier = multiplier;
    }

    public void ResetSpeed()
    {
        currentMultiplier = 1f;
    }

    public void TriggerDeath()
    {
        isDead = true;
        StopAllCoroutines();
        SetRenderersVisible(true);
    }

    public void StartInvincibility(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(InvincibilityRoutine(duration));
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < duration)
        {
            visible = !visible;
            SetRenderersVisible(visible);
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        isInvincible = false;
        SetRenderersVisible(true);
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (Renderer r in renderers)
            if (r != null) r.enabled = visible;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Obstacle"))
        {
            if (PickupManager.Instance != null && PickupManager.Instance.IsShieldActive) return;
            if (isInvincible) return;
            GameManager.Instance?.TakeDamage();
        }
        else if (other.CompareTag("Pickup"))
        {
            PickupController pc = other.GetComponent<PickupController>();
            if (pc != null) pc.Collect();
        }
    }
}
