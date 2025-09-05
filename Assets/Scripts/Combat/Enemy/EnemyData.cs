using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Combat/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite enemySprite;      // For the UI Image.
    public Sprite battleBackground; // Optional background per enemy.
    public Vector2 uiScale = Vector2.one;  // Optional for enemy size scaling.

    public int maxHealth;
    public List<CombatAction> actions;
    public int evasionRate;
    public AudioClip battleStartSound;
}
