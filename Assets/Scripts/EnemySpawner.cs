using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 5f;
    public float spawnDistance = 20f;
    public int maxEnemies = 10;

    [Header("Enemy Settings")]
    public float minHealth = 50f;
    public float maxHealth = 150f;
    public float minSpeed = 2f;
    public float maxSpeed = 5f;

    private float nextSpawnTime;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        // Check if we've reached the maximum number of enemies
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies)
            return;

        // Calculate a random position around the player
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        Vector3 spawnPosition = player.position + randomDirection * spawnDistance;
        spawnPosition.y = 0; // Keep enemies on the ground

        // Check if the position is valid (on the NavMesh)
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            // Spawn the enemy
            GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);
            
            // Set the enemy tag and layer
            enemy.tag = "Enemy";
            enemy.layer = LayerMask.NameToLayer("Enemy");
            
            // Set random properties
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.health = Random.Range(minHealth, maxHealth);
                enemyComponent.moveSpeed = Random.Range(minSpeed, maxSpeed);
            }
        }
    }
} 