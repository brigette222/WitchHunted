using UnityEngine;

public class CameraResurrector : MonoBehaviour
{
    public Camera cameraToEnable;

    void Start()
    {
        StartCoroutine(EnableCameraNextFrame());
    }

    System.Collections.IEnumerator EnableCameraNextFrame()
    {
        yield return null; // Wait 1 frame

        if (cameraToEnable != null)
        {
            cameraToEnable.enabled = false;
            yield return null;

            cameraToEnable.enabled = true;
            cameraToEnable.clearFlags = CameraClearFlags.SolidColor;
            cameraToEnable.backgroundColor = Color.black;

            Debug.Log("[CameraResurrector] Camera re-enabled and reset after scene load.");
        }
        else
        {
            Debug.LogError("[CameraResurrector] No camera assigned!");
        }
    }
}