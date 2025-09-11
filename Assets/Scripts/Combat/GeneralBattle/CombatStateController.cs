using UnityEngine;

public class CombatStateManager : MonoBehaviour
{
    public static CombatStateManager Instance { get; private set; }

    public bool IsCombatActive { get; private set; }

    private void Awake()
    {
        Debug.Log("[CombatStateManager] Awake called.");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CombatStateManager] Duplicate instance found. Destroying self.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[CombatStateManager] Singleton instance set and marked DontDestroyOnLoad.");
    }

    public void SetCombatActive(bool isActive)
    {
        IsCombatActive = isActive;
        Debug.Log("[CombatStateManager] Combat active: " + isActive);
    }
}