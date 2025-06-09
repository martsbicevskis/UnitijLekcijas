using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSpeedController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float currentSpeed = 1f;
    public float minSpeed = 0.1f;
    public float maxSpeed = 5f;
    public float speedStep = 0.5f;

    [Header("UI Elements")]
    public Slider speedSlider;
    public TextMeshProUGUI speedText;
    public Button speedUpButton;
    public Button speedDownButton;
    public Button resetButton;

    [Header("Input Settings")]
    public KeyCode speedUpKey = KeyCode.Equals; // = key
    public KeyCode speedDownKey = KeyCode.Minus; // - key
    public KeyCode resetSpeedKey = KeyCode.R; // R key

    void Start()
    {
        // Initialize the speed
        SetGameSpeed(currentSpeed);
        
        // Set up UI elements if they exist
        SetupUI();
        
        Debug.Log($"Game speed initialized at {currentSpeed}x");
    }

    void Update()
    {
        // Handle keyboard input
        HandleKeyboardInput();
    }

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

    void SetupUI()
    {
        // Set up slider
        if (speedSlider != null)
        {
            speedSlider.minValue = minSpeed;
            speedSlider.maxValue = maxSpeed;
            speedSlider.value = currentSpeed;
            speedSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        // Set up buttons
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

        // Update text
        UpdateSpeedText();
    }

    public void SetGameSpeed(float newSpeed)
    {
        // Clamp the speed between min and max values
        newSpeed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
        
        // Set the time scale
        Time.timeScale = newSpeed;
        currentSpeed = newSpeed;
        
        // Update UI
        UpdateUI();
        
        Debug.Log($"Game speed set to {newSpeed}x");
    }

    public void IncreaseSpeed()
    {
        float newSpeed = currentSpeed + speedStep;
        SetGameSpeed(newSpeed);
    }

    public void DecreaseSpeed()
    {
        float newSpeed = currentSpeed - speedStep;
        SetGameSpeed(newSpeed);
    }

    public void ResetSpeed()
    {
        SetGameSpeed(1f);
    }

    void OnSliderChanged(float value)
    {
        SetGameSpeed(value);
    }

    void UpdateUI()
    {
        // Update slider
        if (speedSlider != null)
        {
            speedSlider.value = currentSpeed;
        }

        // Update text
        UpdateSpeedText();
    }

    void UpdateSpeedText()
    {
        if (speedText != null)
        {
            speedText.text = $"Game Speed: {currentSpeed:F1}x";
        }
    }

    // Public methods for external access
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public void SetSpeedRange(float min, float max)
    {
        minSpeed = min;
        maxSpeed = max;
        
        if (speedSlider != null)
        {
            speedSlider.minValue = minSpeed;
            speedSlider.maxValue = maxSpeed;
        }
        
        // Clamp current speed to new range
        SetGameSpeed(currentSpeed);
    }

    void OnDestroy()
    {
        // Reset time scale when the controller is destroyed
        Time.timeScale = 1f;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Reset time scale when game is paused
        if (pauseStatus)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = currentSpeed;
        }
    }
} 