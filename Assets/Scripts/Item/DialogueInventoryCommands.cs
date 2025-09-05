using UnityEngine;
using Yarn.Unity;

public class DialogueInventoryCommands : MonoBehaviour
{
    [Header("Assign your ItemData assets")]
    public ItemData daturaFlower;
    public ItemData healingMixture;
    public ItemData breadLoaf;
    public ItemData smokableHerbs;

    private DialogueRunner runner;

    void Awake()
    {
        runner = FindObjectOfType<DialogueRunner>();
        if (runner == null)
        {
            Debug.LogError("? DialogueRunner not found in the scene.");
        }
    }

    [YarnCommand("give")]
    public void GiveItemFromTrade()
    {
        Debug.Log("?? [GIVE] Dialogue command triggered");

        if (runner == null || runner.VariableStorage == null)
        {
            Debug.LogError("? DialogueRunner or VariableStorage is null.");
            return;
        }

        if (!runner.VariableStorage.TryGetValue("$trade_item", out object valueObj))
        {
            Debug.LogError("? Could not retrieve $trade_item from VariableStorage.");
            return;
        }

        string selectedItem = valueObj as string;

        Debug.Log($"?? Retrieved $trade_item: {selectedItem}");

        if (string.IsNullOrEmpty(selectedItem))
        {
            Debug.LogWarning("?? No trade item selected (empty string).");
            return;
        }

        ItemData itemToGive = null;

        switch (selectedItem)
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
                Debug.LogError("? Unknown trade item: " + selectedItem);
                return;
        }

        if (itemToGive == null)
        {
            Debug.LogError($"? No ItemData assigned for trade item '{selectedItem}'. Check the Unity inspector!");
            return;
        }

        if (Inventory.instance == null)
        {
            Debug.LogError("? Inventory.instance is null! Make sure the Player GameObject has the Inventory component.");
            return;
        }

        Debug.Log($"? Giving item to inventory: {itemToGive.displayName}");
        Inventory.instance.AddItem(itemToGive);

        runner.VariableStorage.SetValue("$trade_item", "");

        Debug.Log("? Trade item given and $trade_item reset.");
    }
}
