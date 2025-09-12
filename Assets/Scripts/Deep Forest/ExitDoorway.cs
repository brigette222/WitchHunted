using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))] // Ensure this GameObject always has a BoxCollider2D
public class ExitDoorway : MonoBehaviour //normal doors between scenes -currently not used in this version of the game
{
    private void Reset()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>(); // Grab the BoxCollider2D
        box.size = new Vector2(1.5f, 1.5f); // Set default collider size
        box.isTrigger = true; // Make sure collider works as a trigger (not a physical block)
    }

    private void OnTriggerEnter2D(Collider2D other) // Called when something enters trigger zone
    {
        if (other.CompareTag("Player")) // Only react if the Player enters
        {
            Debug.Log("Player entered ExitDoorway."); //  Debug message for confirmation

            // when in use add load next scene logic
        }
    }
}