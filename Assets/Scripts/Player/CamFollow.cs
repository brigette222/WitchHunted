using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamFollow : MonoBehaviour
{
    public Transform target;           // The object to follow (your Player)
    public float followSpeed = 5f;     // Smooth speed
    public Vector2 offset = new Vector2(0f, 1f); // Camera offset from player

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            -10f // Always behind the scene in 2D
        );

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }
}