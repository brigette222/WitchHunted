using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public int CurHp;
    public int MaxHp;
    private Image image; // Cached reference to this character's UI Image
    private Color originalColor;
    public bool IsPlayer;
    private static Dictionary<Character, Color> originalColors = new Dictionary<Character, Color>();
    public List<CombatAction> CombatActions = new List<CombatAction>();

    [Range(0f, 1f)] public float EvasionRate = 0.1f;
    [SerializeField] private float moveSpeed = 400f;

    private Character opponent;
    private Vector3 startPos;
    public event UnityAction OnHealthChange;
    public static event UnityAction<Character> OnDie;

    [SerializeField] private AudioClip hurtSFX;
    private AudioSource audioSource;
    public AudioClip healSound; // Assign in Inspector
    private bool actionInProgress = false;

    // --- Opponent API (needed by CombatTrigger.cs) ---
    public void SetOpponent(Character newOpponent)
    {
        opponent = newOpponent;
    }

    public Character GetOpponent()
    {
        return opponent;
    }

    public void ClearOpponent()
    {
        opponent = null;
    }
    // --------------------------------------------------

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        image = GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
            Debug.Log($"[Character] Cached image color for {name}");
        }
        else
        {
            Debug.LogWarning($"[Character] No Image found on {name}!");
        }

        Debug.Log($"[Character] {name} (IsPlayer={IsPlayer}) spawned with {CurHp}/{MaxHp} HP.");

        // IMPORTANT: Only actors that actually take turns should register.
        // Player: yes. Main enemy (root) that has EnemyAI: yes. Limbs (no EnemyAI): no.
        bool isTurnTaker = IsPlayer || (GetComponent<EnemyAI>() != null);
        if (isTurnTaker && TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterCharacter(this);
            Debug.Log($"[Character] {name} registered with TurnManager (isTurnTaker={isTurnTaker}).");
        }
    }

    public void FlashColor(Color flashColor)
    {
        // Try conventional child names first
        string targetName = IsPlayer ? "CombatCharacter" : "CombatEnemy";
        Image img = null;

        Transform child = transform.Find(targetName);
        if (child != null) img = child.GetComponent<Image>();

        // Fallback: Image on this object
        if (img == null) img = GetComponent<Image>();

        // Final fallback: any Image under this object (active or inactive)
        if (img == null) img = GetComponentInChildren<Image>(true);

        if (img == null)
        {
            Debug.LogWarning($"[FlashColor] No Image found on {name} (or child '{targetName}').");
            return;
        }

        // Play hurt sound if available
        if (audioSource != null && hurtSFX != null)
        {
            audioSource.PlayOneShot(hurtSFX);
            Debug.Log($"[FlashColor] Playing hurt SFX on {name}");
        }

        Debug.Log($"[FlashColor] Flashing image on {name} — Color: {flashColor}");
        StartCoroutine(FlashColorCoroutine(img, flashColor));
    }

    public IEnumerator FlashColorCoroutine(Color flashColor, float duration = 0.2f)
    {
        // Try conventional child names first
        string targetName = IsPlayer ? "CombatCharacter" : "CombatEnemy";
        Image img = null;

        Transform child = transform.Find(targetName);
        if (child != null) img = child.GetComponent<Image>();

        // Fallbacks
        if (img == null) img = GetComponent<Image>();
        if (img == null) img = GetComponentInChildren<Image>(true);

        if (img == null)
        {
            Debug.LogWarning($"[FlashColor] No Image found for coroutine on {name}.");
            yield break;
        }

        Color original = img.color;
        img.color = flashColor;
        yield return new WaitForSeconds(duration);
        img.color = original;
    }

    private IEnumerator FlashColorCoroutine(Image img, Color flashColor)
    {
        Color originalColor = img.color;
        img.color = flashColor;

        yield return new WaitForSeconds(0.15f); // Quick flash time

        img.color = originalColor;
        Debug.Log("[FlashColor] Flash reset to original color.");
    }

    public void ResetStartPos()
    {
        // Cache UI position robustly for UI objects (RectTransform) or world objects
        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            // Use anchoredPosition for UI so we don't drift to screen corners
            startPos = rt.anchoredPosition3D;
            Debug.Log($"[Character] {name} startPos reset (UI) to {startPos}");
        }
        else
        {
            startPos = transform.position;
            Debug.Log($"[Character] {name} startPos reset (world) to {startPos}");
        }
    }

    public void TakeDamage(int damageToTake)
    {
        if (CurHp <= 0)
        {
            Debug.LogWarning($"[Character] Tried to damage {name}, but they are already dead. Skipping.");
            return;
        }

        float roll = Random.Range(0f, 1f);
        Debug.Log($"[DEBUG] {name} Evasion Check: Roll = {roll}, EvasionRate = {EvasionRate}");

        if (roll < EvasionRate)
        {
            Debug.Log($"[Character] {name} DODGED the attack! (Roll: {roll}, Evasion Rate: {EvasionRate})");
            return;
        }

        CurHp -= damageToTake;
        FlashColor(Color.red);
        OnHealthChange?.Invoke();
        Debug.Log($"[FlashColor] audioSource = {audioSource}, hurtSFX = {hurtSFX}");
        Debug.Log($"[Character] {name} took {damageToTake} damage. Current HP: {CurHp}/{MaxHp}");

        if (CurHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"[Character] {name} has died!");
        OnDie?.Invoke(this);

        if (IsPlayer)
        {
            gameObject.SetActive(false);
            return;
        }

        // Only the main enemy (root with EnemyAI) should end combat here.
        // Limbs (no EnemyAI) just deactivate.
        bool isMainEnemy = GetComponent<EnemyAI>() != null;
        if (isMainEnemy)
        {
            StartCoroutine(DelayedDeathAndCombatEnd());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator DelayedDeathAndCombatEnd()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
        CombatManager.Instance.EndCombat();
    }

    public void Heal(int healAmount)
    {
        CurHp += healAmount;
        Debug.Log($"[Character] {name} healed {healAmount} HP. Current HP: {CurHp}/{MaxHp}");

        if (CurHp > MaxHp)
            CurHp = MaxHp;

        OnHealthChange?.Invoke();
        StartCoroutine(FlashColorCoroutine(Color.green, 0.2f));

        if (healSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(healSound);
            Debug.Log("[Character] Played healing sound.");
        }

        if (IsPlayer)
        {
            PlayerNeeds needs = FindObjectOfType<PlayerNeeds>();
            if (needs != null)
            {
                needs.health.curValue = CurHp;
                needs.UpdateUI();
                Debug.Log($"[Character] Synced PlayerNeeds after healing: {CurHp}");
            }
            else
            {
                Debug.LogWarning("[Character] Could not find PlayerNeeds after healing.");
            }
        }
    }

    public void CastCombatAction(CombatAction combatAction)
    {
        if (actionInProgress)
        {
            Debug.LogWarning($"[Character] {name} tried to act, but action is already in progress!");
            return;
        }

        if (IsPlayer)
        {
            PlayerNeeds playerNeeds = FindObjectOfType<PlayerNeeds>();

            if (playerNeeds == null)
            {
                Debug.LogError("[Character] PlayerNeeds script not found! Cannot check resources.");
                return;
            }

            if (combatAction.MagicCost > 0 && playerNeeds.magik.curValue < combatAction.MagicCost)
            {
                Debug.LogWarning($"[Character] Not enough Magik to use {combatAction.DisplayName}");
                return;
            }

            if (combatAction.StaminaCost > 0 && playerNeeds.stamina.curValue < combatAction.StaminaCost)
            {
                Debug.LogWarning($"[Character] Not enough Stamina to use {combatAction.DisplayName}");
                return;
            }

            playerNeeds.SpendMagik(combatAction.MagicCost);
            playerNeeds.SpendStamina(combatAction.StaminaCost);
        }

        Debug.Log($"[Character] {name} is casting {combatAction.DisplayName} — Damage: {combatAction.Damage}, Heal: {combatAction.HealAmount}");

        actionInProgress = true;
        StartCoroutine(PerformActionWithOptionalVFX(combatAction));
    }

    IEnumerator PerformActionWithOptionalVFX(CombatAction action)
    {
        yield return ExecuteCombatActionLogic(action);

        string vfxName = action.VFXName;
        if (!string.IsNullOrEmpty(vfxName))
        {
            VFXManager.Instance.PlayVFX(vfxName);

            GameObject vfx = GameObject.Find(vfxName);
            Animator animator = vfx?.GetComponent<Animator>();

            float waitTime = 1f;
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                waitTime = animator.GetCurrentAnimatorStateInfo(0).length;
            }

            Debug.Log($"[Character] Waiting {waitTime}s for {vfxName} to finish...");
            yield return new WaitForSeconds(waitTime);
        }

        Debug.Log("[Character] Ending turn after VFX (or no VFX)");
        SafeEndTurn();
    }

    IEnumerator WaitForHealVFXThenContinue(string vfxName, CombatAction action)
    {
        GameObject vfx = GameObject.Find(vfxName);

        if (vfx == null)
        {
            Debug.LogWarning("[Character] VFX GameObject not found: " + vfxName);
            yield return ExecuteCombatActionLogic(action);
            SafeEndTurn();
            yield break;
        }

        Animator animator = vfx.GetComponent<Animator>();
        if (animator != null)
        {
            float waitTime = animator.GetCurrentAnimatorStateInfo(0).length;
            Debug.Log($"[Character] Waiting {waitTime}s for {vfxName} to finish...");
            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            Debug.LogWarning("[Character] No Animator on " + vfxName + " — fallback to 1s delay");
            yield return new WaitForSeconds(1f);
        }

        yield return ExecuteCombatActionLogic(action);
        Debug.Log("[Character] Ending turn after VFX");
        SafeEndTurn();
    }

    IEnumerator ExecuteCombatActionLogic(CombatAction action)
    {
        if (action.Damage > 0)
        {
            Debug.Log("[Character] Damage action triggered");
            yield return AttackOpponent(action);
        }
        else if (action.ProjectilePrefab != null)
        {
            GameObject proj = Instantiate(action.ProjectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Projectile>().Initialize(opponent, SafeEndTurn);
        }
        else if (action.HealAmount > 0)
        {
            Debug.Log("[Character] Healing action triggered");
            Heal(action.HealAmount);
        }
        yield return null;
    }

    IEnumerator AttackOpponent(CombatAction combatAction)
    {
        Character target = null;

        if (CombatManager.Instance != null && IsPlayer)
            target = CombatManager.Instance.GetCurrentTarget();

        if (target == null)
            target = opponent;

        if (target == null)
        {
            if (IsPlayer)
            {
                foreach (var c in FindObjectsOfType<Character>())
                {
                    if (!c.IsPlayer && c.GetComponent<EnemyAI>() != null && c.gameObject.activeInHierarchy)
                    {
                        target = c; break;
                    }
                }
            }
            else
            {
                foreach (var c in FindObjectsOfType<Character>())
                {
                    if (c.IsPlayer && c.gameObject.activeInHierarchy)
                    {
                        target = c; break;
                    }
                }
            }
        }

        if (target == null)
        {
            Debug.LogError("[Character] AttackOpponent: target is NULL. Aborting.");
            SafeEndTurn();
            yield break;
        }

        Debug.Log($"[Character] {name} is starting AttackOpponent against {target.name}");

        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            if (startPos == default) startPos = rectTransform.anchoredPosition3D;
        }
        else
        {
            if (startPos == default) startPos = transform.position;
        }

        // Move forward
        if (rectTransform != null)
        {
            while (Vector3.Distance(rectTransform.anchoredPosition3D, target.transform.localPosition) > 0.1f)
            {
                rectTransform.anchoredPosition3D = Vector3.MoveTowards(
                    rectTransform.anchoredPosition3D,
                    target.transform.localPosition,
                    moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
        else
        {
            while (Vector3.Distance(transform.position, target.transform.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target.transform.position,
                    moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // VFX: try AttackSlash1, then AttackSlash1VFX
        if (combatAction.DisplayName == "Attack")
        {
            GameObject slashVFX = GameObject.Find("AttackSlash1");
            if (slashVFX == null) slashVFX = GameObject.Find("AttackSlash1VFX");

            if (slashVFX != null)
            {
                slashVFX.SetActive(true);
                var anim = slashVFX.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.Rebind(); anim.Play(0);
                    yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
                }
                else
                {
                    yield return new WaitForSeconds(0.75f);
                }
                slashVFX.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[Character] Attack VFX not found (AttackSlash1/AttackSlash1VFX).");
                yield return new WaitForSeconds(0.3f);
            }
        }

        // Find an Image on the target for flash (works for limbs or root)
        Image targetImg = null;
        string childName = target.IsPlayer ? "CombatCharacter" : "CombatEnemy";
        Transform imageTransform = target.transform.Find(childName);
        if (imageTransform != null) targetImg = imageTransform.GetComponent<Image>();
        if (targetImg == null) targetImg = target.GetComponent<Image>();
        if (targetImg == null) targetImg = target.GetComponentInChildren<Image>(true);

        // Guard against attacking already-dead limb
        if (target.CurHp <= 0)
        {
            Debug.LogWarning($"[Character] Target {target.name} already dead. Skipping damage.");
        }
        else
        {
            Color original = targetImg ? targetImg.color : Color.white;
            if (targetImg) targetImg.color = Color.red;

            AudioSource audio = target.GetComponent<AudioSource>();
            if (audio != null && audio.clip != null) audio.Play();

            target.TakeDamage(combatAction.Damage);

            yield return new WaitForSeconds(0.2f);
            if (targetImg) targetImg.color = original;
        }

        yield return new WaitForSeconds(0.25f);

        // Move back to start
        if (rectTransform != null)
        {
            while (Vector3.Distance(rectTransform.anchoredPosition3D, startPos) > 0.1f)
            {
                rectTransform.anchoredPosition3D = Vector3.MoveTowards(
                    rectTransform.anchoredPosition3D,
                    startPos,
                    moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
        else
        {
            while (Vector3.Distance(transform.position, startPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    startPos,
                    moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        SafeEndTurn();
    }

    private void SafeEndTurn()
    {
        if (!actionInProgress)
        {
            Debug.LogWarning($"[Character] {name} tried to EndTurn but action was already ended.");
            return;
        }

        actionInProgress = false;
        TurnManager.Instance.EndTurn();
    }

    public void NotifyHealthChanged()
    {
        OnHealthChange?.Invoke();
    }
    public void ApplyAggregateHealth(int aggregateCurHp, int aggregateMaxHp)
    {
        MaxHp = Mathf.Max(0, aggregateMaxHp);
        CurHp = Mathf.Clamp(aggregateCurHp, 0, MaxHp);
        OnHealthChange?.Invoke(); // legal: invoked from inside Character
    }

    public void ForceDeath()
    {
        // Avoid double death or killing disabled objects during transition
        if (!gameObject.activeInHierarchy) return;
        if (CurHp <= 0) return;

        CurHp = 0;
        Die();
    }


    public void SyncWithPlayerNeeds(PlayerNeeds needs)
    {
        this.CurHp = Mathf.RoundToInt(needs.health.curValue);
        this.MaxHp = Mathf.RoundToInt(needs.health.maxValue);
    }

    public void SyncBackToPlayerNeeds(PlayerNeeds needs)
    {
        needs.health.curValue = this.CurHp;
        needs.UpdateUI();
    }

    public float GetHealthPercentage()
    {
        return (float)CurHp / MaxHp;
    }

    public void ResetCharacter()
    {
        Debug.Log($"[Character] Resetting {name}...");

        CurHp = MaxHp;
        actionInProgress = false;
        opponent = null;
        OnHealthChange?.Invoke();

        if (image != null)
            image.color = originalColor;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        Debug.Log($"[Character] {name} reset with {CurHp}/{MaxHp} HP.");
    }
}