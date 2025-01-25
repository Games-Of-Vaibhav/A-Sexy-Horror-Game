using UnityEngine;

public class FireLightFlicker : MonoBehaviour
{
    [Header("Light Settings")]
    public Light fireLight;             // Assign the light representing fire
    public float minIntensity = 0.8f;   // Minimum intensity of the light
    public float maxIntensity = 1.2f;   // Maximum intensity of the light
    public float flickerSpeed = 0.1f;   // Speed of intensity changes (lower is faster)

    private float targetIntensity;      // The target intensity to flicker towards

    private void Start()
    {
        if (fireLight == null)
        {
            Debug.LogError("Fire Light is not assigned in FireLightFlicker script.");
        }

        // Initialize the target intensity
        targetIntensity = fireLight.intensity;
    }

    private void Update()
    {
        if (fireLight == null) return;

        // Gradually move the light intensity towards the target intensity
        fireLight.intensity = Mathf.Lerp(fireLight.intensity, targetIntensity, flickerSpeed);

        // If close enough to the target intensity, set a new random target
        if (Mathf.Abs(fireLight.intensity - targetIntensity) < 0.05f)
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
        }
    }
}
