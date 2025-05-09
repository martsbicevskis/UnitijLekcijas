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

    [Header("Visual Effects")]
    public GameObject hitEffect;
    public float hitEffectDuration = 0.5f;

    private Transform player;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isDead = false;
    private CapsuleCollider enemyCollider;

    void Start()
    {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Set up the NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

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
        Debug.Log($"Enemy spawned with health: {health}, layer: {LayerMask.LayerToName(gameObject.layer)}");
    }

    void Update()
    {
        if (isDead) return;

        // Move towards the player
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }

        // Check if we can attack the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        
        // Try to damage the player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"Enemy taking damage: {amount}. Current health: {health}");
        health -= amount;
        
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
    }

    void Die()
    {
        Debug.Log("Enemy dying");
        isDead = true;
        
        // Disable the NavMeshAgent
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Disable the collider
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // Destroy the enemy after a short delay
        Destroy(gameObject, 2f);
    }
} 