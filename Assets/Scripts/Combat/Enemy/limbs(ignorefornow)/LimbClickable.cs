using UnityEngine;
using UnityEngine.EventSystems;

public class LimbClickable : MonoBehaviour, IPointerClickHandler
{
    public Character characterRef;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (characterRef != null)
        {
            CombatManager.Instance.SetTarget(characterRef);
            Debug.Log($"[LimbClickable] Selected target: {characterRef.name}");
        }
        else
        {
            Debug.LogWarning("[LimbClickable] No Character assigned on this limb.");
        }
    }
}