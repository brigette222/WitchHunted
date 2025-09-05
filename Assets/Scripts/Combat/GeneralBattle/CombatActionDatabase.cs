using System.Collections.Generic;
using UnityEngine;

public class CombatActionDatabase : MonoBehaviour
{
    public static CombatActionDatabase Instance; // Singleton for easy access

    [System.Serializable]
    public struct ItemActionPair
    {
        public ItemData item;
        public CombatAction combatAction;
    }

    public List<ItemActionPair> itemActionPairs = new List<ItemActionPair>(); // Holds all possible items and actions

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // **Find the corresponding CombatAction for an Item**
    public CombatAction GetCombatActionForItem(ItemData item)
    {
        foreach (var pair in itemActionPairs)
        {
            if (pair.item == item)
                return pair.combatAction;
        }
        return null; // Return null if no match is found
    }
}