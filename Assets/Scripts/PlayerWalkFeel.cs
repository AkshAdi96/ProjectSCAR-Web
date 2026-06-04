using UnityEngine;

public class PlayerWalkFeel : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public AudioSource footstepAudioSource;
    public AudioClip footstepClip;

    [Header("Camera Shake (Headbob)")]
    public float bobFrequency = 10f; // How fast the camera shakes
    public float bobAmplitude = 0.05f; // How high/low the camera moves

    [Header("Footsteps")]
    public float stepInterval = 0.5f; // Time between each footstep sound

    private float defaultCameraY;
    private float timer;
    private float stepTimer;
    private Vector3 lastPosition;

    void Start()
    {
        // Remember where the camera started so we can reset it when standing still
        if (playerCamera != null)
        {
            defaultCameraY = playerCamera.transform.localPosition.y;
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        // 1. Calculate if the capsule actually moved this frame (ignores falling)
        Vector3 currentPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 previousPosition = new Vector3(lastPosition.x, 0, lastPosition.z);
        float distanceMoved = Vector3.Distance(currentPosition, previousPosition);

        bool isMoving = distanceMoved > 0.005f; // Threshold to prevent micro-jitters
        lastPosition = transform.position;

        if (isMoving)
        {
            // 2. Handle Camera Headbob using a Sine Wave
            timer += Time.deltaTime * bobFrequency;
            float newY = defaultCameraY + (Mathf.Sin(timer) * bobAmplitude);

            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                newY,
                playerCamera.transform.localPosition.z
            );

            // 3. Handle Footstep Audio Timer
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                // Randomize pitch slightly so the footsteps don't sound like a machine gun
                footstepAudioSource.pitch = Random.Range(0.9f, 1.1f);
                footstepAudioSource.PlayOneShot(footstepClip);

                // Reset the timer for the next step
                stepTimer = stepInterval;
            }
        }
        else
        {
            // 4. Smoothly reset camera back to center when player stops walking
            timer = 0;
            stepTimer = stepInterval; // Reset step timer so sound plays immediately on next walk

            float smoothResetY = Mathf.Lerp(playerCamera.transform.localPosition.y, defaultCameraY, Time.deltaTime * 5f);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                smoothResetY,
                playerCamera.transform.localPosition.z
            );
        }
    }
}