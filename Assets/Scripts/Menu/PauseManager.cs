using UnityEngine;

public enum PauseType
{
    None,
    UI,
    Combat
}

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public PauseType CurrentPauseType { get; private set; } = PauseType.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[PauseManager] Initialized.");
    }

    private void Start()
    {
        // ? Auto-resume if we reset while paused
        if (CurrentPauseType != PauseType.None)
        {
            Debug.LogWarning($"[PauseManager] Start() detected leftover pause type: {CurrentPauseType}. Auto-resuming.");
            Resume();
        }
    }

    public void Pause(PauseType type)
    {
        if (CurrentPauseType != PauseType.None)
        {
            Debug.LogWarning($"[PauseManager] Already paused as {CurrentPauseType}. Ignoring new pause: {type}");
            return;
        }

        CurrentPauseType = type;

        if (type == PauseType.UI)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Debug.Log($"[PauseManager] PAUSED. Type: {type}");
    }

    public void Resume()
    {
        if (CurrentPauseType == PauseType.None)
        {
            Debug.LogWarning("[PauseManager] Resume() called, but not currently paused.");
            return;
        }

        if (CurrentPauseType == PauseType.UI)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log($"[PauseManager] RESUMED from {CurrentPauseType}");
        CurrentPauseType = PauseType.None;
    }

    public bool IsPaused(PauseType type)
    {
        return CurrentPauseType == type;
    }

    public bool IsAnyPaused()
    {
        return CurrentPauseType != PauseType.None;
    }
}