using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public Camera playerCamera;

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
    }

    void Shoot()
    {
        // Play muzzle flash effect
        muzzleFlash.Play();

        // Create a ray from the camera
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range))
        {
            // Check if we hit something that can take damage
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            // Create impact effect
            GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 2f); // Destroy impact effect after 2 seconds
        }
    }
}