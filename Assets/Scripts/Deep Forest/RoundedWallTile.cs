using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]// Ensure this GameObject always has a SpriteRenderer attached
public class RoundedWallTile : MonoBehaviour
{
    [Header("Main Wall Settings")]
    public Sprite[] edgeSprites = new Sprite[16]; // Array of wall edge sprites (indexed by bitmask values)
    public LayerMask floorMask; // Defines which layer is considered "floor" for adjacency checks

    [Header("Top Visual Tree Settings")]
    public GameObject topVisualPrefab; // Prefab for visuals above the wall (trees, etc.)
    public Sprite[] topTreeSprites = new Sprite[16]; // Optional sprite variations based on bitmask

    [Header("Side Visual Settings")]
    public GameObject leftVisualPrefab; // Prefab for visuals on the left side
    public Sprite[] leftSprites = new Sprite[16]; // Sprites for left visuals
    public GameObject rightVisualPrefab; // Prefab for visuals on the right side
    public Sprite[] rightSprites = new Sprite[16]; // Sprites for right visuals
    public GameObject bottomVisualPrefab; // Prefab for visuals at the bottom
    public Sprite[] bottomSprites = new Sprite[16]; // Sprites for bottom visuals

    [Header("Diagonal Overrides (used only if bitmask = 0)")]
    public Sprite cornerTopLeftOverride;     // Fallback sprite if isolated tile has top-left neighbor
    public Sprite cornerTopRightOverride;    // Fallback sprite if isolated tile has top-right neighbor
    public Sprite cornerBottomLeftOverride;  // Fallback sprite if isolated tile has bottom-left neighbor
    public Sprite cornerBottomRightOverride; // Fallback sprite if isolated tile has bottom-right neighbor

    private SpriteRenderer sr; // Cached reference to SpriteRenderer

    void Start()
    {
        sr = GetComponent<SpriteRenderer>(); // Cache the SpriteRenderer component
        ApplyEdgeSprite(); // Set correct sprite when the game starts
    }

    // Refresh visuals if something changes in the environment
    public void RefreshBitmaskVisual()
    {
        // Loop through child objects and remove visuals with specific prefixes
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("TopVisual_") ||
                child.name.StartsWith("LeftVisual_") ||
                child.name.StartsWith("RightVisual_") ||
                child.name.StartsWith("BottomVisual_"))
            {
                Destroy(child.gameObject); // Remove old decorations
            }
        }

        ApplyEdgeSprite(); // Reapply correct visuals
    }

    // Assigns the correct sprite + creates visuals based on bitmask
    void ApplyEdgeSprite()
    {
        int bitmask = CalculateBitmask(); // Determine wall shape based on neighbors

        // If tile has no direct neighbors, check diagonal overrides
        if (bitmask == 0)
        {
            Sprite diagonal = GetDiagonalOverride(); // Try to find a corner sprite
            if (diagonal != null)
            {
                sr.sprite = diagonal; // Use diagonal sprite instead
                return; // Exit early
            }
        }
        
        if (bitmask >= 0 && bitmask < edgeSprites.Length && edgeSprites[bitmask] != null)// If valid bitmask, assign edge sprite
        {
            sr.sprite = edgeSprites[bitmask];
        }

        // Spawn optional decorations based on bitmask rules
        if (ShouldShowTopVisual(bitmask)) CreateVisual(topVisualPrefab, topTreeSprites, bitmask, Vector3.up, "TopVisual_");
        if (ShouldShowLeftVisual(bitmask)) CreateVisual(leftVisualPrefab, leftSprites, bitmask, Vector3.left, "LeftVisual_");
        if (ShouldShowRightVisual(bitmask)) CreateVisual(rightVisualPrefab, rightSprites, bitmask, Vector3.right, "RightVisual_");
        if (ShouldShowBottomVisual(bitmask)) CreateVisual(bottomVisualPrefab, bottomSprites, bitmask, Vector3.down, "BottomVisual_");
    }

    // Spawns a decoration prefab with the correct sprite
    void CreateVisual(GameObject prefab, Sprite[] spriteArray, int bitmask, Vector3 offset, string namePrefix)
    {
        // Safety checks (nulls, array bounds, missing sprites)
        if (prefab == null || spriteArray == null || bitmask >= spriteArray.Length || spriteArray[bitmask] == null) return;

        Vector3 spawnPos = transform.position + offset; // Position offset relative to wall
        GameObject visual = Instantiate(prefab, spawnPos, Quaternion.identity); // Spawn prefab
        visual.name = namePrefix + bitmask; // Name for easy cleanup later
        visual.transform.SetParent(transform); // Attach to wall tile

        SpriteRenderer visSR = visual.GetComponent<SpriteRenderer>(); // Get renderer
        visSR.sprite = spriteArray[bitmask]; // Assign sprite based on bitmask
    }

    // Creates bitmask (binary representation of neighbors)
    int CalculateBitmask()
    {
        int mask = 0;
        if (HasFloor(Vector2.up)) mask += 1;    // Up = bit 1
        if (HasFloor(Vector2.right)) mask += 2; // Right = bit 2
        if (HasFloor(Vector2.down)) mask += 4;  // Down = bit 4
        if (HasFloor(Vector2.left)) mask += 8;  // Left = bit 8
        return mask;
    }

    // Public getter for bitmask (used by other scripts)
    public int GetActualBitmask() => CalculateBitmask();

    // Checks if a floor exists at a given direction
    bool HasFloor(Vector2 dir)
    {
        Vector2 checkPos = (Vector2)transform.position + dir; // Position to check
        return Physics2D.OverlapBox(checkPos, Vector2.one * 0.8f, 0, floorMask); // Overlap box detects colliders
    }

    // Handles special diagonal override sprites (used if isolated tile)
    Sprite GetDiagonalOverride()
    {
        if (HasFloor(Vector2.up + Vector2.left) && cornerTopLeftOverride) return cornerTopLeftOverride;
        if (HasFloor(Vector2.up + Vector2.right) && cornerTopRightOverride) return cornerTopRightOverride;
        if (HasFloor(Vector2.down + Vector2.left) && cornerBottomLeftOverride) return cornerBottomLeftOverride;
        if (HasFloor(Vector2.down + Vector2.right) && cornerBottomRightOverride) return cornerBottomRightOverride;
        return null;
    }

    // Rules for when to spawn top/left/right/bottom visuals
    bool ShouldShowTopVisual(int bitmask) => bitmask == 4 || bitmask == 14 || bitmask == 12 || bitmask == 6;
    bool ShouldShowLeftVisual(int bitmask) => bitmask == 2 || bitmask == 6 || bitmask == 10 || bitmask == 14;
    bool ShouldShowRightVisual(int bitmask) => bitmask == 8 || bitmask == 12 || bitmask == 10 || bitmask == 14;
    bool ShouldShowBottomVisual(int bitmask) => bitmask == 1 || bitmask == 5 || bitmask == 4 || bitmask == 7;

#if UNITY_EDITOR
    // Draws debug label in the Scene view (only in editor)
    void OnDrawGizmosSelected()
    {
        int bitmask = CalculateBitmask();
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, "Bitmask ID: " + bitmask);
    }
#endif
}