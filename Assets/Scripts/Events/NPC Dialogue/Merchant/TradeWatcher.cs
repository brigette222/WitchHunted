using UnityEngine;
using Yarn.Unity;

public class TradeWatcher : MonoBehaviour
{
    public DialogueRunner runner;

    public ItemData daturaFlower;
    public ItemData healingMixture;
    public ItemData breadLoaf;
    public ItemData smokableHerbs;

    private string lastProcessedTrade = "";

    void Update()
    {
        if (runner == null || runner.VariableStorage == null)
            return;

        // Get the current trade item value
        if (runner.VariableStorage.TryGetValue("$trade_item", out object valueObj))
        {
            string currentTrade = valueObj as string;

            if (!string.IsNullOrEmpty(currentTrade) && currentTrade != lastProcessedTrade)
            {
                Debug.Log($"?? TradeWatcher detected trade item: {currentTrade}");

                GiveItem(currentTrade);
                lastProcessedTrade = currentTrade;

                // Reset the trade_item variable
                runner.VariableStorage.SetValue("$trade_item", "");
            }
        }
    }

    void GiveItem(string tradeName)
    {
        ItemData itemToGive = null;

        switch (tradeName)
        {
            case "datura": itemToGive = daturaFlower; break;
            case "healing": itemToGive = healingMixture; break;
            case "bread": itemToGive = breadLoaf; break;
            case "herbs": itemToGive = smokableHerbs; break;
        }

        if (itemToGive == null)
        {
            Debug.LogError($"? No ItemData assigned for: {tradeName}");
            return;
        }

        if (Inventory.instance == null)
        {
            Debug.LogError("? Inventory.instance not found!");
            return;
        }

        Inventory.instance.AddItem(itemToGive);
        Debug.Log($"? {itemToGive.displayName} added to inventory via TradeWatcher");
    }
}