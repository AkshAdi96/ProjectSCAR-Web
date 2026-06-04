using System.Collections;
using UnityEngine;

public class NoirLightning : MonoBehaviour
{
    [Header("Visuals")]
    public Light sunLight; // Your blue Moonlight
    public float flashMultiplier = 5f; // Push this higher since your base is only 0.5

    [Header("Audio")]
    public AudioSource thunderAudioSource;
    public AudioClip thunderClip;

    [Header("Timing")]
    public float minTimeBetweenStrikes = 10f;
    public float maxTimeBetweenStrikes = 25f;

    private float baseIntensity;
    private Color baseColor; // We need to remember your specific blue color

    void Start()
    {
        if (sunLight != null)
        {
            baseIntensity = sunLight.intensity;
            baseColor = sunLight.color; // Save the blue
        }

        StartCoroutine(LightningLoop());
    }

    IEnumerator LightningLoop()
    {
        while (true)
        {
            // 1. Wait in the dark blue atmosphere
            float waitTime = Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes);
            yield return new WaitForSeconds(waitTime);

            // 2. The First Flash (Violent, bright, and pure WHITE)
            if (sunLight != null)
            {
                sunLight.color = Color.white;
                sunLight.intensity = baseIntensity * flashMultiplier;
            }
            yield return new WaitForSeconds(0.05f);

            // 3. Flicker off, then secondary flash
            if (sunLight != null) sunLight.intensity = baseIntensity;
            yield return new WaitForSeconds(0.05f);
            if (sunLight != null) sunLight.intensity = baseIntensity * (flashMultiplier / 1.5f);
            yield return new WaitForSeconds(0.1f);

            // 4. Return to your 0.5 Blue Moonlight
            if (sunLight != null)
            {
                sunLight.color = baseColor;
                sunLight.intensity = baseIntensity;
            }

            // 5. The Thunder Audio
            yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));

            if (thunderAudioSource != null && thunderClip != null)
            {
                thunderAudioSource.pitch = Random.Range(0.8f, 1.1f);
                thunderAudioSource.PlayOneShot(thunderClip);
            }
        }
    }
}
