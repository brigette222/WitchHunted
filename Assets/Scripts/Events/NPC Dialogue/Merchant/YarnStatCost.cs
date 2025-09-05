using UnityEngine;
using Yarn.Unity;

public class YarnStatCost : MonoBehaviour
{
    public PlayerNeeds playerNeeds;

    // ? Renamed to avoid duplicate YarnCommand errors
    [YarnCommand("payStatAlt")]
    public void PayStatAlt(string paymentType)
    {
        if (playerNeeds == null)
        {
            playerNeeds = FindObjectOfType<PlayerNeeds>();
            if (playerNeeds == null)
            {
                Debug.LogError("[YarnStatCost] ERROR: No PlayerNeeds found in scene.");
                return;
            }
        }

        switch (paymentType.ToLower())
        {
            case "blood":
                playerNeeds.Heal(-20f); // Deduct health
                Debug.Log("[YarnStatCost] -20 Health paid as blood.");
                break;

            case "memories":
                playerNeeds.Drink(-15f); // Deduct magic
                Debug.Log("[YarnStatCost] -15 Magic paid as memories.");
                break;

            case "rations":
                playerNeeds.Eat(-25f); // Deduct hunger
                Debug.Log("[YarnStatCost] -25 Hunger paid as rations.");
                break;

            case "sweat":
                playerNeeds.Sleep(30f); // Deduct stamina
                Debug.Log("[YarnStatCost] -30 Stamina paid as sweat.");
                break;

            default:
                Debug.LogWarning($"[YarnStatCost] Unknown payment type: {paymentType}");
                break;
        }
    }
}