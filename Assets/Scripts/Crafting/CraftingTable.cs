using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingTable : MonoBehaviour, IInteractable
{
    private CraftingWindow craftingWindow;
    private Player player;

    void Start()
    {
        craftingWindow = FindObjectOfType<CraftingWindow>(true);
        player = FindObjectOfType<Player>();
    }

    public string GetInteractPrompt()
    {
        return "Craft";
    }

    public void OnInteract()
    {
        craftingWindow.gameObject.SetActive(true);
        player.ToggleCursor(true); // Ensure cursor becomes visible when crafting opens.

        // Add listener to close the crafting UI and hide cursor when it's disabled
        craftingWindow.gameObject.SetActive(true);
        craftingWindow.gameObject.GetComponent<Canvas>().enabled = true;
    }

    void Update()
    {
        // If crafting window is closed, hide the cursor again
        if (!craftingWindow.gameObject.activeSelf)
        {
            player.ToggleCursor(false);
        }
    }
}