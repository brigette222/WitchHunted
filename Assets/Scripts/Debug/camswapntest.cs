using UnityEngine;

public class FreshCameraSpawner : MonoBehaviour
{
    public Color backgroundColor = Color.black;
    public float orthographicSize = 5f;
    public Transform playerTarget; // Optional manual assignment
    public Vector2 cameraOffset = new Vector2(0f, 1f);
    public float followSpeed = 5f;

    void Start()
    {
        Debug.Log("[FreshCameraSpawner] Spawning new runtime camera with CamFollow.");

        // Create the camera
        Camera newCam = new GameObject("SpawnedMainCamera").AddComponent<Camera>();
        newCam.tag = "MainCamera";
        newCam.clearFlags = CameraClearFlags.SolidColor;
        newCam.backgroundColor = backgroundColor;
        newCam.orthographic = true;
        newCam.orthographicSize = orthographicSize;
        newCam.transform.position = new Vector3(0, 0, -10);
        newCam.depth = 0;

        // Add AudioListener
        newCam.gameObject.AddComponent<AudioListener>();

        // === ADD YOUR CamFollow SCRIPT ===
        CamFollow camFollow = newCam.gameObject.AddComponent<CamFollow>();
        camFollow.offset = cameraOffset;
        camFollow.followSpeed = followSpeed;

        // Automatically find player or use manual reference
        if (playerTarget != null)
        {
            camFollow.target = playerTarget;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                camFollow.target = player.transform;
                Debug.Log("[FreshCameraSpawner] Assigned Player as CamFollow target.");
            }
            else
            {
                Debug.LogWarning("[FreshCameraSpawner] No Player found in scene for CamFollow.");
            }
        }
    }
}