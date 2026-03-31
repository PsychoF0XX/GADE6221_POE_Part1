
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private float laneSwitchSpeed = 10f;   // lerp speed

    [Header("Forward Speed")]
    [SerializeField] private float forwardSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private int currentLane = 1;          // 0 = left, 1 = centre, 2 = right
    private float targetX;
    private bool isGrounded = true;
    private bool isDead = false;

    private float LaneToX(int lane) => (lane - 1) * laneWidth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        targetX = LaneToX(currentLane);
        baseSpeed = forwardSpeed;
    }

    private void Update()
    {
        if (isDead) return;
        HandleLaneInput();
        HandleJumpInput();

        Debug.Log($"Speed: {forwardSpeed}");
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        MoveForward();
        SmoothLaneSwitch();
        CheckGrounded();
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

    private float baseSpeed;

    public void SetSpeedMultiplier(float multiplier)
    {
        baseSpeed = forwardSpeed;
        forwardSpeed = forwardSpeed * multiplier;
    }

    public void ResetSpeed()
    {
        forwardSpeed = baseSpeed;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Obstacle"))
        {
            // Check if shield pickup is currently protecting the player
            if (PickupManager.Instance != null && PickupManager.Instance.IsShieldActive)
                return;

            isDead = true;
            GameManager.Instance?.OnPlayerDied();
        }
        else if (other.CompareTag("Pickup"))
        {
            PickupController pc = other.GetComponent<PickupController>();
            if (pc != null) pc.Collect();
        }
    }
}