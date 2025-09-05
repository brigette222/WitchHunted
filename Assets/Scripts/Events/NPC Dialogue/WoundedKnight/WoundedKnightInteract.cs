using UnityEngine;
using Yarn.Unity;

public class WoundedKnightInteract : MonoBehaviour
{
    public string yarnNodeName = "WoundedStart";
    public ItemData healingSalve;

    public float detectionRadius = 1.5f;
    public LayerMask playerLayer;

    void Update()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (hit != null && hit.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[WoundedKnightInteract] Player in range and pressed E.");

            DialogueRunner runner = FindObjectOfType<DialogueRunner>();

            if (runner != null && !runner.IsDialogueRunning)
            {
                Debug.Log("[WoundedKnightInteract] DialogueRunner found.");

                if (Inventory.instance != null && healingSalve != null)
                {
                    Debug.Log($"[WoundedKnightInteract] Checking for healing salve: {healingSalve.name}, ID {healingSalve.GetInstanceID()}");

                    bool hasSalve = Inventory.instance.HasItems(healingSalve, 1);
                    Debug.Log($"[WoundedKnightInteract] Player has healing salve? {hasSalve}");

                    runner.VariableStorage.SetValue("$has_salve", hasSalve);
                }
                else
                {
                    Debug.LogWarning("[WoundedKnightInteract] Inventory or healingSalve reference is missing!");
                }

                runner.StartDialogue(yarnNodeName);
                Debug.Log($"[WoundedKnightInteract] Starting node: {yarnNodeName}");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}