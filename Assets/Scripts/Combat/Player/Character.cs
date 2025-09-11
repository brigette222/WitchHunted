using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Character : MonoBehaviour // Main class for characters (player or enemy)
{
    public int CurHp; // Current health points
    public int MaxHp; // Maximum health points
    private Image image; // Cached reference to the UI image component (for flash effects)
    private Color originalColor; // Store original color to restore after flash
    public bool IsPlayer; // True if this character is the player
    private static Dictionary<Character, Color> originalColors = new Dictionary<Character, Color>(); // Optionally track original colors for all characters
    public List<CombatAction> CombatActions = new List<CombatAction>(); // List of actions this character can perform

    [Range(0f, 1f)] public float EvasionRate = 0.1f; // Chance to dodge an attack (between 0 and 1)
    [SerializeField] private float moveSpeed = 400f; // Speed for attack movement

    private Character opponent; // Reference to the character being attacked
    private Vector3 startPos; // Position to return to after attacking
    public event UnityAction OnHealthChange; // Event: health changed
    public static event UnityAction<Character> OnDie; // Event: this character died

    [SerializeField] private AudioClip hurtSFX; // Sound to play when hurt
    private AudioSource audioSource; // AudioSource component to play sounds
    public AudioClip healSound; // Sound to play when healed
    private bool actionInProgress = false; // Flag to prevent overlapping actions

    public void SetOpponent(Character newOpponent) // Assign opponent reference
    {
        opponent = newOpponent; // Store new opponent
    }

    public Character GetOpponent() // Get opponent reference
    {
        return opponent; // Return stored opponent
    }

    public void ClearOpponent() // Remove opponent reference
    {
        opponent = null; // Clear variable
    }

    void Start() // Unity’s start method, runs once when this GameObject becomes active
    {
        audioSource = GetComponent<AudioSource>(); // Cache AudioSource component
        image = GetComponent<Image>(); // Cache Image component if exists
        if (image != null) // If there's an Image component
        {
            originalColor = image.color; // Save original color
        }

        bool isTurnTaker = IsPlayer || (GetComponent<EnemyAI>() != null); // Determine if this character should take turns: player or has EnemyAI
        if (isTurnTaker && TurnManager.Instance != null) // If it should and there's a TurnManager
        {
            TurnManager.Instance.RegisterCharacter(this); // Register this character for turn order
        }
    }

    public void FlashColor(Color flashColor) // Method to flash this character’s image (hurt visual)
    {
        string targetName = IsPlayer ? "CombatCharacter" : "CombatEnemy"; // Decide name of child object to use
        Image img = null; // Local image variable

        Transform child = transform.Find(targetName); // Try finding a child by name
        if (child != null) img = child.GetComponent<Image>(); // If found, get its Image component
        if (img == null) img = GetComponent<Image>(); // Fallback: Image on self
        if (img == null) img = GetComponentInChildren<Image>(true); // Fallback: any child Image, active or not

        if (img == null) return; // If still no image, do nothing

        if (audioSource != null && hurtSFX != null) // If hurt sound can be played
        {
            audioSource.PlayOneShot(hurtSFX); // Play the hurt sound
        }

        StartCoroutine(FlashColorCoroutine(img, flashColor)); // Start the flashing coroutine
    }

    public IEnumerator FlashColorCoroutine(Color flashColor, float duration = 0.2f) // Coroutine to flash then reset color
    {
        string targetName = IsPlayer ? "CombatCharacter" : "CombatEnemy"; // Decide child name again
        Image img = null;

        Transform child = transform.Find(targetName);
        if (child != null) img = child.GetComponent<Image>();
        if (img == null) img = GetComponent<Image>();
        if (img == null) img = GetComponentInChildren<Image>(true);

        if (img == null) yield break; // Exit coroutine if no Image

        Color original = img.color; // Store original color
        img.color = flashColor; // Set to flash color
        yield return new WaitForSeconds(duration); // Wait specified duration
        img.color = original; // Reset back
    }

    private IEnumerator FlashColorCoroutine(Image img, Color flashColor) // Overloaded coroutine using direct image
    {
        Color originalColor = img.color; // Save color
        img.color = flashColor; // Flash
        yield return new WaitForSeconds(0.15f); // Short wait
        img.color = originalColor; // Reset
    }

    public void ResetStartPos() // Method to set starting position for movement
    {
        var rt = GetComponent<RectTransform>(); // Check if UI element
        if (rt != null) // If RectTransform exists
        {
            startPos = rt.anchoredPosition3D; // Use anchored position for UI
        }
        else // Otherwise
        {
            startPos = transform.position; // Use world position
        }
    }

    public void TakeDamage(int damageToTake) // Handles incoming damage
    {
        if (CurHp <= 0) return; // Don't damage dead characters

        float roll = Random.Range(0f, 1f); // Roll for evasion chance
        if (roll < EvasionRate) return; // Dodged successfully

        CurHp -= damageToTake; // Apply damage
        FlashColor(Color.red); // Flash red to indicate hit
        OnHealthChange?.Invoke(); // Notify listeners of health change
        if (CurHp <= 0) Die(); // Handle death if HP drops to 0
    }

    void Die() // Handles death logic
    {
        OnDie?.Invoke(this); // Notify global listeners of death

        if (IsPlayer) // If this is the player
        {
            gameObject.SetActive(false); // Deactivate player object
            return;
        }

        bool isMainEnemy = GetComponent<EnemyAI>() != null; // Determine if this is a main enemy
        if (isMainEnemy) // If so
        {
            StartCoroutine(DelayedDeathAndCombatEnd()); // Delay before ending combat
        }
        else
        {
            gameObject.SetActive(false); // Just deactivate
        }
    }

    IEnumerator DelayedDeathAndCombatEnd() // Delayed combat end after enemy death
    {
        yield return new WaitForSeconds(2f); // Wait a bit for VFX/sound
        gameObject.SetActive(false); // Disable the object
        CombatManager.Instance.EndCombat(); // Notify CombatManager to end combat
    }

    public void Heal(int healAmount) // Handles healing
    {
        CurHp += healAmount; // Add health
        if (CurHp > MaxHp) CurHp = MaxHp; // Clamp to max HP
        OnHealthChange?.Invoke(); // Notify listeners
        StartCoroutine(FlashColorCoroutine(Color.green, 0.2f)); // Flash green to indicate heal

        if (healSound != null && audioSource != null) // If audio is configured
        {
            audioSource.PlayOneShot(healSound); // Play heal sound
        }

        if (IsPlayer) // If player, sync with PlayerNeeds UI
        {
            PlayerNeeds needs = FindObjectOfType<PlayerNeeds>();
            if (needs != null)
            {
                needs.health.curValue = CurHp; // Update internal value
                needs.UpdateUI(); // Refresh the UI
            }
        }
    }

    public void CastCombatAction(CombatAction combatAction) // Initiates a combat action
    {
        if (actionInProgress) return; // Block overlapping actions

        if (IsPlayer) // Resource checks for player
        {
            PlayerNeeds playerNeeds = FindObjectOfType<PlayerNeeds>();
            if (playerNeeds == null) return;

            if (combatAction.MagicCost > 0 && playerNeeds.magik.curValue < combatAction.MagicCost) return;
            if (combatAction.StaminaCost > 0 && playerNeeds.stamina.curValue < combatAction.StaminaCost) return;

            playerNeeds.SpendMagik(combatAction.MagicCost); // Spend magic
            playerNeeds.SpendStamina(combatAction.StaminaCost); // Spend stamina
        }

        actionInProgress = true; // Flag that action is happening
        StartCoroutine(PerformActionWithOptionalVFX(combatAction)); // Start performing action
    }

    IEnumerator PerformActionWithOptionalVFX(CombatAction action) // Executes the action and VFX
    {
        yield return ExecuteCombatActionLogic(action); // Run main action logic

        string vfxName = action.VFXName;
        if (!string.IsNullOrEmpty(vfxName)) // If there's a VFX to play
        {
            VFXManager.Instance.PlayVFX(vfxName); // Tell manager to spawn it

            GameObject vfx = GameObject.Find(vfxName); // Find the VFX object
            Animator animator = vfx?.GetComponent<Animator>(); // Try to get animator

            float waitTime = 1f; // Default delay
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                waitTime = animator.GetCurrentAnimatorStateInfo(0).length; // Use animation length
            }

            yield return new WaitForSeconds(waitTime); // Wait for it to finish
        }

        SafeEndTurn(); // End this character's turn
    }

    IEnumerator ExecuteCombatActionLogic(CombatAction action) // Interprets combat action
    {
        if (action.Damage > 0) // If this is a damage action
        {
            yield return AttackOpponent(action); // Execute attack
        }
        else if (action.ProjectilePrefab != null) // If it's a projectile attack
        {
            GameObject proj = Instantiate(action.ProjectilePrefab, transform.position, Quaternion.identity); // Spawn it
            proj.GetComponent<Projectile>().Initialize(opponent, SafeEndTurn); // Launch it toward opponent
        }
        else if (action.HealAmount > 0) // If healing
        {
            Heal(action.HealAmount); // Perform healing
        }
        yield return null;
    }

    IEnumerator AttackOpponent(CombatAction combatAction) // Handles attacking a target
    {
        Character target = null; // Initialize target reference

        if (CombatManager.Instance != null && IsPlayer) // If player, use combat-selected target
            target = CombatManager.Instance.GetCurrentTarget();

        if (target == null) // Fallback: use assigned opponent
            target = opponent;

        if (target == null) // Final fallback: search scene for any valid target
        {
            foreach (var c in FindObjectsOfType<Character>())
            {
                bool isViableTarget = IsPlayer
                    ? (!c.IsPlayer && c.GetComponent<EnemyAI>() != null && c.gameObject.activeInHierarchy) // Enemy for player
                    : (c.IsPlayer && c.gameObject.activeInHierarchy); // Player for enemy

                if (isViableTarget)
                {
                    target = c;
                    break;
                }
            }
        }

        if (target == null) // If still no target found, abort
        {
            Debug.LogError("[Character] AttackOpponent: target is NULL. Aborting.");
            SafeEndTurn();
            yield break;
        }

        RectTransform rectTransform = GetComponent<RectTransform>(); // Check if UI-based
        if (startPos == default) // Cache original position if not set
            startPos = rectTransform != null ? rectTransform.anchoredPosition3D : transform.position;

        Vector3 targetPos = rectTransform != null ? target.transform.localPosition : target.transform.position; // Get target pos

        // Move to target
        while (Vector3.Distance(rectTransform != null ? rectTransform.anchoredPosition3D : transform.position, targetPos) > 0.1f)
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition3D = Vector3.MoveTowards(rectTransform.anchoredPosition3D, targetPos, moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
            yield return null;
        }

        // Play slash VFX if named "Attack"
        if (combatAction.DisplayName == "Attack")
        {
            GameObject slashVFX = GameObject.Find("AttackSlash1") ?? GameObject.Find("AttackSlash1VFX");

            if (slashVFX != null)
            {
                slashVFX.SetActive(true);
                var anim = slashVFX.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.Rebind();
                    anim.Play(0);
                    yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
                }
                else
                {
                    yield return new WaitForSeconds(0.75f); // Fallback timing
                }
                slashVFX.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[Character] Attack VFX not found (AttackSlash1/AttackSlash1VFX).");
                yield return new WaitForSeconds(0.3f);
            }
        }

        // Visual feedback and damage
        if (target.CurHp > 0)
        {
            Image targetImg = target.transform.Find(target.IsPlayer ? "CombatCharacter" : "CombatEnemy")?.GetComponent<Image>()
                                ?? target.GetComponent<Image>()
                                ?? target.GetComponentInChildren<Image>(true);

            Color original = targetImg ? targetImg.color : Color.white;
            if (targetImg != null) targetImg.color = Color.red;

            AudioSource audio = target.GetComponent<AudioSource>();
            if (audio?.clip != null) audio.Play();

            target.TakeDamage(combatAction.Damage); // Apply damage

            yield return new WaitForSeconds(0.2f);
            if (targetImg != null) targetImg.color = original;
        }
        else
        {
            Debug.LogWarning($"[Character] Target {target.name} already dead. Skipping damage.");
        }

        yield return new WaitForSeconds(0.25f); // Small pause before return

        // Move back to starting position
        Vector3 returnPos = startPos;
        while (Vector3.Distance(rectTransform != null ? rectTransform.anchoredPosition3D : transform.position, returnPos) > 0.1f)
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition3D = Vector3.MoveTowards(rectTransform.anchoredPosition3D, returnPos, moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, returnPos, moveSpeed * Time.deltaTime);
            }
            yield return null;
        }

        SafeEndTurn(); // Turn ends here
    }

    private void SafeEndTurn() // Ends turn safely if still in progress
    {
        if (!actionInProgress) return;
        actionInProgress = false;
        TurnManager.Instance.EndTurn();
    }

    public void NotifyHealthChanged() => OnHealthChange?.Invoke(); // Trigger health update

    public void ApplyAggregateHealth(int aggregateCurHp, int aggregateMaxHp) // External health sync
    {
        MaxHp = Mathf.Max(0, aggregateMaxHp);
        CurHp = Mathf.Clamp(aggregateCurHp, 0, MaxHp);
        OnHealthChange?.Invoke();
    }

    public void ForceDeath() // Forces this character to die
    {
        if (!gameObject.activeInHierarchy) return;
        if (CurHp <= 0) return;

        CurHp = 0;
        Die();
    }

    public void SyncWithPlayerNeeds(PlayerNeeds needs) // Pulls values from PlayerNeeds into this
    {
        this.CurHp = Mathf.RoundToInt(needs.health.curValue);
        this.MaxHp = Mathf.RoundToInt(needs.health.maxValue);
    }

    public void SyncBackToPlayerNeeds(PlayerNeeds needs) // Pushes values back into PlayerNeeds
    {
        needs.health.curValue = this.CurHp;
        needs.UpdateUI();
    }

    public float GetHealthPercentage() // Returns current health as a percentage
    {
        return (float)CurHp / MaxHp;
    }

    public void ResetCharacter() // Full character reset to initial state
    {
        CurHp = MaxHp;
        actionInProgress = false;
        opponent = null;
        OnHealthChange?.Invoke();

        if (image != null)
            image.color = originalColor;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
}