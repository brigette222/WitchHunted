using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public string levelIntroSceneName = "LevelIntro"; // Set this to your intro scene name

    public void StartGame()
    {
        SceneManager.LoadScene(levelIntroSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
