using UnityEngine; 

[CreateAssetMenu(menuName = "Enemy/Visuals")] 
public class EnemyVisualData : ScriptableObject // Stores enemy sprite sets for different states/directions
{
    [Header("Walk Sprites")]
    public Sprite[] walkDown;  // Walking animation frames (facing down)
    public Sprite[] walkUp;    // Walking animation frames (facing up)
    public Sprite[] walkLeft;  // Walking animation frames (facing left)
    public Sprite[] walkRight; // Walking animation frames (facing right)

    [Header("Idle Sprites")]
    public Sprite[] idleDown;  // Idle animation frames (facing down)
    public Sprite[] idleUp;    // Idle animation frames (facing up)
    public Sprite[] idleLeft;  // Idle animation frames (facing left)
    public Sprite[] idleRight; // Idle animation frames (facing right)

    [Header("Chase Sprites")]
    public Sprite[] chaseDown;  // Chasing animation frames (facing down)
    public Sprite[] chaseUp;    // Chasing animation frames (facing up)
    public Sprite[] chaseLeft;  // Chasing animation frames (facing left)
    public Sprite[] chaseRight; // Chasing animation frames (facing right)
}