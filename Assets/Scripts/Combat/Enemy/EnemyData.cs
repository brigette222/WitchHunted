using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Combat/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName; // Display name of the enemy
    public Sprite enemySprite; // Sprite used for enemy in combat UI
    public Sprite battleBackground; // Optional background shown during combat with this enemy
    public Vector2 uiScale = Vector2.one; // Scale adjustment for enemy sprite in UI

    public int maxHealth; // Maximum health of the enemy
    public List<CombatAction> actions; // List of combat actions the enemy can perform
    public int evasionRate; // Chance to evade attacks
    public AudioClip battleStartSound; // Optional sound that plays when battle begins
}
