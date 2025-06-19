using UnityEngine;

public class Barrel : MonoBehaviour
{
    [Header("Barrel Settings")]
    public float mass = 10f;
    public float drag = 0.5f;
    public float angularDrag = 0.5f;
    
    [Header("Physics")]
    public bool useGravity = true;
    public bool isKinematic = false;

    [Header("Audio")]
    public AudioClip destroySound;
    private AudioSource audioSource;

    void Start()
    {
        // Set up the barrel tag (create it if it doesn't exist)
        SetupBarrelTag();
        
        // Set up Rigidbody
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
        
        // Add collider if it doesn't exist
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1f, 1f, 1f);
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        Debug.Log("Barrel initialized with physics");
    }

    void SetupBarrelTag()
    {
        // Try to set the Barrel tag
        try
        {
            gameObject.tag = "Barrel";
        }
        catch (System.ArgumentException)
        {
            // Tag doesn't exist, create it
            Debug.LogWarning("Barrel tag doesn't exist. Please create it in Unity's Tag Manager (Edit > Project Settings > Tags and Layers)");
            
            // Set to untagged for now
            gameObject.tag = "Untagged";
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if colliding with an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Barrel collided with enemy!");
            
            // Apply force to the enemy
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDirection = (collision.gameObject.transform.position - transform.position).normalized;
                knockbackDirection.y = 0.5f; // Add some upward force
                enemyRb.AddForce(knockbackDirection * 3f, ForceMode.Impulse);
            }
        }
    }

    void OnDestroy()
    {
        // Play destroy sound if assigned
        if (destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroySound, 1f);
        }
        // Heal the player by 3 HP when the barrel is destroyed
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(3f);
        }
    }
} 