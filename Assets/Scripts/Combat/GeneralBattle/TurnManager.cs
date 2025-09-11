using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;



// Manages turn-based combat flow between registered characters.



public class TurnManager : MonoBehaviour
{
    [SerializeField] private float nextTurnDelay = 1.0f;

    private List<Character> characters = new List<Character>(); // Turn order list.
    private int curCharacterIndex = -1; // Current index in turn list.
    public Character CurrentCharacter; // Current character taking a turn.

    public event UnityAction<Character> OnBeginTurn;
    public event UnityAction<Character> OnEndTurn;

    public static TurnManager Instance;

    private PlayerNeeds playerNeeds; // Reference to overworld player stats.

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void OnEnable() => Character.OnDie += OnCharacterDie; // Listen for deaths.
    void OnDisable() => Character.OnDie -= OnCharacterDie;

    public void StartCombat(Character player, Character enemy)
    {
        characters.Clear();
        RegisterCharacter(player);
        RegisterCharacter(enemy);
        curCharacterIndex = -1;

        playerNeeds = FindObjectOfType<PlayerNeeds>(); // Link to overworld health.

        if (playerNeeds != null)
        {
            player.CurHp = Mathf.RoundToInt(playerNeeds.health.curValue);
            player.MaxHp = Mathf.RoundToInt(playerNeeds.health.maxValue);
        }

        enemy.CurHp = enemy.MaxHp; // Reset enemy HP.
        BeginNextTurn();
    }

    public void RegisterCharacter(Character character)
    {
        if (!characters.Contains(character)) characters.Add(character); // Add to turn order.
    }

    public void BeginNextTurn()
    {
        if (characters.Count == 0) return;

        curCharacterIndex++;
        if (curCharacterIndex >= characters.Count) curCharacterIndex = 0; // Loop back.

        if (curCharacterIndex < 0 || curCharacterIndex >= characters.Count) return;

        CurrentCharacter = characters[curCharacterIndex];
        if (CurrentCharacter == null) return;

        OnBeginTurn?.Invoke(CurrentCharacter); // Fire turn start event.
    }

    public void EndTurn()
    {
        if (CurrentCharacter == null || characters.Count <= 1) return;

        OnEndTurn?.Invoke(CurrentCharacter); // Fire turn end event.
        Invoke(nameof(BeginNextTurn), nextTurnDelay); // Delay before next turn.
    }

    void OnCharacterDie(Character character)
    {
        characters.Remove(character); // Remove dead character.

        if (character.IsPlayer)
        {
            StartCoroutine(GoToGameOverScreen()); // Loss condition.
        }
        else
        {
            if (characters.Count == 1) StartCoroutine(DelayedCombatEnd()); // Win condition.
        }
    }

    IEnumerator DelayedCombatEnd()
    {
        yield return new WaitForSeconds(2f);

        if (playerNeeds != null && CurrentCharacter != null)
        {
            playerNeeds.health.curValue = CurrentCharacter.CurHp;
            playerNeeds.UpdateUI(); // Sync health to overworld.
        }

        CombatManager.Instance.EndCombat();
    }

    IEnumerator GoToGameOverScreen()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("GameOver"); // Load Game Over scene.
    }

    public void ClearCombatState()
    {
        characters.Clear();
        curCharacterIndex = -1;
        CurrentCharacter = null;
    }
}