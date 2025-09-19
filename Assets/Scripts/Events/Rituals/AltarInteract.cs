using UnityEngine;
using Yarn.Unity;

public class AltarInteract : MonoBehaviour
{
    public string yarnNodeName = "Altar"; // Yarn dialogue node to start when interacting

    [Header("Required Items")]
    public ItemData divinationIncense; // Required ritual item: incense
    public ItemData talismanItem; // Required ritual item: talisman

    public float detectionRadius = 1.0f; // How close player must be to interact
    public LayerMask playerLayer; // Defines which layer counts as player

    void Update() // Called every frame
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer); // Detect player in range

        if (hit != null && hit.CompareTag("Player") && Input.GetKeyDown(KeyCode.E)) // If player is in range and presses E
        {
            DialogueRunner runner = FindObjectOfType<DialogueRunner>(); // Find DialogueRunner in scene
            if (runner != null && !runner.IsDialogueRunning) // Ensure dialogue system is ready
            {
                if (Inventory.instance != null) // If inventory system exists
                {
                    bool hasAllItems = HasAllRequiredItems(); // Check ritual item requirements
                    runner.VariableStorage.SetValue("$has_all_ritual_items", hasAllItems); // Pass result to Yarn variable
                }
                runner.StartDialogue(yarnNodeName); // Start altar dialogue
            }
        }
    }

    private bool HasAllRequiredItems() // Checks if player has required ritual items
    {
        bool hasIncense = Inventory.instance.HasItems(divinationIncense, 1); // Check incense
        bool hasTalisman = Inventory.instance.HasItems(talismanItem, 1); // Check talisman
        return hasIncense && hasTalisman; // Return true only if both items are present
    }

    void OnDrawGizmosSelected() // Draws gizmo in editor when altar is selected
    {
        Gizmos.color = Color.magenta; // Set gizmo color
        Gizmos.DrawWireSphere(transform.position, detectionRadius); // Draw circle for interaction radius
    }
}