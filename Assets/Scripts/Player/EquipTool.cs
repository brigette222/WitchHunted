using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipTool : Equip // Inherits from Equip base class
{
    public float attackRate; // Delay between attacks
    private bool attacking; // Whether the tool is currently in an attack state
    public float attackDistance; // Attack range (for hitting enemies/resources)

    [Header("Resource Gathering")]
    public bool doesGatherResources; // Whether this tool can gather resources

    [Header("Combat")]
    public bool doesDealDamage; // Whether this tool deals combat damage
    public int damage; // Amount of damage dealt

    private Animator anim; // Reference to Animator
    private Camera cam; // Reference to main camera

    void Awake() // Called before Start
    {
        anim = GetComponent<Animator>(); // Cache animator
        cam = Camera.main; // Cache main camera
    }

    public override void OnAttackInput() // Called when attack input is pressed
    {
        if (!attacking) // Only attack if not already attacking
        {
            attacking = true; // Mark as attacking
            anim.SetTrigger("Attack"); // Play attack animation
            Invoke("OnCanAttack", attackRate); // Reset attack state after cooldown
        }
    }

    void OnCanAttack() // Resets attack availability
    {
        attacking = false; // Allow attacking again
    }
}