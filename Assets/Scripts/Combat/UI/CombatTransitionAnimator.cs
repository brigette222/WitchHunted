using System.Collections;
using UnityEngine;

public class CombatTransitionAnimator : MonoBehaviour
{
    public static CombatTransitionAnimator Instance;

    [Header("GameObjects")]
    public GameObject combatTransitionImage; // RawImage with combat animator (slashes)
    public GameObject levelTransitionImage;  // RawImage with level animator (circles)
    public GameObject combatUI;              // Combat UI canvas

    [Header("Delays")]
    public float transitionDelay = 0.5f;     // Extra delay after animation before hiding image

    private Animator animator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            animator = combatTransitionImage.GetComponent<Animator>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void StartCombat(System.Action onComplete)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.TransitionIn(onComplete));
        }
    }

    public static void EndCombat(System.Action onComplete)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.TransitionOut(onComplete));
        }
    }

    private IEnumerator TransitionIn(System.Action onComplete)
    {
        Debug.Log("[CombatTransition] IN: Disabling level transition, enabling combat transition image.");

        if (levelTransitionImage != null) levelTransitionImage.SetActive(false);
        combatTransitionImage.SetActive(true);

        animator.Rebind();
        animator.Play("SlashToBlack");
        Debug.Log("[CombatTransition] Playing SlashToBlack");
        yield return WaitForClip("SlashToBlack");

        Debug.Log("[CombatTransition] Enabling combat UI.");
        combatUI.SetActive(true);

        animator.Play("SlashFromBlack");
        Debug.Log("[CombatTransition] Playing SlashFromBlack");

        // Wait for full animation duration + delay
        yield return new WaitForSeconds(GetCurrentClipLength() + transitionDelay);

        combatTransitionImage.SetActive(false);
        Debug.Log("[CombatTransition] IN complete. Transition image disabled.");

        onComplete?.Invoke();
    }

    private IEnumerator TransitionOut(System.Action onComplete)
    {
        Debug.Log("[CombatTransition] OUT: Enabling combat transition image.");
        combatTransitionImage.SetActive(true);

        animator.Rebind();
        animator.Play("CircleToBlack");
        Debug.Log("[CombatTransition] Playing CircleToBlack");
        yield return WaitForClip("CircleToBlack");

        Debug.Log("[CombatTransition] Disabling combat UI.");
        combatUI.SetActive(false);

        animator.Play("CircleFromBlack");
        Debug.Log("[CombatTransition] Playing CircleFromBlack");

        // Wait for full animation duration + delay
        yield return new WaitForSeconds(GetCurrentClipLength() + transitionDelay);

        combatTransitionImage.SetActive(false);
        if (levelTransitionImage != null) levelTransitionImage.SetActive(true);
        Debug.Log("[CombatTransition] OUT complete. Transition image disabled, level transition re-enabled.");

        onComplete?.Invoke();
    }

    private IEnumerator WaitForClip(string clipName)
    {
        Debug.Log($"[CombatTransition] Waiting for animation: {clipName}");

        // Wait until the correct clip is playing
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(clipName))
            yield return null;

        float duration = animator.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log($"[CombatTransition] '{clipName}' running for {duration} seconds");
        yield return new WaitForSeconds(duration);
    }

    private float GetCurrentClipLength()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }
}

