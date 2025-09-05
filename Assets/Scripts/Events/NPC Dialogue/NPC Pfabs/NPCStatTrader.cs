using UnityEngine;
using Yarn.Unity;

public class NPCStatTrader : MonoBehaviour
{
    private PlayerNeeds playerNeeds;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            playerNeeds = player.GetComponent<PlayerNeeds>();

            if (playerNeeds == null)
                Debug.LogError("[NPCStatTrader] Player found, but no PlayerNeeds component.");
        }
        else
        {
            Debug.LogError("[NPCStatTrader] Player not found in scene.");
        }
    }

    [YarnCommand("payStat")]
    public void PayStat(string paymentType)
    {
        if (playerNeeds == null)
        {
            Debug.LogError("[NPCStatTrader] PlayerNeeds reference is null.");
            return;
        }

        Debug.Log($"[NPCStatTrader] Paying stat: {paymentType}");
        playerNeeds.ApplyTradeCost(paymentType);
    }
}
