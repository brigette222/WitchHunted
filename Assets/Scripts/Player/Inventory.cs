using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public ItemSlotUI[] uiSlots;
    public ItemSlot[] slots;

    public GameObject inventoryWindow;
    public Transform dropPosition;

    [Header("Selected Item")]
    private ItemSlot selectedItem;
    private int selectedItemIndex;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemStatNames;
    public TextMeshProUGUI selectedItemStatValues;
    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unEquipButton;
    public GameObject dropButton;

    private int curEquipIndex;

    private Player controller;
    public PlayerNeeds needs;

    [Header("Events")]
    public UnityEvent onOpenInventory;
    public UnityEvent onCloseInventory;

    private bool hasInitialized = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        controller = GetComponent<Player>();
        needs = GetComponent<PlayerNeeds>();
    }

    void Start()
    {
        if (!hasInitialized)
        {
            slots = new ItemSlot[uiSlots.Length];

            for (int x = 0; x < slots.Length; x++)
            {
                slots[x] = new ItemSlot();
                uiSlots[x].index = x;
                uiSlots[x].Clear();
            }

            ClearSelectedItemWindow();
            inventoryWindow.SetActive(false);
            hasInitialized = true;
        }
        else
        {
            for (int x = 0; x < uiSlots.Length; x++)
            {
                uiSlots[x].index = x;
            }

            UpdateUI();
        }
    }

    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void OnInventoryButton(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (inventoryWindow.activeInHierarchy)
        {
            inventoryWindow.SetActive(false);
            onCloseInventory.Invoke();
            controller.ToggleCursor(false);
        }
        else
        {
            inventoryWindow.SetActive(true);
            onOpenInventory.Invoke();
            ClearSelectedItemWindow();
            controller.ToggleCursor(true);
        }
    }

    public bool IsOpen() => inventoryWindow.activeInHierarchy;

    public void AddItem(ItemData item)
    {
        if (item.canStack)
        {
            ItemSlot slotToStackTo = GetItemStack(item);
            if (slotToStackTo != null)
            {
                slotToStackTo.quantity++;
                UpdateUI();
                return;
            }
        }

        ItemSlot emptySlot = GetEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.item = item;
            emptySlot.quantity = 1;
            UpdateUI();
            return;
        }

        ThrowItem(item);
    }

    void ThrowItem(ItemData item)
    {
        Instantiate(item.dropPrefab, dropPosition.position, Quaternion.identity);
    }

    public void UpdateUI()
    {
        for (int x = 0; x < slots.Length; x++)
        {
            if (slots[x] == null)
                continue;

            if (slots[x].item != null)
            {
                uiSlots[x].Set(slots[x]);
            }
            else
            {
                uiSlots[x].Clear();
            }
        }
    }

    ItemSlot GetItemStack(ItemData item)
    {
        for (int x = 0; x < slots.Length; x++)
        {
            if (slots[x].item == item && slots[x].quantity < item.maxStackAmount)
                return slots[x];
        }
        return null;
    }

    ItemSlot GetEmptySlot()
    {
        for (int x = 0; x < slots.Length; x++)
        {
            if (slots[x].item == null)
                return slots[x];
        }
        return null;
    }

    public void SelectItem(int index)
    {
        if (slots[index].item == null)
            return;

        selectedItem = slots[index];
        selectedItemIndex = index;

        selectedItemName.text = selectedItem.item.displayName;
        selectedItemDescription.text = selectedItem.item.description;

        selectedItemStatNames.text = string.Empty;
        selectedItemStatValues.text = string.Empty;

        foreach (var stat in selectedItem.item.consumables)
        {
            selectedItemStatNames.text += stat.type.ToString() + "\n";
            selectedItemStatValues.text += stat.value.ToString() + "\n";
        }

        useButton.SetActive(selectedItem.item.type == ItemType.Consumable);
        equipButton.SetActive(selectedItem.item.type == ItemType.Equipable && !uiSlots[index].equipped);
        unEquipButton.SetActive(selectedItem.item.type == ItemType.Equipable && uiSlots[index].equipped);
        dropButton.SetActive(true);
    }

    void ClearSelectedItemWindow()
    {
        selectedItem = null;
        selectedItemName.text = "";
        selectedItemDescription.text = "";
        selectedItemStatNames.text = "";
        selectedItemStatValues.text = "";
        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void OnUseButton()
    {
        if (selectedItem.item.type == ItemType.Consumable)
        {
            foreach (var stat in selectedItem.item.consumables)
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

        RemoveSelectedItem();
    }

    public void OnEquipButton()
    {
        if (uiSlots[curEquipIndex].equipped)
            UnEquip(curEquipIndex);

        uiSlots[selectedItemIndex].equipped = true;
        curEquipIndex = selectedItemIndex;
        EquipManager.instance.EquipNew(selectedItem.item);
        UpdateUI();
        SelectItem(selectedItemIndex);
    }

    void UnEquip(int index)
    {
        uiSlots[index].equipped = false;
        EquipManager.instance.UnEquip();
        UpdateUI();
        if (selectedItemIndex == index)
            SelectItem(index);
    }

    public void OnUnEquipButton() => UnEquip(selectedItemIndex);

    public void OnDropButton()
    {
        ThrowItem(selectedItem.item);
        RemoveSelectedItem();
    }

    void RemoveSelectedItem()
    {
        selectedItem.quantity--;

        if (selectedItem.quantity <= 0)
        {
            if (uiSlots[selectedItemIndex].equipped)
                UnEquip(selectedItemIndex);

            selectedItem.item = null;
            ClearSelectedItemWindow();
        }

        UpdateUI();
    }

    public void RemoveItem(ItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item)
            {
                slots[i].quantity--;

                if (slots[i].quantity <= 0)
                {
                    if (uiSlots[i].equipped)
                        UnEquip(i);

                    slots[i].item = null;
                    ClearSelectedItemWindow();
                }

                UpdateUI();
                return;
            }
        }
    }

    public void ReassignUISlots(ItemSlotUI[] newUISlots)
    {
        uiSlots = newUISlots;

        for (int i = 0; i < uiSlots.Length; i++)
        {
            uiSlots[i].index = i;
            uiSlots[i].equipped = false;

            if (slots != null && i < slots.Length)
            {
                uiSlots[i].Set(slots[i]);
            }
            else
            {
                uiSlots[i].Clear();
            }
        }

        UpdateUI();
    }

    public void ClearInventory()
    {
        for (int x = 0; x < slots.Length; x++)
        {
            slots[x].item = null;
            slots[x].quantity = 0;
        }

        UpdateUI();
        ClearSelectedItemWindow();
    }

    public bool HasItems(ItemData item, int quantity)
    {
        int amount = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
            {
                // Compare by name instead of object reference
                if (slots[i].item.name == item.name)
                {
                    amount += slots[i].quantity;
                    Debug.Log($"[Inventory] Found match: {slots[i].item.name} (x{slots[i].quantity}) ? Total: {amount}");

                    if (amount >= quantity)
                    {
                        Debug.Log("[Inventory] Required amount met. Returning true.");
                        return true;
                    }
                }
            }
        }

        Debug.Log("[Inventory] Item not found or not enough quantity.");
        return false;
    }
}

public class ItemSlot
{
    public ItemData item;
    public int quantity;
}