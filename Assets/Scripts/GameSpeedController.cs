using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script manages the game's speed, allowing it to be changed via UI or keyboard input.
public class GameSpeedController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float currentSpeed = 0.5f; // The current playback speed of the game.
    public float minSpeed = 0.1f; // The minimum allowed game speed.
    public float maxSpeed = 5f; // The maximum allowed game speed.
    public float speedStep = 0.5f; // The amount to change the speed by with each step.

    [Header("UI Elements")]
    public Slider speedSlider; // The slider to control game speed.
    public TextMeshProUGUI speedText; // The text to display the current speed.
    public Button speedUpButton; // The button to increase the speed.
    public Button speedDownButton; // The button to decrease the speed.
    public Button resetButton; // The button to reset the speed to normal.

    [Header("Input Settings")]
    public KeyCode speedUpKey = KeyCode.Equals; // The key to increase speed (=).
    public KeyCode speedDownKey = KeyCode.Minus; // The key to decrease speed (-).
    public KeyCode resetSpeedKey = KeyCode.R; // The key to reset speed.

    [Header("Auto-Increase Settings")]
    public bool autoIncrease = true; // Whether the game speed should increase automatically over time.
    public float increaseInterval = 10f; // The interval in seconds for auto-increasing speed.
    public float autoIncreaseAmount = 0.1f; // The amount to increase the speed by automatically.
    private float increaseTimer = 0f; // A timer to track the auto-increase interval.

    // Called when the script instance is being loaded.
    void Start()
    {
        // Set the initial game speed.
        SetGameSpeed(currentSpeed);
        
        // Configure the UI elements.
        SetupUI();
        
        Debug.Log($"Game speed initialized at {currentSpeed}x");
    }

    // Called every frame.
    void Update()
    {
        // Process keyboard input for speed changes.
        HandleKeyboardInput();

        // Automatically increase the game speed over time if enabled.
        if (autoIncrease && currentSpeed < maxSpeed)
        {
            increaseTimer += Time.unscaledDeltaTime; // Use unscaled time to ensure timer is not affected by game speed.
            if (increaseTimer >= increaseInterval)
            {
                increaseTimer = 0f;
                SetGameSpeed(currentSpeed + autoIncreaseAmount);
            }
        }
    }

    // Handles keyboard inputs for controlling the game speed.
    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(speedUpKey))
        {
            IncreaseSpeed();
        }
        else if (Input.GetKeyDown(speedDownKey))
        {
            DecreaseSpeed();
        }
        else if (Input.GetKeyDown(resetSpeedKey))
        {
            ResetSpeed();
        }
    }

    // Sets up the UI elements and their listeners.
    void SetupUI()
    {
        // Configure the speed slider.
        if (speedSlider != null)
        {
            speedSlider.minValue = minSpeed;
            speedSlider.maxValue = maxSpeed;
            speedSlider.value = currentSpeed;
            speedSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        // Add listeners to the control buttons.
        if (speedUpButton != null)
        {
            speedUpButton.onClick.AddListener(IncreaseSpeed);
        }
        if (speedDownButton != null)
        {
            speedDownButton.onClick.AddListener(DecreaseSpeed);
        }
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetSpeed);
        }

        // Update the speed display text.
        UpdateSpeedText();
    }

    // Sets the game's time scale to a new speed.
    public void SetGameSpeed(float newSpeed)
    {
        // Ensure the new speed is within the allowed range.
        newSpeed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
        
        // Apply the new speed to the game's time scale.
        Time.timeScale = newSpeed;
        currentSpeed = newSpeed;
        
        // Update the UI to reflect the change.
        UpdateUI();
        
        Debug.Log($"Game speed set to {newSpeed}x");
    }

    // Increases the game speed by one step.
    public void IncreaseSpeed()
    {
        SetGameSpeed(currentSpeed + speedStep);
    }

    // Decreases the game speed by one step.
    public void DecreaseSpeed()
    {
        SetGameSpeed(currentSpeed - speedStep);
    }

    // Resets the game speed to 1x (normal speed).
    public void ResetSpeed()
    {
        SetGameSpeed(1f);
    }

    // Called when the slider's value changes.
    void OnSliderChanged(float value)
    {
        SetGameSpeed(value);
    }

    // Updates all UI elements to match the current game speed.
    void UpdateUI()
    {
        // Update the slider's value.
        if (speedSlider != null)
        {
            speedSlider.value = currentSpeed;
        }
        // Update the speed display text.
        UpdateSpeedText();
    }

    // Updates the text that displays the current game speed.
    void UpdateSpeedText()
    {
        if (speedText != null)
        {
            speedText.text = $"Game Speed: {currentSpeed:F1}x";
        }
    }

    // Returns the current game speed.
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    // Sets the minimum and maximum speed range.
    public void SetSpeedRange(float min, float max)
    {
        minSpeed = min;
        maxSpeed = max;
        
        if (speedSlider != null)
        {
            speedSlider.minValue = minSpeed;
            speedSlider.maxValue = maxSpeed;
        }
        
        // Re-apply the current speed to clamp it to the new range if necessary.
        SetGameSpeed(currentSpeed);
    }

    // Called when the object is destroyed.
    void OnDestroy()
    {
        // Reset the time scale to normal to avoid a frozen game state.
        Time.timeScale = 1f;
    }

    // Called when the application is paused or resumed.
    void OnApplicationPause(bool pauseStatus)
    {
        // Reset time scale when the game is paused.
        if (pauseStatus)
        {
            Time.timeScale = 1f;
        }
        else
        {
            // Restore the game speed when resumed.
            Time.timeScale = currentSpeed;
        }
    }
} 