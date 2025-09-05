using UnityEngine;

public class PlayerCharacter : Character
{
    void Start()
    {
        IsPlayer = true; // Automatically mark as player when the game starts.
        // Opponent will be set by the CombatTrigger when combat starts.
    }
}