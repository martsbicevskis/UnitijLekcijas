using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public Camera playerCamera;

    [Header("Barrel Spawning")]
    public GameObject barrelPrefab;
    public float barrelSpawnDistance = 5f;
    public LayerMask barrelSpawnLayers = -1; // Default to all layers
    public float barrelSpawnCooldown = 1f;
    private float nextBarrelSpawnTime = 0f;

    [Header("Effects")]
    private WeaponEffects weaponEffects;
    private ParticleSystem muzzleFlash;
    private GameObject impactEffect;
    private WeaponController weaponController;

    private float nextTimeToFire = 0f;

    void Start()
    {
        // If no camera is assigned, try to find it
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        // Set up weapon effects
        weaponEffects = gameObject.AddComponent<WeaponEffects>();
        muzzleFlash = weaponEffects.CreateMuzzleFlash();
        impactEffect = weaponEffects.CreateImpactEffect();

        // Add weapon controller if it doesn't exist
        weaponController = GetComponent<WeaponController>();
        if (weaponController == null)
        {
            weaponController = gameObject.AddComponent<WeaponController>();
        }
    }

    void Update()
    {
        // Check if player can shoot (left mouse button)
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        // Check if player can spawn barrel (right mouse button)
        if (Input.GetButtonDown("Fire2") && Time.time >= nextBarrelSpawnTime)
        {
            nextBarrelSpawnTime = Time.time + barrelSpawnCooldown;
            SpawnBarrel();
        }
    }

    void SpawnBarrel()
    {
        if (barrelPrefab == null)
        {
            Debug.LogWarning("Barrel prefab is not assigned!");
            return;
        }

        // Create a ray from the camera
        RaycastHit hit;
        Vector3 spawnPosition;
        
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range, barrelSpawnLayers))
        {
            // Spawn barrel at the hit point
            spawnPosition = hit.point;
            Debug.Log($"Spawning barrel at hit point: {spawnPosition}");
        }
        else
        {
            // Spawn barrel at maximum range if no hit detected
            spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * barrelSpawnDistance;
            Debug.Log($"Spawning barrel at max distance: {spawnPosition}");
        }

        // Instantiate the barrel
        GameObject barrel = Instantiate(barrelPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Barrel spawned successfully at {spawnPosition}");
    }

    void Shoot()
    {
        // Play muzzle flash effect
        muzzleFlash.Play();
        Debug.Log("Shot fired!");

        // Create a ray from the camera
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range))
        {
            Debug.Log($"Hit something: {hit.transform.name} on layer {LayerMask.LayerToName(hit.transform.gameObject.layer)}");
            
            // Check if we hit an enemy (including parent objects)
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = hit.transform.GetComponentInParent<Enemy>();
            }
            
            if (enemy != null)
            {
                Debug.Log($"Hit enemy! Applying {damage} damage");
                enemy.TakeDamage(damage);
            }
            // Check if we hit a target
            else
            {
                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    Debug.Log($"Hit target! Applying {damage} damage");
                    target.TakeDamage(damage);
                }
            }

            // Create impact effect
            GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 2f); // Destroy impact effect after 2 seconds
        }
        else
        {
            Debug.Log("Shot missed - no hit detected");
        }
    }
}