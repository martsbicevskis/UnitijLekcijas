using UnityEngine;

// This script handles the player's movement, including walking, jumping, and looking around with the mouse.
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // The speed at which the player moves.
    public float mouseSensitivity = 2f; // The sensitivity of the mouse look.
    public float jumpForce = 5f; // The force applied when the player jumps.

    [Header("References")]
    public Camera playerCamera; // A reference to the player's camera.

    [Header("Audio")]
    public AudioClip walkSound; // The sound to play while walking.
    public AudioClip jumpSound; // The sound to play when jumping.

    private CharacterController characterController; // The component that handles movement and collision.
    private float verticalRotation = 0f; // The current vertical rotation of the camera.
    private Vector3 moveDirection = Vector3.zero; // The direction of the player's movement.
    private bool isGrounded; // Whether the player is currently on the ground.

    // Fields for handling knockback effects.
    private Vector3 knockbackVelocity = Vector3.zero; // The current velocity of the knockback.
    private float knockbackTimer = 0f; // A timer to track the duration of the knockback.
    private float knockbackDuration = 0.2f; // How long the knockback effect lasts.
    private float knockbackDecay = 10f; // How quickly the knockback force decreases.

    private AudioSource audioSource; // The component for playing audio clips.
    private float walkStepCooldown = 0.4f; // The cooldown between walking sounds to prevent them from overlapping.
    private float lastWalkStepTime = 0f; // The time when the last walking sound was played.

    // Called when the script instance is being loaded.
    void Start()
    {
        // Get the CharacterController component attached to the player.
        characterController = GetComponent<CharacterController>();
        
        // If the player camera is not assigned, try to find it in the children.
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
        
        // Lock the cursor to the center of the screen and hide it.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Get or add an AudioSource component.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Called every frame.
    void Update()
    {
        // Handle mouse input for looking around.
        HandleMouseLook();

        // Handle keyboard input for movement.
        HandleMovement();

        // Handle jumping.
        HandleJumping();

        // Apply gravity and knockback forces.
        ApplyForces();
    }

    // Controls the camera rotation based on mouse movement.
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player horizontally based on mouse X movement.
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically based on mouse Y movement.
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Clamp the vertical rotation to prevent flipping.
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    // Handles player movement based on keyboard input.
    void HandleMovement()
    {
        isGrounded = characterController.isGrounded; // Check if the player is on the ground.

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Determine the movement direction based on input.
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // Apply the movement to the CharacterController.
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Play walking sound if the player is moving on the ground.
        if (isGrounded && move.magnitude > 0.1f)
        {
            if (walkSound != null && Time.time > lastWalkStepTime + walkStepCooldown)
            {
                audioSource.PlayOneShot(walkSound, 0.5f);
                lastWalkStepTime = Time.time;
            }
        }
    }

    // Handles the jump input.
    void HandleJumping()
    {
        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f; // Keep the player grounded.
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            moveDirection.y = jumpForce;
            if (jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound, 0.7f);
            }
        }
    }

    // Applies gravity and knockback forces to the player.
    void ApplyForces()
    {
        // Apply gravity.
        moveDirection.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);

        // Apply knockback if a knockback is active.
        if (knockbackTimer > 0f)
        {
            characterController.Move(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
            knockbackTimer -= Time.deltaTime;
        }
    }

    // Applies a knockback force to the player from a specific direction.
    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = knockbackDuration;
    }
} 