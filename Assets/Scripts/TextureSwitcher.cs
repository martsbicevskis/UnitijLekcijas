using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script provides UI controls to switch the ground texture using a GroundManager.
public class TextureSwitcher : MonoBehaviour
{
    [Header("References")]
    public GroundManager groundManager; // A reference to the GroundManager script.
    public Button nextButton; // The UI button to switch to the next texture.
    public Button previousButton; // The UI button to switch to the previous texture.
    public TextMeshProUGUI textureNameText; // The UI text to display the current texture's name.

    // Called when the script instance is being loaded.
    void Start()
    {
        // Add listeners to the buttons to call the respective methods when clicked.
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextTexture);
        }
        if (previousButton != null)
        {
            previousButton.onClick.AddListener(PreviousTexture);
        }

        // Initialize the texture name display.
        UpdateTextureName();
    }

    // Called by the 'next' button to switch to the next texture.
    public void NextTexture()
    {
        if (groundManager != null)
        {
            groundManager.NextTexture();
            UpdateTextureName();
        }
    }

    // Called by the 'previous' button to switch to the previous texture.
    public void PreviousTexture()
    {
        if (groundManager != null)
        {
            groundManager.PreviousTexture();
            UpdateTextureName();
        }
    }

    // Updates the UI text to display the name of the current ground texture.
    void UpdateTextureName()
    {
        if (textureNameText != null && groundManager != null && groundManager.textures != null && 
            groundManager.textures.Length > 0 && 
            groundManager.currentTextureIndex < groundManager.textures.Length)
        {
            // Set the text to the name of the current texture from the GroundManager.
            textureNameText.text = groundManager.textures[groundManager.currentTextureIndex].name;
        }
    }
} 