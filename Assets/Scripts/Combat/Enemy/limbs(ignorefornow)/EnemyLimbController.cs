using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// Attach ONCE to the root enemy (the one with EnemyAI + the main HealthBarUI).
/// - Aggregates limb HP into root (your “blood” bar)
/// - Instant death if head or torso dies
/// - Death if all limbs are dead
/// - Safe on startup: won’t kill during initial wiring; also restores limb HP if zero
public class EnemyLimbController : MonoBehaviour
{
    [Header("Root (Main Enemy)")]
    [SerializeField] private Character root;             // health bar listens to this Character

    [Header("Vital Limb References (optional)")]
    [SerializeField] private Character head;             // optional explicit ref; else matched by name
    [SerializeField] private Character torso;            // optional explicit ref; else matched by name (body/chest/torso)

    [Header("All Limbs (include head & torso here too)")]
    [SerializeField] private List<Character> limbs = new List<Character>();

    [Header("Auto-Discovery (optional)")]
    [SerializeField] private bool autoDiscoverChildren = false; // good for procedural spawns
    [SerializeField] private Transform limbsParent;             // container for limbs; if null uses this.transform

    // Name aliases for matching
    private static readonly string[] HeadAliases = { "head", "skull" };
    private static readonly string[] TorsoAliases = { "torso", "body", "chest" };

    // Guards
    private bool _isAggregating = false;   // prevent re-entrancy
    private bool _readyForKill = false;    // block kill logic until after init passes
    private bool _subscriptionsDone = false;

    private void Reset()
    {
        if (!root) root = GetComponent<Character>();
    }

    private void OnEnable()
    {
        if (autoDiscoverChildren)
            DiscoverLimbs();

        // Clean list: no nulls, no root, no player
        limbs = limbs
            .Where(l => l != null && l != root && !l.IsPlayer)
            .Distinct()
            .ToList();

        // Subscribe only to limb health (NOT root)
        foreach (var limb in limbs)
            limb.OnHealthChange += OnAnyLimbHealthChanged;

        _subscriptionsDone = true;

        // Defer init to let Combat UI & resets finish
        StartCoroutine(InitFlow());
    }

    private void OnDisable()
    {
        if (!_subscriptionsDone) return;
        foreach (var limb in limbs)
            if (limb != null) limb.OnHealthChange -= OnAnyLimbHealthChanged;
        _subscriptionsDone = false;
    }

    private System.Collections.IEnumerator InitFlow()
    {
        // Wait two frames so CombatTrigger/ResetEnemyStats/healthbars finish their work
        yield return null;
        yield return null;

        // Ensure limbs are alive at start: if CurHp <= 0, set to Max and reactivate
        foreach (var limb in limbs)
        {
            if (limb == null) continue;

            if (limb.CurHp <= 0)
            {
                limb.CurHp = Mathf.Max(0, limb.MaxHp);
                if (!limb.gameObject.activeSelf) limb.gameObject.SetActive(true);
                // SAFE: call method on Character to raise its own event
                limb.NotifyHealthChanged();
            }
        }

        // Initial aggregate WITHOUT kill checks
        RecalculateAggregateAndPushToRoot(allowKill: false);

        // One more frame, then allow kill logic
        yield return null;
        _readyForKill = true;

        // Do a second aggregate now that everyone is wired
        RecalculateAggregateAndPushToRoot(allowKill: true);
    }

    private void OnAnyLimbHealthChanged()
    {
        if (_isAggregating) return;
        if (!_readyForKill) { RecalculateAggregateAndPushToRoot(allowKill: false); return; }
        RecalculateAggregateAndPushToRoot(allowKill: true);
    }

    private void RecalculateAggregateAndPushToRoot(bool allowKill)
    {
        if (root == null || limbs == null || limbs.Count == 0) return;

        _isAggregating = true;

        int totalMax = 0;
        int totalCur = 0;
        bool anyAlive = false;

        foreach (var limb in limbs)
        {
            if (limb == null) continue;
            totalMax += Mathf.Max(0, limb.MaxHp);
            int cur = Mathf.Clamp(limb.CurHp, 0, limb.MaxHp);
            totalCur += cur;
            if (cur > 0) anyAlive = true;
        }

        // Mirror into root (fires OnHealthChange from within Character)
        root.ApplyAggregateHealth(totalCur, totalMax);

        if (allowKill)
        {
            bool headAlive = ResolveVitalAlive(head, HeadAliases);
            bool torsoAlive = ResolveVitalAlive(torso, TorsoAliases);

            // Kill if head or torso is down, or if absolutely no limb has HP left
            if (!headAlive || !torsoAlive || !anyAlive)
            {
                if (!root.IsPlayer && root.gameObject.activeInHierarchy)
                    root.ForceDeath();
            }
        }

        _isAggregating = false;
    }

    private bool ResolveVitalAlive(Character explicitRef, string[] aliases)
    {
        if (explicitRef != null)
            return explicitRef.CurHp > 0 && explicitRef.gameObject.activeInHierarchy;

        // Fallback: find by alias in name
        var match = limbs.FirstOrDefault(l =>
            l != null && aliases.Any(a => l.name.ToLower().Contains(a)));

        // If no match, treat as alive to avoid false insta-kill
        if (match == null) return true;

        return match.CurHp > 0 && match.gameObject.activeInHierarchy;
    }

    private void DiscoverLimbs()
    {
        var rootXform = limbsParent ? limbsParent : transform;
        var found = rootXform.GetComponentsInChildren<Character>(includeInactive: true)
            .Where(c => c != null && c != root && !c.IsPlayer)
            .ToList();

        limbs = found;

        if (head == null)
            head = limbs.FirstOrDefault(l => HeadAliases.Any(a => l.name.ToLower().Contains(a)));
        if (torso == null)
            torso = limbs.FirstOrDefault(l => TorsoAliases.Any(a => l.name.ToLower().Contains(a)));
    }
}