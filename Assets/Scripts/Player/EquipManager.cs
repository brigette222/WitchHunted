using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquipManager : MonoBehaviour
{
    public Equip curEquip; // Reference to currently equipped item
    public Transform equipParent; // Parent transform where equipped item is attached

    private Player controller; // Reference to the player script

    public static EquipManager instance; // Singleton instance

    void Awake() // Called before Start
    {
        instance = this; // Assign singleton
        controller = GetComponent<Player>(); // Cache Player reference
    }

    public void EquipNew(ItemData item) // Called when equipping a new item
    {
        UnEquip(); // Remove any currently equipped item
        curEquip = Instantiate(item.equipPrefab, equipParent).GetComponent<Equip>(); // Spawn new equipment
    }

    public void UnEquip() // Called when unequipping the current item
    {
        if (curEquip != null) // If something is equipped
        {
            Destroy(curEquip.gameObject); // Destroy the equipped item
            curEquip = null; // Clear reference
        }
    }
}
