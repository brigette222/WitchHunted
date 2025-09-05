using UnityEngine;
using static CombatAction;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    // === Targeting (for limb selection) ===
    public Character CurrentTarget { get; private set; }

    // === Singleton / refs ===
    public static CombatManager Instance;
    public GameObject combatUI;

    // === UI Bars ===
    private HealthBarUI playerHealthBar;
    private HealthBarUI enemyHealthBar;

    // === Action state ===
    private CombatAction currentCombatAction;

    // --------------------------------------
    // Lifecycle
    // --------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

    // --------------------------------------
    // Turn hooks (auto-clear selection)
    // --------------------------------------
    void HandleBeginTurn(Character who)
    {
        if (who != null && who.IsPlayer) ClearTarget();
    }

    void HandleEndTurn(Character who)
    {
        if (who != null && who.IsPlayer) ClearTarget();
    }

    // --------------------------------------
    // Targeting API
    // --------------------------------------
    public void SetTarget(Character target)
    {
        CurrentTarget = target;
        if (target != null)
            Debug.Log($"[CombatManager] Selected target set to: {target.name}");
        else
            Debug.Log("[CombatManager] Selected target cleared (null).");
    }

    public Character GetCurrentTarget() => CurrentTarget;

    public void ClearTarget() => CurrentTarget = null;

    // --------------------------------------
    // Health bars setup (robust + debug)
    // --------------------------------------
    public void SetupHealthBars(Character player, Character enemy)
    {
        Debug.Log("[CM] SetupHealthBars CALLED");

        if (combatUI == null)
        {
            Debug.LogError("[CM] combatUI is NULL. Assign it on CombatManager.");
            return;
        }

        Debug.Log($"[CM] combatUI.activeSelf={combatUI.activeSelf}");
        Debug.Log($"[CM] player={(player ? player.name : "NULL")}  enemy={(enemy ? enemy.name : "NULL")}");

        if (player == null || enemy == null)
        {
            Debug.LogError("[CM] Player or Enemy is NULL — aborting bar setup.");
            return;
        }

        // List every HealthBarUI under the combat UI for diagnostics
        var allBars = combatUI.GetComponentsInChildren<HealthBarUI>(true);
        Debug.Log($"[CM] Found {allBars.Length} HealthBarUI under combatUI:");
        for (int i = 0; i < allBars.Length; i++)
        {
            var hb = allBars[i];
            if (hb == null) continue;
            Debug.Log($"[CM]  [{i}] {GetHierarchyPath(hb.transform)} (activeInHierarchy={hb.gameObject.activeInHierarchy})");
        }

        // Try to infer player & enemy bars
        var inferredPlayerBar = FindPlayerBarFromUI();
        var enemyBarUnderEnemy = enemy.GetComponentInChildren<HealthBarUI>(true);

        if (inferredPlayerBar == null && allBars.Length > 0)
        {
            inferredPlayerBar = allBars[0];
            Debug.Log("[CM] Player bar inference FAILED; falling back to first bar under combatUI.");
        }

        var inferredEnemyBar = enemyBarUnderEnemy != null
            ? enemyBarUnderEnemy
            : FindEnemyBarFromUIExcluding(inferredPlayerBar);

        if (inferredEnemyBar == null)
        {
            Debug.LogWarning("[CM] Enemy bar inference FAILED; will try last-resort selection.");
            // last resort: pick any bar that isn't the chosen player bar
            foreach (var hb in allBars)
            {
                if (hb != null && hb != inferredPlayerBar)
                {
                    inferredEnemyBar = hb;
                    Debug.Log($"[CM] Last-resort enemy bar = {GetHierarchyPath(hb.transform)}");
                    break;
                }
            }
        }

        if (inferredPlayerBar == null || inferredEnemyBar == null)
        {
            Debug.LogError($"[CM] Could not assign health bars. playerBar={(inferredPlayerBar != null)} enemyBar={(inferredEnemyBar != null)}");
            return;
        }

        playerHealthBar = inferredPlayerBar;
        enemyHealthBar = inferredEnemyBar;

        Debug.Log($"[CM] Assigning PLAYER bar -> {GetHierarchyPath(playerHealthBar.transform)}  to {player.name}");
        Debug.Log($"[CM] Assigning ENEMY  bar -> {GetHierarchyPath(enemyHealthBar.transform)}  to {enemy.name}");

        playerHealthBar.Setup(player);
        enemyHealthBar.Setup(enemy);
    }

    // --------------------------------------
    // End combat flow
    // --------------------------------------
    public void EndCombat()
    {
        StartCoroutine(DelayedCombatEnd());
    }

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

        yield return null; // transition handles timing
    }

    // --------------------------------------
    // Action bookkeeping
    // --------------------------------------
    public void SetCurrentCombatAction(CombatAction action)
    {
        currentCombatAction = action;
        Debug.Log($"[CombatManager] Current combat action set to: {action.DisplayName}");
    }

    public CombatAction GetCurrentCombatAction() => currentCombatAction;

    public void ExecuteAction(Character caster, Character target, CombatAction action)
    {
        if (action == null)
        {
            Debug.LogError("[CombatManager] No combat action provided!");
            return;
        }

        SetCurrentCombatAction(action);
        bool isHealing = action.ActionType == CombatAction.Type.Heal;

        if (isHealing)
        {
            caster.Heal(action.HealAmount);
        }
        else
        {
            if (target == null)
            {
                Debug.LogWarning("[CombatManager] ExecuteAction: target was NULL; action ignored.");
                return;
            }
            target.TakeDamage(action.Damage);
        }
    }

    // --------------------------------------
    // Helpers (private)
    // --------------------------------------
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
            if (path.Contains("player"))
            {
                Debug.Log($"[CM] Inferred PLAYER bar by name: {path}");
                return hb;
            }
        }

        if (all.Length == 2)
        {
            Debug.Log("[CM] Assuming first bar is PLAYER (2 bars found, no name hint).");
            return all[0];
        }

        return null;
    }

    private HealthBarUI FindEnemyBarFromUIExcluding(HealthBarUI exclude)
    {
        var all = combatUI.GetComponentsInChildren<HealthBarUI>(true);

        foreach (var hb in all)
        {
            if (hb == null || hb == exclude) continue;
            string path = GetHierarchyPath(hb.transform).ToLowerInvariant();
            if (path.Contains("enemy") || path.Contains("boss"))
            {
                Debug.Log($"[CM] Inferred ENEMY bar by name: {path}");
                return hb;
            }
        }

        foreach (var hb in all)
        {
            if (hb != null && hb != exclude)
                return hb;
        }

        return null;
    }
}

