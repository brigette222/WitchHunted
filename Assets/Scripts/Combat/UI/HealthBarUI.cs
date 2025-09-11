using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour 
{
    [SerializeField] private Image healthFill; // UI image to show health bar fill
    [SerializeField] private TextMeshProUGUI healthText; // UI text showing HP numbers

    private Character character; // Character this bar is displaying

    void OnEnable()
    {
        if (character != null)
        {
            character.OnHealthChange += OnUpdateHealth; // Subscribe to health change
            OnUpdateHealth(); // Initial update
        }
    }

    void OnDisable()
    {
        if (character != null)
            character.OnHealthChange -= OnUpdateHealth; // Unsubscribe
    }

    void Start()
    {
        if (character != null)
        {
            OnUpdateHealth(); // Update at start if already assigned
        }
        else
        {
            Invoke(nameof(DelayedUpdateCheck), 0.1f); // Fallback if Setup not called yet
        }
    }

    void DelayedUpdateCheck()
    {
        if (character != null)
            OnUpdateHealth(); // Retry updating if character was assigned late
    }

    public void Setup(Character assignedCharacter)
    {
        if (character != null)
            character.OnHealthChange -= OnUpdateHealth; // Unsubscribe from old character

        character = assignedCharacter; // Assign new character
        if (character == null) return; // Safety check

        character.OnHealthChange += OnUpdateHealth; // Subscribe to health updates
        OnUpdateHealth(); // Immediate refresh
    }

    public void OnUpdateHealth()
    {
        if (character == null) return; // Null check

        healthFill.fillAmount = character.GetHealthPercentage(); // Update fill bar
        healthText.text = $"{character.CurHp} / {character.MaxHp}"; // Update text
    }

    public void ForceUpdateHealth()
    {
        if (character != null)
            OnUpdateHealth(); // Manual external update trigger
    }
}