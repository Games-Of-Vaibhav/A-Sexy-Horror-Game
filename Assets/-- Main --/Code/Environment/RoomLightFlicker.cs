using UnityEngine;

public class RoomLightFlicker : MonoBehaviour
{
    [Header("Light Settings")]
    public Light roomLight;             // Assign the light representing the room or streetlight
    public float minIntensity = 0f;     // Minimum intensity during flicker
    public float maxIntensity = 1f;     // Maximum intensity during flicker
    public float flickerFrequency = 0.2f; // Frequency of flicker in seconds

    [Header("Behavior Settings")]
    public bool randomizeFlicker = true;  // Randomize flicker intervals
    public float minFlickerTime = 0.1f;   // Minimum time between flickers
    public float maxFlickerTime = 0.5f;   // Maximum time between flickers

    private float nextFlickerTime;        // Time until the next flicker

    private void Start()
    {
        if (roomLight == null)
        {
            Debug.LogError("Room Light is not assigned in RoomLightFlicker script.");
        }

        // Set the initial flicker time
        SetNextFlickerTime();
    }

    private void Update()
    {
        if (roomLight == null) return;

        // Check if it's time for the next flicker
        if (Time.time >= nextFlickerTime)
        {
            // Randomize light intensity for flicker effect
            roomLight.intensity = Random.Range(minIntensity, maxIntensity);

            // Schedule the next flicker
            SetNextFlickerTime();
        }
    }

    private void SetNextFlickerTime()
    {
        if (randomizeFlicker)
        {
            nextFlickerTime = Time.time + Random.Range(minFlickerTime, maxFlickerTime);
        }
        else
        {
            nextFlickerTime = Time.time + flickerFrequency;
        }
    }
}
