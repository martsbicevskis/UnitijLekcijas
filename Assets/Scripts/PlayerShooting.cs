using UnityEngine;

// This script handles the player's shooting mechanics, including firing, raycasting, and barrel spawning.
public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float damage = 10f; // The amount of damage each shot deals.
    public float range = 100f; // The maximum range of a shot.
    public float fireRate = 15f; // The number of shots per second.
    public Camera playerCamera; // A reference to the player's camera.

    [Header("Barrel Spawning")]
    public GameObject barrelPrefab; // The barrel prefab to spawn.
    public float barrelSpawnDistance = 5f; // The distance at which to spawn barrels.
    public LayerMask barrelSpawnLayers = -1; // The layers on which barrels can be spawned.
    public float barrelSpawnCooldown = 1f; // The cooldown for spawning barrels.
    private float nextBarrelSpawnTime = 0f; // A timer for barrel spawning.

    [Header("Effects")]
    private WeaponEffects weaponEffects; // A reference to the weapon effects script.
    private ParticleSystem muzzleFlash; // The particle system for the muzzle flash.
    private GameObject impactEffect; // The effect to create on impact.
    private WeaponController weaponController; // A reference to the weapon controller.

    [Header("Audio")]
    public AudioClip shootingSound; // The sound to play when shooting.
    private AudioSource audioSource; // The component for playing audio.

    private float nextTimeToFire = 0f; // A timer to control the fire rate.

    // Called when the script instance is being loaded.
    void Start()
    {
        // Find the camera if it's not assigned.
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        // Set up weapon effects dynamically.
        weaponEffects = gameObject.AddComponent<WeaponEffects>();
        muzzleFlash = weaponEffects.CreateMuzzleFlash();
        impactEffect = weaponEffects.CreateImpactEffect();

        // Ensure a WeaponController exists.
        weaponController = GetComponent<WeaponController>() ?? gameObject.AddComponent<WeaponController>();
        
        // Ensure an AudioSource exists.
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    // Called every frame.
    void Update()
    {
        // Handle shooting input.
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        // Handle barrel spawning input.
        if (Input.GetButtonDown("Fire2") && Time.time >= nextBarrelSpawnTime)
        {
            nextBarrelSpawnTime = Time.time + barrelSpawnCooldown;
            SpawnBarrel();
        }
    }

    // Spawns a barrel in the world.
    void SpawnBarrel()
    {
        if (barrelPrefab == null)
        {
            Debug.LogWarning("Barrel prefab is not assigned!");
            return;
        }

        // Determine the spawn position using a raycast.
        RaycastHit hit;
        Vector3 spawnPosition;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range, barrelSpawnLayers))
        {
            spawnPosition = hit.point; // Spawn at the hit point.
        }
        else
        {
            spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * barrelSpawnDistance; // Spawn at a default distance.
        }

        // Instantiate the barrel.
        Instantiate(barrelPrefab, spawnPosition, Quaternion.identity);
    }

    // Handles the logic for firing a shot.
    void Shoot()
    {
        // Play visual and audio effects for the shot.
        muzzleFlash.Play();
        if (shootingSound != null)
        {
            audioSource.PlayOneShot(shootingSound, 0.1f);
        }

        // Perform a raycast to detect hits.
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range))
        {
            // Process the hit target.
            HandleHit(hit);

            // Create an impact effect at the hit point.
            GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 2f);
        }
    }

    // Processes the object that was hit by the raycast.
    void HandleHit(RaycastHit hit)
    {
        // Check if the hit object is an enemy.
        Enemy enemy = hit.transform.GetComponent<Enemy>() ?? hit.transform.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            // Apply damage and knockback to the enemy.
            Vector3 knockbackDir = (hit.point - playerCamera.transform.position).normalized;
            enemy.TakeDamage(damage, knockbackDir, null);
            return; // Exit after handling the enemy hit.
        }

        // Check if the hit object is a destructible target.
        Target target = hit.transform.GetComponent<Target>();
        if (target != null)
        {
            // Apply damage to the target.
            target.TakeDamage(damage);
        }
    }
}