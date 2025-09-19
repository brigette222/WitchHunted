using UnityEngine;
using Yarn.Unity;


public class KnightSalveRemover : MonoBehaviour
{
    public ItemData healingSalve; // Reference to the healing salve item

    private DialogueRunner runner; // Reference to Yarn's DialogueRunner

    void Start() // Called once when the object is initialized
    {
        runner = FindObjectOfType<DialogueRunner>(); // Auto-assign DialogueRunner in the scene
    }

    void Update() // Called every frame
    {
        if (runner != null && // Ensure DialogueRunner exists
            runner.VariableStorage != null && // Ensure Yarn variable storage exists
            runner.VariableStorage.TryGetValue("$remove_healing_salve", out bool shouldRemove) && // Check if variable exists
            shouldRemove) // Continue only if the flag is true
        {
            if (Inventory.instance.HasItems(healingSalve, 1)) // If player has at least one healing salve
                Inventory.instance.RemoveItem(healingSalve); // Remove it from inventory

            runner.VariableStorage.SetValue("$remove_healing_salve", false); // Reset flag to prevent repeated removal
        }
    }
}