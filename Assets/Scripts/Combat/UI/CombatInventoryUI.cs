using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;




//currently under construction/ not being used in this version of the game (just for the time being)

public class CombatInventoryUI : MonoBehaviour
{
    // === References to UI elements (assign in Inspector) ===
    public GameObject combatItemList;         // Container that holds the item list UI
    public Transform contentParent;           // Parent object for item buttons (usually a ScrollView's Content)
    public GameObject buttonPrefab;           // Prefab for each inventory button (linked to a CombatAction)

    // === Internal references ===
    private Inventory inventory;              // Reference to player's inventory
    private PlayerNeeds playerNeeds;          // Reference to player stats (used when consuming items)

    void Start()
    {
        // Get reference to the Inventory singleton
        inventory = Inventory.instance;

        // Find the PlayerNeeds component in the scene
        playerNeeds = FindObjectOfType<PlayerNeeds>();

        // Sanity check — ensure required references are present
        if (combatItemList != null)
            combatItemList.SetActive(false); // Hide inventory UI at start
    }

    // === This is triggered externally by CombatActionUI to show the item list ===
    public void PopulateItemList()
    {
        // Safety check: don't proceed if UI root is missing
        if (combatItemList == null) return;

        // Clear all previously created item buttons
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Iterate through each slot in the inventory
        foreach (ItemSlot slot in inventory.slots)
        {
            // Only show consumable items that are present in inventory
            if (slot.item != null && slot.item.type == ItemType.Consumable)
            {
                // Create a new button for this item
                GameObject buttonObj = Instantiate(buttonPrefab, contentParent);

                // Set the button's text (e.g., "Potion (x2)")
                buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = $"{slot.item.displayName} ({slot.quantity})";

                // Add functionality to the button if a CombatAction is linked
                Button button = buttonObj.GetComponent<Button>();

                // Retrieve the CombatAction associated with this item
                CombatAction combatAction = CombatActionDatabase.Instance.GetCombatActionForItem(slot.item);

                // Only hook up the button if the action is valid
                if (combatAction != null)
                {
                    // Add the listener for when the button is clicked
                    button.onClick.AddListener(() => UseItem(slot, combatAction));
                }
            }
        }
    }

    // === Called when the player clicks an item button ===
    void UseItem(ItemSlot slot, CombatAction combatAction)
    {
        // Safety: ensure a valid character is currently taking a turn
        Character player = TurnManager.Instance.CurrentCharacter;
        if (player == null) return;

        // Have the player execute the item’s combat action
        player.CastCombatAction(combatAction);

        // Remove the used item from inventory
        inventory.RemoveItem(slot.item);

        // Refresh the UI list to reflect updated item count
        PopulateItemList();
    }
}