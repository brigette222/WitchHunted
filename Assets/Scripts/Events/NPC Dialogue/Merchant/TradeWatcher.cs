using UnityEngine;
using Yarn.Unity;

public class TradeWatcher : MonoBehaviour
{
    public DialogueRunner runner; // Reference to DialogueRunner controlling Yarn variables

    public ItemData daturaFlower; // Item data for datura
    public ItemData healingMixture; // Item data for healing mixture
    public ItemData breadLoaf; // Item data for bread
    public ItemData smokableHerbs; // Item data for herbs

    private string lastProcessedTrade = ""; // Keeps track of the last processed trade to avoid repeats

    void Update() // Called every frame
    {
        if (runner == null || runner.VariableStorage == null) return; // Skip if DialogueRunner or storage is missing

        if (runner.VariableStorage.TryGetValue("$trade_item", out object valueObj)) // Try to read Yarn variable $trade_item
        {
            string currentTrade = valueObj as string; // Convert stored value to string

            if (!string.IsNullOrEmpty(currentTrade) && currentTrade != lastProcessedTrade) // Only process if not empty and not already handled
            {
                GiveItem(currentTrade); // Give item to player
                lastProcessedTrade = currentTrade; // Remember what was processed
                runner.VariableStorage.SetValue("$trade_item", ""); // Reset variable after handling
            }
        }
    }

    void GiveItem(string tradeName) // Handles giving items based on trade key
    {
        ItemData itemToGive = null; // Placeholder for selected item

        switch (tradeName) // Match Yarn trade key to item data
        {
            case "datura": itemToGive = daturaFlower; break; // Select datura
            case "healing": itemToGive = healingMixture; break; // Select healing mixture
            case "bread": itemToGive = breadLoaf; break; // Select bread
            case "herbs": itemToGive = smokableHerbs; break; // Select herbs
        }

        if (itemToGive != null && Inventory.instance != null) // Ensure item and inventory exist
            Inventory.instance.AddItem(itemToGive); // Add item to inventory
    }
}