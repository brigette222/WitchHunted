using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Visuals")]
public class EnemyVisualData : ScriptableObject
{
    [Header("Walk Sprites")]
    public Sprite[] walkDown;
    public Sprite[] walkUp;
    public Sprite[] walkLeft;
    public Sprite[] walkRight;

    [Header("Idle Sprites")]
    public Sprite[] idleDown;
    public Sprite[] idleUp;
    public Sprite[] idleLeft;
    public Sprite[] idleRight;

    [Header("Chase Sprites")]
    public Sprite[] chaseDown;
    public Sprite[] chaseUp;
    public Sprite[] chaseLeft;
    public Sprite[] chaseRight;
}
