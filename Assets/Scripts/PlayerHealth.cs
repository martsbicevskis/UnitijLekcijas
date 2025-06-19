using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar;
    public TextMeshProUGUI healthText;
    public Image cooldownBar;

    [Header("Death Screen")]
    public TextMeshProUGUI deathText;
    public float fadeInDuration = 1f;
    private float fadeTimer = 0f;
    public bool isDead = false;
    private float stunCooldown = 10f;
    private float lastStunTime = -100f;

    public TextMeshProUGUI respawnText;
    public TextMeshProUGUI scoreText;
    private GameSpeedController gameSpeedController;

    public AudioClip backgroundMusic;
    private AudioSource backgroundAudioSource;

    public AudioClip playerDyingSound;
    private bool hasPlayedDyingSound = false;

    public AudioClip stunAbilitySound;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Hide respawn and score text at start
        if (respawnText != null) respawnText.gameObject.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        // Find GameSpeedController in the scene
        gameSpeedController = FindObjectOfType<GameSpeedController>();

        // Create death text if it doesn't exist
        if (deathText == null)
        {
            CreateDeathText();
        }
        else
        {
            // Hide death text initially
            deathText.color = new Color(1, 1, 1, 0);
        }

        // Play background music
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

    void CreateDeathText()
    {
        // Create a canvas for the death text if it doesn't exist
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DeathCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create the death text
        GameObject deathTextObj = new GameObject("DeathText");
        deathTextObj.transform.SetParent(canvas.transform, false);
        deathText = deathTextObj.AddComponent<TextMeshProUGUI>();
        
        // Set up the death text
        deathText.text = "YOU DIED";
        deathText.fontSize = 72;
        deathText.alignment = TextAlignmentOptions.Center;
        deathText.color = new Color(1, 0, 0, 0); // Start fully transparent
        
        // Position the text in the center of the screen
        RectTransform rectTransform = deathText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(400, 100);
    }

    void Update()
    {
        if (isDead)
        {
            // Fade in the death text
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeInDuration);
            deathText.color = new Color(1, 0, 0, alpha);

            // Allow restart by pressing 'R'
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            // Stun all enemies for 3 seconds when Q is pressed, with cooldown
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (Time.time >= lastStunTime + stunCooldown)
                {
                    Enemy[] enemies = FindObjectsOfType<Enemy>();
                    foreach (Enemy enemy in enemies)
                    {
                        enemy.Stun(3f);
                    }
                    lastStunTime = Time.time;
                    // Play stun ability sound
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
                    float timeLeft = (lastStunTime + stunCooldown) - Time.time;
                    Debug.Log($"Stun ability is on cooldown. {timeLeft:F1} seconds left.");
                }
            }
            // Update cooldown bar
            if (cooldownBar != null)
            {
                float cooldown = Mathf.Clamp01((Time.time - lastStunTime) / stunCooldown);
                cooldownBar.rectTransform.localScale = new Vector3(cooldown, 1, 1);
                Color readyColor = new Color(0.5f, 0.8f, 1f, 1f); // Light blue
                Color cooldownColor = new Color(0.2f, 0.3f, 0.5f, 1f); // Dark blue/gray
                if (cooldown >= 1f)
                {
                    cooldownBar.color = readyColor;
                    Debug.Log($"Cooldown bar color set to READY: {readyColor}");
                }
                else
                {
                    cooldownBar.color = cooldownColor;
                    Debug.Log($"Cooldown bar color set to COOLDOWN: {cooldownColor}");
                }
            }
        }
    }

    public void TakeDamage(float amount, Vector3? knockbackDirection = null, float knockbackForce = 5f)
    {
        if (isDead) return;
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        UpdateHealthUI();

        // Apply knockback if direction is provided
        if (knockbackDirection.HasValue)
        {
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.ApplyKnockback(knockbackDirection.Value, knockbackForce);
                // Trigger camera shake using the playerCamera reference
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

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            float healthPercentage = currentHealth / maxHealth;
            healthBar.rectTransform.localScale = new Vector3(healthPercentage, 1, 1);

            // Interpolate color: green (full) to yellow (mid) to red (low)
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

    void Die()
    {
        isDead = true;
        
        // Disable player movement
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // Disable player shooting
        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            shooting.enabled = false;
        }

        // Show death text
        if (deathText != null)
        {
            deathText.gameObject.SetActive(true);
            // Center deathText
            RectTransform rect = deathText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(400, 100);
            deathText.alignment = TMPro.TextAlignmentOptions.Center;
        }

        // Show respawn and score text
        if (respawnText != null)
        {
            respawnText.gameObject.SetActive(true);
            // Center respawnText
            RectTransform rect = respawnText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, -40); // Slightly below center
            rect.sizeDelta = new Vector2(400, 100);
            respawnText.alignment = TMPro.TextAlignmentOptions.Center;
        }
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            float score = 0f;
            if (gameSpeedController != null)
            {
                score = gameSpeedController.currentSpeed * 100f;
            }
            scoreText.text = $"Score: {score:F0}";
            // Center scoreText
            RectTransform rect = scoreText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, -60); // Slightly below center
            rect.sizeDelta = new Vector2(400, 100);
            scoreText.alignment = TMPro.TextAlignmentOptions.Center;
        }

        // Play dying sound only once
        if (!hasPlayedDyingSound && playerDyingSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.PlayOneShot(playerDyingSound, 1f);
            hasPlayedDyingSound = true;
        }

        Debug.Log("Player died!");
    }
} 