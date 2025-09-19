using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerNeeds : MonoBehaviour, IDamagable
{
    public static PlayerNeeds instance; // Singleton reference

    public Need health; // Player health stat
    public Need magik; // Player magic stat
    public Need hunger; // Player hunger stat
    public Need stamina; // Player stamina stat

    public float noHungerHealthDecay; // Health drain when hunger is empty
    public float noThirstHealthDecay; // Reserved (unused)

    [Range(0f, 1f)] public float evasionRate = 0.1f; // Chance to evade damage

    public UnityEvent onTakeDamage; // Event triggered when taking damage

    private Character combatPlayer; // Reference to combat system representation

    void Awake() => instance = this; // Assign singleton

    void Start() // Initialize needs
    {
        health.uiBar = GameObject.Find("Health Value")?.GetComponent<Image>(); // Bind UI health bar
        hunger.uiBar = GameObject.Find("Hunger Value")?.GetComponent<Image>(); // Bind UI hunger bar
        magik.uiBar = GameObject.Find("Magik Value")?.GetComponent<Image>(); // Bind UI magic bar
        stamina.uiBar = GameObject.Find("Stamina Value")?.GetComponent<Image>(); // Bind UI stamina bar

        health.curValue = health.startValue; // Reset health
        hunger.curValue = hunger.startValue; // Reset hunger
        magik.curValue = magik.startValue; // Reset magik
        stamina.curValue = stamina.startValue; // Reset stamina

        UpdateUI(); // Refresh UI
    }

    void Update() // Runs every frame
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsAnyPaused()) return; // Skip while paused

        hunger.Subtract(hunger.decayRate * Time.deltaTime); // Drain hunger
        magik.Subtract(magik.decayRate * Time.deltaTime); // Drain magik
        stamina.Add(stamina.regenRate * Time.deltaTime); // Regenerate stamina

        if (hunger.curValue == 0f) health.Subtract(noHungerHealthDecay * Time.deltaTime); // Health decay if starving
        if (health.curValue == 0f) Die(); // Trigger death if health empty

        UpdateUI(); // Refresh UI
    }

    public void ApplyTradeCost(string paymentType) // Handles stat trade costs
    {
        switch (paymentType.ToLower())
        {
            case "blood": health.Subtract(20f); break;
            case "memories": magik.Subtract(15f); break;
            case "rations": hunger.Subtract(25f); break;
            case "sweat": stamina.Subtract(30f); break;
        }
        UpdateUI();
    }

    public void UpdateUI() // Updates UI bars
    {
        if (health.uiBar != null) health.uiBar.fillAmount = Mathf.Clamp01(health.GetPercentage());
        if (hunger.uiBar != null) hunger.uiBar.fillAmount = Mathf.Clamp01(hunger.GetPercentage());
        if (magik.uiBar != null) magik.uiBar.fillAmount = Mathf.Clamp01(magik.GetPercentage());
        if (stamina.uiBar != null) stamina.uiBar.fillAmount = Mathf.Clamp01(stamina.GetPercentage());
    }

    public void Heal(float amount) => health.Add(amount); // Heal health
    public void Eat(float amount) => hunger.Add(amount); // Restore hunger
    public void Drink(float amount) => magik.Add(amount); // Restore magik
    public void Sleep(float amount) => stamina.Subtract(amount); // Reduce stamina (work cost)

    public void TakePhysicalDamage(int amount) // Handles incoming physical damage
    {
        float roll = Random.Range(0f, 1f); // Roll for evasion
        if (roll < evasionRate) return; // Evaded
        health.Subtract(amount); // Apply damage
        onTakeDamage?.Invoke(); // Fire event
    }

    public void Die() => SceneManager.LoadScene("GameOver"); // Load game over scene

    public void SyncHealthToCombat(Character combatCharacter) // Sync stats into combat
    {
        combatPlayer = combatCharacter;
        combatPlayer.CurHp = (int)health.curValue; // Set HP
        combatPlayer.MaxHp = (int)health.maxValue; // Set MaxHP
        combatPlayer.EvasionRate = evasionRate; // Sync evasion
    }

    public void SyncHealthFromCombat() // Sync stats back from combat
    {
        if (combatPlayer != null)
        {
            health.curValue = combatPlayer.CurHp; // Restore HP
            UpdateUI();
        }
    }

    public void SpendMagik(float amount) // Spend magic points
    {
        if (magik.curValue >= amount) { magik.curValue -= amount; UpdateUI(); }
    }

    public void SpendStamina(float amount) // Spend stamina points
    {
        if (stamina.curValue >= amount) { stamina.curValue -= amount; UpdateUI(); }
    }
}

[System.Serializable]
public class Need // Represents a stat
{
    [HideInInspector] public float curValue; // Current value
    public float maxValue; // Maximum value
    public float startValue; // Starting value
    public float regenRate; // Rate of regeneration
    public float decayRate; // Rate of decay
    public Image uiBar; // UI representation

    public void Add(float amount) => curValue = Mathf.Min(curValue + amount, maxValue); // Increase stat
    public void Subtract(float amount) => curValue = Mathf.Max(curValue - amount, 0f); // Decrease stat
    public float GetPercentage() => curValue / maxValue; // Normalized value
}

public interface IDamagable // Interface for damageable entities
{
    void TakePhysicalDamage(int damageAmount); // Called when taking damage
}