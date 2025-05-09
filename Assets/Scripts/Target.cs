using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f;
    public GameObject destroyEffect;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // Create destroy effect if assigned
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }

        // Destroy the target
        Destroy(gameObject);
    }
} 