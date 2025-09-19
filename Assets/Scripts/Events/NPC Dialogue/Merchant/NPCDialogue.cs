using UnityEngine;
using Yarn.Unity;

public class NPCDialogue : MonoBehaviour, IInteractable // NPC that starts Yarn dialogue when interacted with
{
    [Header("Yarn Dialogue Settings")] // Inspector header for clarity
    public string yarnNodeName = "MerchantIntro"; // The name of the Yarn dialogue node to start

    public string GetInteractPrompt() // Returns the text shown when player can interact
    {
        return "Talk"; // Simple prompt string
    }

    public void OnInteract() // Called when player interacts with this NPC
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>(); // Find DialogueRunner in the scene
        if (runner != null && !runner.IsDialogueRunning) // Only start dialogue if runner exists and isn't already busy
        {
            runner.StartDialogue(yarnNodeName); // Begin the specified dialogue node
        }
    }
}