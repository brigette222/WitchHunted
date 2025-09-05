using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class RitualNodeSceneLoader : MonoBehaviour
{
    [Header("Yarn Settings")]
    [SerializeField] private DialogueRunner runner; // If not assigned, auto-find
    public string targetNode = "Ritual_Start"; // Node to listen for

    [Header("Scene Settings")]
    public string sceneToLoad = "GameOver"; // Scene to load (TEMP)

    public float delayBeforeLoad = 1.5f;

    private void Awake()
    {
        Debug.Log("[RitualNodeSceneLoader] Awake called.");

        if (runner == null)
        {
            runner = FindObjectOfType<DialogueRunner>();
            if (runner != null)
            {
                Debug.Log("[RitualNodeSceneLoader] Auto-assigned DialogueRunner.");
            }
            else
            {
                Debug.LogError("[RitualNodeSceneLoader] ERROR: DialogueRunner not found in scene!");
            }
        }

        if (runner != null)
        {
            runner.onNodeComplete.AddListener(OnNodeComplete);
            Debug.Log("[RitualNodeSceneLoader] Subscribed to runner.onNodeComplete.");
        }
    }

    private void OnDestroy()
    {
        if (runner != null)
        {
            runner.onNodeComplete.RemoveListener(OnNodeComplete);
            Debug.Log("[RitualNodeSceneLoader] Unsubscribed from runner.onNodeComplete.");
        }
    }

    private void OnNodeComplete(string completedNode)
    {
        Debug.Log($"[RitualNodeSceneLoader] Node completed: {completedNode}");

        if (completedNode == targetNode)
        {
            Debug.Log($"[RitualNodeSceneLoader] Target node '{targetNode}' matched. Starting DelayedLoad.");
            StartCoroutine(DelayedLoad());
        }
        else
        {
            Debug.Log($"[RitualNodeSceneLoader] Node '{completedNode}' completed but did not match target '{targetNode}'. No action taken.");
        }
    }

    private System.Collections.IEnumerator DelayedLoad()
    {
        Debug.Log($"[RitualNodeSceneLoader] DelayedLoad started. Waiting {delayBeforeLoad} seconds...");
        yield return new WaitForSeconds(delayBeforeLoad);

        Debug.Log($"[RitualNodeSceneLoader] Attempting to load scene: {sceneToLoad}");

        if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.Log($"[RitualNodeSceneLoader] Scene '{sceneToLoad}' found. Loading now...");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError($"[RitualNodeSceneLoader] ERROR: Scene '{sceneToLoad}' cannot be loaded! Check Build Settings.");
        }
    }
}