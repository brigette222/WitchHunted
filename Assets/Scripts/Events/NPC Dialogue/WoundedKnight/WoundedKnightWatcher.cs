using UnityEngine;
using Yarn.Unity;

public class WoundedKnightWatcher : MonoBehaviour
{
    public DialogueRunner runner;
    public ItemData healingSalve;
    public ItemData severedHead;

    private void Start()
    {
        if (runner == null) runner = FindObjectOfType<DialogueRunner>();
    }

    private void Update()
    {
        if (runner == null || runner.VariableStorage == null)
            return;

        // HEALING SALVE REMOVAL
        if (runner.VariableStorage.TryGetValue("$remove_healing_salve", out object removeObj) && (bool)removeObj)
        {
            if (Inventory.instance != null && healingSalve != null)
            {
                Inventory.instance.RemoveItem(healingSalve);
                Debug.Log("[WoundedKnightWatcher] Removed Healing Salve from inventory.");
            }
            else
            {
                Debug.LogWarning("[WoundedKnightWatcher] Missing Inventory or Healing Salve reference.");
            }

            runner.VariableStorage.SetValue("$remove_healing_salve", false);
        }

        // SEVERED HEAD ADDITION
        if (runner.VariableStorage.TryGetValue("$take_head", out object takeHeadObj) && (bool)takeHeadObj)
        {
            if (Inventory.instance != null && severedHead != null)
            {
                Inventory.instance.AddItem(severedHead);
                Debug.Log("[WoundedKnightWatcher] Added Severed Head to inventory.");
            }
            else
            {
                Debug.LogWarning("[WoundedKnightWatcher] Missing Inventory or Severed Head reference.");
            }

            runner.VariableStorage.SetValue("$take_head", false);
        }
    }
}