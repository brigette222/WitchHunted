using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public ItemData itemToGive; // Item given when gathered
    public int quantityPerHit = 1; // How many items per hit
    public int capacity; // Total amount resource holds
    public GameObject hitParticle; // Particle effect prefab when hit

    public void Gather(Vector3 hitPoint, Vector3 hitNormal) // Called when player hits resource
    {
        for (int i = 0; i < quantityPerHit; i++) // Loop for items per hit
        {
            if (capacity <= 0) break; // Stop if resource is empty
            capacity -= 1; // Reduce resource capacity
            Inventory.instance.AddItem(itemToGive); // Give item to player
        }

        Destroy(Instantiate(hitParticle, hitPoint, Quaternion.LookRotation(hitNormal, Vector3.up)), 1.0f); // Spawn particle and destroy it after 1s

        if (capacity <= 0) Destroy(gameObject); // Destroy resource if depleted
    }
}
