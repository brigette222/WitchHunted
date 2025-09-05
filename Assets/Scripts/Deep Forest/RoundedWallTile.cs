using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RoundedWallTile : MonoBehaviour
{
    [Header("Main Wall Settings")]
    public Sprite[] edgeSprites = new Sprite[16];
    public LayerMask floorMask;

    [Header("Top Visual Tree Settings")]
    public GameObject topVisualPrefab;
    public Sprite[] topTreeSprites = new Sprite[16];

    [Header("Side Visual Settings")]
    public GameObject leftVisualPrefab;
    public Sprite[] leftSprites = new Sprite[16];
    public GameObject rightVisualPrefab;
    public Sprite[] rightSprites = new Sprite[16];
    public GameObject bottomVisualPrefab;
    public Sprite[] bottomSprites = new Sprite[16];

    [Header("Diagonal Overrides (used only if bitmask = 0)")]
    public Sprite cornerTopLeftOverride;
    public Sprite cornerTopRightOverride;
    public Sprite cornerBottomLeftOverride;
    public Sprite cornerBottomRightOverride;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplyEdgeSprite();
    }

    public void RefreshBitmaskVisual()
    {
        Debug.Log($"[RoundedWallTile] Refreshing visuals at {transform.position}");

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("TopVisual_") ||
                child.name.StartsWith("LeftVisual_") ||
                child.name.StartsWith("RightVisual_") ||
                child.name.StartsWith("BottomVisual_"))
            {
                Destroy(child.gameObject);
            }
        }

        ApplyEdgeSprite();
    }

    void ApplyEdgeSprite()
    {
        int bitmask = CalculateBitmask();
        Debug.Log($"[RoundedWallTile] Bitmask at {transform.position}: {bitmask}");

        if (bitmask == 5 || bitmask == 7 || bitmask == 10 || bitmask == 11 || bitmask == 13 || bitmask == 14 || bitmask == 15)
        {
            Debug.LogWarning($"[RoundedWallTile] Suspicious bitmask {bitmask} at {transform.position}");
        }

        if (bitmask == 0)
        {
            Sprite diagonal = GetDiagonalOverride();
            if (diagonal != null)
            {
                sr.sprite = diagonal;
                Debug.Log("[RoundedWallTile] Using diagonal override sprite.");
                return;
            }
        }

        if (bitmask >= 0 && bitmask < edgeSprites.Length && edgeSprites[bitmask] != null)
        {
            sr.sprite = edgeSprites[bitmask];
        }
        else
        {
            Debug.LogWarning($"[RoundedWallTile] No edge sprite found for bitmask {bitmask} at {transform.position}");
        }

        if (ShouldShowTopVisual(bitmask)) CreateVisual(topVisualPrefab, topTreeSprites, bitmask, Vector3.up, "TopVisual_");
        if (ShouldShowLeftVisual(bitmask)) CreateVisual(leftVisualPrefab, leftSprites, bitmask, Vector3.left, "LeftVisual_");
        if (ShouldShowRightVisual(bitmask)) CreateVisual(rightVisualPrefab, rightSprites, bitmask, Vector3.right, "RightVisual_");
        if (ShouldShowBottomVisual(bitmask)) CreateVisual(bottomVisualPrefab, bottomSprites, bitmask, Vector3.down, "BottomVisual_");
    }

    void CreateVisual(GameObject prefab, Sprite[] spriteArray, int bitmask, Vector3 offset, string namePrefix)
    {
        if (prefab == null || spriteArray == null || bitmask >= spriteArray.Length || spriteArray[bitmask] == null) return;

        Vector3 spawnPos = transform.position + offset;
        GameObject visual = Instantiate(prefab, spawnPos, Quaternion.identity);
        visual.name = namePrefix + bitmask;
        visual.transform.SetParent(transform);

        SpriteRenderer visSR = visual.GetComponent<SpriteRenderer>();
        visSR.sprite = spriteArray[bitmask];
    }

    int CalculateBitmask()
    {
        int mask = 0;
        if (HasFloor(Vector2.up)) mask += 1;
        if (HasFloor(Vector2.right)) mask += 2;
        if (HasFloor(Vector2.down)) mask += 4;
        if (HasFloor(Vector2.left)) mask += 8;
        return mask;
    }

    public int GetActualBitmask()
    {
        return CalculateBitmask();
    }

    bool HasFloor(Vector2 dir)
    {
        Vector2 checkPos = (Vector2)transform.position + dir;
        return Physics2D.OverlapBox(checkPos, Vector2.one * 0.8f, 0, floorMask);
    }

    Sprite GetDiagonalOverride()
    {
        if (HasFloor(Vector2.up + Vector2.left) && cornerTopLeftOverride) return cornerTopLeftOverride;
        if (HasFloor(Vector2.up + Vector2.right) && cornerTopRightOverride) return cornerTopRightOverride;
        if (HasFloor(Vector2.down + Vector2.left) && cornerBottomLeftOverride) return cornerBottomLeftOverride;
        if (HasFloor(Vector2.down + Vector2.right) && cornerBottomRightOverride) return cornerBottomRightOverride;
        return null;
    }

    bool ShouldShowTopVisual(int bitmask) => bitmask == 4 || bitmask == 14 || bitmask == 12 || bitmask == 6;
    bool ShouldShowLeftVisual(int bitmask) => bitmask == 2 || bitmask == 6 || bitmask == 10 || bitmask == 14;
    bool ShouldShowRightVisual(int bitmask) => bitmask == 8 || bitmask == 12 || bitmask == 10 || bitmask == 14;
    bool ShouldShowBottomVisual(int bitmask) => bitmask == 1 || bitmask == 5 || bitmask == 4 || bitmask == 7;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        int bitmask = CalculateBitmask();
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, "Bitmask ID: " + bitmask);
    }
#endif
}