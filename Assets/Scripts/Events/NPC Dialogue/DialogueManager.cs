using UnityEngine;
using Yarn.Unity;

public class DialogueManager : MonoBehaviour
{
    public static bool IsDialogueActive { get; private set; }

    private void Awake()
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>();
        if (runner != null)
        {
            runner.onDialogueStart.AddListener(OnDialogueStart);
            runner.onDialogueComplete.AddListener(OnDialogueComplete);
        }
        else
        {
            Debug.LogError("[DialogueManager] DialogueRunner not found in scene!");
        }
    }

    private void OnDialogueStart()
    {
        IsDialogueActive = true;
        Debug.Log("[DialogueManager] Dialogue started.");
    }

    private void OnDialogueComplete()
    {
        IsDialogueActive = false;
        Debug.Log("[DialogueManager] Dialogue ended.");
    }
}