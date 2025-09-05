using UnityEngine;
using Yarn.Unity;

public class NPCInteract : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public string yarnNodeName = "LostMerchantStart"; // Make sure this matches your .yarn node

    [Header("Detection")]
    public float detectionRadius = 1.0f;
    public LayerMask playerLayer;

    private bool playerInRange;

    void Update()
    {
        // Detect player within range using OverlapCircle
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        playerInRange = hit != null && hit.CompareTag("Player");

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("NPCInteract: Player pressed E near NPC");

            DialogueRunner runner = FindObjectOfType<DialogueRunner>();
            if (runner != null && !runner.IsDialogueRunning)
            {
                runner.StartDialogue(yarnNodeName);
                Debug.Log("NPCInteract: Starting dialogue node ? " + yarnNodeName);
            }
            else if (runner == null)
            {
                Debug.LogWarning("NPCInteract: No DialogueRunner found in scene.");
            }
            else
            {
                Debug.Log("NPCInteract: Dialogue is already running.");
            }
        }
    }

    // Draw radius in Scene view for easier placement
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
