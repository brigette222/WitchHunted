using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class ExitDoorway : MonoBehaviour
{
    private void Reset()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.size = new Vector2(1.5f, 1.5f);
        box.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered ExitDoorway.");

           
        }
    }
}