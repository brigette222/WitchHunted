using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour 
{
    public static VFXManager Instance { get; private set; }

    [SerializeField] private List<GameObject> vfxPrefabs; // List of VFX GameObjects set in Inspector
    private Dictionary<string, GameObject> vfxDictionary = new Dictionary<string, GameObject>(); // Map names to objects

    void Awake()
    {
        if (Instance != null && Instance != this) // Singleton pattern
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeVFXDictionary(); // Populate dictionary on start
    }

    private void InitializeVFXDictionary()
    {
        vfxDictionary.Clear(); // Reset dictionary

        foreach (GameObject vfx in vfxPrefabs) // Loop through prefabs
        {
            if (vfx != null)
            {
                vfxDictionary[vfx.name] = vfx; // Add to dictionary by name
                vfx.SetActive(false); // Ensure all VFX are disabled initially
            }
        }
    }

    public void PlayVFX(string vfxName)
    {
        if (!vfxDictionary.ContainsKey(vfxName)) return; // Ignore if not found

        GameObject vfx = vfxDictionary[vfxName];
        vfx.SetActive(true); // Show VFX

        Animator animator = vfx.GetComponent<Animator>(); // Try to get animator
        if (animator)
        {
            animator.Rebind(); // Reset animation state
            animator.Play(0); // Play first animation state
            StartCoroutine(DisableVFXAfterAnimation(vfx, animator)); // Disable after play
        }
        else
        {
            StartCoroutine(DisableVFXAfterAnimation(vfx, null)); // Fallback timer
        }
    }

    private IEnumerator DisableVFXAfterAnimation(GameObject vfx, Animator animator)
    {
        float animationLength = animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 1.0f; // Use animation length or default
        yield return new WaitForSeconds(animationLength); // Wait before hiding
        vfx.SetActive(false); // Hide VFX
    }
}