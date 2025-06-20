using UnityEngine;
using UnityEngine.UI;

// This script manages the visual representation of the player's weapon, including the gun model and crosshair.
public class WeaponController : MonoBehaviour
{
    [Header("Gun Settings")]
    public GameObject gunModel; // A reference to the 3D model of the gun.
    public Vector3 gunPosition = new Vector3(0.4f, -0.3f, 0.8f); // The position of the gun relative to the camera.
    public Vector3 gunRotation = new Vector3(0f, 0f, 0f); // The rotation of the gun.

    [Header("Crosshair Settings")]
    public Image crosshairImage; // A reference to the UI Image for the crosshair.
    public Color crosshairColor = Color.white; // The color of the crosshair.
    public float crosshairSize = 8f; // The size of the crosshair.

    private Camera playerCamera; // A reference to the player's camera.

    // Called when the script instance is being loaded.
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        
        // Create a default gun model if one is not assigned.
        if (gunModel == null)
        {
            CreateDefaultGunModel();
        }
        else
        {
            SetupGunModel();
        }

        // Create a default crosshair if one is not assigned.
        if (crosshairImage == null)
        {
            CreateCrosshair();
        }
        else
        {
            SetupCrosshair();
        }
    }

    // Creates a simple, default gun model using primitive shapes if no model is provided.
    void CreateDefaultGunModel()
    {
        gunModel = new GameObject("GunModel");
        
        // Create the main body of the gun.
        GameObject gunBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gunBody.transform.SetParent(gunModel.transform);
        gunBody.transform.localScale = new Vector3(0.2f, 0.2f, 0.4f);
        gunBody.transform.localPosition = new Vector3(0, 0, 0.2f);
        
        // Create the barrel of the gun.
        GameObject gunBarrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gunBarrel.transform.SetParent(gunModel.transform);
        gunBarrel.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
        gunBarrel.transform.localPosition = new Vector3(0, 0, 0.5f);
        gunBarrel.transform.localRotation = Quaternion.Euler(90, 0, 0);

        // Create the handle of the gun.
        GameObject gunHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gunHandle.transform.SetParent(gunModel.transform);
        gunHandle.transform.localScale = new Vector3(0.15f, 0.3f, 0.1f);
        gunHandle.transform.localPosition = new Vector3(0, -0.25f, 0.1f);
        gunHandle.transform.localRotation = Quaternion.Euler(30, 0, 0);

        // Assign basic materials for visibility.
        gunBody.GetComponent<Renderer>().material.color = Color.black;
        gunBarrel.GetComponent<Renderer>().material.color = Color.gray;
        gunHandle.GetComponent<Renderer>().material.color = Color.black;

        // Position and parent the newly created gun model.
        SetupGunModel();
    }

    // Positions and parents the gun model to the player's camera.
    void SetupGunModel()
    {
        gunModel.transform.SetParent(playerCamera.transform);
        gunModel.transform.localPosition = gunPosition;
        gunModel.transform.localRotation = Quaternion.Euler(gunRotation);
    }

    // Creates a crosshair UI element if one is not provided.
    void CreateCrosshair()
    {
        // Create a new Canvas for the crosshair.
        GameObject canvasObj = new GameObject("CrosshairCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create the Image object for the crosshair.
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(canvasObj.transform, false);
        crosshairImage = crosshairObj.AddComponent<Image>();
        
        // Configure the newly created crosshair.
        SetupCrosshair();
    }

    // Configures the properties of the crosshair image.
    void SetupCrosshair()
    {
        crosshairImage.color = crosshairColor;
        crosshairImage.rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
        
        // Center the crosshair on the screen.
        crosshairImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairImage.rectTransform.anchoredPosition = Vector2.zero;

        // Disable raycast target to prevent the crosshair from blocking mouse input.
        crosshairImage.raycastTarget = false;

        // Generate a circular sprite for the crosshair.
        crosshairImage.sprite = CreateCircleSprite();
    }

    // Generates a simple, circular sprite programmatically.
    private Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];

        // Fill the texture with a circle shape.
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - size / 2f;
                float dy = y - size / 2f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                // Set pixel alpha based on distance from the center to create a circle.
                colors[y * size + x] = new Color(1, 1, 1, distance <= size / 2f ? 1f : 0f);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // Called every frame.
    void Update()
    {
        // Apply a subtle weapon sway effect when the player is moving.
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            float swayAmount = 0.02f;
            float swaySpeed = 2f;
            float swayX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
            float swayY = Mathf.Cos(Time.time * swaySpeed) * swayAmount;
            
            // Apply the sway offset to the gun's position.
            gunModel.transform.localPosition = gunPosition + new Vector3(swayX, swayY, 0);
        }
        else
        {
            // Return the gun to its original position when not moving.
            gunModel.transform.localPosition = Vector3.Lerp(gunModel.transform.localPosition, gunPosition, Time.deltaTime * 5f);
        }
    }
} 