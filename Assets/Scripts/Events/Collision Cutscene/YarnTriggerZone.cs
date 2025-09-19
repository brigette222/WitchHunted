using UnityEngine;

public class YarnTriggerZone : MonoBehaviour
{
    public string dialogueNode = "TC1"; // The name of the Yarn dialogue node to start when triggered
    [HideInInspector] public bool hasTriggered = false; // Keeps track of whether the trigger has already occurred, hidden in Inspector

    private void OnDrawGizmosSelected() // Called by Unity to draw gizmos in the editor when the object is selected
    {
        Gizmos.color = Color.green; // Sets the gizmo color to green
        Gizmos.DrawWireSphere(transform.position, 1.0f); // Draws a green wireframe sphere at the object's position for visualizing trigger zone
    }
}
