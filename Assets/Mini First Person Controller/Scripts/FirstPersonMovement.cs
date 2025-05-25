using UnityEngine;
using System.Collections.Generic; // Add this line to fix the error

public class FirstPersonMovement : MonoBehaviour
{
    public float walkSpeed = 8f; // Normal walking speed
    public float runSpeed = 12f; // Sprinting speed
    public float jumpForce = 7f; // Jump strength
    public float mouseSensitivity = 100f; // Sensitivity for mouse movement

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Jumping")]
    public bool canJump = true;
    public LayerMask groundLayer;
    private bool isGrounded;

    private Rigidbody rb;
    private CapsuleCollider col;

    private float xRotation = 0f; // Tracks vertical rotation (up/down)

    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        // Ensure Rigidbody settings are correct
        rb.freezeRotation = true;

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        // Check if the player is on the ground using SphereCast (better than Raycast)
        isGrounded = Physics.SphereCast(transform.position, col.radius, Vector3.down, out RaycastHit hit, col.height / 2 + 0.1f, groundLayer);

        // Check for running input
        bool forwardKeyPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        IsRunning = canRun && Input.GetKey(runningKey) && forwardKeyPressed;

        // Determine movement speed
        float targetSpeed = IsRunning ? runSpeed : walkSpeed;
        if (speedOverrides.Count > 0)
        {
            targetSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Get movement input
        float moveX = Input.GetAxis("Horizontal") * targetSpeed;
        float moveZ = Input.GetAxis("Vertical") * targetSpeed;

        // Move the player
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);
    }

    void Update()
    {
        // Handle Jumping
        if (canJump && isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Handle Mouse Input for Camera Rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the camera vertically (up/down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent over-rotation

        // Apply vertical rotation to the camera
        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player horizontally (left/right)
        transform.Rotate(Vector3.up * mouseX);
    }
}