using UnityEngine;

[CreateAssetMenu(fileName = "Combat Action", menuName = "New Combat Action")]
public class CombatAction : ScriptableObject
{
    public enum Type
    {
        Attack,
        Heal
    }

    public AudioClip soundEffect;

    public string DisplayName;
    public Type ActionType;

    [Header("Damage")]
    public int Damage;
    public GameObject ProjectilePrefab;

    [Header("Heal")]
    public int HealAmount;

    [Header("Resource Costs")]
    public float MagicCost;
    public float StaminaCost;

    [Header("VFX Settings")]
    public string VFXPrefabName;
    public string VFXName;
    //  This is now a string, storing the name of the VFX prefab.
}
