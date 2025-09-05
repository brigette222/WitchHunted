using UnityEngine;

public class PauseUIBinder : MonoBehaviour
{
    private void Start()
    {
        // Wait a frame to ensure PauseMenu exists
        StartCoroutine(AssignToPauseMenu());
    }

    System.Collections.IEnumerator AssignToPauseMenu()
    {
        yield return null;

        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
        if (pauseMenu != null)
        {
            pauseMenu.AssignPauseUI(gameObject);
        }
        else
        {
            Debug.LogWarning("[PauseUIBinder] Couldn't find PauseMenu to assign to.");
        }
    }
}
