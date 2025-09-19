using UnityEngine;
using Yarn.Unity;

public class NPCStatTrader : MonoBehaviour
{
    private PlayerNeeds playerNeeds; // Reference to PlayerNeeds for applying trade costs

    void Start() // Called once when the object is initialized
    {
        GameObject player = GameObject.FindWithTag("Player"); // Find the player object by tag
        if (player != null) playerNeeds = player.GetComponent<PlayerNeeds>(); // Try to get PlayerNeeds component from player
    }

    [YarnCommand("payStat")] // Exposes this method as a Yarn command
    public void PayStat(string paymentType) // Deducts stats based on the given payment type
    {
        if (playerNeeds == null) return; // Stop if PlayerNeeds is missing
        playerNeeds.ApplyTradeCost(paymentType); // Apply the cost through PlayerNeeds
    }
}