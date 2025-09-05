using UnityEngine;
using Yarn.Unity;

public class TradeStatWatcher : MonoBehaviour
{
    public DialogueRunner runner;
    public PlayerNeeds playerNeeds;

    string lastProcessedPayment = "";

    void Start()
    {
        if (!runner) runner = FindObjectOfType<DialogueRunner>();
        if (!playerNeeds) playerNeeds = FindObjectOfType<PlayerNeeds>();
    }

    void Update()
    {
        if (!runner || runner.VariableStorage == null || !playerNeeds) return;

        if (runner.VariableStorage.TryGetValue("$trade_payment", out object valueObj))
        {
            string payment = valueObj as string;

            if (!string.IsNullOrEmpty(payment) && payment != lastProcessedPayment)
            {
                playerNeeds.ApplyTradeCost(payment);
                lastProcessedPayment = payment;
                runner.VariableStorage.SetValue("$trade_payment", ""); // Reset after applying
            }
        }
    }
}