using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float health = 100f;
    public float moveSpeed = 3.5f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    [Header("Physics Settings")]
    public float gravity = 9.81f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer = 1; // Default layer
    public float airControl = 0.3f; // How much control enemy has while in air

    [Header("Pathfinding Settings")]
    public float pathUpdateInterval = 0.5f;
    public float stoppingDistance = 1f;
    public float pathfindingHeight = 1f; // Height to check for pathfinding

    [Header("Barrel Interaction")]
    public float barrelKnockbackForce = 5f;
    public float barrelDetectionRadius = 2f;
    public string barrelTag = "Barrel"; // Configurable tag

    [Header("Visual Effects")]
    public GameObject hitEffect;
    public float hitEffectDuration = 0.5f;

    [Header("Knockback Settings")]
    public float shotKnockbackForce = 5f; // Knockback force when shot

    [Header("Color Settings")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Player Knockback Settings")]
    public float playerKnockbackForce = 7f;

    [Header("Jump & Spin Settings")]
    public float jumpForce = 7f;
    public float spinForce = 360f;
    public float minJumpSpinInterval = 2f;
    public float maxJumpSpinInterval = 6f;

    private Transform player;
    private Rigidbody rb;
    private NavMeshAgent pathfindingAgent;
    private float lastAttackTime;
    private bool isDead = false;
    private CapsuleCollider enemyCollider;
    private bool isGrounded = false;
    private Vector3 moveDirection;
    private float nextPathUpdate;
    private Vector3 currentDestination;
    private bool hasValidPath = false;
    private bool isPathfinding = false;
    private Vector3 lastHitDirection = Vector3.zero; // Store last hit direction
    private MeshRenderer meshRenderer;
    private Material enemyMaterial;
    private bool doJumpSpin = false;
    private float spinAmount = 0f;
    private bool isStunned = false;
    private float stunTimer = 0f;
    public GameObject stunEffectPrefab; // Assign in inspector if you want a visual effect
    private GameObject activeStunEffect;

    void Start()
    {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Set up Rigidbody for physics
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

        // Set up NavMeshAgent for pathfinding (but not movement)
        pathfindingAgent = GetComponent<NavMeshAgent>();
        if (pathfindingAgent == null)
        {
            pathfindingAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        // Configure NavMeshAgent to not interfere with physics
        pathfindingAgent.enabled = false;
        pathfindingAgent.radius = 0.5f;
        pathfindingAgent.height = 2f;
        pathfindingAgent.stoppingDistance = stoppingDistance;
        pathfindingAgent.updatePosition = false; // Don't let NavMeshAgent control position
        pathfindingAgent.updateRotation = false; // Don't let NavMeshAgent control rotation

        // Set up the collider
        enemyCollider = GetComponent<CapsuleCollider>();
        if (enemyCollider == null)
        {
            enemyCollider = gameObject.AddComponent<CapsuleCollider>();
            enemyCollider.height = 2f;
            enemyCollider.radius = 0.5f;
            enemyCollider.center = new Vector3(0, 1f, 0);
        }

        // Make sure the enemy is on the correct layer
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        
        // Initialize pathfinding
        nextPathUpdate = Time.time + pathUpdateInterval;
        currentDestination = transform.position;
        
        // Get the MeshRenderer (assume it's on a child called "Body" or on this GameObject)
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Use a unique material instance for this enemy
            enemyMaterial = meshRenderer.material;
        }
        else
        {
            Debug.LogWarning("Enemy MeshRenderer not found!");
        }
        UpdateColor();
        
        Debug.Log($"Enemy spawned with health: {health}, layer: {LayerMask.LayerToName(gameObject.layer)}");
        StartCoroutine(JumpSpinRoutine());
    }

    void Update()
    {
        if (isDead) return;
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

        // Check if grounded
        CheckGrounded();

        // Update pathfinding
        if (Time.time >= nextPathUpdate && !isPathfinding)
        {
            UpdatePathfinding();
            nextPathUpdate = Time.time + pathUpdateInterval;
        }

        // Calculate movement direction
        CalculateMovementDirection();

        // Check if we can attack the player
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }

        // Check for nearby barrels
        CheckForBarrels();

        UpdateColor();
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (isStunned) return;

        // Apply movement force
        if (moveDirection.magnitude > 0.1f)
        {
            float currentMoveSpeed = isGrounded ? moveSpeed : moveSpeed * airControl;
            Vector3 moveForce = moveDirection * currentMoveSpeed;
            rb.AddForce(moveForce, ForceMode.VelocityChange);
        }

        // Limit horizontal velocity
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }

        // Handle jump and spin
        if (doJumpSpin && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            rb.AddTorque(Vector3.up * spinAmount, ForceMode.Impulse);
            doJumpSpin = false;
        }
    }

    void CheckGrounded()
    {
        // Cast a ray downward to check if grounded
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer);
    }

    void UpdatePathfinding()
    {
        if (player == null || isPathfinding) return;

        // Only attempt pathfinding if we're grounded and on a NavMesh
        if (!isGrounded)
        {
            // If not grounded, use direct movement
            currentDestination = player.position;
            hasValidPath = false;
            return;
        }

        isPathfinding = true;
        StartCoroutine(CalculatePath());
    }

    System.Collections.IEnumerator CalculatePath()
    {
        // Temporarily enable NavMeshAgent for pathfinding
        pathfindingAgent.enabled = true;
        
        // Wait a frame to ensure the agent is properly initialized
        yield return null;
        
        // Check if the agent is on a NavMesh before attempting pathfinding
        if (!pathfindingAgent.isOnNavMesh)
        {
            Debug.LogWarning($"Enemy at {transform.position} is not on NavMesh, using direct movement");
            pathfindingAgent.enabled = false;
            isPathfinding = false;
            
            // Fallback to direct movement
            currentDestination = player.position;
            hasValidPath = false;
            yield break;
        }

        // Attempt pathfinding
        bool pathfindingSuccess = false;
        try
        {
            pathfindingAgent.SetDestination(player.position);
            pathfindingSuccess = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Pathfinding failed for enemy: {e.Message}");
            pathfindingSuccess = false;
        }

        // Wait a frame for the path to be calculated (only if pathfinding was successful)
        if (pathfindingSuccess)
        {
            yield return null;
            
            if (pathfindingAgent.hasPath && pathfindingAgent.remainingDistance > stoppingDistance)
            {
                // Get the next waypoint on the path
                if (pathfindingAgent.path.corners.Length > 1)
                {
                    currentDestination = pathfindingAgent.path.corners[1];
                    hasValidPath = true;
                }
                else
                {
                    currentDestination = player.position;
                    hasValidPath = true;
                }
            }
            else
            {
                // No valid path, move directly towards player
                currentDestination = player.position;
                hasValidPath = false;
            }
        }
        else
        {
            // Fallback to direct movement
            currentDestination = player.position;
            hasValidPath = false;
        }

        // Always disable NavMeshAgent after pathfinding
        pathfindingAgent.enabled = false;
        isPathfinding = false;
    }

    void CalculateMovementDirection()
    {
        if (player == null) return;

        if (hasValidPath)
        {
            // Use pathfinding destination
            Vector3 directionToDestination = (currentDestination - transform.position).normalized;
            directionToDestination.y = 0; // Keep movement horizontal
            moveDirection = directionToDestination;
        }
        else
        {
            // Fallback to direct movement towards player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0; // Keep movement horizontal
            moveDirection = directionToPlayer;
        }
    }

    void CheckForBarrels()
    {
        // Find all barrels within detection radius
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, barrelDetectionRadius);
        
        foreach (Collider col in nearbyColliders)
        {
            // Safer tag check
            if (col.gameObject.CompareTag(barrelTag))
            {
                // Apply knockback force away from barrel
                Vector3 knockbackDirection = (transform.position - col.transform.position).normalized;
                knockbackDirection.y = 0.5f; // Add some upward force
                
                if (rb != null)
                {
                    rb.AddForce(knockbackDirection * barrelKnockbackForce, ForceMode.Impulse);
                    Debug.Log($"Enemy knocked back by barrel with force: {barrelKnockbackForce}");
                }
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        
        // Try to damage the player
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Knockback direction: from enemy to player
                Vector3 knockbackDir = (player.position - transform.position).normalized;
                playerHealth.TakeDamage(attackDamage, knockbackDir, playerKnockbackForce);
            }
        }
    }

    // Call this to apply knockback when shot
    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (rb != null && !isDead)
        {
            Vector3 knockback = direction.normalized;
            knockback.y = 0.3f; // Add a bit of upward force
            rb.AddForce(knockback * force, ForceMode.Impulse);
            lastHitDirection = knockback;
            Debug.Log($"Enemy knocked back with force {force} in direction {knockback}");
        }
    }

    public void TakeDamage(float amount, Vector3? hitDirection = null, float? knockbackForce = null)
    {
        Debug.Log($"Enemy taking damage: {amount}. Current health: {health}");
        health -= amount;
        
        // Apply knockback if direction is provided
        if (hitDirection.HasValue)
        {
            ApplyKnockback(hitDirection.Value, knockbackForce ?? shotKnockbackForce);
        }
        
        // Create hit effect
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(effect, hitEffectDuration);
        }

        if (health <= 0 && !isDead)
        {
            Debug.Log("Enemy health reached 0, calling Die()");
            Die();
        }
        else
        {
            Debug.Log($"Enemy health after damage: {health}");
        }

        UpdateColor();
    }

    void Die()
    {
        Debug.Log("Enemy dying");
        isDead = true;
        
        // Disable physics
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Disable pathfinding
        if (pathfindingAgent != null)
        {
            pathfindingAgent.enabled = false;
        }

        // Disable the collider
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // Destroy the enemy after a short delay
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        // Draw barrel detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, barrelDetectionRadius);
        
        // Draw ground check ray
        Gizmos.color = Color.green;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawRay(rayStart, Vector3.down * (groundCheckDistance + 0.1f));
        
        // Draw current destination
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(currentDestination, 0.5f);
        Gizmos.DrawLine(transform.position, currentDestination);
    }

    // Interpolate color based on health
    void UpdateColor()
    {
        if (enemyMaterial == null) return;
        float healthPercent = Mathf.Clamp01(health / 100f);
        Color lerpedColor;
        if (healthPercent > 0.5f)
        {
            // Green to yellow
            float t = (healthPercent - 0.5f) * 2f;
            lerpedColor = Color.Lerp(midHealthColor, fullHealthColor, t);
        }
        else
        {
            // Red to yellow
            float t = healthPercent * 2f;
            lerpedColor = Color.Lerp(lowHealthColor, midHealthColor, t);
        }
        enemyMaterial.color = lerpedColor;
    }

    System.Collections.IEnumerator JumpSpinRoutine()
    {
        while (!isDead)
        {
            float wait = Random.Range(minJumpSpinInterval, maxJumpSpinInterval);
            yield return new WaitForSeconds(wait);
            if (!isDead && isGrounded)
            {
                doJumpSpin = true;
                spinAmount = Random.Range(-spinForce, spinForce);
            }
        }
    }

    public void Stun(float duration)
    {
        if (isDead) return;
        isStunned = true;
        stunTimer = duration;
        if (stunEffectPrefab != null && activeStunEffect == null)
        {
            activeStunEffect = Instantiate(stunEffectPrefab, transform.position + Vector3.up, Quaternion.identity, transform);
        }
    }
} 