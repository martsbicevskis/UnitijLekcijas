using UnityEngine;

public class GroundManager : MonoBehaviour
{
    [System.Serializable]
    public class TextureData
    {
        public string name;
        public Texture2D texture;
        public float scale = 10f;
        public float smoothness = 0.1f;
    }

    [Header("Ground Settings")]
    public float groundSize = 20f;
    public float groundHeight = 1f;

    [Header("Textures")]
    public TextureData[] textures;
    public int currentTextureIndex = 0;

    private Material groundMaterial;

    void Start()
    {
        CreateGround();
    }

    void CreateGround()
    {
        // Create a new material
        groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        // Apply the first texture
        ApplyTexture(0);

        // Apply the material to the ground
        GetComponent<Renderer>().material = groundMaterial;
    }

    void ApplyTexture(int index)
    {
        if (textures == null || textures.Length == 0)
        {
            Debug.LogWarning("No textures assigned!");
            groundMaterial.color = new Color(0.2f, 0.6f, 0.2f);
            return;
        }

        // Make sure the index is valid
        index = Mathf.Clamp(index, 0, textures.Length - 1);
        currentTextureIndex = index;

        TextureData textureData = textures[index];
        
        if (textureData.texture != null)
        {
            // Set up the material with the texture
            groundMaterial.SetTexture("_BaseMap", textureData.texture);
            groundMaterial.SetFloat("_Smoothness", textureData.smoothness);
            groundMaterial.SetTextureScale("_BaseMap", new Vector2(textureData.scale, textureData.scale));
        }
        else
        {
            Debug.LogWarning($"Texture {textureData.name} is not assigned!");
        }
    }

    // Call this function to switch to the next texture
    public void NextTexture()
    {
        ApplyTexture(currentTextureIndex + 1);
    }

    // Call this function to switch to the previous texture
    public void PreviousTexture()
    {
        ApplyTexture(currentTextureIndex - 1);
    }

    // Call this function to switch to a specific texture by index
    public void SetTexture(int index)
    {
        ApplyTexture(index);
    }
} 