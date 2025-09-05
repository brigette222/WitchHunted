using UnityEngine;
using Yarn.Unity;

public class AltarInteract : MonoBehaviour
{
    public string yarnNodeName = "Altar";

    [Header("Required Items")]
    public ItemData divinationIncense;
    public ItemData talismanItem; // NEW! Drag your talisman item here.

    public float detectionRadius = 1.0f;
    public LayerMask playerLayer;

    void Update()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (hit != null && hit.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            DialogueRunner runner = FindObjectOfType<DialogueRunner>();

            if (runner != null && !runner.IsDialogueRunning)
            {
                if (Inventory.instance != null)
                {
                    bool hasAllItems = HasAllRequiredItems();
                    runner.VariableStorage.SetValue("$has_all_ritual_items", hasAllItems);
                    Debug.Log($"[AltarInteract] Set $has_all_ritual_items = {hasAllItems}");
                }

                runner.StartDialogue(yarnNodeName);
                Debug.Log($"[AltarInteract] Starting dialogue node: {yarnNodeName}");
            }
        }
    }

    private bool HasAllRequiredItems()
    {
        bool hasIncense = Inventory.instance.HasItems(divinationIncense, 1);
        bool hasTalisman = Inventory.instance.HasItems(talismanItem, 1);

        if (!hasIncense)
        {
            Debug.LogWarning("[AltarInteract] Missing Divination Incense!");
        }

        if (!hasTalisman)
        {
            Debug.LogWarning("[AltarInteract] Missing Talisman!");
        }

        return hasIncense && hasTalisman;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}