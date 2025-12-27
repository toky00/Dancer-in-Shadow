using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    private Plane groundPlane = new Plane(Vector3.up, new Vector3(0, 0, 2.5f));
    private Quaternion targetRotation;
    [SerializeField] private GameObject facingObject;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runningParamName = "Running";

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    private CharacterController controller;
    private Camera mainCamera;
    private Vector3 horizontalVelocity = Vector3.zero;
    private float verticalVelocity = 0f;
    private float currentMaxSpeed = 0f;
    [SerializeField] private bool isRunning = false;

    private InputActionMap playerMap;
    private InputAction movementAction;
    private InputAction runToggleAction;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        currentMaxSpeed = walkSpeed;
    }

    private void OnEnable()
    {
        playerMap = inputActions.FindActionMap("Player");
        movementAction = playerMap.FindAction("Move");
        runToggleAction = playerMap.FindAction("Sprint");
        runToggleAction.performed += _ => isRunning = !isRunning;
        runToggleAction.performed += _ => animator.SetBool("Running", isRunning);
        playerMap.Enable();
    }

    private void OnDisable()
    {
        runToggleAction.performed -= _ => isRunning = !isRunning;
        playerMap.Disable();
    }

    void Movement()
    {
        // Read movement input
        Vector2 input = movementAction.ReadValue<Vector2>();

        // Camera-relative direction
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * input.y + cameraRight * input.x;
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            moveDirection.Normalize();
        }

        // Target speed
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        // Smooth currentMaxSpeed (key fix: prevents anim snap on toggle, enables normalization)
        currentMaxSpeed = Mathf.Lerp(currentMaxSpeed, targetSpeed, acceleration * Time.deltaTime);

        // Target velocity
        Vector3 targetHorizontalVelocity = moveDirection * targetSpeed;

        // Smooth velocity (momentum)
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontalVelocity, acceleration * Time.deltaTime);

        // Animation (key fix: use facingObject for local space + currentMaxSpeed works now)
        if (animator != null)
        {
            animator.SetBool(runningParamName, isRunning);

            Vector3 localVelocity = facingObject.transform.InverseTransformDirection(horizontalVelocity);
            float xInput = currentMaxSpeed > 0.01f ? Mathf.Clamp(localVelocity.x / currentMaxSpeed, -1f, 1f) : 0f;
            float yInput = currentMaxSpeed > 0.01f ? Mathf.Clamp(localVelocity.z / currentMaxSpeed, -1f, 1f) : 0f;

            animator.SetFloat("XInput", xInput);
            animator.SetFloat("YInput", yInput);
        }

        // Gravity
        if (controller.isGrounded)
        {
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Move
        Vector3 move = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }

    void UpdateRotation()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 direction = targetPoint - facingObject.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                targetRotation = Quaternion.LookRotation(direction);
            }
            facingObject.transform.rotation = Quaternion.Slerp(facingObject.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void Update()
    {
        UpdateRotation();
        Movement();
    }
}