using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionAnimator : MonoBehaviour
{
    public Animator animator;
    public RawImage transitionImage;
    public GameObject combatUI;

    [Header("Animation Durations")]
    public float circleDuration = 1f;
    public float slashDuration = 1f;

    private static TransitionAnimator instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlaySceneEntry();
    }

    // ============ LEVEL TRANSITION ============

    public static void TriggerSceneTransition(string sceneName)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.PlayAndLoad(sceneName));
        }
        else
        {
            Debug.LogWarning("No TransitionAnimator found. Loading scene instantly.");
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator PlayAndLoad(string sceneName)
    {
        EnableImage();
        animator.Play("CircleToBlack");
        yield return new WaitForSeconds(instance.circleDuration);

        SceneManager.LoadScene(sceneName);

        // Wait one frame to allow scene to load, then refresh canvas
        yield return null;
        Canvas.ForceUpdateCanvases();
        Debug.Log("[TransitionAnimator] Canvas forced to update after scene load.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Canvas.ForceUpdateCanvases(); // Backup: refresh canvas again
        Debug.Log($"[TransitionAnimator] Scene loaded: {scene.name}");

        if (animator == null)
        {
            Debug.LogWarning("[TransitionAnimator] No animator found. Skipping entry animation.");
            return;
        }

        PlaySceneEntry();
    }

    private void PlaySceneEntry()
    {
        if (animator == null)
        {
            Debug.LogWarning("[TransitionAnimator] Animator is null in PlaySceneEntry(). Skipping.");
            return;
        }

        EnableImage();
        animator.Play("CircleFromBlack");
        StartCoroutine(DisableAfter("CircleFromBlack", circleDuration));
    }

    // ============ COMBAT TRANSITION ============

    public static void StartCombatTransition(System.Action onComplete)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.SlashTransitionIn(onComplete));
        }
    }

    public static void EndCombatTransition(System.Action onComplete)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.CircleTransitionOut(onComplete));
        }
    }

    private IEnumerator SlashTransitionIn(System.Action onComplete)
    {
        Debug.Log("[TransitionAnimator] Starting combat IN transition.");

        EnableImage();
        animator.Play("SlashToBlack");

        MusicManager music = FindObjectOfType<MusicManager>();
        if (music != null)
        {
            music.StartCombatMusic();
            Debug.Log("[TransitionAnimator] Combat music triggered.");
        }

        yield return new WaitForSeconds(slashDuration);

        if (combatUI != null)
        {
            combatUI.SetActive(true);
            Debug.Log("[TransitionAnimator] Combat UI ENABLED.");
        }

        animator.Play("SlashFromBlack");
        Debug.Log("[TransitionAnimator] SlashFromBlack started. RawImage will disable in 1 second.");
        StartCoroutine(DisableImageAfterDelay(1f));

        onComplete?.Invoke();
    }

    private IEnumerator CircleTransitionOut(System.Action onComplete)
    {
        Debug.Log("[TransitionAnimator] Starting combat OUT transition.");

        EnableImage();
        animator.Play("CircleToBlack");
        yield return new WaitForSeconds(circleDuration);

        if (combatUI != null)
        {
            combatUI.SetActive(false);
            Debug.Log("[TransitionAnimator] Combat UI DISABLED.");
        }

        animator.Play("CircleFromBlack");
        Debug.Log("[TransitionAnimator] Playing CircleFromBlack.");
        StartCoroutine(DisableImageAfterDelay(circleDuration + 0.1f));

        onComplete?.Invoke();
    }

    // ============ Helpers ============

    private void EnableImage()
    {
        if (transitionImage != null)
        {
            transitionImage.enabled = true;
            Debug.Log("[TransitionAnimator] RawImage ENABLED.");
        }
    }

    private void DisableImage()
    {
        if (transitionImage != null)
        {
            transitionImage.enabled = false;
            Debug.Log("[TransitionAnimator] RawImage DISABLED.");
        }
    }

    private IEnumerator DisableImageAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        DisableImage();
    }

    private IEnumerator DisableAfter(string clipName, float duration)
    {
        yield return new WaitForSeconds(duration + 0.1f);
        DisableImage();
    }
}