using UnityEngine;

public class FreshCameraSpawner : MonoBehaviour
{
    public Color backgroundColor = Color.black; // Background color for the new camera
    public float orthographicSize = 5f; // Default zoom level for the camera
    public Transform playerTarget; // Optional manual target assignment
    public Vector2 cameraOffset = new Vector2(0f, 1f); // How far camera should offset from the player
    public float followSpeed = 5f; // Smoothness of camera follow

    void Start()
    {
        //  CREATE NEW CAMERA OBJECT 
        Camera newCam = new GameObject("SpawnedMainCamera").AddComponent<Camera>(); // Make a new GameObject with a Camera component
        newCam.tag = "MainCamera"; // Mark as the main camera so Unity recognizes it
        newCam.clearFlags = CameraClearFlags.SolidColor; // Clear screen with solid color
        newCam.backgroundColor = backgroundColor; // Apply background color
        newCam.orthographic = true; // Make it orthographic (2D style)
        newCam.orthographicSize = orthographicSize; // Set zoom level
        newCam.transform.position = new Vector3(0, 0, -10); // Position camera behind the scene
        newCam.depth = 0; // Render order priority

        // ADD AUDIO LISTENER
        newCam.gameObject.AddComponent<AudioListener>(); // Required for Unity’s sound system

        // ADD FOLLOW SCRIPT
        CamFollow camFollow = newCam.gameObject.AddComponent<CamFollow>(); // Attach your custom camera follow script
        camFollow.offset = cameraOffset; // Set offset from player
        camFollow.followSpeed = followSpeed; // Set smooth follow speed

        // ASSIGN CAMERA TARGET
        if (playerTarget != null) // If manually assigned, use that
        {
            camFollow.target = playerTarget;
        }
        else // Otherwise, try to find the Player by tag
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                camFollow.target = player.transform; // Assign found player as target
            }
        }
    }
}