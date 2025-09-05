using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSortingOrder : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        sr.sortingOrder = 10000 - Mathf.RoundToInt(transform.position.y * 100);
    }
}