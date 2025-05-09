using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar;
    public TextMeshProUGUI healthText;

    [Header("Death Screen")]
    public TextMeshProUGUI deathText;
    public float fadeInDuration = 1f;
    private float fadeTimer = 0f;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

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
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            float healthPercentage = currentHealth / maxHealth;
            healthBar.rectTransform.localScale = new Vector3(healthPercentage, 1, 1);
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
        }

        Debug.Log("Player died!");
    }
} 