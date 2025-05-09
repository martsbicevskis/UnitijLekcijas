using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextureSwitcher : MonoBehaviour
{
    public GroundManager groundManager;
    public Button nextButton;
    public Button previousButton;
    public TextMeshProUGUI textureNameText;

    void Start()
    {
        // Set up button listeners
        if (nextButton != null)
            nextButton.onClick.AddListener(NextTexture);
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousTexture);

        // Update the texture name display
        UpdateTextureName();
    }

    public void NextTexture()
    {
        groundManager.NextTexture();
        UpdateTextureName();
    }

    public void PreviousTexture()
    {
        groundManager.PreviousTexture();
        UpdateTextureName();
    }

    void UpdateTextureName()
    {
        if (textureNameText != null && groundManager.textures != null && 
            groundManager.textures.Length > 0 && 
            groundManager.currentTextureIndex < groundManager.textures.Length)
        {
            textureNameText.text = groundManager.textures[groundManager.currentTextureIndex].name;
        }
    }
} 