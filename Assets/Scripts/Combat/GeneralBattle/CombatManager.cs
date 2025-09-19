using UnityEngine;
using static CombatAction;
using System.Collections;



public class CombatManager : MonoBehaviour // Script Manages turn-based combat

{
    public Character CurrentTarget { get; private set; } // Currently selected target in combat
    public static CombatManager Instance; // Singleton instance of CombatManager
    public GameObject combatUI; // Root UI element containing health bars

    private HealthBarUI playerHealthBar; // Player's health bar UI reference
    private HealthBarUI enemyHealthBar; // Enemy's health bar UI reference
    private CombatAction currentCombatAction; // Currently active combat action

    void Awake() // Runs before Start, handles singleton logic
    {
        if (Instance == null) Instance = this; // Assign this instance
        else Destroy(gameObject); // Destroy duplicates
    }

    void OnEnable() // Subscribe to turn events when enabled
    {
        if (TurnManager.Instance != null) // Ensure TurnManager exists
        {
            TurnManager.Instance.OnBeginTurn += HandleBeginTurn; // Listen for start of turn
            TurnManager.Instance.OnEndTurn += HandleEndTurn; // Listen for end of turn
        }
    }

    void OnDisable() // Unsubscribe when disabled/destroyed
    {
        if (TurnManager.Instance != null) // Ensure TurnManager exists
        {
            TurnManager.Instance.OnBeginTurn -= HandleBeginTurn; // Stop listening to start of turn
            TurnManager.Instance.OnEndTurn -= HandleEndTurn; // Stop listening to end of turn
        }
    }

    void HandleBeginTurn(Character who) // Called at the start of a turn
    {
        if (who != null && who.IsPlayer) ClearTarget(); // Reset target when player’s turn starts
    }

    void HandleEndTurn(Character who) // Called at the end of a turn
    {
        if (who != null && who.IsPlayer) ClearTarget(); // Reset target when player’s turn ends
    }

    public void SetTarget(Character target) => CurrentTarget = target; // Assign a new combat target
    public Character GetCurrentTarget() => CurrentTarget; // Retrieve current target
    public void ClearTarget() => CurrentTarget = null; // Clear the selected target

    public void SetupHealthBars(Character player, Character enemy) // Initializes health bar UI for player and enemy
    {
        if (combatUI == null || player == null || enemy == null) return; // Stop if missing references

        var allBars = combatUI.GetComponentsInChildren<HealthBarUI>(true); // Collect all health bars from UI
        var inferredPlayerBar = FindPlayerBarFromUI(); // Try to infer player’s health bar

        if (inferredPlayerBar == null && allBars.Length > 0) inferredPlayerBar = allBars[0]; // Fallback to first available bar
        var enemyBarUnderEnemy = enemy.GetComponentInChildren<HealthBarUI>(true); // Try to get health bar under enemy
        var inferredEnemyBar = enemyBarUnderEnemy != null ? enemyBarUnderEnemy : FindEnemyBarFromUIExcluding(inferredPlayerBar); // Infer enemy bar

        if (inferredEnemyBar == null) // Last resort fallback
        {
            foreach (var hb in allBars)
            {
                if (hb != null && hb != inferredPlayerBar)
                {
                    inferredEnemyBar = hb;
                    break;
                }
            }
        }

        if (inferredPlayerBar == null || inferredEnemyBar == null) return; // Stop if either bar is missing

        playerHealthBar = inferredPlayerBar; // Assign player health bar
        enemyHealthBar = inferredEnemyBar; // Assign enemy health bar

        playerHealthBar.Setup(player); // Setup player health bar
        enemyHealthBar.Setup(enemy); // Setup enemy health bar
    }

    public void EndCombat() => StartCoroutine(DelayedCombatEnd()); // Starts coroutine to end combat

    IEnumerator DelayedCombatEnd() // Handles post-combat transition
    {
        TransitionAnimator.EndCombatTransition(() => // Play end combat animation, then run callback
        {
            var pn = FindObjectOfType<PlayerNeeds>(); // Find PlayerNeeds
            if (pn != null) pn.SyncHealthFromCombat(); // Sync health values after combat

            var combatTrigger = FindObjectOfType<CombatTrigger2D>(); // Find combat trigger
            if (combatTrigger != null) combatTrigger.EndCombat(); // Notify combat trigger

            if (TurnManager.Instance != null) TurnManager.Instance.ClearCombatState(); // Reset turn manager state
        });

        yield return null; // Wait one frame (transition handles delay itself)
    }

    public void SetCurrentCombatAction(CombatAction action) => currentCombatAction = action; // Set the current action
    public CombatAction GetCurrentCombatAction() => currentCombatAction; // Get the current action

    public void ExecuteAction(Character caster, Character target, CombatAction action) // Executes a combat action
    {
        if (action == null) return; // Stop if no action provided

        SetCurrentCombatAction(action); // Save action
        bool isHealing = action.ActionType == CombatAction.Type.Heal; // Check if healing

        if (isHealing) caster.Heal(action.HealAmount); // Heal caster
        else
        {
            if (target == null) return; // Stop if no target for damage
            target.TakeDamage(action.Damage); // Apply damage
        }
    }

    private string GetHierarchyPath(Transform t) // Builds a string path of transform hierarchy
    {
        if (t == null) return "(null)"; // Handle null
        System.Text.StringBuilder sb = new System.Text.StringBuilder(t.name); // Start with name
        Transform p = t.parent; // Walk up hierarchy
        while (p != null)
        {
            sb.Insert(0, p.name + "/"); // Insert parent name
            p = p.parent; // Continue to next parent
        }
        return sb.ToString(); // Return full path
    }

    private HealthBarUI FindPlayerBarFromUI() // Attempts to find player’s health bar in UI
    {
        var all = combatUI.GetComponentsInChildren<HealthBarUI>(true); // Get all bars
        foreach (var hb in all)
        {
            if (hb == null) continue; // Skip nulls
            string path = GetHierarchyPath(hb.transform).ToLowerInvariant(); // Get lowercase hierarchy path
            if (path.Contains("player")) return hb; // Match if path contains "player"
        }
        if (all.Length == 2) return all[0]; // If only two bars, assume first is player
        return null; // Nothing found
    }

    private HealthBarUI FindEnemyBarFromUIExcluding(HealthBarUI exclude) // Finds enemy’s bar, excluding player’s bar
    {
        var all = combatUI.GetComponentsInChildren<HealthBarUI>(true); // Get all bars
        foreach (var hb in all)
        {
            if (hb == null || hb == exclude) continue; // Skip nulls and excluded bar
            string path = GetHierarchyPath(hb.transform).ToLowerInvariant(); // Get lowercase path
            if (path.Contains("enemy") || path.Contains("boss")) return hb; // Match if enemy or boss
        }
        foreach (var hb in all) // Last fallback
        {
            if (hb != null && hb != exclude) return hb; // Return first valid bar not excluded
        }
        return null; // Nothing found
    }
}