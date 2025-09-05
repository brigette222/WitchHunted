using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Image))]
public class LimbTarget : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Character limbCharacter;

    private void Reset()
    {
        if (!limbCharacter) limbCharacter = GetComponentInParent<Character>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!limbCharacter || limbCharacter.CurHp <= 0) return;
        if (CombatManager.Instance) CombatManager.Instance.SetTarget(limbCharacter);
    }
}
