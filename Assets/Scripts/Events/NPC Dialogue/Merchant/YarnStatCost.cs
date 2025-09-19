using UnityEngine;
using Yarn.Unity;

public class YarnStatCost : MonoBehaviour
{
    public PlayerNeeds playerNeeds; // Reference to PlayerNeeds for applying stat changes

    [YarnCommand("payStatAlt")] // Yarn command that can be called in dialogue
    public void PayStatAlt(string paymentType) // Handles stat payment from dialogue
    {
        if (playerNeeds == null) // If reference is missing...
        {
            playerNeeds = FindObjectOfType<PlayerNeeds>(); // Try to find PlayerNeeds in the scene
            if (playerNeeds == null) return; // Stop if still not found
        }

        switch (paymentType.ToLower()) // Match the payment type (case-insensitive)
        {
            case "blood": playerNeeds.Heal(-20f); break; // Deduct 20 health
            case "memories": playerNeeds.Drink(-15f); break; // Deduct 15 magic
            case "rations": playerNeeds.Eat(-25f); break; // Deduct 25 hunger
            case "sweat": playerNeeds.Sleep(30f); break; // Deduct stamina (check PlayerNeeds logic if this restores instead)
            default: break; // If payment type not recognized, do nothing
        }
    }
}