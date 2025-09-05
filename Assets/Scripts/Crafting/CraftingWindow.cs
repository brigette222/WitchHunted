using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingWindow : MonoBehaviour
{
    public CraftingRecipeUI[] recipeUIs;

    public static CraftingWindow instance;

    void Awake()
    {
        instance = this;
    }

    public void ShowCraftingUI()
    {
        gameObject.SetActive(true);
    }

    public void HideCraftingUI()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        Inventory.instance.onOpenInventory.AddListener(OnOpenInventory);
    }

    void OnDisable()
    {
        Inventory.instance.onOpenInventory.RemoveListener(OnOpenInventory);
    }

    void OnOpenInventory()
    {
        gameObject.SetActive(false);
    }

    // called when we click on a crafting recipe to craft it
    public void Craft(CraftingRecipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogError("[CraftingWindow] ? Recipe is NULL.");
            return;
        }

        Debug.Log($"[CraftingWindow] ?? Attempting to craft: {recipe.name}");

        // Safety check on cost array
        if (recipe.cost == null || recipe.cost.Length == 0)
        {
            Debug.LogWarning("[CraftingWindow] ?? Recipe has no cost assigned!");
        }

        // Remove ingredients
        for (int i = 0; i < recipe.cost.Length; i++)
        {
            if (recipe.cost[i] == null || recipe.cost[i].item == null)
            {
                Debug.LogError($"[CraftingWindow] ? Missing cost item at index {i}.");
                continue;
            }

            Debug.Log($"[CraftingWindow] ? Removing {recipe.cost[i].quantity}x {recipe.cost[i].item.displayName}");

            for (int x = 0; x < recipe.cost[i].quantity; x++)
            {
                Inventory.instance.RemoveItem(recipe.cost[i].item);
            }
        }

        if (Inventory.instance == null)
        {
            Debug.LogError("[CraftingWindow] ? Inventory.instance is NULL!");
            return;
        }

        if (recipe.itemToCraft == null)
        {
            Debug.LogError("[CraftingWindow] ? itemToCraft is NULL in recipe: " + recipe.name);
            return;
        }

        Debug.Log($"[CraftingWindow] ? Adding crafted item: {recipe.itemToCraft.displayName}");
        Inventory.instance.AddItem(recipe.itemToCraft);

        // ? FIXED: update UI AFTER crafting and inventory change
        StartCoroutine(RefreshUIAfterDelay());

        Debug.Log("[CraftingWindow] ?? Crafting complete!");
    }

    // ? Delayed UI refresh ensures inventory state is accurate
    IEnumerator RefreshUIAfterDelay()
    {
        yield return null; // wait 1 frame
        for (int i = 0; i < recipeUIs.Length; i++)
        {
            recipeUIs[i].UpdateCanCraft();
        }
    }
}