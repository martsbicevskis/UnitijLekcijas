using UnityEngine;

// This script makes a GameObject destructible by giving it health and a death sequence.
public class Target : MonoBehaviour
{
    public float health = 50f; // The health of the target.
    public GameObject destroyEffect; // A particle effect or other GameObject to instantiate upon destruction.

    // This method is called to apply damage to the target.
    public void TakeDamage(float amount)
    {
        // Reduce health by the damage amount.
        health -= amount;
        
        // If health drops to or below zero, the target is destroyed.
        if (health <= 0f)
        {
            Die();
        }
    }

    // This method handles the destruction of the target.
    void Die()
    {
        // If a destruction effect is assigned, instantiate it at the target's position.
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }

        // Destroy the GameObject this script is attached to.
        Destroy(gameObject);
    }
} 