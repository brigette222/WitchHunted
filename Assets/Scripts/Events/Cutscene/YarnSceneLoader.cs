using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class YarnSceneLoader : MonoBehaviour
{
    public string sceneToLoad = "Intro"; // Set in Inspector
    public float delayBeforeLoad = 2f;

    public void LoadSceneAfterDelay()
    {
        StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}