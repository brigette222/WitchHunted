using UnityEngine; // Unity engine core features

public class CombatTrigger2D : MonoBehaviour // Detects enemies and initiates combat
{
    public GameObject combatUI; // UI container for combat
    public float detectionRadius = 1.5f; // Range to detect enemies
    public LayerMask enemyLayer; // Layer mask used to identify enemies

    private bool inCombat = false; // Tracks whether combat is already active

    void Update() // Runs every frame
    {
        if (inCombat) return; // Skip if combat already triggered

        // Detect enemies in range
        Collider2D nearbyEnemy = Physics2D.OverlapCircle(transform.position, detectionRadius, enemyLayer);
        if (nearbyEnemy != null)
        {
            Character worldEnemy = nearbyEnemy.GetComponent<Character>(); // Get Character component
            if (worldEnemy != null)
            {
                TriggerCombat(worldEnemy); // Start combat
            }
        }
    }

    void TriggerCombat(Character worldEnemy) // Handles setup and transition into combat
    {
        TransitionAnimator.StartCombatTransition(() =>
        {
            Character player = FindCombatUIPlayer(); // Get player character from combat UI
            if (player == null) return; // Exit if player not found

            string enemyType = worldEnemy.name.Replace("(Clone)", "").Trim(); // Clean up name
            worldEnemy.gameObject.SetActive(false); // Hide world enemy

            Character combatEnemy = FindCombatUIEnemy(enemyType); // Find matching combat UI enemy
            if (combatEnemy == null) return; // Exit if match not found

            player.gameObject.SetActive(true); // Enable player in UI
            combatEnemy.gameObject.SetActive(true); // Enable enemy in UI

            player.ResetCharacter(); // Reset player HP/status
            combatEnemy.ResetCharacter(); // Reset enemy HP/status

            player.ResetStartPos(); // Cache UI start position
            combatEnemy.ResetStartPos();

            player.SetOpponent(combatEnemy); // Link opponents
            combatEnemy.SetOpponent(player);

            TurnManager.Instance.StartCombat(player, combatEnemy); // Begin combat turns
            CombatManager.Instance.SetupHealthBars(player, combatEnemy); // Setup HP bars

            if (PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.None)
            {
                PauseManager.Instance.Pause(PauseType.Combat); // Pause overworld
            }

            inCombat = true; // Mark as in combat
        });
    }

    Character FindCombatUIPlayer() // Returns player character from UI
    {
        return combatUI.GetComponentInChildren<PlayerCharacter>(true);
    }

    Character FindCombatUIEnemy(string enemyType) // Finds matching UI enemy by name
    {
        Character[] allChars = combatUI.GetComponentsInChildren<Character>(true);
        var rootEnemies = new System.Collections.Generic.List<Character>();

        foreach (var c in allChars)
        {
            if (c != null && c.GetComponent<EnemyAI>() != null)
                rootEnemies.Add(c); // Only include root enemies
        }

        Character selectedRoot = null;
        foreach (var root in rootEnemies)
        {
            if (root.name.Contains(enemyType))
            {
                selectedRoot = root;
                break;
            }
        }

        if (selectedRoot == null && rootEnemies.Count > 0)
        {
            selectedRoot = rootEnemies[0]; // Fallback if no match
        }
        else if (selectedRoot == null)
        {
            return null; // No enemies found
        }

        selectedRoot.gameObject.SetActive(true); // Activate selected enemy
        foreach (Transform t in selectedRoot.transform.GetComponentsInChildren<Transform>(true))
            t.gameObject.SetActive(true); // Activate all children

        foreach (var root in rootEnemies)
        {
            if (root != selectedRoot)
                root.gameObject.SetActive(false); // Deactivate others
        }

        return selectedRoot;
    }

    public void EndCombat() // Called when combat finishes
    {
        combatUI.SetActive(false); // Hide UI

        if (PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.Combat)
        {
            PauseManager.Instance.Resume(); // Resume overworld
        }

        MusicManager musicManager = FindObjectOfType<MusicManager>();
        if (musicManager != null)
        {
            musicManager.EndCombatMusic(); // Stop combat music
        }

        inCombat = false; // Allow future encounters
    }

    void OnDrawGizmosSelected() // Draws detection range in Scene view
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}