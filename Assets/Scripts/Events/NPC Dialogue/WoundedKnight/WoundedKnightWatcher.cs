using UnityEngine;
using Yarn.Unity;

public class WoundedKnightWatcher : MonoBehaviour
{
    public DialogueRunner runner; // Reference to Yarn's DialogueRunner
    public ItemData healingSalve; // Reference to the healing salve item
    public ItemData severedHead; // Reference to the severed head item

    private void Start() // Called once when the object is initialized
    {
        if (runner == null) runner = FindObjectOfType<DialogueRunner>(); // Auto-assign DialogueRunner if not set
    }

    private void Update() // Called every frame
    {
        if (runner == null || runner.VariableStorage == null) return; // Skip if DialogueRunner or storage is missing

        if (runner.VariableStorage.TryGetValue("$remove_healing_salve", out object removeObj) && (bool)removeObj) // Check if healing salve should be removed
        {
            if (Inventory.instance != null && healingSalve != null) Inventory.instance.RemoveItem(healingSalve); // Remove healing salve if possible
            runner.VariableStorage.SetValue("$remove_healing_salve", false); // Reset flag after handling
        }

        if (runner.VariableStorage.TryGetValue("$take_head", out object takeHeadObj) && (bool)takeHeadObj) // Check if severed head should be added
        {
            if (Inventory.instance != null && severedHead != null) Inventory.instance.AddItem(severedHead); // Add severed head if possible
            runner.VariableStorage.SetValue("$take_head", false); // Reset flag after handling
        }
    }
}