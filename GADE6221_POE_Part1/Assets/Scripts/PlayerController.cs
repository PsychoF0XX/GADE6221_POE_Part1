
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
    [SerializeField] private float speedRange = 60f;       // maxSpeed = forwardSpeed + speedRange
    [SerializeField] private float speedRampRate = 0.4f;   // units/sec added to base speed over time

    private float maxSpeed;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private int currentLane = 1;          // 0 = left, 1 = centre, 2 = right
    private float targetX;
    private bool isGrounded = true;
    private bool isDead = false;
    private bool isInvincible = false;
    private float baseSpeed;
    private float currentMultiplier = 1f;

    private float LaneToX(int lane) => (lane - 1) * laneWidth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;
        targetX = LaneToX(currentLane);
        baseSpeed = forwardSpeed;
        maxSpeed = forwardSpeed + speedRange;
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
        // Always ramp baseSpeed — even during a boost
        baseSpeed = Mathf.Min(baseSpeed + speedRampRate * Time.fixedDeltaTime, maxSpeed);
        // forwardSpeed always reflects baseSpeed * current multiplier (1x when no boost)
        forwardSpeed = baseSpeed * currentMultiplier;
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

    // Called by GameManager when all lives are lost
    public void TriggerDeath()
    {
        isDead = true;
    }

    // Called by GameManager when player takes damage but still has lives remaining
    public void StartInvincibility(float duration)
    {
        StartCoroutine(InvincibilityRoutine(duration));
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
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
