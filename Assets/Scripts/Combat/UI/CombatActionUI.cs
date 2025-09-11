using System.Collections;  

using UnityEngine; 
using UnityEngine.UI;   
using TMPro;   

public class CombatActionUI : MonoBehaviour // UI controller for combat actions  
{
    [SerializeField] private GameObject visualContainer; // Container for action buttons UI  
    [SerializeField] private Button[] combatActionButtons; // Buttons for each combat action  

    public static CombatActionUI Instance { get; private set; } // Singleton reference  

    // VFX game objects  
    [SerializeField] private GameObject Heal1VFX;
    [SerializeField] private GameObject AttackSlash1VFX;

    [SerializeField] private GameObject CombatMagikList; // UI list for magic actions  
    [SerializeField] private GameObject CombatSkillList; // UI list for skill actions  
    [SerializeField] private GameObject CombatItemList; // UI list for items  
    [SerializeField] private Button MagikButton; // Button to toggle magic list  
    [SerializeField] private Button SkillsButton; // Button to toggle skills list  
    [SerializeField] private Button ItemButton; // Button to toggle items list  

    private bool isTogglingMagic = false; // Flag to prevent rapid toggle spam for magic  
    private bool isTogglingSkills = false; // Same for skills  
    private bool isTogglingItems = false; // Same for items  

    void Awake() // Unity lifecycle: before Start  
    {
        Instance = this; // Set singleton instance  
    }

    void Start() // Unity lifecycle: after Awake, before first frame  
    {
        // No logs: initialization only  
    }

    void OnEnable() // Called when this object becomes enabled/active  
    {
        TurnManager.Instance.OnBeginTurn += OnBeginTurn; // Subscribe to turn start  
        TurnManager.Instance.OnEndTurn += OnEndTurn; // Subscribe to turn end  

        if (MagikButton.onClick.GetPersistentEventCount() == 0)
            MagikButton.onClick.AddListener(ToggleMagicUI); // Ensure toggle listener is set  

        if (SkillsButton.onClick.GetPersistentEventCount() == 0)
            SkillsButton.onClick.AddListener(ToggleSkillsUI); // Same for skills  

        if (ItemButton.onClick.GetPersistentEventCount() == 0)
            ItemButton.onClick.AddListener(ToggleItemUI); // Same for items  
    }

    void OnDisable() // Called when object is disabled/inactive  
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnBeginTurn -= OnBeginTurn; // Unsubscribe from events  
            TurnManager.Instance.OnEndTurn -= OnEndTurn;
        }

        MagikButton.onClick.RemoveListener(ToggleMagicUI); // Remove toggle listeners  
        SkillsButton.onClick.RemoveListener(ToggleSkillsUI);
        ItemButton.onClick.RemoveListener(ToggleItemUI);
    }

    void OnBeginTurn(Character character) // Called when a character’s turn begins  
    {
        if (!character.IsPlayer) // If it’s an enemy’s turn  
        {
            CombatMagikList.SetActive(false); // Hide magic list  
            CombatSkillList.SetActive(false); // Hide skills list  
            CombatItemList.SetActive(false); // Hide items list  
            return; // Exit, do not show UI  
        }

        visualContainer.SetActive(true); // Show action buttons UI  

        for (int i = 0; i < combatActionButtons.Length; i++)
        {
            if (i < character.CombatActions.Count) // If this button maps to a real action  
            {
                CombatAction ca = character.CombatActions[i]; // Get the action  

                combatActionButtons[i].gameObject.SetActive(true); // Make button visible  
                combatActionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = ca.DisplayName; // Set its label  

                combatActionButtons[i].onClick.RemoveAllListeners(); // Clear old listeners  
                combatActionButtons[i].onClick.AddListener(() => OnClickCombatAction(ca)); // Add listener to trigger this action  

            }
            else
            {
                combatActionButtons[i].gameObject.SetActive(false); // Hide unused buttons  
            }
        }
    }

    void OnEndTurn(Character character) // Called when a character’s turn ends  
    {
        if (character.IsPlayer) // If it was the player  
        {
            visualContainer.SetActive(false); // Hide action buttons UI  
        }
    }

    public void OnClickCombatAction(CombatAction combatAction) // Called when player clicks an action button  
    {
        // VFX for specific action names  
        if (combatAction.DisplayName == "Zyciokrag" && Heal1VFX != null)
        {
            Heal1VFX.SetActive(true); // Show healing VFX  

            Animator animator = Heal1VFX.GetComponent<Animator>(); // Get animator for VFX  
            if (animator != null)
            {
                animator.Rebind();
                animator.Play(0);
                StartCoroutine(DisableVFXAfterAnimation(Heal1VFX)); // Turn off after animation  
            }
        }

        if (combatAction.DisplayName == "Attack" && AttackSlash1VFX != null)
        {
            StartCoroutine(PlayAttackSlashVFXDelayed(1f)); // Delay slash VFX  
        }

        Character caster = TurnManager.Instance.CurrentCharacter; // Who is using the action  
        Character selected = CombatManager.Instance != null ? CombatManager.Instance.GetCurrentTarget() : null; // Chosen target  

        if (selected == null) // If no target selected  
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
        }

        if (combatAction.ActionType == CombatAction.Type.Heal) // If action is a heal, don’t select target  
        {
            caster.CastCombatAction(combatAction);
            return;
        }

        if (selected != null && CombatManager.Instance != null) // If valid target exists  
        {
            CombatManager.Instance.SetTarget(selected); // Set it  
        }

        caster.CastCombatAction(combatAction); // Execute action  
    }



    IEnumerator DisableVFXAfterAnimation(GameObject vfxObject) // Turns off VFX after its animation finishes  
    {
        if (vfxObject == null) // Safety check  
            yield break;

        Animator animator = vfxObject.GetComponent<Animator>(); // Get animator  

        if (animator != null) // If animator exists  
        {
            animator.Rebind();
            animator.Play(0);
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // Wait animation length  
        }
        else
        {
            yield return new WaitForSeconds(0.5f); // Fallback wait  
        }

        vfxObject.SetActive(false); // Disable VFX object  
    }

    public void ToggleMagicUI() // Toggles the magic actions panel  
    {
        if (CombatMagikList == null || isTogglingMagic) return; // Block if null or already toggling  
        isTogglingMagic = true;

        bool isNowActive = !CombatMagikList.activeSelf; // Determine new state  
        CombatMagikList.SetActive(isNowActive);
        CombatSkillList.SetActive(false);
        CombatItemList.SetActive(false);

        StartCoroutine(ResetToggleFlag(() => isTogglingMagic = false)); // Reset toggle blocker  
    }

    public void ToggleSkillsUI() // Toggles the skill actions panel  
    {
        if (CombatSkillList == null || isTogglingSkills) return;
        isTogglingSkills = true;

        bool isNowActive = !CombatSkillList.activeSelf;
        CombatSkillList.SetActive(isNowActive);
        CombatMagikList.SetActive(false);
        CombatItemList.SetActive(false);

        StartCoroutine(ResetToggleFlag(() => isTogglingSkills = false));
    }

    public void ToggleItemUI() // Toggles the item actions panel  
    {
        if (CombatItemList == null || isTogglingItems) return;
        isTogglingItems = true;

        bool isNowActive = !CombatItemList.activeSelf;

        if (isNowActive) // If enabling items panel  
        {
            CombatInventoryUI inventoryUI = FindObjectOfType<CombatInventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.PopulateItemList(); // Fill item list UI  
            }
        }

        CombatItemList.SetActive(isNowActive);
        CombatMagikList.SetActive(false);
        CombatSkillList.SetActive(false);

        StartCoroutine(ResetToggleFlag(() => isTogglingItems = false));
    }

    private IEnumerator ResetToggleFlag(System.Action resetAction) // Short coroutine to reset toggle flags  
    {
        yield return new WaitForSeconds(0.1f);
        resetAction();
    }

    public IEnumerator PlayAttackSlashVFXDelayed(float delay) // Plays the attack slash VFX after a delay  
    {
        yield return new WaitForSeconds(delay);

        if (AttackSlash1VFX == null) yield break; // Safety null check  

        AttackSlash1VFX.SetActive(true);

        Animator animator = AttackSlash1VFX.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Play(0);
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f); // Fallback delay  
        }

        AttackSlash1VFX.SetActive(false);
    }
}  



