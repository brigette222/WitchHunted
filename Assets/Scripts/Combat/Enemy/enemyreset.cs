using UnityEngine;

public class enemyreset : MonoBehaviour
{
    void Start()
    {
        // Resume the game if paused
        if (PauseManager.Instance != null && PauseManager.Instance.IsAnyPaused())
        {
            PauseManager.Instance.Resume();
        }

        // Reset all enemy walkers so they start moving again
        EnemyWalker[] allEnemies = FindObjectsOfType<EnemyWalker>();
        foreach (EnemyWalker enemy in allEnemies)
        {
            enemy.ResetEnemy();
        }
    }
}
