using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingWindow : MonoBehaviour
{
    public CraftingRecipeUI[] recipeUIs; // Array of recipe UI slots
    public static CraftingWindow instance; // Singleton instance

    void Awake() { instance = this; } // Assign singleton reference

    public void ShowCraftingUI() { gameObject.SetActive(true); } // Show window
    public void HideCraftingUI() { gameObject.SetActive(false); } // Hide window

    void OnEnable() { Inventory.instance.onOpenInventory.AddListener(OnOpenInventory); } // Listen for inventory open
    void OnDisable() { Inventory.instance.onOpenInventory.RemoveListener(OnOpenInventory); } // Stop listening

    void OnOpenInventory() { gameObject.SetActive(false); } // Close crafting if inventory opens

    // Called when we click on a crafting recipe to craft it
    public void Craft(CraftingRecipe recipe)
    {
        if (recipe == null || Inventory.instance == null || recipe.itemToCraft == null) return; // Safety checks

        // Remove required resources
        for (int i = 0; i < recipe.cost.Length; i++)
        {
            if (recipe.cost[i] == null || recipe.cost[i].item == null) continue; // Skip invalid cost entries

            for (int x = 0; x < recipe.cost[i].quantity; x++)
            {
                Inventory.instance.RemoveItem(recipe.cost[i].item); // Remove each required item
            }
        }

        Inventory.instance.AddItem(recipe.itemToCraft); // Add crafted item to inventory
        StartCoroutine(RefreshUIAfterDelay()); // Refresh UI after inventory updates
    }

    IEnumerator RefreshUIAfterDelay()
    {
        yield return null; // Wait 1 frame to ensure inventory is updated
        for (int i = 0; i < recipeUIs.Length; i++)
        {
            recipeUIs[i].UpdateCanCraft(); // Refresh each recipe’s UI
        }
    }
}