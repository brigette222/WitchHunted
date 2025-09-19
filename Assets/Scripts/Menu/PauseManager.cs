using UnityEngine;


public enum PauseType { None, UI, Combat } // Defines different pause states

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; } // Singleton reference
    public PauseType CurrentPauseType { get; private set; } = PauseType.None; // Current pause state

    private void Awake() // Runs before Start
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; } // Enforce singleton
        Instance = this; // Set instance
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    private void Start() // Called once when initialized
    {
        if (CurrentPauseType != PauseType.None) Resume(); // Auto-resume if paused on load
    }

    public void Pause(PauseType type) // Pauses the game with a given type
    {
        if (CurrentPauseType != PauseType.None) return; // Ignore if already paused
        CurrentPauseType = type; // Set pause state

        if (type == PauseType.UI) // UI pause affects timescale and cursor
        {
            Time.timeScale = 0f; // Stop time
            Cursor.lockState = CursorLockMode.None; // Unlock cursor
            Cursor.visible = true; // Show cursor
        }
    }

    public void Resume() // Resumes the game
    {
        if (CurrentPauseType == PauseType.None) return; // Ignore if not paused

        if (CurrentPauseType == PauseType.UI) // Restore timescale/cursor if UI pause
        {
            Time.timeScale = 1f; // Resume time
            Cursor.lockState = CursorLockMode.Locked; // Lock cursor
            Cursor.visible = false; // Hide cursor
        }

        CurrentPauseType = PauseType.None; // Clear pause state
    }

    public bool IsPaused(PauseType type) => CurrentPauseType == type; // Check if paused with specific type
    public bool IsAnyPaused() => CurrentPauseType != PauseType.None; // Check if paused at all
}