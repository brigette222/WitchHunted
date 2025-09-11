using UnityEngine;
using static CombatAction;
using System.Collections;



// Script Manages turn-based combat, target selection, and UI bar setup.




public class CombatManager : MonoBehaviour
{
    public Character CurrentTarget { get; private set; } // Currently selected target.

    public static CombatManager Instance; // Singleton.
    public GameObject combatUI; // Root UI element for health bars.

    private HealthBarUI playerHealthBar; // Player's health bar UI.
    private HealthBarUI enemyHealthBar;  // Enemy's health bar UI.

    private CombatAction currentCombatAction; // Active combat action.

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // Enforce singleton.
    }

    void OnEnable()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnBeginTurn += HandleBeginTurn;
            TurnManager.Instance.OnEndTurn += HandleEndTurn;
        }
    }

    void OnDisable()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnBeginTurn -= HandleBeginTurn;
            TurnManager.Instance.OnEndTurn -= HandleEndTurn;
        }
    }

    void HandleBeginTurn(Character who)
    {
        if (who != null && who.IsPlayer) ClearTarget(); // Reset target at start of player turn.
    }

    void HandleEndTurn(Character who)
    {
        if (who != null && who.IsPlayer) ClearTarget(); // Reset target at end of player turn.
    }

    public void SetTarget(Character target) => CurrentTarget = target; // Assign target.

    public Character GetCurrentTarget() => CurrentTarget; // Get assigned target.

    public void ClearTarget() => CurrentTarget = null; // Clear target.

    public void SetupHealthBars(Character player, Character enemy)
    {
        if (combatUI == null || player == null || enemy == null) return;

        var allBars = combatUI.GetComponentsInChildren<HealthBarUI>(true); // Collect all bars.

        var inferredPlayerBar = FindPlayerBarFromUI();
        if (inferredPlayerBar == null && allBars.Length > 0) inferredPlayerBar = allBars[0]; // Fallback.

        var enemyBarUnderEnemy = enemy.GetComponentInChildren<HealthBarUI>(true);
        var inferredEnemyBar = enemyBarUnderEnemy != null ? enemyBarUnderEnemy : FindEnemyBarFromUIExcluding(inferredPlayerBar);

        if (inferredEnemyBar == null)
        {
            foreach (var hb in allBars)
            {
                if (hb != null && hb != inferredPlayerBar)
                {
                    inferredEnemyBar = hb; // Last-resort fallback.
                    break;
                }
            }
        }

        if (inferredPlayerBar == null || inferredEnemyBar == null) return;

        playerHealthBar = inferredPlayerBar;
        enemyHealthBar = inferredEnemyBar;

        playerHealthBar.Setup(player);
        enemyHealthBar.Setup(enemy);
    }

    public void EndCombat() => StartCoroutine(DelayedCombatEnd()); // Kick off end transition.

    IEnumerator DelayedCombatEnd()
    {
        TransitionAnimator.EndCombatTransition(() =>
        {
            var pn = FindObjectOfType<PlayerNeeds>();
            if (pn != null) pn.SyncHealthFromCombat();

            var combatTrigger = FindObjectOfType<CombatTrigger2D>();
            if (combatTrigger != null) combatTrigger.EndCombat();

            if (TurnManager.Instance != null) TurnManager.Instance.ClearCombatState();
        });

        yield return null; // Let transition handle delay.
    }

    public void SetCurrentCombatAction(CombatAction action) => currentCombatAction = action; // Assign action.

    public CombatAction GetCurrentCombatAction() => currentCombatAction; // Retrieve action.

    public void ExecuteAction(Character caster, Character target, CombatAction action)
    {
        if (action == null) return;

        SetCurrentCombatAction(action);
        bool isHealing = action.ActionType == CombatAction.Type.Heal;

        if (isHealing)
        {
            caster.Heal(action.HealAmount);
        }
        else
        {
            if (target == null) return;
            target.TakeDamage(action.Damage);
        }
    }

    private string GetHierarchyPath(Transform t)
    {
        if (t == null) return "(null)";
        System.Text.StringBuilder sb = new System.Text.StringBuilder(t.name);
        Transform p = t.parent;
        while (p != null)
        {
            sb.Insert(0, p.name + "/");
            p = p.parent;
        }
        return sb.ToString();
    }

    private HealthBarUI FindPlayerBarFromUI()
    {
        var all = combatUI.GetComponentsInChildren<HealthBarUI>(true);
        foreach (var hb in all)
        {
            if (hb == null) continue;
            string path = GetHierarchyPath(hb.transform).ToLowerInvariant();
            if (path.Contains("player")) return hb;
        }

        if (all.Length == 2) return all[0]; // Default to first.
        return null;
    }

    private HealthBarUI FindEnemyBarFromUIExcluding(HealthBarUI exclude)
    {
        var all = combatUI.GetComponentsInChildren<HealthBarUI>(true);

        foreach (var hb in all)
        {
            if (hb == null || hb == exclude) continue;
            string path = GetHierarchyPath(hb.transform).ToLowerInvariant();
            if (path.Contains("enemy") || path.Contains("boss")) return hb;
        }

        foreach (var hb in all)
        {
            if (hb != null && hb != exclude) return hb;
        }

        return null;
    }
}