using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 5f;
    public float spawnDistance = 35f;
    public int maxEnemies = 10;
    public float spawnHeight = 10f; // Height above ground to spawn enemies
    public float spawnHeightVariation = 3f; // Random variation in spawn height

    [Header("Enemy Settings")]
    public float minHealth = 50f;
    public float maxHealth = 150f;
    public float minSpeed = 2f;
    public float maxSpeed = 5f;

    [Header("Barrel Spawning")]
    public GameObject barrelPrefab;
    public float barrelSpawnInterval = 6.5f; // Slightly slower than enemy spawn
    public int maxBarrels = 10;

    private float nextSpawnTime;
    private float nextBarrelSpawnTime;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        nextSpawnTime = Time.time + spawnInterval;
        nextBarrelSpawnTime = Time.time + barrelSpawnInterval;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
        if (Time.time >= nextBarrelSpawnTime)
        {
            SpawnBarrel();
            nextBarrelSpawnTime = Time.time + barrelSpawnInterval;
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
        
        // Add random height variation
        float randomHeight = spawnHeight + Random.Range(-spawnHeightVariation, spawnHeightVariation);
        spawnPosition.y = randomHeight; // Spawn enemies in the air

        // Spawn the enemy at the calculated position
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.transform.localScale = Vector3.one * 1.5f;
        
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
        
        Debug.Log($"Enemy spawned at height: {spawnPosition.y}");
    }

    void SpawnBarrel()
    {
        if (barrelPrefab == null) return;
        if (GameObject.FindGameObjectsWithTag("Barrel").Length >= maxBarrels) return;
        // Calculate a random position around the player
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        Vector3 spawnPosition = player.position + randomDirection * spawnDistance;
        // Add random height variation
        float randomHeight = spawnHeight + Random.Range(-spawnHeightVariation, spawnHeightVariation);
        spawnPosition.y = randomHeight;
        // Spawn the barrel at the calculated position
        GameObject barrel = Instantiate(barrelPrefab, spawnPosition, Quaternion.identity);
        barrel.tag = "Barrel";
        Debug.Log($"Barrel spawned at height: {spawnPosition.y}");
    }
} 