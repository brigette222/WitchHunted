using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory instance; // Singleton instance

    public ItemSlotUI[] uiSlots; // UI slots representing inventory
    public ItemSlot[] slots; // Data slots storing items

    public GameObject inventoryWindow; // UI window reference
    public Transform dropPosition; // Where dropped items appear in world

    [Header("Selected Item")]
    private ItemSlot selectedItem; // Currently selected slot
    private int selectedItemIndex; // Index of selected slot
    public TextMeshProUGUI selectedItemName; // UI name text
    public TextMeshProUGUI selectedItemDescription; // UI description text
    public TextMeshProUGUI selectedItemStatNames; // UI stat names
    public TextMeshProUGUI selectedItemStatValues; // UI stat values
    public GameObject useButton; // Button for consumables
    public GameObject equipButton; // Button for equippable
    public GameObject unEquipButton; // Button for unequipping
    public GameObject dropButton; // Button for dropping

    private int curEquipIndex; // Index of equipped slot

    private Player controller; // Reference to player
    public PlayerNeeds needs; // Reference to player needs system

    [Header("Events")]
    public UnityEvent onOpenInventory; // Event when opened
    public UnityEvent onCloseInventory; // Event when closed

    private bool hasInitialized = false; // Tracks initialization

    void Awake() // Called before Start
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; } // Enforce singleton
        instance = this; // Assign instance
        controller = GetComponent<Player>(); // Cache player reference
        needs = GetComponent<PlayerNeeds>(); // Cache player needs reference
    }

    void Start() // Called once
    {
        if (!hasInitialized) // Initialize slots
        {
            slots = new ItemSlot[uiSlots.Length]; // Create slot array
            for (int x = 0; x < slots.Length; x++) { slots[x] = new ItemSlot(); uiSlots[x].index = x; uiSlots[x].Clear(); }
            ClearSelectedItemWindow(); // Reset selected window
            inventoryWindow.SetActive(false); // Hide window
            hasInitialized = true; // Mark initialized
        }
        else // If already initialized, just resync UI
        {
            for (int x = 0; x < uiSlots.Length; x++) uiSlots[x].index = x;
            UpdateUI();
        }
    }

    void Update() // Called every frame
    {
        if (Keyboard.current.iKey.wasPressedThisFrame) Toggle(); // Toggle inventory with I key
    }

    public void OnInventoryButton(InputAction.CallbackContext context) // Input System handler
    {
        if (context.phase == InputActionPhase.Started) Toggle(); // Toggle on press
    }

    public void Toggle() // Opens/closes inventory
    {
        if (inventoryWindow.activeInHierarchy) // If open
        {
            inventoryWindow.SetActive(false); // Hide window
            onCloseInventory.Invoke(); // Trigger close event
            controller.ToggleCursor(false); // Lock cursor
        }
        else // If closed
        {
            inventoryWindow.SetActive(true); // Show window
            onOpenInventory.Invoke(); // Trigger open event
            ClearSelectedItemWindow(); // Reset details
            controller.ToggleCursor(true); // Unlock cursor
        }
    }

    public bool IsOpen() => inventoryWindow.activeInHierarchy; // Check if open

    public void AddItem(ItemData item) // Add item to inventory
    {
        if (item.canStack) // If stackable
        {
            ItemSlot slotToStackTo = GetItemStack(item); // Try stack
            if (slotToStackTo != null) { slotToStackTo.quantity++; UpdateUI(); return; }
        }

        ItemSlot emptySlot = GetEmptySlot(); // Try empty slot
        if (emptySlot != null) { emptySlot.item = item; emptySlot.quantity = 1; UpdateUI(); return; }

        ThrowItem(item); // If full, drop it
    }

    void ThrowItem(ItemData item) => Instantiate(item.dropPrefab, dropPosition.position, Quaternion.identity); // Drop item in world

    public void UpdateUI() // Refresh UI
    {
        for (int x = 0; x < slots.Length; x++)
        {
            if (slots[x] == null) continue;
            if (slots[x].item != null) uiSlots[x].Set(slots[x]); else uiSlots[x].Clear();
        }
    }

    ItemSlot GetItemStack(ItemData item) // Find stack slot
    {
        for (int x = 0; x < slots.Length; x++) if (slots[x].item == item && slots[x].quantity < item.maxStackAmount) return slots[x];
        return null;
    }

    ItemSlot GetEmptySlot() // Find empty slot
    {
        for (int x = 0; x < slots.Length; x++) if (slots[x].item == null) return slots[x];
        return null;
    }

    public void SelectItem(int index) // Selects an item
    {
        if (slots[index].item == null) return;
        selectedItem = slots[index]; selectedItemIndex = index;
        selectedItemName.text = selectedItem.item.displayName; // Show name
        selectedItemDescription.text = selectedItem.item.description; // Show description
        selectedItemStatNames.text = string.Empty; selectedItemStatValues.text = string.Empty; // Clear stats
        foreach (var stat in selectedItem.item.consumables) { selectedItemStatNames.text += stat.type + "\n"; selectedItemStatValues.text += stat.value + "\n"; } // Add stats
        useButton.SetActive(selectedItem.item.type == ItemType.Consumable); // Show use if consumable
        equipButton.SetActive(selectedItem.item.type == ItemType.Equipable && !uiSlots[index].equipped); // Show equip if equippable
        unEquipButton.SetActive(selectedItem.item.type == ItemType.Equipable && uiSlots[index].equipped); // Show unequip if equipped
        dropButton.SetActive(true); // Always show drop
    }

    void ClearSelectedItemWindow() // Clears UI details
    {
        selectedItem = null; selectedItemName.text = ""; selectedItemDescription.text = "";
        selectedItemStatNames.text = ""; selectedItemStatValues.text = "";
        useButton.SetActive(false); equipButton.SetActive(false); unEquipButton.SetActive(false); dropButton.SetActive(false);
    }

    public void OnUseButton() // Consume item
    {
        if (selectedItem.item.type == ItemType.Consumable) // If consumable
        {
            foreach (var stat in selectedItem.item.consumables) // Apply each effect
            {
                switch (stat.type)
                {
                    case ConsumableType.Health: needs.Heal(stat.value); break;
                    case ConsumableType.Hunger: needs.Eat(stat.value); break;
                    case ConsumableType.Magik: needs.Drink(stat.value); break;
                    case ConsumableType.Stamina: needs.Sleep(stat.value); break;
                }
            }
        }
        RemoveSelectedItem(); // Remove after use
    }

    public void OnEquipButton() // Equip item
    {
        if (uiSlots[curEquipIndex].equipped) UnEquip(curEquipIndex); // Unequip current
        uiSlots[selectedItemIndex].equipped = true; curEquipIndex = selectedItemIndex; // Mark equipped
        EquipManager.instance.EquipNew(selectedItem.item); // Equip item
        UpdateUI(); SelectItem(selectedItemIndex); // Refresh UI
    }

    void UnEquip(int index) // Unequip item
    {
        uiSlots[index].equipped = false; // Mark as not equipped
        EquipManager.instance.UnEquip(); // Clear equipment
        UpdateUI(); if (selectedItemIndex == index) SelectItem(index); // Refresh
    }

    public void OnUnEquipButton() => UnEquip(selectedItemIndex); // Button handler

    public void OnDropButton() { ThrowItem(selectedItem.item); RemoveSelectedItem(); } // Drop item

    void RemoveSelectedItem() // Removes one of selected item
    {
        selectedItem.quantity--; // Decrease count
        if (selectedItem.quantity <= 0) // If none left
        {
            if (uiSlots[selectedItemIndex].equipped) UnEquip(selectedItemIndex); // Unequip if needed
            selectedItem.item = null; ClearSelectedItemWindow(); // Clear slot
        }
        UpdateUI(); // Refresh
    }

    public void RemoveItem(ItemData item) // Removes item by reference
    {
        for (int i = 0; i < slots.Length; i++) // Search all slots
        {
            if (slots[i].item == item) // Found match
            {
                slots[i].quantity--; // Decrease count
                if (slots[i].quantity <= 0) // If empty
                {
                    if (uiSlots[i].equipped) UnEquip(i); // Unequip if equipped
                    slots[i].item = null; ClearSelectedItemWindow(); // Clear slot
                }
                UpdateUI(); return; // Refresh and stop
            }
        }
    }

    public void ReassignUISlots(ItemSlotUI[] newUISlots) // Rebinds UI slots dynamically
    {
        uiSlots = newUISlots; // Store reference
        for (int i = 0; i < uiSlots.Length; i++) // Loop through slots
        {
            uiSlots[i].index = i; uiSlots[i].equipped = false; // Reset
            if (slots != null && i < slots.Length) uiSlots[i].Set(slots[i]); else uiSlots[i].Clear(); // Populate
        }
        UpdateUI(); // Refresh
    }

    public void ClearInventory() // Clears all items
    {
        for (int x = 0; x < slots.Length; x++) { slots[x].item = null; slots[x].quantity = 0; } // Reset slots
        UpdateUI(); ClearSelectedItemWindow(); // Refresh
    }

    public bool HasItems(ItemData item, int quantity) // Checks if player has required items
    {
        int amount = 0; // Running total
        for (int i = 0; i < slots.Length; i++) // Loop through slots
        {
            if (slots[i].item != null && slots[i].item.name == item.name) // Match by name
            {
                amount += slots[i].quantity; // Add quantity
                if (amount >= quantity) return true; // Enough found
            }
        }
        return false; // Not enough
    }
}

public class ItemSlot // Represents one inventory slot
{
    public ItemData item; // Item reference
    public int quantity; // Quantity stored
}