using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class RitualNodeSceneLoader : MonoBehaviour
{
    [Header("Yarn Settings")]
    [SerializeField] private DialogueRunner runner; // DialogueRunner reference (auto-assigned if null)
    public string targetNode = "Ritual_Start"; // Yarn node name to listen for

    [Header("Scene Settings")]
    public string sceneToLoad = "GameOver"; // Scene to load when ritual completes
    public float delayBeforeLoad = 1.5f; // Delay before loading scene

    private void Awake() // Called when object is initialized
    {
        if (runner == null) runner = FindObjectOfType<DialogueRunner>(); // Auto-find DialogueRunner if not set
        if (runner != null) runner.onNodeComplete.AddListener(OnNodeComplete); // Subscribe to node completion event
    }

    private void OnDestroy() // Called when object is destroyed
    {
        if (runner != null) runner.onNodeComplete.RemoveListener(OnNodeComplete); // Unsubscribe from node completion
    }

    private void OnNodeComplete(string completedNode) // Called when Yarn finishes a node
    {
        if (completedNode == targetNode) StartCoroutine(DelayedLoad()); // If target node matches, start scene load
    }

    private System.Collections.IEnumerator DelayedLoad() // Coroutine for scene load delay
    {
        yield return new WaitForSeconds(delayBeforeLoad); // Wait for set time before loading
        if (Application.CanStreamedLevelBeLoaded(sceneToLoad)) SceneManager.LoadScene(sceneToLoad); // Load scene if valid
    }
}