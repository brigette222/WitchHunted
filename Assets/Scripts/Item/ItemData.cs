using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { Resource, Equipable, Consumable } // Defines general item category
public enum ConsumableType { Hunger, Magik, Health, Stamina } // Defines what a consumable affects

[CreateAssetMenu(fileName = "Item", menuName = "New Item")] // Allows creating new items via Unity's right-click menu
public class ItemData : ScriptableObject
{
    public string id; // Unique identifier for the item (used for saving/trading)

    [Header("Info")]
    public string displayName; // Item name shown in UI
    public string description; // Item description for tooltips
    public ItemType type; // General type of item (resource, equipable, consumable)
    public Sprite icon; // Icon used in UI
    public GameObject dropPrefab; // Prefab dropped in the world when item is discarded

    [Header("Stacking")]
    public bool canStack; // Whether multiple copies stack in inventory
    public int maxStackAmount; // Maximum number of items in a stack

    [Header("Consumable")]
    public ItemDataConsumable[] consumables; // Defines what stats this consumable affects

    [Header("Equipable")]
    public GameObject equipPrefab; // Prefab instantiated when item is equipped

    [Header("Combat Use (Optional)")]
    public CombatAction combatAction; // Links item to a combat action (optional)
}

[System.Serializable]
public class ItemDataConsumable
{
    public ConsumableType type; // The stat this consumable affects
    public float value; // Amount applied to the stat (positive = restore, negative = reduce)
}