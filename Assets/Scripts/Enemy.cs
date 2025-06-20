using UnityEngine;
using UnityEngine.AI;

// This script defines the behavior of an enemy, including movement, pathfinding, attacking, and health management.
public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float health = 100f; // The current health of the enemy.
    public float moveSpeed = 3.5f; // The movement speed of the enemy.
    public float attackRange = 2f; // The range within which the enemy can attack the player.
    public float attackDamage = 10f; // The amount of damage the enemy deals to the player.
    public float attackCooldown = 1f; // The time between enemy attacks.

    [Header("Physics Settings")]
    public float gravity = 9.81f; // The force of gravity applied to the enemy.
    public float groundCheckDistance = 0.1f; // The distance to check for ground beneath the enemy.
    public LayerMask groundLayer = 1; // The layer considered as ground.
    public float airControl = 0.3f; // The amount of control the enemy has while in the air.

    [Header("Pathfinding Settings")]
    public float pathUpdateInterval = 0.5f; // How often the enemy updates its path to the player.
    public float stoppingDistance = 1f; // The distance at which the enemy stops moving towards its target.
    public float pathfindingHeight = 1f; // The height used for pathfinding checks.

    [Header("Barrel Interaction")]
    public float barrelKnockbackForce = 5f; // The force applied to the enemy when hit by a barrel.
    public float barrelDetectionRadius = 2f; // The radius to detect nearby barrels.
    public string barrelTag = "Barrel"; // The tag used to identify barrels.

    [Header("Visual Effects")]
    public GameObject hitEffect; // The effect to show when the enemy is hit.
    public float hitEffectDuration = 0.5f; // How long the hit effect lasts.

    [Header("Knockback Settings")]
    public float shotKnockbackForce = 5f; // The force of knockback when the enemy is shot.

    [Header("Color Settings")]
    public Color fullHealthColor = Color.green; // Color when at full health.
    public Color midHealthColor = Color.yellow; // Color at medium health.
    public Color lowHealthColor = Color.red; // Color at low health.

    [Header("Player Knockback Settings")]
    public float playerKnockbackForce = 7f; // The force to apply to the player on attack.

    [Header("Jump & Spin Settings")]
    public float jumpForce = 7f; // The force of the enemy's jump.
    public float spinForce = 360f; // The force of the enemy's spin.
    public float minJumpSpinInterval = 2f; // The minimum time between jump/spin actions.
    public float maxJumpSpinInterval = 6f; // The maximum time between jump/spin actions.

    [Header("Audio")]
    public AudioClip ghostAttackSound; // Sound for the enemy's attack.
    public AudioClip ghostIdleSound; // First idle sound.
    public AudioClip ghostIdleSound2; // Second idle sound.
    public AudioClip ghostIdleSound3; // Third idle sound.
    private AudioClip chosenIdleSound; // The currently selected idle sound.
    private Coroutine idleSoundCoroutine; // Coroutine for playing idle sounds.

    // Private variables for internal state management.
    private Transform player; // Reference to the player's transform.
    private Rigidbody rb; // The enemy's Rigidbody component.
    private NavMeshAgent pathfindingAgent; // The NavMeshAgent for pathfinding.
    private float lastAttackTime; // The time of the last attack.
    private bool isDead = false; // Whether the enemy is dead.
    private CapsuleCollider enemyCollider; // The enemy's collider.
    private bool isGrounded = false; // Whether the enemy is on the ground.
    private Vector3 moveDirection; // The current movement direction.
    private float nextPathUpdate; // The time for the next path update.
    private Vector3 currentDestination; // The current pathfinding destination.
    private bool hasValidPath = false; // Whether a valid path exists.
    private bool isPathfinding = false; // Whether pathfinding is in progress.
    private Vector3 lastHitDirection = Vector3.zero; // The direction of the last hit.
    private MeshRenderer meshRenderer; // The enemy's mesh renderer.
    private Material enemyMaterial; // The enemy's material instance.
    private bool doJumpSpin = false; // Flag to trigger a jump/spin.
    private float spinAmount = 0f; // The amount to spin.
    private bool isStunned = false; // Whether the enemy is stunned.
    private float stunTimer = 0f; // The timer for the stun duration.
    public GameObject stunEffectPrefab; // Visual effect for the stun.
    private GameObject activeStunEffect; // The active stun effect instance.
    private AudioSource audioSource; // The component for playing audio.
    private PlayerHealth playerHealth; // Reference to the player's health script.

    // Global cooldown to prevent idle sounds from overlapping.
    private static float lastGlobalIdleSoundTime = -100f;
    private static float globalIdleSoundCooldown = 2.5f;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Find and store a reference to the player and their health component.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        
        // Set up the Rigidbody for physics simulation.
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.mass = 1f;
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Configure the NavMeshAgent for pathfinding logic (not for movement).
        pathfindingAgent = GetComponent<NavMeshAgent>();
        if (pathfindingAgent == null)
        {
            pathfindingAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        pathfindingAgent.enabled = false; // Disable NavMeshAgent's automatic control.
        pathfindingAgent.radius = 0.5f;
        pathfindingAgent.height = 2f;
        pathfindingAgent.stoppingDistance = stoppingDistance;
        pathfindingAgent.updatePosition = false; // Physics-based movement will handle position.
        pathfindingAgent.updateRotation = false; // Rotation will be handled manually.

        // Ensure the enemy has a CapsuleCollider.
        enemyCollider = GetComponent<CapsuleCollider>();
        if (enemyCollider == null)
        {
            enemyCollider = gameObject.AddComponent<CapsuleCollider>();
            enemyCollider.height = 2f;
            enemyCollider.radius = 0.5f;
            enemyCollider.center = new Vector3(0, 1f, 0);
        }

        // Assign the enemy to the "Enemy" layer for collision and targeting purposes.
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        
        // Initialize pathfinding variables.
        nextPathUpdate = Time.time + pathUpdateInterval;
        currentDestination = transform.position;
        
        // Get the MeshRenderer and create a unique material instance to change its color.
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            enemyMaterial = meshRenderer.material;
        }
        else
        {
            Debug.LogWarning("Enemy MeshRenderer not found!");
        }
        
        Debug.Log($"Enemy spawned with health: {health}, layer: {LayerMask.LayerToName(gameObject.layer)}");
        // Start the routine for random jump/spin actions.
        StartCoroutine(JumpSpinRoutine());

        // Set up the AudioSource component.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Choose a random idle sound from the available clips.
        AudioClip[] idleSounds = new AudioClip[] { ghostIdleSound, ghostIdleSound2, ghostIdleSound3 };
        var validIdleSounds = System.Array.FindAll(idleSounds, s => s != null);
        if (validIdleSounds.Length > 0)
        {
            chosenIdleSound = validIdleSounds[Random.Range(0, validIdleSounds.Length)];
        }
        else
        {
            chosenIdleSound = null;
        }
    }

    // Called every frame.
    void Update()
    {
        // Do nothing if the enemy is dead.
        if (isDead) return;

        // Handle stun logic.
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                if (activeStunEffect != null) Destroy(activeStunEffect);
            }
            return;
        }

        // Check if the enemy is on the ground.
        CheckGrounded();

        // Update pathfinding periodically.
        if (Time.time >= nextPathUpdate && !isPathfinding)
        {
            UpdatePathfinding();
            nextPathUpdate = Time.time + pathUpdateInterval;
        }

        // Determine the movement direction based on the path.
        CalculateMovementDirection();

        // Check if the enemy is in range to attack the player.
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }

        // Check for nearby barrels to interact with.
        CheckForBarrels();
    }

    // Called every fixed-rate frame, used for physics calculations.
    void FixedUpdate()
    {
        // Do nothing if the enemy is dead or stunned.
        if (isDead) return;
        if (isStunned) return;

        // Apply movement force based on the calculated direction.
        if (moveDirection.magnitude > 0.1f)
        {
            float currentMoveSpeed = isGrounded ? moveSpeed : moveSpeed * airControl;
            Vector3 moveForce = moveDirection * currentMoveSpeed;
            rb.AddForce(moveForce, ForceMode.VelocityChange);
        }

        // Limit the enemy's horizontal velocity to the move speed.
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }

        // Execute jump and spin if triggered.
        if (doJumpSpin && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            rb.AddTorque(Vector3.up * spinAmount, ForceMode.Impulse);
            doJumpSpin = false;
        }
    }

    // Checks if the enemy is currently on the ground.
    void CheckGrounded()
    {
        // Use a raycast downwards to detect the ground.
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer);
        Debug.DrawRay(rayStart, Vector3.down * (groundCheckDistance + 0.1f), isGrounded ? Color.green : Color.red);
    }

    // Initiates the pathfinding process.
    void UpdatePathfinding()
    {
        // Ensure pathfinding is not already running.
        if (!isPathfinding)
        {
            // Start the coroutine to calculate the path.
            StartCoroutine(CalculatePath());
        }
    }

    // Coroutine to calculate the path to the player using NavMeshAgent.
    System.Collections.IEnumerator CalculatePath()
    {
        isPathfinding = true; // Mark pathfinding as in progress.
        
        // Temporarily enable NavMeshAgent to calculate the path.
        pathfindingAgent.enabled = true;
        pathfindingAgent.nextPosition = transform.position; // Ensure agent starts from the current position.

        NavMeshPath path = new NavMeshPath();
        
        // Check if a valid player position is available.
        if (player != null)
        {
            // Request a path calculation to the player's position.
            pathfindingAgent.CalculatePath(player.position, path);
        }

        // If the path calculation is successful and complete.
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            hasValidPath = true; // Mark that a valid path has been found.
            
            // If the path has points, set the destination to the next corner.
            if (path.corners.Length > 1)
            {
                currentDestination = path.corners[1]; // The next point to move towards.
            }
            else if (path.corners.Length == 1)
            {
                currentDestination = path.corners[0]; // If there's only one point, move directly to it.
            }
        }
        else
        {
            hasValidPath = false; // Mark the path as invalid.
        }

        // Disable the NavMeshAgent again to allow for physics-based movement.
        pathfindingAgent.enabled = false;
        isPathfinding = false; // Mark pathfinding as finished.
        yield return null; // End the coroutine.
    }

    // Determines the direction the enemy should move.
    void CalculateMovementDirection()
    {
        // If there's a valid path, move towards the next destination point.
        if (hasValidPath)
        {
            moveDirection = (currentDestination - transform.position).normalized;
            
            // Make the enemy face the direction of movement.
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0, moveDirection.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
            // If no valid path, stop moving.
            moveDirection = Vector3.zero;
        }
    }

    // Checks for nearby barrels and applies a force away from them.
    void CheckForBarrels()
    {
        // Find all colliders within the barrel detection radius on the default layer.
        Collider[] colliders = Physics.OverlapSphere(transform.position, barrelDetectionRadius, 1 << LayerMask.NameToLayer("Default"));
        
        foreach (var col in colliders)
        {
            // If a collider with the "Barrel" tag is found.
            if (col.CompareTag(barrelTag))
            {
                // Calculate a direction away from the barrel and apply a force.
                Vector3 directionFromBarrel = (transform.position - col.transform.position).normalized;
                rb.AddForce(directionFromBarrel * barrelKnockbackForce, ForceMode.Impulse);
                Debug.Log("Enemy avoided a barrel!");
            }
        }
    }

    // Logic for the enemy's attack.
    void Attack()
    {
        // Ensure the player exists and is not dead.
        if (player == null || (playerHealth != null && playerHealth.isDead)) return;
        
        // Play the attack sound if available.
        if (ghostAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(ghostAttackSound, 0.7f);
        }
        
        Debug.Log("Enemy attacking player!");
        // Reset the attack cooldown timer.
        lastAttackTime = Time.time;
        // Damage the player and apply knockback.
        playerHealth?.TakeDamage(attackDamage, (player.position - transform.position).normalized, playerKnockbackForce);
    }

    // Applies a knockback force to the enemy.
    public void ApplyKnockback(Vector3 direction, float force)
    {
        // Ensure the direction is normalized and apply the force.
        direction.Normalize();
        rb.AddForce(direction * force, ForceMode.Impulse);
        Debug.Log($"Enemy knocked back with force: {force}");
    }

    // Reduces the enemy's health and handles the consequences.
    public void TakeDamage(float amount, Vector3? hitDirection = null, float? knockbackForce = null)
    {
        // Do nothing if the enemy is already dead.
        if (isDead) return;

        health -= amount;
        Debug.Log($"Enemy took {amount} damage, health is now {health}");
        
        // Show a hit effect if one is assigned.
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, hitEffectDuration);
        }

        // Apply knockback if a direction and force are provided.
        if (hitDirection.HasValue)
        {
            lastHitDirection = hitDirection.Value;
            float force = knockbackForce.HasValue ? knockbackForce.Value : shotKnockbackForce;
            ApplyKnockback(lastHitDirection, force);
        }

        // If health drops to zero, the enemy dies.
        if (health <= 0)
        {
            Die();
        }
    }

    // Handles the enemy's death.
    void Die()
    {
        isDead = true;
        Debug.Log("Enemy has died.");
        
        // Stop any existing audio.
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Disable the collider to prevent further interactions.
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        // Stop the Rigidbody from moving.
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true; // Stop physics simulation.
        }

        // Destroy the enemy GameObject after a short delay.
        Destroy(gameObject, 2f);
    }

    // Draws debug gizmos in the editor for visualization.
    void OnDrawGizmosSelected()
    {
        // Draw a wire sphere to show the attack range.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw a wire sphere to show the barrel detection radius.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, barrelDetectionRadius);

        // Draw a line to show the current movement direction.
        if (moveDirection.magnitude > 0.1f)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + moveDirection * 2f);
        }
    }

    // Coroutine for the enemy's jump and spin behavior.
    System.Collections.IEnumerator JumpSpinRoutine()
    {
        // Loop indefinitely while the enemy is alive.
        while (!isDead)
        {
            // Wait for a random interval before the next action.
            float waitTime = Random.Range(minJumpSpinInterval, maxJumpSpinInterval);
            yield return new WaitForSeconds(waitTime);

            // Trigger a jump and spin.
            doJumpSpin = true;
            // Choose a random spin direction.
            spinAmount = Random.Range(0, 2) == 0 ? spinForce : -spinForce;

            // Play a random idle sound if available and cooldown has passed.
            if (chosenIdleSound != null && Time.time >= lastGlobalIdleSoundTime + globalIdleSoundCooldown)
            {
                // Play a sound with a lower volume.
                audioSource.PlayOneShot(chosenIdleSound, 0.15f);
                lastGlobalIdleSoundTime = Time.time;
            }
        }
    }

    // Stuns the enemy for a given duration.
    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        
        // Stop movement.
        rb.velocity = Vector3.zero;

        // Show stun effect if available.
        if (stunEffectPrefab != null && activeStunEffect == null)
        {
            activeStunEffect = Instantiate(stunEffectPrefab, transform.position + Vector3.up, Quaternion.identity, transform);
        }

        // Play an idle sound to indicate being stunned.
        if (chosenIdleSound != null)
        {
            // audioSource.PlayOneShot(chosenIdleSound, 0.4f); // Removed this line
        }
    }

    // Coroutine to play idle sounds at random intervals. (Currently unused)
    private System.Collections.IEnumerator PlayIdleSoundsRandomly(AudioClip[] idleSounds)
    {
        // This coroutine is not currently called but is kept for potential future use.
        while (!isDead)
        {
            yield return new WaitForSeconds(Random.Range(5f, 15f));
            var validSounds = System.Array.FindAll(idleSounds, s => s != null);
            if (validSounds.Length > 0)
            {
                AudioClip clip = validSounds[Random.Range(0, validSounds.Length)];
                audioSource.PlayOneShot(clip, 0.2f);
            }
        }
    }
} 