using UnityEngine;

public class DelayedCameraEnable : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(EnableCameraNextFrame());
    }

    System.Collections.IEnumerator EnableCameraNextFrame()
    {
        // Wait 1 frame so the scene loads *without* camera interference
        yield return null;

        Camera cam = GetComponent<Camera>();
        if (cam != null && !cam.enabled)
        {
            cam.enabled = true;
            Debug.Log("[DelayedCameraEnable] Camera enabled after 1 frame.");
        }
    }
}