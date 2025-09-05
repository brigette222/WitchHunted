using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatInventoryUI : MonoBehaviour
{
    public GameObject combatItemList; // Assign "CombatItemList" in the Inspector
    public Transform contentParent;   // Assign the "Content" object inside the Scroll View
    public GameObject buttonPrefab;   // Assign the CombatInventoryButton prefab in the Inspector

    private Inventory inventory;
    private PlayerNeeds playerNeeds;

    void Start()
    {
        inventory = Inventory.instance;
        playerNeeds = FindObjectOfType<PlayerNeeds>();

        if (inventory == null)
        {
            Debug.LogError("[CombatInventoryUI] Inventory instance not found!");
        }

        if (playerNeeds == null)
        {
            Debug.LogError("[CombatInventoryUI] PlayerNeeds component not found!");
        }

        if (combatItemList == null)
        {
            Debug.LogError("[CombatInventoryUI] ERROR: CombatItemList is NULL! Make sure it's assigned in the Inspector.");
        }
        else
        {
            combatItemList.SetActive(false); // Hide at start
        }
    }

    // **Directly toggled by CombatActionUI**
    public void PopulateItemList()
    {
        if (combatItemList == null)
        {
            Debug.LogError("[CombatInventoryUI] ERROR: CombatItemList is NULL! Can't populate items.");
            return;
        }

        // Clear previous buttons
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"[CombatInventoryUI] Populating item list... Inventory slots: {inventory.slots.Length}");

        // Get player's items
        foreach (ItemSlot slot in inventory.slots)
        {
            if (slot.item != null && slot.item.type == ItemType.Consumable)
            {
                Debug.Log($"[CombatInventoryUI] Found item: {slot.item.displayName} (x{slot.quantity})");

                GameObject buttonObj = Instantiate(buttonPrefab, contentParent);
                buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = $"{slot.item.displayName} ({slot.quantity})";

                Button button = buttonObj.GetComponent<Button>();

                // **Check if the item has a matching combat action**
                CombatAction combatAction = CombatActionDatabase.Instance.GetCombatActionForItem(slot.item);
                if (combatAction == null)
                {
                    Debug.LogWarning($"[CombatInventoryUI] No CombatAction found for {slot.item.displayName}! Button will not work.");
                }
                else
                {
                    Debug.Log($"[CombatInventoryUI] Assigning CombatAction '{combatAction.DisplayName}' to {slot.item.displayName} button.");
                    button.onClick.AddListener(() => UseItem(slot, combatAction));
                }
            }
        }
    }

    void UseItem(ItemSlot slot, CombatAction combatAction)
    {
        Debug.Log($"[CombatInventoryUI] Using item: {slot.item.displayName}, linked to action: {combatAction.DisplayName}");

        // **Ensure the player exists**
        Character player = TurnManager.Instance.CurrentCharacter;
        if (player == null)
        {
            Debug.LogError("[CombatInventoryUI] ERROR: No CurrentCharacter found in TurnManager!");
            return;
        }

        // **Tell the player to use the combat action**
        player.CastCombatAction(combatAction);

        // **Remove item from inventory**
        inventory.RemoveItem(slot.item);
        PopulateItemList(); // Refresh UI

        Debug.Log($"[CombatInventoryUI] {slot.item.displayName} used, UI updated.");
    }
}