using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControls : MonoBehaviour
{
    #region Rotation
    [Header("Rotation Settings")]
    [SerializeField] private GameObject pivot; // Assign your pivot GameObject (e.g., character or scene center) in Inspector
    [SerializeField] private float rotationSpeed = 0.5f; // Sensitivity: higher = faster rotation (pixels to degrees)
    [SerializeField] private float smoothingSpeed = 5f; // Adjust in Inspector for rotation damping (higher = snappier)
    [SerializeField] private float maxTiltDeviation = 15f; // Max degrees to tilt up/down from initial angle

    private float minPitch;
    private float maxPitch;
    private float currentXAmount;
    private float currentYAmount;
    #endregion
    #region Zooming
    [Header("Zoom Settings")]
    [SerializeField] private float minDistance = 5f; // Minimum zoom distance
    [SerializeField] private float maxDistance = 50f; // Maximum zoom distance
    [SerializeField] private float zoomSpeed = 5f; // Scroll sensitivity (higher = faster zoom)
    [SerializeField] private float zoomSmoothingSpeed = 8f; // Smoothness of zoom (higher = snappier)

    private float targetDistance;
    #endregion

    void Start()
    {
        if (pivot == null) return;

        // Calculate initial signed pitch and clamp ranges
        float initialPitch = GetSignedPitch(pivot.transform.rotation);
        minPitch = initialPitch - maxTiltDeviation;
        maxPitch = initialPitch + maxTiltDeviation;

        // Initialize target to current distance from parent pivot (assumes camera is child of pivot)
        targetDistance = transform.localPosition.magnitude;
    }

    void CameraRotation()
    {
        if (pivot == null) return; // Safety check

        // Get mouse movement delta only if middle button is held
        Vector2 mouseDelta = Vector2.zero;
        if (Mouse.current.middleButton.isPressed)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }

        // Calculate desired rotation amounts (zero if not pressed)
        float desiredYAmount = -mouseDelta.x * rotationSpeed; // For Y-axis (yaw), invert if needed
        float desiredXAmount = mouseDelta.y * rotationSpeed; // For X-axis (pitch/tilt), invert if needed

        // Smoothly interpolate current amounts towards desired
        currentYAmount = Mathf.Lerp(currentYAmount, desiredYAmount, Time.deltaTime * smoothingSpeed);
        currentXAmount = Mathf.Lerp(currentXAmount, desiredXAmount, Time.deltaTime * smoothingSpeed);

        // Apply Y rotation (global Y-axis, no clamp)
        pivot.transform.Rotate(0f, currentYAmount, 0f, Space.World);

        // Store state before X rotation
        Quaternion oldRotation = pivot.transform.rotation;

        // Apply X rotation (local X-axis)
        pivot.transform.Rotate(currentXAmount, 0f, 0f, Space.Self);

        // Check and clamp pitch
        float newPitch = GetSignedPitch(pivot.transform.rotation);
        if (newPitch < minPitch || newPitch > maxPitch)
        {
            // Revert if out of bounds
            pivot.transform.rotation = oldRotation;
        }
    }

    void CameraZoom()
    {
        // Read mouse scroll wheel delta (Y > 0 = scroll up = zoom in)
        float scrollDelta = Mouse.current.scroll.y.ReadValue();

        // Adjust target distance (invert scroll for intuitive zoom in/out)
        if (Mathf.Abs(scrollDelta) > 0.01f) // Ignore tiny noise
        {
            targetDistance -= scrollDelta * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // Lerp current distance smoothly
        float currentDistance = transform.localPosition.magnitude;
        if (Mathf.Approximately(currentDistance, 0f)) return; // Safety

        float lerpedDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSmoothingSpeed);

        // Update local position to zoom towards pivot (preserves relative direction)
        transform.localPosition = transform.localPosition.normalized * lerpedDistance;
    }
    void Update()
    {
        CameraRotation();
        CameraZoom();
    }

    private float GetSignedPitch(Quaternion rot)
    {
        float pitch = rot.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        return pitch;
    }
}
