using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class YarnSceneLoader : MonoBehaviour
{
    public string sceneToLoad = "Intro"; // Name of the scene to load (set in Inspector)
    public float delayBeforeLoad = 2f; // Time (seconds) to wait before loading

    public void LoadSceneAfterDelay() // Public method to trigger delayed scene load
    {
        StartCoroutine(DelayedLoad()); // Start coroutine that waits before loading
    }

    private IEnumerator DelayedLoad() // Coroutine for waiting then loading
    {
        yield return new WaitForSeconds(delayBeforeLoad); // Pause for set delay
        SceneManager.LoadScene(sceneToLoad); // Load the specified scene
    }
}