using UnityEngine;

// This script handles the spawning of enemies and barrels in the game world.
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab; // The enemy GameObject to spawn.
    public float spawnInterval = 5f; // Time between enemy spawns.
    public float spawnDistance = 35f; // Distance from the player to spawn enemies.
    public int maxEnemies = 10; // The maximum number of enemies allowed in the scene.
    public float spawnHeight = 10f; // The base height above the ground to spawn enemies.
    public float spawnHeightVariation = 3f; // Random variation added to the spawn height.

    [Header("Enemy Settings")]
    public float minHealth = 50f; // The minimum health for a spawned enemy.
    public float maxHealth = 150f; // The maximum health for a spawned enemy.
    public float minSpeed = 2f; // The minimum speed for a spawned enemy.
    public float maxSpeed = 5f; // The maximum speed for a spawned enemy.

    [Header("Barrel Spawning")]
    public GameObject barrelPrefab; // The barrel GameObject to spawn.
    public float barrelSpawnInterval = 6.5f; // Time between barrel spawns.
    public int maxBarrels = 10; // The maximum number of barrels allowed in the scene.

    private float nextSpawnTime; // Timer for the next enemy spawn.
    private float nextBarrelSpawnTime; // Timer for the next barrel spawn.
    private Transform player; // A reference to the player's transform.

    // Called when the script instance is being loaded.
    void Start()
    {
        // Find the player by their tag.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // Initialize the spawn timers.
        nextSpawnTime = Time.time + spawnInterval;
        nextBarrelSpawnTime = Time.time + barrelSpawnInterval;
    }

    // Called every frame.
    void Update()
    {
        // Check if it's time to spawn an enemy.
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
        // Check if it's time to spawn a barrel.
        if (Time.time >= nextBarrelSpawnTime)
        {
            SpawnBarrel();
            nextBarrelSpawnTime = Time.time + barrelSpawnInterval;
        }
    }

    // Handles the logic for spawning an enemy.
    void SpawnEnemy()
    {
        // Do not spawn if the maximum number of enemies has been reached.
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies)
            return;

        // Calculate a random spawn position in a circle around the player.
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        Vector3 spawnPosition = player.position + randomDirection * spawnDistance;
        
        // Apply a random height variation to the spawn position.
        float randomHeight = spawnHeight + Random.Range(-spawnHeightVariation, spawnHeightVariation);
        spawnPosition.y = randomHeight; // Spawn enemies in the air so they fall to the ground.

        // Instantiate the enemy prefab at the calculated position.
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.transform.localScale = Vector3.one * 1.5f; // Set the enemy's scale.
        
        // Set the enemy's tag and layer for identification and collision.
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemy");
        
        // Assign random health and speed properties to the enemy.
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.health = Random.Range(minHealth, maxHealth);
            enemyComponent.moveSpeed = Random.Range(minSpeed, maxSpeed);
        }
        
        Debug.Log($"Enemy spawned at height: {spawnPosition.y}");
    }

    // Handles the logic for spawning a barrel.
    void SpawnBarrel()
    {
        // Do not spawn if the prefab is not assigned or max barrels are in the scene.
        if (barrelPrefab == null) return;
        if (GameObject.FindGameObjectsWithTag("Barrel").Length >= maxBarrels) return;
        
        // Calculate a random spawn position around the player.
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        Vector3 spawnPosition = player.position + randomDirection * spawnDistance;
        
        // Apply a random height variation.
        float randomHeight = spawnHeight + Random.Range(-spawnHeightVariation, spawnHeightVariation);
        spawnPosition.y = randomHeight;
        
        // Instantiate the barrel and set its tag.
        GameObject barrel = Instantiate(barrelPrefab, spawnPosition, Quaternion.identity);
        barrel.tag = "Barrel";
        Debug.Log($"Barrel spawned at height: {spawnPosition.y}");
    }
} 