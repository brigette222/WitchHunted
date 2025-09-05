using UnityEngine;

public class EnemyVisualSetup : MonoBehaviour
{
    public EnemyVisualData visualData;

    void Start()
    {
        EnemyWalker walker = GetComponent<EnemyWalker>();
        if (walker == null)
        {
            Debug.LogError("[EnemyVisualSetup] No EnemyWalker found!");
            return;
        }

        if (visualData == null)
        {
            Debug.LogError("[EnemyVisualSetup] No visualData assigned!");
            return;
        }

        walker.SetupAnimationSprites(
            visualData.walkDown, visualData.walkUp, visualData.walkLeft, visualData.walkRight,
            visualData.idleDown, visualData.idleUp, visualData.idleLeft, visualData.idleRight,
            visualData.chaseDown, visualData.chaseUp, visualData.chaseLeft, visualData.chaseRight
        );

        Debug.Log("[EnemyVisualSetup] Visuals assigned for: " + gameObject.name);
    }
}
