using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement (Momentum & Weight)")]
    public float walkSpeed = 3.5f; 
    public float runSpeed = 7.5f;
    [Tooltip("Higher number = heavier character that takes longer to speed up/slow down")]
    public float movementSmoothTime = 0.15f; 

    [Header("Gravity & Jump")]
    public float jumpHeight = 1.2f;
    public float gravity = -18f; // Pro tip: Standard -9.81 feels floaty. Heavier is better.
    public float terminalVelocity = -40f;

    [Header("References")]
    public CharacterController controller;
    public Transform playerCamera;
    public Camera camComponent; // Needed for the sprint FOV effect
    public Joystick movementJoystick;
    public float sensitivity = 2f;

    [Header("Dynamic Camera (FOV)")]
    public float walkFOV = 60f;
    public float runFOV = 70f;
    public float fovTransitionSpeed = 5f;

    // Internal State
    private float currentSpeed;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private bool isSprinting = false;

    // Smoothing Variables
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;

    void Start()
    {
        if (Application.platform != RuntimePlatform.Android)
            Cursor.lockState = CursorLockMode.Locked;

        if (camComponent == null && playerCamera != null)
            camComponent = playerCamera.GetComponent<Camera>();
    }

    void Update()
    {
        HandleGravity();
        HandleLook();
        HandleMovement();
        HandleCameraFOV();
    }

    void HandleGravity()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keeps the capsule glued down when walking down stairs
        }

        velocity.y += gravity * Time.deltaTime;
        if (velocity.y < terminalVelocity) velocity.y = terminalVelocity;
        
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = 0;
        float mouseY = 0;

        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                // Only look if touching RIGHT side and NOT touching a UI button
                if (touch.position.x > Screen.width / 2 && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    mouseX = touch.deltaPosition.x * sensitivity * 0.1f;
                    mouseY = touch.deltaPosition.y * sensitivity * 0.1f;
                }
            }
        }
        else
        {
            // PC Fallback
            mouseX = Input.GetAxis("Mouse X") * sensitivity;
            mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        }

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // 1. Get raw input
        Vector2 targetInput = new Vector2(movementJoystick.Horizontal, movementJoystick.Vertical);
        if (targetInput == Vector2.zero) // PC Fallback
        {
            targetInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        // 2. Normalize so diagonal movement isn't faster than walking straight
        if (targetInput.magnitude > 1f) targetInput.Normalize();

        // 3. Smooth the input (This creates physical weight/momentum)
        currentInputVector = Vector2.SmoothDamp(currentInputVector, targetInput, ref smoothInputVelocity, movementSmoothTime);

        // 4. Apply Sprint Logic
        currentSpeed = isSprinting ? runSpeed : walkSpeed;

        // 5. Move Character
        Vector3 move = transform.right * currentInputVector.x + transform.forward * currentInputVector.y;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleCameraFOV()
    {
        if (camComponent != null)
        {
            // Only pull the FOV back if we are sprinting AND actually moving forward
            bool isActuallyMoving = currentInputVector.magnitude > 0.1f;
            float targetFOV = (isSprinting && isActuallyMoving) ? runFOV : walkFOV;
            
            camComponent.fieldOfView = Mathf.Lerp(camComponent.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }
    }

    // --- UI BUTTON FUNCTIONS ---
    public void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void ToggleSprint()
    {
        isSprinting = !isSprinting;
    }
}