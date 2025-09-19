using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource normalTheme; // Background theme
    public AudioSource combatTheme; // Combat music
    public AudioSource cutsceneTheme; // Cutscene music
    public float fadeDuration = 1.5f; // Time it takes to fade between tracks

    public enum StartingTheme { Normal, Combat, Cutscene } // Possible starting tracks
    public StartingTheme startWith = StartingTheme.Normal; // Which theme starts first

    private Coroutine currentFade; // Active fade coroutine
    private static MusicManager instance; // Singleton instance

    void Awake() // Runs before Start
    {
        if (instance == null) instance = this; // Assign singleton
        else { Destroy(gameObject); return; } // Enforce single instance
    }

    void Start() // Initialize and start selected theme
    {
        PrepareTrack(normalTheme); // Prepare normal track
        PrepareTrack(combatTheme); // Prepare combat track
        PrepareTrack(cutsceneTheme); // Prepare cutscene track

        switch (startWith) // Choose starting theme
        {
            case StartingTheme.Normal: PlayTheme(normalTheme); break;
            case StartingTheme.Combat: PlayTheme(combatTheme); break;
            case StartingTheme.Cutscene: PlayTheme(cutsceneTheme); break;
        }
    }

    public void StartCombatMusic() => StartFade(combatTheme, GetCurrentPlayingTrack()); // Fade into combat
    public void EndCombatMusic() // Reset to normal after combat
    {
        if (combatTheme != null) combatTheme.Stop(); // Stop combat
        if (cutsceneTheme != null) cutsceneTheme.Stop(); // Stop cutscene
        if (normalTheme != null) // Restart normal
        {
            normalTheme.volume = 1f;
            if (!normalTheme.isPlaying) normalTheme.Play();
        }
    }

    public void StartCutsceneMusic() => StartFade(cutsceneTheme, GetCurrentPlayingTrack()); // Fade into cutscene
    public void StopCutsceneMusic() => StartFade(normalTheme, GetCurrentPlayingTrack()); // Fade back to normal

    public void SwitchTo(AudioSource newTrack) // Switch to a specific track
    {
        AudioSource current = GetCurrentPlayingTrack(); // Get current
        if (current != null && current == newTrack) return; // Already playing
        StartFade(newTrack, current); // Switch with fade
    }

    private AudioSource GetCurrentPlayingTrack() // Returns whichever track is active
    {
        if (normalTheme != null && normalTheme.isPlaying) return normalTheme;
        if (combatTheme != null && combatTheme.isPlaying) return combatTheme;
        if (cutsceneTheme != null && cutsceneTheme.isPlaying) return cutsceneTheme;
        return null;
    }

    private void StartFade(AudioSource fadeIn, AudioSource fadeOut) // Begin fade transition
    {
        if (currentFade != null) StopCoroutine(currentFade); // Stop previous fade
        currentFade = StartCoroutine(FadeMusic(fadeIn, fadeOut)); // Start new one
    }

    private IEnumerator FadeMusic(AudioSource fadeIn, AudioSource fadeOut) // Smoothly fade between two tracks
    {
        if (fadeIn != null && !fadeIn.isPlaying) // Start fade-in track if needed
        {
            fadeIn.volume = 0f;
            fadeIn.Play();
        }

        float timer = 0f;
        while (timer < fadeDuration) // Perform crossfade
        {
            float t = timer / fadeDuration;
            if (fadeOut != null) fadeOut.volume = Mathf.Lerp(1f, 0f, t);
            if (fadeIn != null) fadeIn.volume = Mathf.Lerp(0f, 1f, t);
            timer += Time.unscaledDeltaTime; // Unscaled for pause safety
            yield return null;
        }

        if (fadeOut != null) { fadeOut.Stop(); fadeOut.volume = 1f; } // Stop old track
        if (fadeIn != null) fadeIn.volume = 1f; // Ensure fade-in is maxed
    }

    private void PrepareTrack(AudioSource track) // Reset track for playback
    {
        if (track != null)
        {
            track.loop = true;
            track.volume = 0f;
            track.Stop();
        }
    }

    private void PlayTheme(AudioSource track) // Instantly play a theme
    {
        if (track != null)
        {
            track.volume = 1f;
            track.Play();
        }
    }
}
