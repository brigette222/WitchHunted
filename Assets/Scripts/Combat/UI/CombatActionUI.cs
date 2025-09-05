using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatActionUI : MonoBehaviour
{
    [SerializeField] private GameObject visualContainer;
    [SerializeField] private Button[] combatActionButtons;

    public static CombatActionUI Instance { get; private set; }

    //VFX
    [SerializeField] private GameObject Heal1VFX;
    [SerializeField] private GameObject AttackSlash1VFX;

    [SerializeField] private GameObject CombatMagikList;
    [SerializeField] private GameObject CombatSkillList;
    [SerializeField] private GameObject CombatItemList;
    [SerializeField] private Button MagikButton;
    [SerializeField] private Button SkillsButton;
    [SerializeField] private Button ItemButton;


    private bool isTogglingMagic = false;
    private bool isTogglingSkills = false;
    private bool isTogglingItems = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("[CombatActionUI] Start() has been called.");
    }

    void OnEnable()
    {
        Debug.Log("[CombatActionUI] OnEnable fired — Subscribing to TurnManager events.");

        TurnManager.Instance.OnBeginTurn += OnBeginTurn;
        TurnManager.Instance.OnEndTurn += OnEndTurn;

        if (MagikButton.onClick.GetPersistentEventCount() == 0)
            MagikButton.onClick.AddListener(ToggleMagicUI);

        if (SkillsButton.onClick.GetPersistentEventCount() == 0)
            SkillsButton.onClick.AddListener(ToggleSkillsUI);

        if (ItemButton.onClick.GetPersistentEventCount() == 0)
            ItemButton.onClick.AddListener(ToggleItemUI);
    }

    void OnDisable()
    {
        Debug.Log("[CombatActionUI] OnDisable fired — Unsubscribing from TurnManager events.");

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnBeginTurn -= OnBeginTurn;
            TurnManager.Instance.OnEndTurn -= OnEndTurn;
        }
        else
        {
            Debug.LogWarning("[CombatActionUI] TurnManager instance is NULL during OnDisable. Skipping event unsubscription.");
        }

        MagikButton.onClick.RemoveListener(ToggleMagicUI);
        SkillsButton.onClick.RemoveListener(ToggleSkillsUI);
        ItemButton.onClick.RemoveListener(ToggleItemUI);
    }

    void OnBeginTurn(Character character)
    {
        Debug.Log($"[CombatActionUI] OnBeginTurn fired — Character: {character.name}, IsPlayer: {character.IsPlayer}");

        if (!character.IsPlayer)
        {
            Debug.Log("[CombatActionUI] Enemy's turn — Hiding all lists.");
            CombatMagikList.SetActive(false);
            CombatSkillList.SetActive(false);
            CombatItemList.SetActive(false);
            return;
        }

        visualContainer.SetActive(true);
        Debug.Log("[CombatActionUI] Player's turn — Showing action buttons.");

        for (int i = 0; i < combatActionButtons.Length; i++)
        {
            if (i < character.CombatActions.Count)
            {
                CombatAction ca = character.CombatActions[i];

                combatActionButtons[i].gameObject.SetActive(true);
                combatActionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = ca.DisplayName;

                combatActionButtons[i].onClick.RemoveAllListeners();
                combatActionButtons[i].onClick.AddListener(() => OnClickCombatAction(ca));

                Debug.Log($"[CombatActionUI] Button {i} set to action: {ca.DisplayName}");
            }
            else
            {
                combatActionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnEndTurn(Character character)
    {
        Debug.Log($"[CombatActionUI] OnEndTurn fired — Character: {character.name}");

        if (character.IsPlayer)
        {
            Debug.Log("[CombatActionUI] Player ended turn. Hiding action buttons.");
            visualContainer.SetActive(false);
        }
    }


    public void OnClickCombatAction(CombatAction combatAction)
    {
        Debug.Log($"[CombatActionUI] Player clicked action: {combatAction.DisplayName}");

        // VFX (existing behavior)
        if (combatAction.DisplayName == "Zyciokrag" && Heal1VFX != null)
        {
            Debug.Log("[VFX] Enabling Heal1VFX");
            Heal1VFX.SetActive(true);

            Animator animator = Heal1VFX.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Play(0);
                Debug.Log("[VFX] Playing Heal1VFX animation");
                StartCoroutine(DisableVFXAfterAnimation(Heal1VFX));
            }
            else
            {
                Debug.LogWarning("[VFX] Animator missing on Heal1VFX.");
            }
        }

        // Optional delayed slash VFX for basic Attack
        if (combatAction.DisplayName == "Attack" && AttackSlash1VFX != null)
        {
            Debug.Log("[Character] Delayed trigger of AttackSlash1 VFX from Character.cs");
            StartCoroutine(PlayAttackSlashVFXDelayed(1f));
        }

        // === Resolve the target (selected limb if any) ===
        Character caster = TurnManager.Instance.CurrentCharacter;
        Character selected = CombatManager.Instance != null ? CombatManager.Instance.GetCurrentTarget() : null;

        // If no limb was selected, default to enemy root that is active (has EnemyAI)
        if (selected == null)
        {
            Character fallbackEnemy = null;
            foreach (var c in FindObjectsOfType<Character>())
            {
                if (!c.IsPlayer && c.GetComponent<EnemyAI>() != null && c.gameObject.activeInHierarchy)
                {
                    fallbackEnemy = c; break;
                }
            }
            selected = fallbackEnemy;
            Debug.Log(selected != null
                ? $"[CombatActionUI] No limb selected — defaulting to enemy root '{selected.name}'."
                : "[CombatActionUI] No limb selected and no enemy root found!");
        }
        else
        {
            Debug.Log($"[CombatActionUI] Targeting limb '{selected.name}'.");
        }

        // Heals target self; no need to set a target
        if (combatAction.ActionType == CombatAction.Type.Heal)
        {
            caster.CastCombatAction(combatAction);
            return;
        }

        // For player attacks, store chosen target so Character uses it (no SetOpponent needed)
        if (selected != null && CombatManager.Instance != null)
        {
            CombatManager.Instance.SetTarget(selected);
        }

        caster.CastCombatAction(combatAction);
    }



    // **Coroutine to Disable VFX After Animation**
    IEnumerator DisableVFXAfterAnimation(GameObject vfxObject)
    {
        if (vfxObject == null)
        {
            Debug.LogError("[VFX] VFX object is NULL! Make sure it's assigned.");
            yield break;
        }

        Animator animator = vfxObject.GetComponent<Animator>();

        if (animator != null)
        {
            Debug.Log($"[VFX] Playing animation on {vfxObject.name}");
            animator.Rebind();
            animator.Play(0);

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        else
        {
            Debug.LogWarning("[VFX] No Animator found on VFX. Disabling after 0.5s fallback.");
            yield return new WaitForSeconds(0.5f);
        }

        vfxObject.SetActive(false);
        Debug.Log($"[VFX] Disabled {vfxObject.name} after animation.");
    }



    // **Toggles Magic UI Panel**
    public void ToggleMagicUI()
    {
        if (CombatMagikList == null || isTogglingMagic) return;

        isTogglingMagic = true;
        Debug.Log("[CombatActionUI] ToggleMagicUI() called.");

        bool isNowActive = !CombatMagikList.activeSelf;
        CombatMagikList.SetActive(isNowActive);
        CombatSkillList.SetActive(false);
        CombatItemList.SetActive(false);

        Debug.Log($"[CombatActionUI] Magic List Active? {CombatMagikList.activeSelf}");

        StartCoroutine(ResetToggleFlag(() => isTogglingMagic = false));
    }

    // **Toggles Skills UI Panel**
    public void ToggleSkillsUI()
    {
        if (CombatSkillList == null || isTogglingSkills) return;

        isTogglingSkills = true;
        Debug.Log("[CombatActionUI] ToggleSkillsUI() called.");

        bool isNowActive = !CombatSkillList.activeSelf;
        CombatSkillList.SetActive(isNowActive);
        CombatMagikList.SetActive(false);
        CombatItemList.SetActive(false);

        Debug.Log($"[CombatActionUI] Skills List Active? {CombatSkillList.activeSelf}");

        StartCoroutine(ResetToggleFlag(() => isTogglingSkills = false));
    }

    public void ToggleItemUI()
    {
        if (CombatItemList == null || isTogglingItems) return;

        isTogglingItems = true;
        Debug.Log("[CombatActionUI] ToggleItemUI() called.");

        bool isNowActive = !CombatItemList.activeSelf;

        if (isNowActive)
        {
            CombatInventoryUI inventoryUI = FindObjectOfType<CombatInventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.PopulateItemList();
                Debug.Log("[CombatActionUI] Called PopulateItemList() from CombatInventoryUI.");
            }
            else
            {
                Debug.LogError("[CombatActionUI] ERROR: CombatInventoryUI not found!");
            }
        }

        CombatItemList.SetActive(isNowActive);
        CombatMagikList.SetActive(false);
        CombatSkillList.SetActive(false);

        Debug.Log($"[CombatActionUI] Item List Active? {CombatItemList.activeSelf}");

        StartCoroutine(ResetToggleFlag(() => isTogglingItems = false));
    }

    private IEnumerator ResetToggleFlag(System.Action resetAction)
    {
        yield return new WaitForSeconds(0.1f);
        resetAction();
    }

    public IEnumerator PlayAttackSlashVFXDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (AttackSlash1VFX == null)
        {
            Debug.LogError("[VFX] AttackSlash1VFX is NULL! Make sure it's assigned.");
            yield break;
        }

        Debug.Log("[VFX] Delayed enabling of AttackSlash1VFX.");
        AttackSlash1VFX.SetActive(true);

        Animator animator = AttackSlash1VFX.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Play(0);
            Debug.Log("[VFX] Playing AttackSlash1VFX animation");

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        else
        {
            Debug.LogWarning("[VFX] No Animator on AttackSlash1VFX. Using fallback.");
            yield return new WaitForSeconds(0.5f);
        }

        AttackSlash1VFX.SetActive(false);
        Debug.Log("[VFX] Disabled AttackSlash1VFX after animation.");
    }

}



