using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TextMeshProUGUI healthText;

    private Character character;

    void OnEnable()
    {
        if (character != null)
        {
            character.OnHealthChange += OnUpdateHealth;
            Debug.Log($"[HealthBarUI] OnEnable — Subscribed to {character.name}'s health change event.");
            OnUpdateHealth();
        }
        else
        {
            Debug.LogWarning("[HealthBarUI] OnEnable fired, but character is NULL — waiting for Setup.");
        }
    }

    void OnDisable()
    {
        if (character != null)
        {
            character.OnHealthChange -= OnUpdateHealth;
            Debug.Log($"[HealthBarUI] OnDisable — Unsubscribed from {character.name}'s health change event.");
        }
    }

    void Start()
    {
        if (character != null)
        {
            Debug.Log($"[HealthBarUI] Start — Updating health for {character.name}.");
            OnUpdateHealth();
        }
        else
        {
            Debug.LogWarning("[HealthBarUI] Start — character is NULL, invoking delayed update.");
            Invoke(nameof(DelayedUpdateCheck), 0.1f);
        }
    }

    void DelayedUpdateCheck()
    {
        if (character != null)
        {
            Debug.Log($"[HealthBarUI] Delayed Update — character is {character.name}, forcing UI update.");
            OnUpdateHealth();
        }
        else
        {
            Debug.LogError("[HealthBarUI] Delayed Update — character STILL NULL. UI won't update.");
        }
    }

    public void Setup(Character assignedCharacter)
    {
        if (character != null)
        {
            character.OnHealthChange -= OnUpdateHealth;
            Debug.Log($"[HealthBarUI] Setup — Unsubscribed from old character {character.name}.");
        }

        character = assignedCharacter;

        if (character == null)
        {
            Debug.LogError("[HealthBarUI] Setup was called with a NULL character. Check combat spawning.");
            return;
        }

        // IMPORTANT: Do NOT change character HP here. UI should only display.
        character.OnHealthChange += OnUpdateHealth;
        Debug.Log($"[HealthBarUI] Setup — Subscribed to {character.name}. Forcing immediate health UI refresh.");

        OnUpdateHealth();
    }

    public void OnUpdateHealth()
    {
        if (character != null)
        {
            healthFill.fillAmount = character.GetHealthPercentage();
            healthText.text = $"{character.CurHp} / {character.MaxHp}";
            Debug.Log($"[HealthBarUI] Updated for {character.name}: {character.CurHp}/{character.MaxHp}");
        }
        else
        {
            Debug.LogError("[HealthBarUI] Tried to update, but character is NULL.");
        }
    }

    public void ForceUpdateHealth()
    {
        if (character != null)
        {
            OnUpdateHealth();
            Debug.Log($"[HealthBarUI] Force updated for {character.name}: {character.CurHp}/{character.MaxHp}");
        }
        else
        {
            Debug.LogError("[HealthBarUI] ForceUpdateHealth called with NULL character.");
        }
    }
}
