using System.Collections; // For IEnumerator and coroutines
using UnityEngine; // Core Unity
using UnityEngine.SceneManagement; // For scene loading
using UnityEngine.UI; // For RawImage UI element

public class TransitionAnimator : MonoBehaviour
{
    public Animator animator; // Animator that controls transitions
    public RawImage transitionImage; // UI overlay for fade effects
    public GameObject combatUI; // Combat UI to toggle
    public float circleDuration = 1f; // Circle transition time
    public float slashDuration = 1f; // Slash transition time

    private static TransitionAnimator instance; // Singleton reference

    private void Awake()
    {
        if (instance == null) // If no instance exists
        {
            instance = this; // Assign this as the instance
            DontDestroyOnLoad(gameObject); // Persist across scenes
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load event
        }
        else Destroy(gameObject); // Destroy duplicates
    }

    private void Start() { PlaySceneEntry(); } // Play entry animation at start

    // ===== Scene Transition =====
    public static void TriggerSceneTransition(string sceneName) // Start scene transition
    {
        if (instance != null) instance.StartCoroutine(instance.PlayAndLoad(sceneName)); // Use animator if available
        else SceneManager.LoadScene(sceneName); // Otherwise load instantly
    }

    private IEnumerator PlayAndLoad(string sceneName) // Coroutine: fade out, load, then fade in
    {
        EnableImage(); // Show transition overlay
        animator.Play("CircleToBlack"); // Play fade to black
        yield return new WaitForSeconds(instance.circleDuration); // Wait fade duration
        SceneManager.LoadScene(sceneName); // Load new scene
        yield return null; // Wait one frame
        Canvas.ForceUpdateCanvases(); // Force UI refresh
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // Called after scene loads
    {
        Canvas.ForceUpdateCanvases(); // Ensure UI refresh
        if (animator == null) return; // Skip if no animator
        PlaySceneEntry(); // Play entry fade in
    }

    private void PlaySceneEntry() // Entry transition
    {
        if (animator == null) return; // Skip if missing
        EnableImage(); // Show overlay
        animator.Play("CircleFromBlack"); // Fade from black
        StartCoroutine(DisableAfter(circleDuration)); // Disable overlay after animation
    }

    // ===== Combat Transition =====
    public static void StartCombatTransition(System.Action onComplete) { if (instance != null) instance.StartCoroutine(instance.SlashTransitionIn(onComplete)); } // Combat IN
    public static void EndCombatTransition(System.Action onComplete) { if (instance != null) instance.StartCoroutine(instance.CircleTransitionOut(onComplete)); } // Combat OUT

    private IEnumerator SlashTransitionIn(System.Action onComplete) // Combat transition IN
    {
        EnableImage(); // Show overlay
        animator.Play("SlashToBlack"); // Play slash fade
        FindObjectOfType<MusicManager>()?.StartCombatMusic(); // Trigger combat music if found
        yield return new WaitForSeconds(slashDuration); // Wait duration
        if (combatUI) combatUI.SetActive(true); // Enable combat UI
        animator.Play("SlashFromBlack"); // Play reverse slash fade
        StartCoroutine(DisableImageAfterDelay(1f)); // Disable overlay later
        onComplete?.Invoke(); // Call completion callback
    }

    private IEnumerator CircleTransitionOut(System.Action onComplete) // Combat transition OUT
    {
        EnableImage(); // Show overlay
        animator.Play("CircleToBlack"); // Fade to black
        yield return new WaitForSeconds(circleDuration); // Wait duration
        if (combatUI) combatUI.SetActive(false); // Hide combat UI
        animator.Play("CircleFromBlack"); // Fade back in
        StartCoroutine(DisableImageAfterDelay(circleDuration + 0.1f)); // Disable overlay after
        onComplete?.Invoke(); // Call completion callback
    }

    // ===== Helpers =====
    private void EnableImage() { if (transitionImage) transitionImage.enabled = true; } // Show overlay
    private void DisableImage() { if (transitionImage) transitionImage.enabled = false; } // Hide overlay
    private IEnumerator DisableImageAfterDelay(float s) { yield return new WaitForSeconds(s); DisableImage(); } // Hide after delay
    private IEnumerator DisableAfter(float s) { yield return new WaitForSeconds(s + 0.1f); DisableImage(); } // Hide after animation
}