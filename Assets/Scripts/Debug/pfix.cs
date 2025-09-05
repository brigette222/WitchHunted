using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerResetter : MonoBehaviour
{
    void Start()
    {
        Player original = FindObjectOfType<Player>();
        if (original == null)
        {
            Debug.LogWarning("[PlayerResetter] No Player found.");
            return;
        }

        // If it's not in the active scene, force clone and delete original
        if (original.gameObject.scene.name == "DontDestroyOnLoad")
        {
            Debug.Log("[PlayerResetter] Player is in DontDestroyOnLoad — cloning into active scene.");

            GameObject clone = Instantiate(original.gameObject, Vector3.zero, Quaternion.identity);
            clone.name = original.gameObject.name;

            Destroy(original.gameObject); // destroy DontDestroy version
        }
        else
        {
            Debug.Log("[PlayerResetter] Player is in the correct scene.");
        }
    }
}