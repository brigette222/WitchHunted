using System.Collections;
using UnityEngine;

public class DialogueEnabler : MonoBehaviour
{
    public GameObject dialogueSystem; // Assign this in the Inspector

    void Start()
    {
        StartCoroutine(EnableDialogueAfterDelay());
    }

    IEnumerator EnableDialogueAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // Adjust the delay as needed
        if (dialogueSystem != null)
        {
            dialogueSystem.SetActive(true);
            Debug.Log("[DialogueEnabler] Dialogue system enabled after delay.");
        }
        else
        {
            Debug.LogWarning("[DialogueEnabler] Dialogue system not assigned.");
        }
    }
}