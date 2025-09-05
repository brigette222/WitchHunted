using UnityEngine;
using Yarn.Unity;

public class YarnTalismanGiver : MonoBehaviour
{
    public DialogueRunner runner;

    [Header("Reward Item")]
    public ItemData forestTalisman;

    private string lastProcessedItem = "";

    void Start()
    {
        if (runner == null)
        {
            runner = FindObjectOfType<DialogueRunner>();
            if (runner == null)
            {
                Debug.LogError("[YarnTalismanGiver] DialogueRunner not found in scene!");
            }
        }
    }

    void Update()
    {
        if (runner == null || runner.VariableStorage == null)
            return;

        if (runner.VariableStorage.TryGetValue("$knight_reward", out object valueObj))
        {
            string currentItem = valueObj as string;

            if (!string.IsNullOrEmpty(currentItem) && currentItem != lastProcessedItem)
            {
                Debug.Log($"[YarnTalismanGiver] Detected knight reward key: {currentItem}");

                GiveItem(currentItem);
                lastProcessedItem = currentItem;

                // Clear the variable so it doesn't repeat
                runner.VariableStorage.SetValue("$knight_reward", "");
            }
        }
    }

    void GiveItem(string itemKey)
    {
        ItemData itemToGive = null;

        switch (itemKey)
        {
            case "Talisman":
                itemToGive = forestTalisman;
                break;
        }

        if (itemToGive == null)
        {
            Debug.LogError($"[YarnTalismanGiver] No ItemData assigned for key: {itemKey}");
            return;
        }

        if (Inventory.instance == null)
        {
            Debug.LogError("[YarnTalismanGiver] Inventory.instance not found!");
            return;
        }

        Inventory.instance.AddItem(itemToGive);
        Debug.Log($"[YarnTalismanGiver] Gave {itemToGive.displayName} to player.");
    }
}