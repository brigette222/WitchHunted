using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Reference to pause menu UI

    private void Start() // Called once at initialization
    {
        if (pauseMenuUI == null) // If UI not assigned
            pauseMenuUI = transform.Find("PauseCanvas")?.gameObject; // Try to auto-find child PauseCanvas
    }

    private void Update() // Called every frame
    {
        if (PauseManager.Instance == null) return; // Stop if PauseManager missing

        if (Input.GetKeyDown(KeyCode.Escape)) // Check for Escape key press
        {
            if (PauseManager.Instance.CurrentPauseType == PauseType.Combat) return; // Block pause during combat
            if (PauseManager.Instance.CurrentPauseType == PauseType.UI) Resume(); // If already paused, resume
            else Pause(); // Otherwise, pause
        }

        if (pauseMenuUI == null) // Try to recover UI if it was destroyed or unassigned
            pauseMenuUI = GameObject.Find("PauseCanvas");
    }

    public void Pause() // Activates pause menu
    {
        if (pauseMenuUI == null) return; // Stop if UI missing
        pauseMenuUI.SetActive(true); // Show UI
        PauseManager.Instance.Pause(PauseType.UI); // Set pause state
    }

    public void Resume() // Deactivates pause menu
    {
        if (pauseMenuUI == null) return; // Stop if UI missing
        pauseMenuUI.SetActive(false); // Hide UI
        PauseManager.Instance.Resume(); // Resume game
    }

    public void QuitToMenu() // Loads main menu
    {
        PauseManager.Instance.Resume(); // Ensure game is resumed before leaving
        SceneManager.LoadScene("MainMenu"); // Load main menu scene
    }

    public void AssignPauseUI(GameObject ui) // Dynamically assign UI reference
    {
        pauseMenuUI = ui; // Store reference
    }
}