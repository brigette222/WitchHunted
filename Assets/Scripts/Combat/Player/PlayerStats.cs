using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Combat/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public int currentHealth;
    public int maxHealth;

    public int currentMagic;
    public int maxMagic;

    public int currentStamina;
    public int maxStamina;
}