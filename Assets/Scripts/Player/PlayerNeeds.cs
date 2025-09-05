using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerNeeds : MonoBehaviour, IDamagable
{
    public static PlayerNeeds instance; // ? Instance for SaveManager

    public Need health;
    public Need magik;
    public Need hunger;
    public Need stamina;

    public float noHungerHealthDecay;
    public float noThirstHealthDecay; // Kept for future, not used now

    [Range(0f, 1f)] public float evasionRate = 0.1f;

    public UnityEvent onTakeDamage;

    private Character combatPlayer;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        health.uiBar = GameObject.Find("Health Value")?.GetComponent<Image>();
        hunger.uiBar = GameObject.Find("Hunger Value")?.GetComponent<Image>();
        magik.uiBar = GameObject.Find("Magik Value")?.GetComponent<Image>();
        stamina.uiBar = GameObject.Find("Stamina Value")?.GetComponent<Image>();

        health.curValue = health.startValue;
        hunger.curValue = hunger.startValue;
        magik.curValue = magik.startValue;
        stamina.curValue = stamina.startValue;

        UpdateUI();
    }

    void Update()
    {
        // ? Pause-aware: stops draining stats if game is paused
        if (PauseManager.Instance != null && PauseManager.Instance.IsAnyPaused())
        {
            return;
        }

        // ? Stat drain/gain
        hunger.Subtract(hunger.decayRate * Time.deltaTime);
        magik.Subtract(magik.decayRate * Time.deltaTime);
        stamina.Add(stamina.regenRate * Time.deltaTime);

        // ? Health penalty only from hunger
        if (hunger.curValue == 0f)
        {
            health.Subtract(noHungerHealthDecay * Time.deltaTime);
        }

        // ? Magik no longer affects health
        // if (magik.curValue == 0f)
        //     health.Subtract(noThirstHealthDecay * Time.deltaTime);

        if (health.curValue == 0f)
        {
            Die();
        }

        UpdateUI();
    }

    public void ApplyTradeCost(string paymentType)
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

    public void UpdateUI()
    {
        if (health.uiBar != null) health.uiBar.fillAmount = Mathf.Clamp01(health.GetPercentage());
        if (hunger.uiBar != null) hunger.uiBar.fillAmount = Mathf.Clamp01(hunger.GetPercentage());
        if (magik.uiBar != null) magik.uiBar.fillAmount = Mathf.Clamp01(magik.GetPercentage());
        if (stamina.uiBar != null) stamina.uiBar.fillAmount = Mathf.Clamp01(stamina.GetPercentage());
    }

    public void Heal(float amount) { health.Add(amount); }
    public void Eat(float amount) { hunger.Add(amount); }
    public void Drink(float amount) { magik.Add(amount); }
    public void Sleep(float amount) { stamina.Subtract(amount); }

    public void TakePhysicalDamage(int amount)
    {
        float roll = Random.Range(0f, 1f);
        if (roll < evasionRate)
            return;

        health.Subtract(amount);
        onTakeDamage?.Invoke();
    }

    public void Die()
    {
        Debug.Log("[PlayerNeeds] Player has died. Loading GameOver scene...");
        SceneManager.LoadScene("GameOver");
    }

    public void SyncHealthToCombat(Character combatCharacter)
    {
        combatPlayer = combatCharacter;
        combatPlayer.CurHp = (int)health.curValue;
        combatPlayer.MaxHp = (int)health.maxValue;
        combatPlayer.EvasionRate = evasionRate;
    }

    public void SyncHealthFromCombat()
    {
        if (combatPlayer != null)
        {
            health.curValue = combatPlayer.CurHp;
            UpdateUI();
        }
    }

    public void SpendMagik(float amount)
    {
        if (magik.curValue >= amount)
        {
            magik.curValue -= amount;
            UpdateUI();
        }
    }

    public void SpendStamina(float amount)
    {
        if (stamina.curValue >= amount)
        {
            stamina.curValue -= amount;
            UpdateUI();
        }
    }
}

[System.Serializable]
public class Need
{
    [HideInInspector] public float curValue;
    public float maxValue;
    public float startValue;
    public float regenRate;
    public float decayRate;
    public Image uiBar;

    public void Add(float amount) { curValue = Mathf.Min(curValue + amount, maxValue); }
    public void Subtract(float amount) { curValue = Mathf.Max(curValue - amount, 0f); }
    public float GetPercentage() { return curValue / maxValue; }
}

public interface IDamagable
{
    void TakePhysicalDamage(int damageAmount);
}