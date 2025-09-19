using UnityEngine;

public class TradeStatPayment : MonoBehaviour
{
    public PlayerNeeds playerNeeds; // Reference to the PlayerNeeds script for applying stat changes

    void Start() // Called when the object is first initialized
    {
        playerNeeds = FindObjectOfType<PlayerNeeds>(); // Automatically find the PlayerNeeds component in the scene
    }

    public void ApplyStatPayment(string paymentType) // Applies a cost based on the type of "payment"
    {
        if (playerNeeds == null) return; // If no PlayerNeeds found, stop (nothing to apply)

        switch (paymentType.ToLower()) // Match the payment type (case-insensitive)
        {
            case "blood": // Payment with health
                playerNeeds.Heal(-20f); // Deduct 20 health
                break;
            case "memories": // Payment with magic
                playerNeeds.Drink(-15f); // Deduct 15 magic
                break;
            case "rations": // Payment with hunger
                playerNeeds.Eat(-25f); // Deduct 25 hunger
                break;
            case "sweat": // Payment with stamina
                playerNeeds.Sleep(30f); // Deduct stamina (value is positive, so check PlayerNeeds logic)
                break;
            default: // If no valid type matches
                break; // Do nothing
        }
    }
}