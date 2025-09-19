using UnityEngine;

public class enemyreset : MonoBehaviour
{
    void Start() // Called once when the object is initialized
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsAnyPaused()) // If game is paused
            PauseManager.Instance.Resume(); // Resume the game

        EnemyWalker[] allEnemies = FindObjectsOfType<EnemyWalker>(); // Find all EnemyWalker components in the scene
        foreach (EnemyWalker enemy in allEnemies) enemy.ResetEnemy(); // Reset each enemy so they start moving again
    }
}
