using UnityEngine;

public class YarnTriggerZone : MonoBehaviour
{
    public string dialogueNode = "TC1";
    [HideInInspector] public bool hasTriggered = false;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }
}