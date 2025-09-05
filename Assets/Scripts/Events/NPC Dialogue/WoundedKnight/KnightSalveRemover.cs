using UnityEngine;
using Yarn.Unity;

public class KnightSalveRemover : MonoBehaviour
{
    public ItemData healingSalve;

    private DialogueRunner runner;

    void Start()
    {
        runner = FindObjectOfType<DialogueRunner>();
    }

    void Update()
    {
        if (runner != null &&
            runner.VariableStorage != null &&
            runner.VariableStorage.TryGetValue("$remove_healing_salve", out bool shouldRemove) &&
            shouldRemove)
        {
            if (Inventory.instance.HasItems(healingSalve, 1))
            {
                Inventory.instance.RemoveItem(healingSalve);
                Debug.Log("[KnightSalveRemover] Removed healing salve.");
            }

            // Reset the variable to prevent multiple removals
            runner.VariableStorage.SetValue("$remove_healing_salve", false);
        }
    }
}