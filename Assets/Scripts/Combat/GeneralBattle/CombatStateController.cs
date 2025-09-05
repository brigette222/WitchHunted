using UnityEngine;

public class CombatStateManager : MonoBehaviour
{
    public static CombatStateManager Instance { get; private set; }

    public bool IsCombatActive { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCombatActive(bool isActive)
    {
        IsCombatActive = isActive;
        Debug.Log("[CombatStateManager] Combat active: " + isActive);
    }
}