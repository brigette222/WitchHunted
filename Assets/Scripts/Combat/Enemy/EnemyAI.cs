using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Character character; // the ROOT enemy Character

    void Awake()
    {
        character = GetComponent<Character>();
        if (character == null)
        {
            Debug.LogError($"[EnemyAI] {name} could not find its Character component!");
        }
    }

    void OnEnable()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnBeginTurn += OnBeginTurn;
        }
        else
        {
            Debug.LogError("[EnemyAI] ERROR: TurnManager instance is NULL!");
        }
    }

    void OnDisable()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnBeginTurn -= OnBeginTurn;
        }
    }

    void OnBeginTurn(Character activeCharacter)
    {
        if (character != activeCharacter) return;

        Character player = FindPlayerCharacter();
        if (player == null)
        {
            Debug.LogError("[EnemyAI] Could not find player character!");
            return;
        }

        CombatAction chosenAction = DecideAction(player);

        // No SetOpponent required — Character.AttackOpponent will resolve
        // the player as fallback if no opponent is set.
        character.CastCombatAction(chosenAction);
    }

    Character FindPlayerCharacter()
    {
        Character[] all = GameObject.FindObjectsOfType<Character>(true);
        foreach (Character c in all)
        {
            if (c.IsPlayer) return c;
        }
        return null;
    }

    // Use your existing decide logic (unchanged)
    CombatAction DecideAction(Character player)
    {
        if (character.CombatActions.Count == 0)
        {
            Debug.LogError($"[EnemyAI] {character.name} has no combat actions assigned!");
            return null;
        }

        foreach (CombatAction action in character.CombatActions)
        {
            if (action.HealAmount > 0 && character.CurHp <= character.MaxHp * 0.3f)
            {
                Debug.Log($"[EnemyAI] {character.name} low HP -> healing: {action.DisplayName}");
                return action;
            }
        }

        if (player.CurHp <= player.MaxHp * 0.3f)
        {
            foreach (CombatAction action in character.CombatActions)
            {
                if (action.Damage <= 15)
                {
                    Debug.Log($"[EnemyAI] Player weak -> use weaker attack: {action.DisplayName}");
                    return action;
                }
            }
        }

        CombatAction randomAttack = character.CombatActions[Random.Range(0, character.CombatActions.Count)];
        Debug.Log($"[EnemyAI] Random attack: {randomAttack.DisplayName}");
        return randomAttack;
    }

    Character PickAttackSource()
    {
        // Collect active limb Characters under this root (exclude the root itself)
        var limbs = new List<Character>();
        Character[] underRoot = GetComponentsInChildren<Character>(true);
        foreach (var c in underRoot)
        {
            if (c == null) continue;
            if (c == character) continue;                 // skip root
            if (!c.gameObject.activeInHierarchy) continue; // only active limbs
            limbs.Add(c);
        }

        if (limbs.Count > 0)
        {
            var limb = limbs[Random.Range(0, limbs.Count)];
            return limb;
        }

        // fallback to root if no limbs
        return character;
    }
}