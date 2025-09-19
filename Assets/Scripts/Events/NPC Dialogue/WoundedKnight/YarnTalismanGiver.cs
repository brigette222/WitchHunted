using UnityEngine; // Unity core namespace
using Yarn.Unity; // Required for DialogueRunner and Yarn integration

public class YarnTalismanGiver : MonoBehaviour
{
    public DialogueRunner runner; // Reference to Yarn's DialogueRunner

    [Header("Reward Item")] // Inspector header
    public ItemData forestTalisman; // Item given as a reward (forest talisman)

    private string lastProcessedItem = ""; // Stores last processed reward to prevent duplicates

    void Start() // Called once when the object is initialized
    {
        if (runner == null) runner = FindObjectOfType<DialogueRunner>(); // Auto-assign DialogueRunner if not set
    }

    void Update() // Called every frame
    {
        if (runner == null || runner.VariableStorage == null) return; // Skip if DialogueRunner or storage is missing

        if (runner.VariableStorage.TryGetValue("$knight_reward", out object valueObj)) // Check for Yarn variable $knight_reward
        {
            string currentItem = valueObj as string; // Cast object to string
            if (!string.IsNullOrEmpty(currentItem) && currentItem != lastProcessedItem) // Process only if not empty and new
            {
                GiveItem(currentItem); // Give item reward
                lastProcessedItem = currentItem; // Remember the last item to prevent duplicates
                runner.VariableStorage.SetValue("$knight_reward", ""); // Reset Yarn variable
            }
        }
    }

    void GiveItem(string itemKey) // Gives item to player based on key
    {
        ItemData itemToGive = null; // Placeholder for reward item
        switch (itemKey) // Match reward key
        {
            case "Talisman": itemToGive = forestTalisman; break; // Assign forest talisman
        }
        if (itemToGive != null && Inventory.instance != null) Inventory.instance.AddItem(itemToGive); // Add item if valid
    }
}