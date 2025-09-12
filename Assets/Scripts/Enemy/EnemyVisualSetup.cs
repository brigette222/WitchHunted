using UnityEngine;

public class EnemyVisualSetup : MonoBehaviour
{
    public EnemyVisualData visualData; // Reference to sprite data asset

    void Start()
    {
        EnemyWalker walker = GetComponent<EnemyWalker>(); // Get EnemyWalker component
        if (walker == null || visualData == null) return; // Exit if setup is invalid

        // Pass sprite sets from ScriptableObject into the EnemyWalker
        walker.SetupAnimationSprites(
            visualData.walkDown, visualData.walkUp, visualData.walkLeft, visualData.walkRight,
            visualData.idleDown, visualData.idleUp, visualData.idleLeft, visualData.idleRight,
            visualData.chaseDown, visualData.chaseUp, visualData.chaseLeft, visualData.chaseRight
        );
    }
}
