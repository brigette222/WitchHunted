using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// location crafting - not currently in use
public class CraftingTable : MonoBehaviour, IInteractable // A crafting station the player can interact with
{
    private CraftingWindow craftingWindow; // Reference to the crafting UI
    private Player player; // Reference to the player for cursor control

    void Start()
    {
        craftingWindow = FindObjectOfType<CraftingWindow>(true); // Find the crafting window (even if inactive)
        player = FindObjectOfType<Player>(); // Find the player in the scene
    }

    public string GetInteractPrompt() { return "Craft"; } // Text shown when player can interact

    public void OnInteract() // Called when the player interacts with the table
    {
        craftingWindow.gameObject.SetActive(true); // Open the crafting window
        player.ToggleCursor(true); // Show the cursor so player can interact with UI

        // Ensure canvas is enabled (redundant if SetActive is enough, but ensures UI shows properly)
        craftingWindow.gameObject.SetActive(true);
        craftingWindow.gameObject.GetComponent<Canvas>().enabled = true;
    }

    void Update()
    {
        if (!craftingWindow.gameObject.activeSelf) // If the crafting window is closed
        {
            player.ToggleCursor(false); // Hide the cursor again
        }
    }
}