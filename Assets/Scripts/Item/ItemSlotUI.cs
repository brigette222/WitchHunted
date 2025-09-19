using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Button button; // UI button for selecting the slot
    public Image icon; // UI image showing the item’s icon
    public TextMeshProUGUI quantityText; // UI text showing stack quantity
    public int index; // Index of this slot in the inventory
    public bool equipped; // Whether this slot is currently equipped

    private ItemSlot curSlot; // The current slot data this UI is showing
    private Outline outline; // Outline component used to highlight equipped slots

    void Awake() // Called when object is created
    {
        outline = GetComponent<Outline>(); // Cache the Outline component
        if (button != null) button.onClick.AddListener(OnButtonClick); // Register click event
    }

    void OnEnable() // Called when object is enabled
    {
        UpdateVisual(); // Refresh slot visuals
    }

    public void Set(ItemSlot slot) // Assigns a slot to this UI
    {
        curSlot = slot; // Store the slot
        UpdateVisual(); // Refresh visuals
    }

    public void Clear() // Clears the slot display
    {
        curSlot = null; // Remove reference
        icon.gameObject.SetActive(false); // Hide icon
        quantityText.text = ""; // Clear quantity text
        if (outline != null) outline.enabled = false; // Disable highlight
    }

    public void SetEquipped(bool value) // Sets equipped state
    {
        equipped = value; // Store equipped state
        if (outline != null) outline.enabled = value; // Update outline
    }

    public void OnButtonClick() // Called when the slot button is clicked
    {
        Inventory.instance.SelectItem(index); // Notify inventory of selection
    }

    private void UpdateVisual() // Updates icon, text, and outline
    {
        if (curSlot != null && curSlot.item != null) // If slot has a valid item
        {
            icon.sprite = curSlot.item.icon; // Set icon sprite
            icon.gameObject.SetActive(true); // Show icon
            quantityText.text = curSlot.quantity > 1 ? curSlot.quantity.ToString() : ""; // Show quantity if stacked
        }
        else Clear(); // Otherwise reset display

        if (outline != null) outline.enabled = equipped; // Update outline for equipped state
    }
}
