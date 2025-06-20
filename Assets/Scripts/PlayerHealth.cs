using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

// This script manages the player's health, damage taking, healing, and death sequence.
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f; // The maximum health of the player.
    public float currentHealth; // The current health of the player.
    public Image healthBar; // The UI element representing the health bar.
    public TextMeshProUGUI healthText; // The UI text displaying the health values.
    public Image cooldownBar; // The UI element for the stun ability cooldown.

    [Header("Death Screen")]
    public TextMeshProUGUI deathText; // The text to display upon death.
    public float fadeInDuration = 1f; // How long it takes for the death text to fade in.
    private float fadeTimer = 0f; // A timer for the fade-in effect.
    public bool isDead = false; // Whether the player is currently dead.
    private float stunCooldown = 10f; // The cooldown period for the stun ability.
    private float lastStunTime = -100f; // The time when the stun ability was last used.

    public TextMeshProUGUI respawnText; // The UI text instructing how to respawn.
    public TextMeshProUGUI scoreText; // The UI text displaying the final score.
    private GameSpeedController gameSpeedController; // A reference to the game speed controller.

    [Header("Audio")]
    public AudioClip backgroundMusic; // The background music clip.
    private AudioSource backgroundAudioSource; // The AudioSource for the background music.

    public AudioClip playerDyingSound; // The first sound to play when the player dies.
    public AudioClip playerDyingSound2; // The second sound to play after a delay when the player dies.
    public float dyingSoundDelay = 0.5f; // The delay between the two dying sounds.
    private bool hasPlayedDyingSound = false; // Ensures the dying sounds are played only once.

    public AudioClip stunAbilitySound; // The sound for the stun ability.

    // Called when the script instance is being loaded.
    void Start()
    {
        // Initialize player health.
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Hide the respawn and score text at the start of the game.
        if (respawnText != null) respawnText.gameObject.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        // Find the GameSpeedController in the scene.
        gameSpeedController = FindObjectOfType<GameSpeedController>();

        // Create or prepare the death text UI element.
        if (deathText == null)
        {
            CreateDeathText();
        }
        else
        {
            deathText.color = new Color(1, 1, 1, 0); // Hide it initially.
        }

        // Set up and play the background music.
        if (backgroundMusic != null)
        {
            backgroundAudioSource = GetComponent<AudioSource>();
            if (backgroundAudioSource == null)
            {
                backgroundAudioSource = gameObject.AddComponent<AudioSource>();
            }
            backgroundAudioSource.clip = backgroundMusic;
            backgroundAudioSource.loop = true;
            backgroundAudioSource.playOnAwake = true;
            backgroundAudioSource.volume = 0.5f;
            backgroundAudioSource.Play();
        }
    }

    // Dynamically creates the "YOU DIED" text if it's not assigned in the Inspector.
    void CreateDeathText()
    {
        // Ensure a Canvas exists to hold the UI text.
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DeathCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create the TextMeshProUGUI element for the death text.
        GameObject deathTextObj = new GameObject("DeathText");
        deathTextObj.transform.SetParent(canvas.transform, false);
        deathText = deathTextObj.AddComponent<TextMeshProUGUI>();
        
        // Configure the appearance and properties of the death text.
        deathText.text = "YOU DIED";
        deathText.fontSize = 72;
        deathText.alignment = TextAlignmentOptions.Center;
        deathText.color = new Color(1, 0, 0, 0); // Start fully transparent.
        
        // Position the text in the center of the screen.
        RectTransform rectTransform = deathText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(400, 100);
    }

    // Called every frame.
    void Update()
    {
        // If the player is dead, handle death screen logic.
        if (isDead)
        {
            // Fade in the death text over time.
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeInDuration);
            deathText.color = new Color(1, 0, 0, alpha);

            // Listen for the 'R' key to restart the scene.
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else // If the player is alive, handle abilities and UI updates.
        {
            // Handle the stun ability input (Q key).
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // Check if the ability is off cooldown.
                if (Time.time >= lastStunTime + stunCooldown)
                {
                    // Stun all enemies in the scene.
                    Enemy[] enemies = FindObjectsOfType<Enemy>();
                    foreach (Enemy enemy in enemies)
                    {
                        enemy.Stun(3f);
                    }
                    lastStunTime = Time.time; // Reset cooldown timer.
                    
                    // Play the stun ability sound.
                    if (stunAbilitySound != null)
                    {
                        AudioSource audioSource = GetComponent<AudioSource>();
                        if (audioSource == null)
                        {
                            audioSource = gameObject.AddComponent<AudioSource>();
                        }
                        audioSource.PlayOneShot(stunAbilitySound, 0.5f);
                    }
                }
                else
                {
                    // Log a message if the ability is on cooldown.
                    float timeLeft = (lastStunTime + stunCooldown) - Time.time;
                    Debug.Log($"Stun ability is on cooldown. {timeLeft:F1} seconds left.");
                }
            }
            
            // Update the cooldown bar's appearance.
            if (cooldownBar != null)
            {
                float cooldownProgress = Mathf.Clamp01((Time.time - lastStunTime) / stunCooldown);
                cooldownBar.rectTransform.localScale = new Vector3(cooldownProgress, 1, 1);
                
                // Change color based on whether the ability is ready.
                Color readyColor = new Color(0.5f, 0.8f, 1f, 1f); // Light blue
                Color onCooldownColor = new Color(0.2f, 0.3f, 0.5f, 1f); // Dark blue/gray
                if (cooldownProgress >= 1f)
                {
                    cooldownBar.color = readyColor;
                    Debug.Log($"Cooldown bar color set to READY: {readyColor}");
                }
                else
                {
                    cooldownBar.color = onCooldownColor;
                    Debug.Log($"Cooldown bar color set to COOLDOWN: {onCooldownColor}");
                }
            }
        }
    }

    // Reduces the player's health by a specified amount.
    public void TakeDamage(float amount, Vector3? knockbackDirection = null, float knockbackForce = 5f)
    {
        if (isDead) return; // Don't take damage if already dead.
        
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Clamp health to a minimum of 0.
        
        UpdateHealthUI();

        // Apply knockback if a direction is provided.
        if (knockbackDirection.HasValue)
        {
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.ApplyKnockback(knockbackDirection.Value, knockbackForce);
                
                // Trigger camera shake for impact.
                if (movement.playerCamera != null)
                {
                    CameraShake shake = movement.playerCamera.GetComponent<CameraShake>();
                    if (shake != null)
                    {
                        shake.Shake();
                    }
                }
            }
        }

        // Check if the player has run out of health.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Increases the player's health by a specified amount.
    public void Heal(float amount)
    {
        if (isDead) return; // Can't heal if dead.
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Clamp health to the maximum.
        UpdateHealthUI();
    }

    // Updates the health bar UI and text.
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            float healthPercentage = currentHealth / maxHealth;
            healthBar.rectTransform.localScale = new Vector3(healthPercentage, 1, 1);

            // Interpolate the health bar color from green to yellow to red.
            Color full = Color.green;
            Color mid = Color.yellow;
            Color low = Color.red;
            Color lerpedColor;
            if (healthPercentage > 0.5f)
            {
                float t = (healthPercentage - 0.5f) * 2f;
                lerpedColor = Color.Lerp(mid, full, t);
            }
            else
            {
                float t = healthPercentage * 2f;
                lerpedColor = Color.Lerp(low, mid, t);
            }
            healthBar.color = lerpedColor;
        }

        if (healthText != null)
        {
            healthText.text = $"Health: {Mathf.Ceil(currentHealth)}/{maxHealth}";
        }
    }

    // Handles the player's death.
    void Die()
    {
        isDead = true;
        
        // Disable player movement and shooting.
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerShooting>().enabled = false;

        // Show and configure the death text.
        if (deathText != null)
        {
            deathText.gameObject.SetActive(true);
            RectTransform rect = deathText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(400, 100);
            deathText.alignment = TMPro.TextAlignmentOptions.Center;
        }

        // Show and configure the respawn text.
        if (respawnText != null)
        {
            respawnText.gameObject.SetActive(true);
            RectTransform rect = respawnText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, -40); // Position below the death text.
            rect.sizeDelta = new Vector2(400, 100);
            respawnText.alignment = TMPro.TextAlignmentOptions.Center;
        }
        
        // Show and configure the final score text.
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            float score = 0f;
            if (gameSpeedController != null)
            {
                score = gameSpeedController.currentSpeed * 100f;
            }
            scoreText.text = $"Score: {score:F0}";
            RectTransform rect = scoreText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, -60); // Position below the respawn text.
            rect.sizeDelta = new Vector2(400, 100);
            scoreText.alignment = TMPro.TextAlignmentOptions.Center;
        }

        // Play the sequence of dying sounds.
        if (!hasPlayedDyingSound && (playerDyingSound != null || playerDyingSound2 != null))
        {
            StartCoroutine(PlayDyingSounds());
            hasPlayedDyingSound = true;
        }

        Debug.Log("Player died!");
    }

    // A coroutine to play two dying sounds with a delay between them.
    IEnumerator PlayDyingSounds()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Play the first sound immediately.
        if (playerDyingSound != null)
        {
            audioSource.PlayOneShot(playerDyingSound, 1f);
        }

        // Wait for the specified delay.
        yield return new WaitForSeconds(dyingSoundDelay);

        // Play the second sound on a temporary object and destroy it after 1 second.
        if (playerDyingSound2 != null)
        {
            // Create a temporary GameObject to host the AudioSource for the second sound.
            GameObject tempAudioObj = new GameObject("TempDyingSound");
            tempAudioObj.transform.position = transform.position;
            AudioSource tempAudioSource = tempAudioObj.AddComponent<AudioSource>();
            
            // Configure and play the sound.
            tempAudioSource.clip = playerDyingSound2;
            tempAudioSource.volume = 1f;
            tempAudioSource.Play();

            // Destroy the temporary object after 1 second, which effectively cuts off the sound.
            Destroy(tempAudioObj, 1f);
        }
    }
} 