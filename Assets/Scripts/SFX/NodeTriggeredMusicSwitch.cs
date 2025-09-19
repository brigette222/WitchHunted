using UnityEngine;
using Yarn.Unity;

public class NodeTriggeredMusicSwitch : MonoBehaviour
{
    [Header("Music Switch Settings")]
    public string targetNodeName; // Node to listen for
    public AudioSource newTrackToPlay; // Track to switch to when node completes

    private void Awake() // Called when object is created
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>(); // Find DialogueRunner
        if (runner != null)
        {
            runner.onNodeComplete.AddListener(HandleNodeEnd); // Listen for node completion
        }
    }

    private void HandleNodeEnd(string nodeName) // Called when a node finishes
    {
        if (nodeName == targetNodeName) // If it's the target node
        {
            MusicManager mm = FindObjectOfType<MusicManager>(); // Find MusicManager
            if (mm != null && newTrackToPlay != null) // Ensure references exist
            {
                mm.SwitchTo(newTrackToPlay); // Switch music
            }
        }
    }
}
