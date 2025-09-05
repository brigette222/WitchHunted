using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource normalTheme;
    public AudioSource combatTheme;
    public AudioSource cutsceneTheme;
    public float fadeDuration = 1.5f;

    public enum StartingTheme { Normal, Combat, Cutscene }
    public StartingTheme startWith = StartingTheme.Normal;

    private Coroutine currentFade;
    private static MusicManager instance;

    void Awake()
    {
        Debug.Log("[MusicManager] Awake()");

        if (instance == null)
        {
          
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("[MusicManager] Start()");

        PrepareTrack(normalTheme);
        PrepareTrack(combatTheme);
        PrepareTrack(cutsceneTheme);

        switch (startWith)
        {
            case StartingTheme.Normal:
                PlayTheme(normalTheme);
                break;
            case StartingTheme.Combat:
                PlayTheme(combatTheme);
                break;
            case StartingTheme.Cutscene:
                PlayTheme(cutsceneTheme);
                break;
        }
    }

    public void StartCombatMusic()
    {
        Debug.Log("[MusicManager] Fading to COMBAT theme.");
        StartFade(combatTheme, GetCurrentPlayingTrack());
    }

    public void EndCombatMusic()
    {
        Debug.Log("[MusicManager] Resetting to normal theme after combat.");

        // Stop all other tracks manually
        if (combatTheme != null) combatTheme.Stop();
        if (cutsceneTheme != null) cutsceneTheme.Stop();

        // Reset normal theme
        if (normalTheme != null)
        {
            normalTheme.volume = 1f;
            if (!normalTheme.isPlaying)
            {
                normalTheme.Play();
                Debug.Log("[MusicManager] Normal theme restarted.");
            }
        }
    }

    public void StartCutsceneMusic()
    {
        Debug.Log("[MusicManager] Fading to CUTSCENE theme.");
        StartFade(cutsceneTheme, GetCurrentPlayingTrack());
    }

    public void StopCutsceneMusic()
    {
        Debug.Log("[MusicManager] Fading back to NORMAL theme.");
        StartFade(normalTheme, GetCurrentPlayingTrack());
    }

    public void SwitchTo(AudioSource newTrack)
    {
        AudioSource current = GetCurrentPlayingTrack();

        if (current != null && current == newTrack)
        {
            Debug.Log("[MusicManager] New track is already playing.");
            return;
        }

        Debug.Log("[MusicManager] Switching to new track: " + newTrack?.clip?.name);
        StartFade(newTrack, current);
    }

    private AudioSource GetCurrentPlayingTrack()
    {
        if (normalTheme != null && normalTheme.isPlaying) return normalTheme;
        if (combatTheme != null && combatTheme.isPlaying) return combatTheme;
        if (cutsceneTheme != null && cutsceneTheme.isPlaying) return cutsceneTheme;
        return null;
    }

    private void StartFade(AudioSource fadeIn, AudioSource fadeOut)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeMusic(fadeIn, fadeOut));
    }

    private IEnumerator FadeMusic(AudioSource fadeIn, AudioSource fadeOut)
    {
        if (fadeIn != null && !fadeIn.isPlaying)
        {
            fadeIn.volume = 0f;
            fadeIn.Play();
            Debug.Log("[MusicManager] Fade-in track started.");
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            if (fadeOut != null)
                fadeOut.volume = Mathf.Lerp(1f, 0f, t);

            if (fadeIn != null)
                fadeIn.volume = Mathf.Lerp(0f, 1f, t);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (fadeOut != null)
        {
            fadeOut.Stop();
            fadeOut.volume = 1f;
            Debug.Log("[MusicManager] Fade-out track stopped.");
        }

        if (fadeIn != null)
        {
            fadeIn.volume = 1f;
        }
    }

    private void PrepareTrack(AudioSource track)
    {
        if (track != null)
        {
            track.loop = true;
            track.volume = 0f;
            track.Stop();
        }
    }

    private void PlayTheme(AudioSource track)
    {
        if (track != null)
        {
            track.volume = 1f;
            track.Play();
            Debug.Log("[MusicManager] Playing: " + track.clip?.name);
        }
    }
}
