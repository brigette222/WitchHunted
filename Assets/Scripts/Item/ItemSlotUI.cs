using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TextMeshProUGUI quantityText;
    public int index;
    public bool equipped;

    private ItemSlot curSlot;
    private Outline outline;

    void Awake()
    {
        outline = GetComponent<Outline>();
        if (button != null)
            button.onClick.AddListener(OnButtonClick);
    }

    void OnEnable()
    {
        UpdateVisual();
    }

    public void Set(ItemSlot slot)
    {
        curSlot = slot;
        UpdateVisual();
    }

    public void Clear()
    {
        curSlot = null;
        icon.gameObject.SetActive(false);
        quantityText.text = "";
        if (outline != null) outline.enabled = false;
    }

    public void SetEquipped(bool value)
    {
        equipped = value;
        if (outline != null)
            outline.enabled = value;
    }

    public void OnButtonClick()
    {
        Inventory.instance.SelectItem(index);
    }

    private void UpdateVisual()
    {
        if (curSlot != null && curSlot.item != null)
        {
            icon.sprite = curSlot.item.icon;
            icon.gameObject.SetActive(true);
            quantityText.text = curSlot.quantity > 1 ? curSlot.quantity.ToString() : "";
        }
        else
        {
            Clear();
        }

        if (outline != null)
            outline.enabled = equipped;
    }
}