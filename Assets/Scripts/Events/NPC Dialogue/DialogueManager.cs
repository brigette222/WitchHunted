using UnityEngine;
using Yarn.Unity;

public class DialogueManager : MonoBehaviour
{
    public static bool IsDialogueActive { get; private set; } // Tracks if dialogue is currently running (accessible globally)

    private void Awake() // Called when this object is first created
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>(); // Find the DialogueRunner in the scene
        if (runner != null) // If found, subscribe to its events
        {
            runner.onDialogueStart.AddListener(OnDialogueStart); // Hook into start of dialogue
            runner.onDialogueComplete.AddListener(OnDialogueComplete); // Hook into end of dialogue
        }
        else
        {
            Debug.LogError("[DialogueManager] DialogueRunner not found in scene!"); // Error if DialogueRunner missing
        }
    }

    private void OnDialogueStart() // Called when dialogue begins
    {
        IsDialogueActive = true; // Mark dialogue as active
    }

    private void OnDialogueComplete() // Called when dialogue finishes
    {
        IsDialogueActive = false; // Mark dialogue as inactive
    }
}