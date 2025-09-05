using UnityEngine;
using System.Collections;

public class CombatTrigger2D : MonoBehaviour
{
    public GameObject combatUI;
    public float detectionRadius = 1.5f;
    public LayerMask enemyLayer;

    private bool inCombat = false;

    void Update()
    {
        if (inCombat) return;

        Collider2D nearbyEnemy = Physics2D.OverlapCircle(transform.position, detectionRadius, enemyLayer);
        if (nearbyEnemy != null)
        {
            Character worldEnemy = nearbyEnemy.GetComponent<Character>();
            if (worldEnemy != null)
            {
                TriggerCombat(worldEnemy);
            }
        }
    }

    void TriggerCombat(Character worldEnemy)
    {
        Debug.Log("[CombatTrigger2D] Starting combat...");

        // ? DO NOT pause yet — wait for combat setup to finish

        TransitionAnimator.StartCombatTransition(() =>
        {
            // Find player character in the UI
            Character player = FindCombatUIPlayer();
            if (player == null)
            {
                Debug.LogError("[CombatTrigger2D] Combat UI PlayerCharacter not found!");
                return;
            }

            // Clean up world enemy
            string enemyType = worldEnemy.name.Replace("(Clone)", "").Trim();
            worldEnemy.gameObject.SetActive(false);

            // Find enemy character in the UI
            Character combatEnemy = FindCombatUIEnemy(enemyType);
            if (combatEnemy == null)
            {
                Debug.LogError($"[CombatTrigger2D] No matching combat UI enemy found for '{enemyType}'!");
                return;
            }

            // Enable and prepare both characters
            player.gameObject.SetActive(true);
            combatEnemy.gameObject.SetActive(true);

            player.ResetCharacter();
            combatEnemy.ResetCharacter();

            player.ResetStartPos();
            combatEnemy.ResetStartPos();

            player.SetOpponent(combatEnemy);
            combatEnemy.SetOpponent(player);

            TurnManager.Instance.StartCombat(player, combatEnemy);
            CombatManager.Instance.SetupHealthBars(player, combatEnemy);

            // ? Pause overworld logic via PauseManager
            if (PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.None)
            {
                PauseManager.Instance.Pause(PauseType.Combat);
                Debug.Log("[CombatTrigger2D] PauseManager ? Combat pause triggered");
            }

            inCombat = true;
        });
    }

    Character FindCombatUIPlayer()
    {
        return combatUI.GetComponentInChildren<PlayerCharacter>(true);
    }

    Character FindCombatUIEnemy(string enemyType)
    {
        Debug.Log($"[CombatTrigger2D] FindCombatUIEnemy('{enemyType}')");

        // We will consider ONLY root enemies (they have EnemyAI).
        Character[] allChars = combatUI.GetComponentsInChildren<Character>(true);

        // First, collect all roots (Character that ALSO have EnemyAI).
        var rootEnemies = new System.Collections.Generic.List<Character>();
        foreach (var c in allChars)
        {
            if (c == null) continue;
            if (c.GetComponent<EnemyAI>() != null) // root marker
            {
                rootEnemies.Add(c);
            }
        }
        Debug.Log($"[CombatTrigger2D] Found {rootEnemies.Count} root enemies (with EnemyAI) under combatUI.");

        // Find the root whose name matches the type
        Character selectedRoot = null;
        foreach (var root in rootEnemies)
        {
            if (root.name.Contains(enemyType))
            {
                selectedRoot = root;
                break;
            }
        }

        if (selectedRoot == null)
        {
            // fall back: pick the first root
            if (rootEnemies.Count > 0)
            {
                selectedRoot = rootEnemies[0];
                Debug.LogWarning($"[CombatTrigger2D] No root matched '{enemyType}'. Falling back to first root: {selectedRoot.name}");
            }
            else
            {
                Debug.LogError($"[CombatTrigger2D] No root enemies (with EnemyAI) found in combatUI!");
                return null;
            }
        }

        // Now, activate ONLY the selected root tree. Do NOT touch other Character objects
        // outside this root (we won't blanket-disable everything anymore).
        // Enable the whole selected root hierarchy:
        selectedRoot.gameObject.SetActive(true);
        foreach (Transform t in selectedRoot.transform.GetComponentsInChildren<Transform>(true))
            t.gameObject.SetActive(true);

        // Deactivate OTHER root trees (only their roots; they will hide their children with them).
        foreach (var root in rootEnemies)
        {
            if (root == selectedRoot) continue;
            root.gameObject.SetActive(false);
        }

        Debug.Log($"[CombatTrigger2D] Selected combat enemy root: {selectedRoot.name}");
        return selectedRoot;
    }

    public void EndCombat()
    {
        Debug.Log("[CombatTrigger2D] Ending combat and resetting state.");
        combatUI.SetActive(false);

        if (PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.Combat)
        {
            PauseManager.Instance.Resume();
            Debug.Log("[CombatTrigger2D] PauseManager ? Combat pause ended");
        }

        // ? Should still be here
        MusicManager musicManager = FindObjectOfType<MusicManager>();
        if (musicManager != null)
        {
            musicManager.EndCombatMusic();
        }

        inCombat = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}