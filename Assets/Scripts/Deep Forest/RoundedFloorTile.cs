using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RoundedFloorTile : MonoBehaviour
{
    [Header("Main Floor Edge Sprites (0–15)")]
    public Sprite[] edgeSprites = new Sprite[16];  // Cardinal direction bitmask (0–15)
    public LayerMask floorMask;
    public LayerMask wallMask;

    [Header("Outer Corner Sprites (for bitmask = 0)")]
    public Sprite cornerTopLeft;
    public Sprite cornerTopRight;
    public Sprite cornerBottomLeft;
    public Sprite cornerBottomRight;

    [Header("Middle Corner Sprites")]
    public Sprite midCornerTop;
    public Sprite midCornerRight;
    public Sprite midCornerBottom;
    public Sprite midCornerLeft;

    [Header("Corner Wall Detection (Overlay if wall is using these)")]
    public Sprite wallCornerTopLeft;
    public Sprite wallCornerTopRight;
    public Sprite wallCornerBottomLeft;
    public Sprite wallCornerBottomRight;

    [Header("Overlay Sprites to display on this floor tile")]
    public Sprite overlayTopLeft;
    public Sprite overlayTopRight;
    public Sprite overlayBottomLeft;
    public Sprite overlayBottomRight;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplySprite();
        DetectCornerWalls();
        StoreBitmask();  // <- NEW: stores the floor bitmask for trees
    }

    void ApplySprite()
    {
        int bitmask = GetCardinalBitmask();

        // Handle diagonal-only (bitmask = 0)
        if (bitmask == 0)
        {
            Sprite diagonal = GetOuterCorner();
            if (diagonal != null)
            {
                sr.sprite = diagonal;
                return;
            }
        }

        // Handle middle corners
        Sprite middle = GetMiddleCorner();
        if (middle != null)
        {
            sr.sprite = middle;
            return;
        }

        // Standard 0–15 bitmask edge sprite
        if (bitmask >= 0 && bitmask < edgeSprites.Length && edgeSprites[bitmask] != null)
        {
            sr.sprite = edgeSprites[bitmask];
        }
    }

    void DetectCornerWalls()
    {
        TryDetectWallCorner(Vector2.up + Vector2.left, wallCornerBottomRight, overlayBottomRight, "BottomRightOverlay");
        TryDetectWallCorner(Vector2.up + Vector2.right, wallCornerBottomLeft, overlayBottomLeft, "BottomLeftOverlay");
        TryDetectWallCorner(Vector2.down + Vector2.left, wallCornerTopRight, overlayTopRight, "TopRightOverlay");
        TryDetectWallCorner(Vector2.down + Vector2.right, wallCornerTopLeft, overlayTopLeft, "TopLeftOverlay");
    }

    void TryDetectWallCorner(Vector2 dir, Sprite targetWallCorner, Sprite overlaySprite, string overlayName)
    {
        if (targetWallCorner == null || overlaySprite == null) return;

        Vector2 checkPos = (Vector2)transform.position + dir;
        Collider2D hit = Physics2D.OverlapBox(checkPos, Vector2.one * 0.8f, 0, wallMask);

        if (hit != null)
        {
            var wallTile = hit.GetComponent<SpriteRenderer>();
            if (wallTile != null && wallTile.sprite == targetWallCorner)
            {
                GameObject overlay = new GameObject(overlayName);
                overlay.transform.position = transform.position;
                overlay.transform.SetParent(transform);

                var overlaySR = overlay.AddComponent<SpriteRenderer>();
                overlaySR.sprite = overlaySprite;
                overlaySR.sortingOrder = sr.sortingOrder + 1;
            }
        }
    }

    int GetCardinalBitmask()
    {
        int mask = 0;
        if (HasFloor(Vector2.up)) mask += 1;
        if (HasFloor(Vector2.right)) mask += 2;
        if (HasFloor(Vector2.down)) mask += 4;
        if (HasFloor(Vector2.left)) mask += 8;
        return mask;
    }

    Sprite GetOuterCorner()
    {
        if (HasFloor(Vector2.up + Vector2.left)) return cornerTopLeft;
        if (HasFloor(Vector2.up + Vector2.right)) return cornerTopRight;
        if (HasFloor(Vector2.down + Vector2.left)) return cornerBottomLeft;
        if (HasFloor(Vector2.down + Vector2.right)) return cornerBottomRight;
        return null;
    }

    Sprite GetMiddleCorner()
    {
        bool up = HasFloor(Vector2.up);
        bool down = HasFloor(Vector2.down);
        bool left = HasFloor(Vector2.left);
        bool right = HasFloor(Vector2.right);

        bool upLeft = HasFloor(Vector2.up + Vector2.left);
        bool upRight = HasFloor(Vector2.up + Vector2.right);
        bool downLeft = HasFloor(Vector2.down + Vector2.left);
        bool downRight = HasFloor(Vector2.down + Vector2.right);

        if (!up && upLeft && upRight) return midCornerTop;
        if (!right && upRight && downRight) return midCornerRight;
        if (!down && downLeft && downRight) return midCornerBottom;
        if (!left && upLeft && downLeft) return midCornerLeft;

        return null;
    }

    bool HasFloor(Vector2 dir)
    {
        Vector2 pos = (Vector2)transform.position + dir;
        return Physics2D.OverlapBox(pos, Vector2.one * 0.8f, 0, floorMask);
    }

    void StoreBitmask()
    {
        int bitmask = GetCardinalBitmask();
        TileBitmaskInfo info = GetComponent<TileBitmaskInfo>();
        if (info == null)
        {
            info = gameObject.AddComponent<TileBitmaskInfo>();
        }

        info.bitmaskValue = bitmask;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        int bitmask = GetCardinalBitmask();
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, "Floor Bitmask: " + bitmask);
    }
#endif
}
