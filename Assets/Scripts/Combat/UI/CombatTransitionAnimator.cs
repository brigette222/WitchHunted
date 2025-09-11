using System.Collections;
using UnityEngine;

public class CombatTransitionAnimator : MonoBehaviour //controls transition animations
{
    public static CombatTransitionAnimator Instance; // Singleton reference

    [Header("GameObjects")]
    public GameObject combatTransitionImage; // Slash animation overlay
    public GameObject levelTransitionImage;  // Circle animation overlay
    public GameObject combatUI;              // Combat UI canvas

    [Header("Delays")]
    public float transitionDelay = 0.5f;     // Delay after animation

    private Animator animator; // Animator controlling transitions

    private void Awake()
    {
        if (Instance == null) // Set up singleton
        {
            Instance = this;
            animator = combatTransitionImage.GetComponent<Animator>(); // Cache animator
        }
        else
        {
            Destroy(gameObject); // Enforce single instance
        }
    }

    public static void StartCombat(System.Action onComplete) // Public entry to start combat
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.TransitionIn(onComplete)); // Run coroutine
    }

    public static void EndCombat(System.Action onComplete) // Public entry to end combat
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.TransitionOut(onComplete)); // Run coroutine
    }

    private IEnumerator TransitionIn(System.Action onComplete)
    {
        if (levelTransitionImage != null) levelTransitionImage.SetActive(false); // Hide level transition
        combatTransitionImage.SetActive(true); // Show combat slash screen

        animator.Rebind(); // Reset animator state
        animator.Play("SlashToBlack"); // Play in animation
        yield return WaitForClip("SlashToBlack"); // Wait for animation to finish

        combatUI.SetActive(true); // Enable combat UI
        animator.Play("SlashFromBlack"); // Play out animation

        yield return new WaitForSeconds(GetCurrentClipLength() + transitionDelay); // Wait then continue
        combatTransitionImage.SetActive(false); // Hide transition overlay

        onComplete?.Invoke(); // Callback after transition
    }

    private IEnumerator TransitionOut(System.Action onComplete)
    {
        combatTransitionImage.SetActive(true); // Show overlay

        animator.Rebind(); // Reset animator
        animator.Play("CircleToBlack"); // Play exit animation
        yield return WaitForClip("CircleToBlack"); // Wait for it

        combatUI.SetActive(false); // Hide combat UI
        animator.Play("CircleFromBlack"); // Play re-entry animation

        yield return new WaitForSeconds(GetCurrentClipLength() + transitionDelay); // Wait
        combatTransitionImage.SetActive(false); // Hide overlay
        if (levelTransitionImage != null) levelTransitionImage.SetActive(true); // Show level transition

        onComplete?.Invoke(); // Callback
    }

    private IEnumerator WaitForClip(string clipName)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(clipName)) yield return null; // Wait until correct clip
        float duration = animator.GetCurrentAnimatorStateInfo(0).length; // Get clip length
        yield return new WaitForSeconds(duration); // Wait for clip to finish
    }

    private float GetCurrentClipLength() => animator.GetCurrentAnimatorStateInfo(0).length; // Helper: clip duration
}