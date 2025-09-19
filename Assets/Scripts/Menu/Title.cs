using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public string levelIntroSceneName = "LevelIntro"; // Name of the intro scene to load

    public void StartGame() // Loads the intro scene
    {
        SceneManager.LoadScene(levelIntroSceneName); // Load specified scene
    }

    public void QuitGame() // Exits the application
    {
        Application.Quit(); // Quit the game (works only in builds)
    }
}