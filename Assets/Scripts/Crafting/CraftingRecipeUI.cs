using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeUI : MonoBehaviour
{
    public CraftingRecipe recipe; // The recipe this UI element represents
    public Image backgroundImage; // Background color changes if craftable or not
    public Image icon; // Icon of the crafted item
    public TextMeshProUGUI itemName; // Name of the crafted item
    public Image[] resourceCosts; // Array of icons showing required resources

    public Color canCraftColor; // Background color when craftable
    public Color cannotCraftColor; // Background color when not craftable
    private bool canCraft; // Tracks if recipe can currently be crafted

    void OnEnable() { UpdateCanCraft(); } // Check crafting status when UI is enabled

    public void UpdateCanCraft() // Updates UI to show if crafting is possible
    {
        canCraft = true; // if crafting is possile, check requirements

        for (int i = 0; i < recipe.cost.Length; i++) // Loop through required resources
        {
            if (!Inventory.instance.HasItems(recipe.cost[i].item, recipe.cost[i].quantity)) // If missing any
            {
                canCraft = false; // Mark as not craftable
                break; // Stop checking further
            }
        }

        backgroundImage.color = canCraft ? canCraftColor : cannotCraftColor; // Set background color accordingly
    }

    void Start()
    {
        icon.sprite = recipe.itemToCraft.icon; // Show item icon
        itemName.text = recipe.itemToCraft.displayName; // Show item name

        for (int i = 0; i < resourceCosts.Length; i++) // Loop through UI slots for resources
        {
            if (i < recipe.cost.Length) // If this slot corresponds to a cost
            {
                resourceCosts[i].gameObject.SetActive(true); // Show it
                resourceCosts[i].sprite = recipe.cost[i].item.icon; // Set resource icon
                resourceCosts[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = recipe.cost[i].quantity.ToString(); // Show amount
            }
            else
                resourceCosts[i].gameObject.SetActive(false); // Hide unused slots
        }
    }

    public void OnClickButton() // Called when player clicks the craft button
    {
        UpdateCanCraft(); // Re-check resources before crafting

        if (canCraft) // If still valid
        {
            CraftingWindow.instance.Craft(recipe); // Perform crafting
            UpdateCanCraft(); // Refresh UI in case resources ran out
        }
    }
}
