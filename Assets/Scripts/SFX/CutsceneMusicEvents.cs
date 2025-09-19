using UnityEngine;
using Yarn.Unity;

public class CutsceneMusicEvents : MonoBehaviour
{
    public bool fadeInOnDialogueStart = true; // Fade in when dialogue starts
    public bool fadeOutOnDialogueComplete = true; // Fade out when dialogue ends
    public bool fadeOutOnNodeComplete = false; // Fade out on specific node complete

    void Awake() // Runs on object creation
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>(); // Find DialogueRunner
        if (runner != null)
        {
            if (fadeInOnDialogueStart) runner.onDialogueStart.AddListener(() => FadeInMusic()); // Hook dialogue start
            if (fadeOutOnDialogueComplete) runner.onDialogueComplete.AddListener(() => FadeOutMusic()); // Hook dialogue complete
            if (fadeOutOnNodeComplete) runner.onNodeComplete.AddListener((string _) => FadeOutMusic()); // Hook node complete
        }
    }

    void FadeInMusic() // Fades in cutscene music
    {
        MusicManager mm = FindObjectOfType<MusicManager>(); // Find MusicManager
        if (mm != null) mm.StartCutsceneMusic(); // Start cutscene music
    }

    void FadeOutMusic() // Fades out cutscene music
    {
        MusicManager mm = FindObjectOfType<MusicManager>(); // Find MusicManager
        if (mm != null) mm.StopCutsceneMusic(); // Stop cutscene music
    }
}