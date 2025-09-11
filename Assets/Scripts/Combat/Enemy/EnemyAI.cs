using System.Collections.Generic;
using UnityEngine;

// Controls enemy behavior during its turn in battle
public class EnemyAI : MonoBehaviour
{
    public Character character; // Reference to the Character component.

    void Awake()
    {
        character = GetComponent<Character>(); // Get Character on awake.
    }

    void OnEnable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnBeginTurn += OnBeginTurn; // Subscribe to turn event.
    }

    void OnDisable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnBeginTurn -= OnBeginTurn; // Unsubscribe when disabled.
    }

    void OnBeginTurn(Character activeCharacter)
    {
        if (character != activeCharacter) return; // Act only on this enemy's turn.

        Character player = FindPlayerCharacter(); // Find player to target.
        if (player == null) return;

        CombatAction chosenAction = DecideAction(player); // Choose what to do.
        character.CastCombatAction(chosenAction); // Execute it.
    }

    Character FindPlayerCharacter()
    {
        Character[] all = GameObject.FindObjectsOfType<Character>(true); // Include inactive.
        foreach (Character c in all)
            if (c.IsPlayer) return c; // First found player.
        return null;
    }

    CombatAction DecideAction(Character player)
    {
        if (character.CombatActions.Count == 0) return null; // Can't act.

        foreach (CombatAction action in character.CombatActions)
            if (action.HealAmount > 0 && character.CurHp <= character.MaxHp * 0.3f)
                return action; // Prioritize healing.

        if (player.CurHp <= player.MaxHp * 0.3f)
            foreach (CombatAction action in character.CombatActions)
                if (action.Damage <= 15)
                    return action; // Controlled attack.

        return character.CombatActions[Random.Range(0, character.CombatActions.Count)]; // Fallback: random.
    }

    Character PickAttackSource()// not currently in use
    {
        var limbs = new List<Character>();
        Character[] underRoot = GetComponentsInChildren<Character>(true); // Include inactive children.
        foreach (var c in underRoot)
        {
            if (c == null || c == character || !c.gameObject.activeInHierarchy) continue; // Skip self or inactive.
            limbs.Add(c);
        }

        if (limbs.Count > 0)
            return limbs[Random.Range(0, limbs.Count)]; // Random active limb.

        return character; // Default to self.
    }
}