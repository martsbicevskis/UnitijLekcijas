using UnityEngine;

// This script manages the appearance of the ground, including its texture and material properties.
public class GroundManager : MonoBehaviour
{
    // A container for texture data, including the texture itself, its scale, and smoothness.
    [System.Serializable]
    public class TextureData
    {
        public string name; // The name of the texture.
        public Texture2D texture; // The texture file.
        public float scale = 10f; // The tiling scale of the texture.
        public float smoothness = 0.1f; // The smoothness value of the material.
    }

    [Header("Ground Settings")]
    public float groundSize = 20f; // The size of the ground plane.
    public float groundHeight = 1f; // The height of the ground plane.

    [Header("Textures")]
    public TextureData[] textures; // An array of available textures for the ground.
    public int currentTextureIndex = 0; // The index of the currently applied texture.

    private Material groundMaterial; // The material used for the ground.

    // Called when the script instance is being loaded.
    void Start()
    {
        // Create and configure the ground.
        CreateGround();
    }

    // Sets up the ground's material and applies the initial texture.
    void CreateGround() 
    {
        // Create a new material using the URP Lit shader.
        groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        // Apply the default texture.
        ApplyTexture(0);

        // Assign the new material to the ground's renderer.
        GetComponent<Renderer>().material = groundMaterial;
    }

    // Applies a new texture to the ground material based on the given index.
    void ApplyTexture(int index)
    {
        // If no textures are assigned, set a default color and return.
        if (textures == null || textures.Length == 0)
        {
            Debug.LogWarning("No textures assigned!");
            groundMaterial.color = new Color(0.2f, 0.6f, 0.2f); // Default green color.
            return;
        }

        // Clamp the index to be within the bounds of the textures array.
        index = Mathf.Clamp(index, 0, textures.Length - 1);
        currentTextureIndex = index;

        TextureData textureData = textures[index];
        
        // If the texture is valid, apply it to the material.
        if (textureData.texture != null)
        {
            groundMaterial.SetTexture("_BaseMap", textureData.texture);
            groundMaterial.SetFloat("_Smoothness", textureData.smoothness);
            groundMaterial.SetTextureScale("_BaseMap", new Vector2(textureData.scale, textureData.scale));
        }
        else
        {
            Debug.LogWarning($"Texture {textureData.name} is not assigned!");
        }
    }

    // Switches to the next texture in the array.
    public void NextTexture()
    {
        ApplyTexture(currentTextureIndex + 1);
    }

    // Switches to the previous texture in the array.
    public void PreviousTexture()
    {
        ApplyTexture(currentTextureIndex - 1);
    }

    // Switches to a specific texture by its index.
    public void SetTexture(int index)
    {
        ApplyTexture(index);
    }
} 