using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LimbSelectable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Assign the limb's Character here")]
    public Character character;

    [Header("Optional highlight")]
    public Image highlightTarget;    // if left null, we'll try to use this Image
    public float hoverScale = 1.05f;

    private Image selfImage;
    private Vector3 baseScale;

    void Awake()
    {
        selfImage = GetComponent<Image>();
        if (highlightTarget == null) highlightTarget = selfImage;
        baseScale = transform.localScale;

        if (character == null)
            Debug.LogWarning($"[LimbSelectable] '{name}' has no Character assigned. Clicks will log but not select.");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (character == null)
        {
            Debug.LogWarning($"[LimbSelectable] Clicked '{name}' but no Character is assigned.");
            return;
        }

        CombatManager.Instance.SetTarget(character);
        Debug.Log($"[LimbSelectable] CLICK: Selected limb '{character.name}' (HP {character.CurHp}/{character.MaxHp}).");
        FlashSelect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = baseScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = baseScale;
    }

    private void FlashSelect()
    {
        if (highlightTarget == null) return;
        // brief flash
        highlightTarget.CrossFadeAlpha(0.6f, 0.05f, true);
        highlightTarget.CrossFadeAlpha(1f, 0.15f, true);
    }
}