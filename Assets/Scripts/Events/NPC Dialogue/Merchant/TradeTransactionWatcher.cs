using UnityEngine;
using Yarn.Unity;

public class TradeTransactionWatcher : MonoBehaviour
{
    public DialogueRunner runner; // Reference to DialogueRunner handling Yarn dialogue
    public PlayerNeeds playerNeeds; // Reference to PlayerNeeds for applying stat costs

    public ItemData daturaFlower; // Item given if trade involves datura
    public ItemData healingMixture; // Item given if trade involves healing potion
    public ItemData breadLoaf; // Item given if trade involves bread
    public ItemData smokableHerbs; // Item given if trade involves herbs

    private void Start() // Called once when the object is initialized
    {
        if (runner == null) runner = FindObjectOfType<DialogueRunner>(); // Auto-assign DialogueRunner if not set
        if (playerNeeds == null) playerNeeds = FindObjectOfType<PlayerNeeds>(); // Auto-assign PlayerNeeds if not set
    }

    void Update() // Called every frame
    {
        if (runner == null || runner.VariableStorage == null || playerNeeds == null) return; // Skip if setup is incomplete

        if (runner.VariableStorage.TryGetValue("$trade_complete", out object tradeCompletedObj) && (bool)tradeCompletedObj) // Check if trade is flagged complete
        {
            if (runner.VariableStorage.TryGetValue("$trade_payment", out object paymentObj)) // Check for trade payment value
            {
                string payment = paymentObj as string; // Cast object to string
                if (!string.IsNullOrEmpty(payment)) playerNeeds.ApplyTradeCost(payment); // Apply payment cost if valid
            }

            if (runner.VariableStorage.TryGetValue("$trade_item", out object itemObj)) // Check for trade item value
            {
                string tradeItem = itemObj as string; // Cast object to string
                if (!string.IsNullOrEmpty(tradeItem)) GiveItem(tradeItem); // Give item if valid
            }

            runner.VariableStorage.SetValue("$trade_complete", false); // Reset trade complete flag
            runner.VariableStorage.SetValue("$trade_payment", ""); // Reset trade payment
            runner.VariableStorage.SetValue("$trade_item", ""); // Reset trade item
        }
    }

    void GiveItem(string itemKey) // Handles giving item to player based on key
    {
        ItemData itemToGive = null; // Placeholder for chosen item

        switch (itemKey) // Match the item key to a defined item
        {
            case "datura": itemToGive = daturaFlower; break; // Select datura flower
            case "healing": itemToGive = healingMixture; break; // Select healing mixture
            case "bread": itemToGive = breadLoaf; break; // Select bread loaf
            case "herbs": itemToGive = smokableHerbs; break; // Select smokable herbs
            default: return; // Unknown item key stop
        }

        if (itemToGive != null && Inventory.instance != null) Inventory.instance.AddItem(itemToGive); // Add item to inventory if valid
    }
}