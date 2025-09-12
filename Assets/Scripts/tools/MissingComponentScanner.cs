using UnityEngine;
using UnityEditor; // Needed for editor-only features


public class MissingComponentScanner
{
#if UNITY_EDITOR
    [MenuItem("Tools/Scan For Missing Components")] // Adds a menu item in Unity: Tools - Scan For Missing Components
    static void ScanScene()
    {
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>()) // Scan all GameObjects
        {
            if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave) continue; // Skip hidden/internal
            if (EditorUtility.IsPersistent(obj)) continue; // Skip prefabs in project view

            Component[] components = obj.GetComponents<Component>(); // Get all attached components
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                { // Missing component found  } // Placeholder: no debug output
                }
            }
        }
#endif
    }
}