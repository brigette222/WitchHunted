using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

    private void Start()
    {
        Debug.Log("[PauseMenu] Start() called");

        if (pauseMenuUI == null)
        {
            pauseMenuUI = transform.Find("PauseCanvas")?.gameObject;

            if (pauseMenuUI != null)
                Debug.Log("[PauseMenu] Auto-assigned PauseCanvas.");
            else
                Debug.LogWarning("[PauseMenu] PauseCanvas not found.");
        }
    }

    private void Update()
    {
        if (PauseManager.Instance == null)
        {
            Debug.LogWarning("[PauseMenu] PauseManager not found.");
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"[PauseMenu] Escape pressed. Current PauseType: {PauseManager.Instance.CurrentPauseType}");

            if (PauseManager.Instance.CurrentPauseType == PauseType.Combat)
            {
                Debug.Log("[PauseMenu] Blocked UI pause because Combat is active.");
                return;
            }

            if (PauseManager.Instance.CurrentPauseType == PauseType.UI)
                Resume();
            else
                Pause();
        }

        if (pauseMenuUI == null)
        {
            GameObject found = GameObject.Find("PauseCanvas");
            if (found != null)
            {
                pauseMenuUI = found;
                Debug.Log("[PauseMenu] PauseCanvas re-assigned at runtime.");
            }
        }
    }

    public void Pause()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogWarning("[PauseMenu] PauseCanvas is null. Cannot pause.");
            return;
        }

        pauseMenuUI.SetActive(true);
        PauseManager.Instance.Pause(PauseType.UI);
        Debug.Log("[PauseMenu] PauseMenuUI activated.");
    }

    public void Resume()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogWarning("[PauseMenu] PauseCanvas is null. Cannot resume.");
            return;
        }

        pauseMenuUI.SetActive(false);
        PauseManager.Instance.Resume();
        Debug.Log("[PauseMenu] PauseMenuUI deactivated.");
    }

    public void QuitToMenu()
    {
        Debug.Log("[PauseMenu] Quit to menu called.");
        PauseManager.Instance.Resume();
        SceneManager.LoadScene("MainMenu");
    }

    public void AssignPauseUI(GameObject ui)
    {
        pauseMenuUI = ui;
        Debug.Log("[PauseMenu] Pause UI assigned dynamically: " + ui.name);
    }
}