using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;

    [Header("References")]
    public Camera playerCamera; // Reference to the player's camera

    private CharacterController characterController;
    private float verticalRotation = 0f;
    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded;

    // Knockback fields
    private Vector3 knockbackVelocity = Vector3.zero;
    private float knockbackTimer = 0f;
    private float knockbackDuration = 0.2f;
    private float knockbackDecay = 10f;

    void Start()
    {
        // Get the CharacterController component
        characterController = GetComponent<CharacterController>();
        
        // If no camera is assigned, try to find it
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
        
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Handle mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera up/down
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Limit looking up/down to 90 degrees
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Handle movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // Apply movement
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            moveDirection.y = jumpForce;
        }

        // Apply gravity
        moveDirection.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);

        // Check if player is grounded
        isGrounded = characterController.isGrounded;
        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f;
        }

        // Apply knockback if active
        if (knockbackTimer > 0f)
        {
            characterController.Move(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
            knockbackTimer -= Time.deltaTime;
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = knockbackDuration;
    }
} 