using UnityEngine;
using Yarn.Unity;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Header("Yarn Dialogue Settings")]
    public string yarnNodeName = "MerchantIntro"; // Set this to your dialogue node name

    public string GetInteractPrompt()
    {
        return "Talk";
    }

    public void OnInteract()
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>();
        if (runner != null && !runner.IsDialogueRunning)
        {
            runner.StartDialogue(yarnNodeName);
        }
        else
        {
            Debug.LogWarning("DialogueRunner not found or already running.");
        }
    }
}