using UnityEngine;

// This script is responsible for creating and managing visual effects for weapons, such as muzzle flashes and impact particles.
public class WeaponEffects : MonoBehaviour
{
    [Header("Muzzle Flash Settings")]
    public float muzzleFlashDuration = 0.05f; // The duration of the muzzle flash effect.
    public Color muzzleFlashColor = new Color(1f, 0.7f, 0.3f); // The color of the muzzle flash.
    public float muzzleFlashSize = 0.2f; // The size of the muzzle flash particles.

    [Header("Impact Effect Settings")]
    public float impactDuration = 0.5f; // The duration of the impact effect.
    public Color impactColor = new Color(0.8f, 0.8f, 0.8f); // The color of the impact particles.
    public float impactSize = 0.3f; // The size of the impact particles.

    // Creates and configures a ParticleSystem for the muzzle flash effect.
    public ParticleSystem CreateMuzzleFlash()
    {
        // Create a new GameObject to host the muzzle flash.
        GameObject muzzleFlashObj = new GameObject("MuzzleFlash");
        muzzleFlashObj.transform.SetParent(transform); // Parent it to the weapon or camera.
        muzzleFlashObj.transform.localPosition = new Vector3(0, 0, 0.5f); // Position it in front of the camera/gun.

        // Add and configure the ParticleSystem component.
        ParticleSystem muzzleFlash = muzzleFlashObj.AddComponent<ParticleSystem>();
        var main = muzzleFlash.main;
        main.duration = muzzleFlashDuration;
        main.loop = false;
        main.startLifetime = muzzleFlashDuration;
        main.startSpeed = 0f;
        main.startSize = muzzleFlashSize;
        main.startColor = muzzleFlashColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World; // Particles should not move with the weapon.

        // Configure the emission module to create a burst of particles.
        var emission = muzzleFlash.emission;
        emission.enabled = true;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 20));

        // Configure the shape of the emission.
        var shape = muzzleFlash.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.01f;

        // Configure the renderer for the particles.
        var renderer = muzzleFlash.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard; // Particles always face the camera.
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit")); // Use a simple unlit material.
        renderer.material.color = muzzleFlashColor;

        return muzzleFlash;
    }

    // Creates and configures a GameObject with a ParticleSystem for the bullet impact effect.
    public GameObject CreateImpactEffect()
    {
        // Create a new GameObject to host the impact effect.
        GameObject impactObj = new GameObject("ImpactEffect");
        ParticleSystem impact = impactObj.AddComponent<ParticleSystem>();

        // Configure the main settings of the particle system.
        var main = impact.main;
        main.duration = impactDuration;
        main.loop = false;
        main.startLifetime = impactDuration;
        main.startSpeed = 2f;
        main.startSize = impactSize;
        main.startColor = impactColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Configure the emission module for a burst of particles on impact.
        var emission = impact.emission;
        emission.enabled = true;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 30));

        // Configure the shape of the emission.
        var shape = impact.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.1f;

        // Configure the particle renderer.
        var renderer = impact.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = impactColor;

        // Add a point light to the impact effect for extra visual flair.
        Light impactLight = impactObj.AddComponent<Light>();
        impactLight.type = LightType.Point;
        impactLight.intensity = 2f;
        impactLight.range = 2f;
        impactLight.color = impactColor;
        impactLight.enabled = false; // Initially disabled.

        // Add a controller script to manage the light's lifetime.
        impactObj.AddComponent<ImpactLightController>();

        return impactObj;
    }
}

// A helper class to control the brief flash of light on bullet impact.
public class ImpactLightController : MonoBehaviour
{
    private Light impactLight; // The light component to control.
    private float duration = 0.1f; // How long the light stays on.
    private float timer = 0f; // A timer to track the duration.

    // Called when the script instance is being loaded.
    void Start()
    {
        impactLight = GetComponent<Light>();
        impactLight.enabled = true; // Turn the light on.
    }

    // Called every frame.
    void Update()
    {
        // Increment the timer and disable the light and script once the duration has passed.
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            impactLight.enabled = false;
            Destroy(this); // Remove this script component.
        }
    }
} 