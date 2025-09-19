using UnityEngine;
using Yarn.Unity;

public class WoundedKnightInteract : MonoBehaviour
{
    public string yarnNodeName = "WoundedStart"; // The Yarn node to start when interacting
    public ItemData healingSalve; // Reference to the healing salve item

    public float detectionRadius = 1.5f; // Distance required to interact
    public LayerMask playerLayer; // LayerMask defining what counts as player

    void Update() // Called every frame
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer); // Check for player in range

        if (hit != null && hit.CompareTag("Player") && Input.GetKeyDown(KeyCode.E)) // If player is in range and presses E
        {
            DialogueRunner runner = FindObjectOfType<DialogueRunner>(); // Find DialogueRunner in scene

            if (runner != null && !runner.IsDialogueRunning) // Ensure runner exists and no dialogue is running
            {
                if (Inventory.instance != null && healingSalve != null) // Check inventory and healing salve reference
                {
                    bool hasSalve = Inventory.instance.HasItems(healingSalve, 1); // Check if player has healing salve
                    runner.VariableStorage.SetValue("$has_salve", hasSalve); // Set Yarn variable for dialogue logic
                }

                runner.StartDialogue(yarnNodeName); // Start Yarn dialogue
            }
        }
    }

    void OnDrawGizmosSelected() // Draws gizmo in editor when object is selected
    {
        Gizmos.color = Color.red; // Set gizmo color to red
        Gizmos.DrawWireSphere(transform.position, detectionRadius); // Draw a circle showing interaction radius
    }
}