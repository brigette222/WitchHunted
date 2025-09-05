using UnityEngine;
using Yarn.Unity;

public class CutsceneMusicEvents : MonoBehaviour
{
    public bool fadeInOnDialogueStart = true;
    public bool fadeOutOnDialogueComplete = true;
    public bool fadeOutOnNodeComplete = false;

    void Awake()
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>();
        if (runner != null)
        {
            if (fadeInOnDialogueStart)
                runner.onDialogueStart.AddListener(() => FadeInMusic());

            if (fadeOutOnDialogueComplete)
                runner.onDialogueComplete.AddListener(() => FadeOutMusic());

            if (fadeOutOnNodeComplete)
                runner.onNodeComplete.AddListener((string _) => FadeOutMusic());
        }
    }

    void FadeInMusic()
    {
        MusicManager mm = FindObjectOfType<MusicManager>();
        if (mm != null)
        {
            mm.StartCutsceneMusic();
            Debug.Log("[CutsceneMusicEvents] Cutscene music fading IN.");
        }
    }

    void FadeOutMusic()
    {
        MusicManager mm = FindObjectOfType<MusicManager>();
        if (mm != null)
        {
            mm.StopCutsceneMusic();
            Debug.Log("[CutsceneMusicEvents] Cutscene music fading OUT.");
        }
    }
}