using UnityEngine;
using System.Collections; 

public class TileSpawner : MonoBehaviour
{
    ForestManager forMan;

    void Awake()
    {
        forMan = FindObjectOfType<ForestManager>();
        GameObject goFloor = Instantiate(forMan.FloorPrefab, transform.position, Quaternion.identity) as GameObject;
        goFloor.name = forMan.FloorPrefab.name;
        goFloor.transform.SetParent(forMan.transform);
        if (transform.position.x > forMan.maxX)
        {
            forMan.maxX = transform.position.x;
        }
        if (transform.position.x < forMan.minX)
        {
            forMan.minX = transform.position.x;
        }
        if (transform.position.y > forMan.maxY)
        {
            forMan.maxY = transform.position.y;
        }
        if (transform.position.y < forMan.minY)
        {
            forMan.minY = transform.position.y;
        }
    }


    void Start()
    {
        StartCoroutine(DelayedWallSpawn());
    }

    IEnumerator DelayedWallSpawn()
    {
        yield return new WaitForSeconds(0.1f); // Short delay to let Player spawn first

        LayerMask envMask = LayerMask.GetMask("Wall", "Floor");
        Vector2 hitSize = Vector2.one * 0.8f;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 targetPos = new Vector2(transform.position.x + x, transform.position.y + y);
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, envMask);
                if (!hit)
                {
                    GameObject goWall = Instantiate(forMan.WallPrefab, targetPos, Quaternion.identity);
                    goWall.name = forMan.WallPrefab.name;
                    goWall.transform.SetParent(forMan.transform);
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }

}
