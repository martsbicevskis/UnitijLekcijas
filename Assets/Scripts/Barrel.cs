using UnityEngine;

// This script defines the behavior of a barrel, including physics, interactions, and destruction.
public class Barrel : MonoBehaviour
{
    [Header("Barrel Settings")]
    public float mass = 10f; // Mass of the barrel's Rigidbody.
    public float drag = 0.5f; // Air resistance of the barrel.
    public float angularDrag = 0.5f; // Rotational air resistance.
    
    [Header("Physics")]
    public bool useGravity = true; // Whether the barrel is affected by gravity.
    public bool isKinematic = false; // Whether the barrel is controlled by physics or script.

    [Header("Audio")]
    public AudioClip destroySound; // Sound to play when the barrel is destroyed.
    private AudioSource audioSource; // Component to play audio.

    // Called when the script instance is being loaded.
    void Start()
    {
        // Set up the barrel's tag for identification.
        SetupBarrelTag();
        
        // Configure the Rigidbody component for physics simulation.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.useGravity = useGravity;
        rb.isKinematic = isKinematic;
        
        // Ensure the barrel has a collider for physical interactions.
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1f, 1f, 1f);
        }
        
        // Set up the AudioSource for playing sounds.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        Debug.Log("Barrel initialized with physics");
    }

    // Ensures the "Barrel" tag exists and is assigned to this GameObject.
    void SetupBarrelTag()
    {
        // Attempt to assign the "Barrel" tag.
        try
        {
            gameObject.tag = "Barrel";
        }
        catch (System.ArgumentException)
        {
            // If the tag doesn't exist, log a warning and default to "Untagged".
            Debug.LogWarning("Barrel tag doesn't exist. Please create it in Unity's Tag Manager (Edit > Project Settings > Tags and Layers)");
            gameObject.tag = "Untagged";
        }
    }

    // Called when this collider/rigidbody has begun touching another rigidbody/collider.
    void OnCollisionEnter(Collision collision)
    {
        // Check if the barrel collided with an object tagged as "Enemy".
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Barrel collided with enemy!");
            
            // Apply a knockback force to the enemy.
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDirection = (collision.gameObject.transform.position - transform.position).normalized;
                knockbackDirection.y = 0.5f; // Add some upward force to the knockback.
                enemyRb.AddForce(knockbackDirection * 3f, ForceMode.Impulse);
            }
        }
    }

    // Called when the MonoBehaviour will be destroyed.
    void OnDestroy()
    {
        // Play the destruction sound if available.
        if (destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroySound, 1f);
        }

        // Heal the player for 3 HP upon destruction.
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(3f);
        }
    }
} 