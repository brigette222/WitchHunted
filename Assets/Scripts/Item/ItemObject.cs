using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable // Represents a world item that can be picked up
{
    public ItemData item; // Reference to the item’s data (defines its properties)

    public string GetInteractPrompt() // Returns the interaction prompt shown to player
    {
        return "Pickup " + (item != null ? item.displayName : "Unknown Item"); // Show name if available
    }

    public void OnInteract() // Called when the player interacts with the item
    {
        if (item == null) return; // Stop if item data is missing
        if (Inventory.instance == null) return; // Stop if inventory system is missing

        Inventory.instance.AddItem(item); // Add item to the player’s inventory
        Destroy(gameObject); // Remove item from the scene after pickup
    }
}