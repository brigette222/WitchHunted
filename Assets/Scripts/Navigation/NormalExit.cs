using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))] // Ensure a BoxCollider2D exists
public class NormalExit : MonoBehaviour
{
    [SerializeField] private string nextLevelName; // Scene name to load

    private void Reset() // Called when script is added or reset
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>(); // Get collider
        box.size = new Vector2(1.5f, 1.5f); // Set default size
        box.isTrigger = true; // Make collider a trigger
    }

    private void OnTriggerEnter2D(Collider2D other) // Called when something enters trigger
    {
        if (other.CompareTag("Player")) // Only respond to Player
        {
            // TODO: figure out which version of this script i've been using, delete the other and add scene loading logic to the correct one
        }
    }
}
