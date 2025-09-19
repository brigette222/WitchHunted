using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))] // Ensure a SpriteRenderer is attached
public class PlayerSortingOrder : MonoBehaviour
{
    private SpriteRenderer sr; // Reference to the SpriteRenderer

    void Awake() => sr = GetComponent<SpriteRenderer>(); // Cache the SpriteRenderer

    void LateUpdate() // Called after Update(), ideal for sorting adjustments
    {
        sr.sortingOrder = 10000 - Mathf.RoundToInt(transform.position.y * 100); // Higher Y = lower sortingOrder
    }
}