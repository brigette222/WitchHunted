using UnityEngine;
using Yarn.Unity;

public class TradeTransactionWatcher : MonoBehaviour
{
    public DialogueRunner runner;
    public PlayerNeeds playerNeeds;

    // Assign your item data here in inspector
    public ItemData daturaFlower;
    public ItemData healingMixture;
    public ItemData breadLoaf;
    public ItemData smokableHerbs;

    private void Start()
    {
        if (runner == null) runner = FindObjectOfType<DialogueRunner>();
        if (playerNeeds == null) playerNeeds = FindObjectOfType<PlayerNeeds>();
    }

    void Update()
    {
        if (runner == null || runner.VariableStorage == null || playerNeeds == null)
            return;

        // Check if the trade is completed
        if (runner.VariableStorage.TryGetValue("$trade_complete", out object tradeCompletedObj) && (bool)tradeCompletedObj)
        {
            // Process payment
            if (runner.VariableStorage.TryGetValue("$trade_payment", out object paymentObj))
            {
                string payment = paymentObj as string;
                if (!string.IsNullOrEmpty(payment))
                {
                    playerNeeds.ApplyTradeCost(payment);
                    Debug.Log($"[TradeTransactionWatcher] Payment '{payment}' applied.");
                }
            }

            // Process item
            if (runner.VariableStorage.TryGetValue("$trade_item", out object itemObj))
            {
                string tradeItem = itemObj as string;
                if (!string.IsNullOrEmpty(tradeItem))
                {
                    GiveItem(tradeItem);
                }
            }

            // Reset variables after trade
            runner.VariableStorage.SetValue("$trade_complete", false);
            runner.VariableStorage.SetValue("$trade_payment", "");
            runner.VariableStorage.SetValue("$trade_item", "");
        }
    }

    void GiveItem(string itemKey)
    {
        ItemData itemToGive = null;

        switch (itemKey)
        {
            case "datura":
                itemToGive = daturaFlower;
                break;
            case "healing":
                itemToGive = healingMixture;
                break;
            case "bread":
                itemToGive = breadLoaf;
                break;
            case "herbs":
                itemToGive = smokableHerbs;
                break;
            default:
                Debug.LogWarning($"[TradeTransactionWatcher] Unknown trade item '{itemKey}'");
                return;
        }

        if (itemToGive != null && Inventory.instance != null)
        {
            Inventory.instance.AddItem(itemToGive);
            Debug.Log($"[TradeTransactionWatcher] Player received item: {itemToGive.displayName}");
        }
        else
        {
            Debug.LogError("[TradeTransactionWatcher] ItemData or Inventory missing!");
        }
    }
}