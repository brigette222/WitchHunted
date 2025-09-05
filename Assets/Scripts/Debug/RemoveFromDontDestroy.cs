using UnityEngine;
using UnityEngine.SceneManagement;

public class RemoveFromDontDestroy : MonoBehaviour
{
    void Awake()
    {
        // Find the root scene (the first loaded scene)
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        Debug.Log("[RemoveFromDontDestroy] Forced player into active scene.");
    }
}