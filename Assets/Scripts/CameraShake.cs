using UnityEngine;
using System.Collections;

// This script creates a camera shake effect when called.
public class CameraShake : MonoBehaviour
{
    [Header("Screen Shake Settings")]
    public float shakeDuration = 0.2f; // How long the shake effect lasts.
    public float shakeMagnitude = 0.2f; // The intensity of the shake.
    public float shakeFrequency = 25f; // How fast the camera shakes.

    private Coroutine shakeCoroutine; // A reference to the running shake coroutine.

    // Public method to initiate the camera shake.
    public void Shake()
    {
        // If a shake is already in progress, stop it before starting a new one.
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        // Start the shake coroutine.
        shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    // Coroutine that performs the camera shake logic.
    private IEnumerator ShakeCoroutine()
    {
        Vector3 originalPos = transform.localPosition; // Store the original camera position.
        float elapsed = 0f; // Timer to track the shake duration.

        // Loop for the duration of the shake.
        while (elapsed < shakeDuration)
        {
            // Generate random offsets using Perlin noise for a smoother shake.
            float offsetX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * shakeMagnitude;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f * shakeMagnitude;
            
            // Apply the offset to the camera's local position.
            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            
            elapsed += Time.deltaTime; // Increment the timer.
            yield return null; // Wait for the next frame.
        }

        // Reset the camera to its original position after the shake is finished.
        transform.localPosition = originalPos;
        shakeCoroutine = null; // Clear the coroutine reference.
    }
} 