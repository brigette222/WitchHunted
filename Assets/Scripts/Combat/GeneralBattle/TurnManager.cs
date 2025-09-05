using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // <-- Make sure this is included

public class TurnManager : MonoBehaviour
{
    [SerializeField] private float nextTurnDelay = 1.0f;

    private List<Character> characters = new List<Character>();
    private int curCharacterIndex = -1;
    public Character CurrentCharacter;

    public event UnityAction<Character> OnBeginTurn;
    public event UnityAction<Character> OnEndTurn;

    public static TurnManager Instance;

    private PlayerNeeds playerNeeds; // Reference to overworld player stats

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        Character.OnDie += OnCharacterDie;
    }

    void OnDisable()
    {
        Character.OnDie -= OnCharacterDie;
    }

    public void StartCombat(Character player, Character enemy)
    {
        Debug.Log($"[TurnManager] Starting combat between Player ({player.name}) and Enemy ({enemy.name})");

        characters.Clear();
        RegisterCharacter(player);
        RegisterCharacter(enemy);

        curCharacterIndex = -1;

        //  Sync Player stats FROM PlayerNeeds ONLY ONCE
        playerNeeds = FindObjectOfType<PlayerNeeds>();
        if (playerNeeds != null)
        {
            player.CurHp = Mathf.RoundToInt(playerNeeds.health.curValue);
            player.MaxHp = Mathf.RoundToInt(playerNeeds.health.maxValue);

            Debug.Log($"[TurnManager] Linked PlayerNeeds to combat: Player starts with {player.CurHp}/{player.MaxHp} HP");
        }
        else
        {
            Debug.LogError("[TurnManager] PlayerNeeds not found! Health won't sync.");
        }

        //  Enemy HP full at start
        enemy.CurHp = enemy.MaxHp;
        Debug.Log($"[TurnManager] Enemy {enemy.name} HP reset to {enemy.CurHp}/{enemy.MaxHp}");

        BeginNextTurn();
    }

    public void RegisterCharacter(Character character)
    {
        if (!characters.Contains(character))
        {
            characters.Add(character);
            Debug.Log($"[TurnManager] Registered character: {character.name}");
        }
    }

    public void BeginNextTurn()
    {
        Debug.Log("[TurnManager] --------------------");
        Debug.Log($"[TurnManager] BeginNextTurn() CALLED");

        if (characters.Count == 0)
        {
            Debug.LogWarning("[TurnManager] No characters left to take a turn!");
            return;
        }

        curCharacterIndex++;

        if (curCharacterIndex >= characters.Count)
        {
            curCharacterIndex = 0;
            Debug.Log("[TurnManager] Looping back to start of turn order.");
        }

        // ? Safety check in case index is still invalid
        if (curCharacterIndex < 0 || curCharacterIndex >= characters.Count)
        {
            Debug.LogError($"[TurnManager] curCharacterIndex out of bounds: {curCharacterIndex}, list count: {characters.Count}");
            return;
        }

        CurrentCharacter = characters[curCharacterIndex];

        if (CurrentCharacter == null)
        {
            Debug.LogError("[TurnManager] CurrentCharacter is NULL at turn start! Something is very wrong.");
            return;
        }

        Debug.Log($"[TurnManager] It is now {CurrentCharacter.name}'s turn. IsPlayer = {CurrentCharacter.IsPlayer}");
        Debug.Log($"[TurnManager] CurrentCharacter HP at start of turn: {CurrentCharacter.CurHp}/{CurrentCharacter.MaxHp}");

        OnBeginTurn?.Invoke(CurrentCharacter);
    }


    public void EndTurn()
    {
        if (CurrentCharacter == null || characters.Count <= 1)
        {
            Debug.LogWarning("[TurnManager] Skipping turn because combat is likely ending.");
            return;
        }

        Debug.Log($"[TurnManager] {CurrentCharacter.name} ends turn. Calling BeginNextTurn after {nextTurnDelay} seconds.");

        OnEndTurn?.Invoke(CurrentCharacter);
        Invoke(nameof(BeginNextTurn), nextTurnDelay);
    }

    void OnCharacterDie(Character character)
    {
        Debug.Log($"[TurnManager] {character.name} has died. Checking win/lose conditions.");

        characters.Remove(character);

        if (character.IsPlayer)
        {
            Debug.Log("[TurnManager] Player has died — you lose!");
            StartCoroutine(GoToGameOverScreen());
        }
        else
        {
            Debug.Log("[TurnManager] Enemy has died — you win!");

            if (characters.Count == 1) // Only player remains
            {
                StartCoroutine(DelayedCombatEnd());
            }
        }
    }

    IEnumerator DelayedCombatEnd()
    {
        Debug.Log("[TurnManager] Combat will end in 2 seconds...");
        yield return new WaitForSeconds(2f);

        // **Sync Combat Health Overworld Stats (ONLY if player is alive)
        if (playerNeeds != null && CurrentCharacter != null)
        {
            playerNeeds.health.curValue = CurrentCharacter.CurHp;
            playerNeeds.UpdateUI();
            Debug.Log($"[TurnManager] Combat ended. Player's health updated to {playerNeeds.health.curValue}");
        }

        CombatManager.Instance.EndCombat();
    }

    IEnumerator GoToGameOverScreen()
    {
        Debug.Log("[TurnManager] Player is dead. Transitioning to Game Over screen in 2 seconds...");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("GameOver"); //Make sure your Game Over scene is named correctly
    }

    public void ClearCombatState()
    {
        Debug.Log("[TurnManager] Clearing combat state...");
        characters.Clear();
        curCharacterIndex = -1;
        CurrentCharacter = null;
    }
}