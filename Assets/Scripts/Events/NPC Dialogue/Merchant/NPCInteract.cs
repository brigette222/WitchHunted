using UnityEngine;
using Yarn.Unity;

public class NPCInteract : MonoBehaviour
{
    [Header("Dialogue Settings")] // Inspector header for clarity
    public string yarnNodeName = "LostMerchantStart"; // The Yarn dialogue node to start when interacting

    [Header("Detection")] // Inspector header for detection settings
    public float detectionRadius = 1.0f; // How close the player must be to interact
    public LayerMask playerLayer; // Defines what counts as the "player" layer

    private bool playerInRange; // Tracks whether the player is close enough

    void Update() // Called every frame
    {
        // Detect player within range using a 2D overlap circle
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        // Player is considered "in range" if the collider exists and has the Player tag
        playerInRange = hit != null && hit.CompareTag("Player");

        // If player is in range and presses E...
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            DialogueRunner runner = FindObjectOfType<DialogueRunner>(); // Find DialogueRunner in the scene

            // Start dialogue if runner exists and isn't already busy
            if (runner != null && !runner.IsDialogueRunning)
            {
                runner.StartDialogue(yarnNodeName); // Start the specified Yarn dialogue node
            }
        }
    }

    private void OnDrawGizmosSelected() // Draws debug gizmos in the Scene view (editor only)
    {
        Gizmos.color = Color.yellow; // Set gizmo color to yellow
        Gizmos.DrawWireSphere(transform.position, detectionRadius); // Draw a circle showing the detection radius
    }
}
