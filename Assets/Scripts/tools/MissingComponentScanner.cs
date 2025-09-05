using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class MissingComponentScanner : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Scan For Missing Components")]
    static void ScanScene()
    {
        int missingCount = 0;

        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                continue;

            // Skip prefabs in project window
            if (EditorUtility.IsPersistent(obj))
                continue;

            Component[] components = obj.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"[Missing Component] GameObject '{obj.name}' in scene '{obj.scene.name}' has a missing component at index {i}", obj);
                    missingCount++;
                }
            }
        }

        if (missingCount == 0)
            Debug.Log("[Scanner] No missing components found!");
        else
            Debug.Log($"[Scanner] Found {missingCount} missing components.");
    }
#endif
}
