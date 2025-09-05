using UnityEngine;

public class TradeStatPayment : MonoBehaviour
{
    public PlayerNeeds playerNeeds;

    void Start()
    {
        // Ensure we have the PlayerNeeds script reference
        playerNeeds = FindObjectOfType<PlayerNeeds>();
    }

    public void ApplyStatPayment(string paymentType)
    {
        if (playerNeeds == null)
        {
            Debug.LogWarning("[TradeStatPayment] PlayerNeeds is missing. Cannot apply cost.");
            return;
        }

        switch (paymentType.ToLower())
        {
            case "blood":
                playerNeeds.Heal(-20f);  // Deduct health for blood
                Debug.Log("[TradeStatPayment] -20 Health for blood.");
                break;
            case "memories":
                playerNeeds.Drink(-15f);  // Deduct magic for memories
                Debug.Log("[TradeStatPayment] -15 Magic for memories.");
                break;
            case "rations":
                playerNeeds.Eat(-25f);  // Deduct hunger for rations
                Debug.Log("[TradeStatPayment] -25 Hunger for rations.");
                break;
            case "sweat":
                playerNeeds.Sleep(30f);  // Deduct stamina for sweat
                Debug.Log("[TradeStatPayment] -30 Stamina for sweat.");
                break;
            default:
                Debug.LogWarning($"[TradeStatPayment] Unknown payment method: {paymentType}");
                break;
        }
    }
}