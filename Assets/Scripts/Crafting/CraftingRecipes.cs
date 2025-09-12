using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Crafting Recipe", menuName = "New Crafting Recipe")] // Allows creating recipes via Unity’s Create menu
public class CraftingRecipe : ScriptableObject 
{
    public ItemData itemToCraft; // The resulting item this recipe produces
    public ResourceCost[] cost;  // The resources required to craft it
}

[System.Serializable] // Makes the class visible in the Inspector
public class ResourceCost
{
    public ItemData item;  // Specific resource/item required (e.g., wood, iron)
    public int quantity;   // How many units of that resource are needed
}