using UnityEngine;
using System.Collections;

public class TileSpawner : MonoBehaviour
{
    ForestManager forMan; // Reference to ForestManager in the scene

    void Awake()
    {
        forMan = FindObjectOfType<ForestManager>(); // Find the ForestManager instance
        GameObject goFloor = Instantiate(forMan.FloorPrefab, transform.position, Quaternion.identity) as GameObject; // Spawn a floor tile
        goFloor.name = forMan.FloorPrefab.name; // Give it a clean name
        goFloor.transform.SetParent(forMan.transform); // Parent under ForestManager for organization

        // Expand ForestManager’s bounds based on this tile’s position
        if (transform.position.x > forMan.maxX) { forMan.maxX = transform.position.x; }
        if (transform.position.x < forMan.minX) { forMan.minX = transform.position.x; }
        if (transform.position.y > forMan.maxY) { forMan.maxY = transform.position.y; }
        if (transform.position.y < forMan.minY) { forMan.minY = transform.position.y; }
    }

    void Start()
    {
        StartCoroutine(DelayedWallSpawn()); // Spawn walls after a short delay
    }

    IEnumerator DelayedWallSpawn()
    {
        yield return new WaitForSeconds(0.1f); // Let Player spawn first before placing walls

        LayerMask envMask = LayerMask.GetMask("Wall", "Floor"); // Check against walls + floors
        Vector2 hitSize = Vector2.one * 0.8f; // Size of overlap check box

        // Loop around this tile (-1 to 1 in X and Y)
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 targetPos = new Vector2(transform.position.x + x, transform.position.y + y); // Position to check
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, envMask); // Detect if something’s already there

                if (!hit) // If no floor/wall found
                {
                    GameObject goWall = Instantiate(forMan.WallPrefab, targetPos, Quaternion.identity); // Place wall
                    goWall.name = forMan.WallPrefab.name; // Clean name
                    goWall.transform.SetParent(forMan.transform); // Parent under ForestManager
                }
            }
        }

        Destroy(gameObject); // Remove spawner after it’s done its job
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white; // Set gizmo color
        Gizmos.DrawCube(transform.position, Vector3.one); // Draw a cube to visualize spawner in Scene view
    }
}
