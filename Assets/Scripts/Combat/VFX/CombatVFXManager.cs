using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [SerializeField] private List<GameObject> vfxPrefabs; // Assign VFX manually in Inspector
    private Dictionary<string, GameObject> vfxDictionary = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeVFXDictionary();
    }

    private void InitializeVFXDictionary()
    {
        vfxDictionary.Clear();

        foreach (GameObject vfx in vfxPrefabs)
        {
            if (vfx != null)
            {
                vfxDictionary[vfx.name] = vfx;
                vfx.SetActive(false); // Ensure all are disabled at start
                Debug.Log($"[VFXManager] Registered VFX: {vfx.name}");
            }
            else
            {
                Debug.LogWarning("[VFXManager] Skipping a null VFX entry in the list.");
            }
        }
    }

    public void PlayVFX(string vfxName)
    {
        if (!vfxDictionary.ContainsKey(vfxName))
        {
            Debug.LogWarning($"[VFX] No VFX found with name: {vfxName}");
            return;
        }

        GameObject vfx = vfxDictionary[vfxName];

        // Enable VFX
        vfx.SetActive(true);
        Debug.Log($"[VFX] Enabled {vfx.name}");

        // Reset and play animation
        Animator animator = vfx.GetComponent<Animator>();
        if (animator)
        {
            animator.Rebind();
            animator.Play(0);
            Debug.Log($"[VFX] Playing animation on {vfx.name}");

            StartCoroutine(DisableVFXAfterAnimation(vfx, animator));
        }
        else
        {
            Debug.LogWarning($"[VFX] Animator not found on {vfx.name}, disabling after 1s fallback.");
            StartCoroutine(DisableVFXAfterAnimation(vfx, null));
        }
    }

    private IEnumerator DisableVFXAfterAnimation(GameObject vfx, Animator animator)
    {
        float animationLength = animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 1.0f;
        Debug.Log($"[VFX] Animation length: {animationLength} seconds");

        yield return new WaitForSeconds(animationLength);

        vfx.SetActive(false);
        Debug.Log($"[VFX] Disabled {vfx.name} after animation.");
    }
}