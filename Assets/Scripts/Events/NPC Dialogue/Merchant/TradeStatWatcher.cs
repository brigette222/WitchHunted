using UnityEngine;
using Yarn.Unity;

public class TradeStatWatcher : MonoBehaviour
{
    public DialogueRunner runner; // Reference to the DialogueRunner (handles Yarn dialogue)
    public PlayerNeeds playerNeeds; // Reference to the PlayerNeeds script (handles player stats)

    string lastProcessedPayment = ""; // Tracks the last applied payment to avoid repeats

    void Start() // Called once when the object is initialized
    {
        if (!runner) runner = FindObjectOfType<DialogueRunner>(); // Auto-assign DialogueRunner if not set
        if (!playerNeeds) playerNeeds = FindObjectOfType<PlayerNeeds>(); // Auto-assign PlayerNeeds if not set
    }

    void Update() // Called every frame
    {
        if (!runner || runner.VariableStorage == null || !playerNeeds) return;  // If any required reference is missing, stop here
 
        if (runner.VariableStorage.TryGetValue("$trade_payment", out object valueObj))   // Check if Yarn variable "$trade_payment" has a value
        {
            string payment = valueObj as string; // Convert stored object to string

            if (!string.IsNullOrEmpty(payment) && payment != lastProcessedPayment)  // Only apply if the string is not empty and hasn't already been processed
            {
                playerNeeds.ApplyTradeCost(payment); // Deduct the appropriate stat cost
                lastProcessedPayment = payment; // Remember the last payment to prevent re-applying
                runner.VariableStorage.SetValue("$trade_payment", ""); // Reset variable after applying
            }
        }
    }
}