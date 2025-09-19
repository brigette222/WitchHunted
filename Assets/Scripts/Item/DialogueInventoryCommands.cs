using UnityEngine;
using Yarn.Unity;

public class DialogueInventoryCommands : MonoBehaviour
{
    [Header("Assign your ItemData assets")]
    public ItemData daturaFlower; // Item data for datura
    public ItemData healingMixture; // Item data for healing mixture
    public ItemData breadLoaf; // Item data for bread
    public ItemData smokableHerbs; // Item data for herbs

    private DialogueRunner runner; // Reference to DialogueRunner

    void Awake() // Called once when object is initialized
    {
        runner = FindObjectOfType<DialogueRunner>(); // Auto-assign DialogueRunner
    }

    [YarnCommand("give")] // Makes this method callable from Yarn dialogue
    public void GiveItemFromTrade() // Handles giving an item to inventory
    {
        if (runner == null || runner.VariableStorage == null) return; // Stop if DialogueRunner or storage is missing
        if (!runner.VariableStorage.TryGetValue("$trade_item", out object valueObj)) return; // Stop if trade item not found

        string selectedItem = valueObj as string; // Cast stored value to string
        if (string.IsNullOrEmpty(selectedItem)) return; // Stop if item string is empty

        ItemData itemToGive = null; // Placeholder for resolved item

        switch (selectedItem) // Match trade key to assigned ItemData
        {
            case "datura": itemToGive = daturaFlower; break;
            case "healing": itemToGive = healingMixture; break;
            case "bread": itemToGive = breadLoaf; break;
            case "herbs": itemToGive = smokableHerbs; break;
            default: return; 
        }

        if (itemToGive == null || Inventory.instance == null) return; // Stop if item or inventory missing

        Inventory.instance.AddItem(itemToGive); // Add item to player inventory
        runner.VariableStorage.SetValue("$trade_item", ""); // Reset trade item after giving
    }
}
