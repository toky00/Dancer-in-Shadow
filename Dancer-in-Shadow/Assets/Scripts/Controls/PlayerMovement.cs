using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;  // Add this import for the new Input System

public class PlayerMovement : MonoBehaviour
{
    private Transform m_transform;

    private void Start()
    {
        m_transform = this.transform;
    }

    private void LookAtMouse()
    {
        // Get mouse position in screen space using the new Input System
        Vector2 mouseScreenPos2D = Mouse.current.position.ReadValue();
        
        // Convert to Vector3 and set Z to the distance from camera to the world plane (assuming 2D setup with objects at Z=0 and camera at negative Z)
        Vector3 mouseScreenPos = new Vector3(mouseScreenPos2D.x, mouseScreenPos2D.y, -Camera.main.transform.position.z);
        
        // Convert screen position to world position
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        
        // Calculate direction and rotation
        Vector3 direction = mouseWorldPos - m_transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        quaternion rotation = Quaternion.AngleAxis(angle - 90, Vector3.up);
        m_transform.rotation = rotation;
    }

    private void Update()
    {
        LookAtMouse();
    }
}