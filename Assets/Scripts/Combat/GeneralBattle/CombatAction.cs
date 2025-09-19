using UnityEngine;

[CreateAssetMenu(fileName = "Combat Action", menuName = "New Combat Action")]
public class CombatAction : ScriptableObject
{
    public enum Type { Attack, Heal } // Defines the type of combat action

    public AudioClip soundEffect; // Optional sound played when action is used
    public string DisplayName; // Name shown in UI
    public Type ActionType; // Whether this action is Attack or Heal

    [Header("Damage")]
    public int Damage; // Amount of damage dealt (if Attack)
    public GameObject ProjectilePrefab; // Optional projectile prefab for ranged attacks

    [Header("Heal")]
    public int HealAmount; // Amount healed (if Heal)

    [Header("Resource Costs")]
    public float MagicCost; // Amount of magic required
    public float StaminaCost; // Amount of stamina required

    [Header("VFX Settings")]
    public string VFXPrefabName; // The prefab name of the visual effect to spawn
    public string VFXName; // Human-readable VFX name for reference
}
