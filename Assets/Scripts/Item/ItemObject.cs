using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemData item;

    public string GetInteractPrompt()
    {
        return "Pickup " + (item != null ? item.displayName : "Unknown Item");
    }

    public void OnInteract()
    {
        Debug.Log("OnInteract() called for: " + gameObject.name);

        if (item == null)
        {
            Debug.LogError(" ERROR: ItemObject is missing ItemData! Cannot pick up.");
            return;
        }

        if (Inventory.instance == null)
        {
            Debug.LogError(" ERROR: Inventory instance not found! Cannot add item.");
            return;
        }

        Debug.Log(" Attempting to pick up: " + item.displayName);

        Inventory.instance.AddItem(item);

        Debug.Log(" Item successfully added to inventory: " + item.displayName);

        Destroy(gameObject);
    }
}