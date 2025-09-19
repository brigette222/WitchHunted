using UnityEngine;

[RequireComponent(typeof(Camera))] // Ensure a Camera component exists on this GameObject
public class CamFollow : MonoBehaviour
{
    public Transform target; // The object to follow (e.g., Player)
    public float followSpeed = 5f; // Lerp speed for smooth following
    public Vector2 offset = new Vector2(0f, 1f); // Offset from target position

    private void LateUpdate() // Called after Update to avoid jitter
    {
        if (target == null) return; // Do nothing if no target assigned

        Vector3 desiredPosition = new Vector3( // Build desired camera position
            target.position.x + offset.x, // Offset X
            target.position.y + offset.y, // Offset Y
            -10f // Keep camera behind the scene in 2D
        );

        transform.position = Vector3.Lerp( // Smoothly move camera toward target
            transform.position, // Current camera position
            desiredPosition, // Target position
            followSpeed * Time.deltaTime // Interpolation factor
        );
    }
}