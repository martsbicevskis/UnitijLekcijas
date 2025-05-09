using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [Header("Gun Settings")]
    public GameObject gunModel; // Reference to the gun model
    public Vector3 gunPosition = new Vector3(0.4f, -0.3f, 0.8f); // Position relative to camera
    public Vector3 gunRotation = new Vector3(0f, 0f, 0f); // Rotation of the gun

    [Header("Crosshair Settings")]
    public Image crosshairImage; // Reference to the crosshair UI image
    public Color crosshairColor = Color.white;
    public float crosshairSize = 8f; // Smaller size

    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        
        // Create and set up the gun model if it doesn't exist
        if (gunModel == null)
        {
            CreateDefaultGunModel();
        }
        else
        {
            SetupGunModel();
        }

        // Create and set up the crosshair if it doesn't exist
        if (crosshairImage == null)
        {
            CreateCrosshair();
        }
        else
        {
            SetupCrosshair();
        }
    }

    void CreateDefaultGunModel()
    {
        // Create a simple gun model using primitive shapes
        gunModel = new GameObject("GunModel");
        
        // Create the gun body
        GameObject gunBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gunBody.transform.SetParent(gunModel.transform);
        gunBody.transform.localScale = new Vector3(0.2f, 0.2f, 0.4f);
        gunBody.transform.localPosition = new Vector3(0, 0, 0.2f);
        
        // Create the gun barrel
        GameObject gunBarrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gunBarrel.transform.SetParent(gunModel.transform);
        gunBarrel.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
        gunBarrel.transform.localPosition = new Vector3(0, 0, 0.5f);
        gunBarrel.transform.localRotation = Quaternion.Euler(90, 0, 0);

        // Create the gun handle
        GameObject gunHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gunHandle.transform.SetParent(gunModel.transform);
        gunHandle.transform.localScale = new Vector3(0.15f, 0.3f, 0.1f);
        gunHandle.transform.localPosition = new Vector3(0, -0.25f, 0.1f);
        gunHandle.transform.localRotation = Quaternion.Euler(30, 0, 0);

        // Add materials to make them more visible
        gunBody.GetComponent<Renderer>().material.color = Color.black;
        gunBarrel.GetComponent<Renderer>().material.color = Color.gray;
        gunHandle.GetComponent<Renderer>().material.color = Color.black;

        // Set up the gun model
        SetupGunModel();
    }

    void SetupGunModel()
    {
        // Parent the gun model to the camera
        gunModel.transform.SetParent(playerCamera.transform);
        gunModel.transform.localPosition = gunPosition;
        gunModel.transform.localRotation = Quaternion.Euler(gunRotation);
    }

    void CreateCrosshair()
    {
        // Create a canvas for the crosshair
        GameObject canvasObj = new GameObject("CrosshairCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create the crosshair image
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(canvasObj.transform, false);
        crosshairImage = crosshairObj.AddComponent<Image>();
        
        // Set up the crosshair
        SetupCrosshair();
    }

    void SetupCrosshair()
    {
        // Set crosshair properties
        crosshairImage.color = crosshairColor;
        crosshairImage.rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
        
        // Center the crosshair
        crosshairImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairImage.rectTransform.anchoredPosition = Vector2.zero;

        // Make sure the crosshair is visible
        crosshairImage.raycastTarget = false;

        // Make the crosshair circular
        crosshairImage.sprite = CreateCircleSprite();
    }

    private Sprite CreateCircleSprite()
    {
        // Create a circular sprite
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - size / 2;
                float dy = y - size / 2;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = distance <= size / 2 ? 1f : 0f;
                colors[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        // Add some subtle movement to the gun when moving
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            float swayAmount = 0.02f;
            float swaySpeed = 2f;
            float swayX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
            float swayY = Mathf.Cos(Time.time * swaySpeed) * swayAmount;
            
            gunModel.transform.localPosition = gunPosition + new Vector3(swayX, swayY, 0);
        }
        else
        {
            // Return to original position
            gunModel.transform.localPosition = gunPosition;
        }
    }
} 