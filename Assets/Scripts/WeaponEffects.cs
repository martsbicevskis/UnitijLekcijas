using UnityEngine;

public class WeaponEffects : MonoBehaviour
{
    [Header("Muzzle Flash Settings")]
    public float muzzleFlashDuration = 0.05f;
    public Color muzzleFlashColor = new Color(1f, 0.7f, 0.3f); // Orange-yellow color
    public float muzzleFlashSize = 0.2f;

    [Header("Impact Effect Settings")]
    public float impactDuration = 0.5f;
    public Color impactColor = new Color(0.8f, 0.8f, 0.8f); // Light gray color
    public float impactSize = 0.3f;

    public ParticleSystem CreateMuzzleFlash()
    {
        // Create a new GameObject for the muzzle flash
        GameObject muzzleFlashObj = new GameObject("MuzzleFlash");
        muzzleFlashObj.transform.SetParent(transform);
        muzzleFlashObj.transform.localPosition = new Vector3(0, 0, 0.5f); // Position in front of the camera

        // Add and configure the particle system
        ParticleSystem muzzleFlash = muzzleFlashObj.AddComponent<ParticleSystem>();
        var main = muzzleFlash.main;
        main.duration = muzzleFlashDuration;
        main.loop = false;
        main.startLifetime = muzzleFlashDuration;
        main.startSpeed = 0f;
        main.startSize = muzzleFlashSize;
        main.startColor = muzzleFlashColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Configure emission
        var emission = muzzleFlash.emission;
        emission.enabled = true;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 20));

        // Configure shape
        var shape = muzzleFlash.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.01f;

        // Configure renderer
        var renderer = muzzleFlash.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = muzzleFlashColor;

        return muzzleFlash;
    }

    public GameObject CreateImpactEffect()
    {
        // Create a new GameObject for the impact effect
        GameObject impactObj = new GameObject("ImpactEffect");
        ParticleSystem impact = impactObj.AddComponent<ParticleSystem>();

        // Configure main settings
        var main = impact.main;
        main.duration = impactDuration;
        main.loop = false;
        main.startLifetime = impactDuration;
        main.startSpeed = 2f;
        main.startSize = impactSize;
        main.startColor = impactColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Configure emission
        var emission = impact.emission;
        emission.enabled = true;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 30));

        // Configure shape
        var shape = impact.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.1f;

        // Configure renderer
        var renderer = impact.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = impactColor;

        // Add a light component for additional effect
        Light impactLight = impactObj.AddComponent<Light>();
        impactLight.type = LightType.Point;
        impactLight.intensity = 2f;
        impactLight.range = 2f;
        impactLight.color = impactColor;
        impactLight.enabled = false;

        // Add a script to handle the light
        impactObj.AddComponent<ImpactLightController>();

        return impactObj;
    }
}

// Helper class to control the impact light
public class ImpactLightController : MonoBehaviour
{
    private Light impactLight;
    private float duration = 0.1f;
    private float timer = 0f;

    void Start()
    {
        impactLight = GetComponent<Light>();
        impactLight.enabled = true;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            impactLight.enabled = false;
            Destroy(this);
        }
    }
} 