using UnityEngine;

public class PauseUIBinder : MonoBehaviour
{
    private void Start() // Called once when object is initialized
    {
        StartCoroutine(AssignToPauseMenu()); // Start coroutine to assign UI
    }

    System.Collections.IEnumerator AssignToPauseMenu() // Coroutine to delay assignment by 1 frame
    {
        yield return null; // Wait a frame to ensure PauseMenu exists
        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>(); // Find PauseMenu in scene
        if (pauseMenu != null) pauseMenu.AssignPauseUI(gameObject); // Assign this UI to PauseMenu
    }
}
