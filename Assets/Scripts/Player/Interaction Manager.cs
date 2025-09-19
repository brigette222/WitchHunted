using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public float checkRate = 0.05f; // How often we check for interactables
    private float lastCheckTime; // Time of last check
    public float maxCheckDistance = 0.8f; // Detection radius for interactables
    public LayerMask layerMask; // Layers that count as interactables

    private GameObject curInteractGameObject; // Current detected interactable object
    private IInteractable curInteractable; // Current interactable interface

    public TextMeshProUGUI promptText; // UI text for interaction prompt

    void Start() // Called once at initialization
    {
        promptText.gameObject.SetActive(false); // Hide prompt at start
    }

    void Update() // Called every frame
    {
        if (Time.time - lastCheckTime > checkRate) // Run detection only at intervals
        {
            lastCheckTime = Time.time; // Update last check timestamp
            Collider2D hit = Physics2D.OverlapCircle(transform.position, maxCheckDistance, layerMask); // Detect interactable

            if (hit != null) // If something is detected
            {
                if (hit.gameObject != curInteractGameObject) // If it's a new target
                {
                    curInteractGameObject = hit.gameObject; // Cache new object
                    curInteractable = hit.GetComponent<IInteractable>(); // Get interactable interface
                    if (curInteractable != null) SetPromptText(); // Show prompt
                    else ClearPrompt(); // Hide if not interactable
                }
            }
            else ClearPrompt(); // Nothing detected = clear prompt
        }

        if (Input.GetKeyDown(KeyCode.E)) // If interact key pressed
        {
            if (curInteractable != null) // If valid interactable
            {
                curInteractable.OnInteract(); // Trigger interaction
                ClearPrompt(); // Clear prompt after interacting
            }
        }
    }

    void SetPromptText() // Displays prompt text
    {
        if (curInteractable != null)
        {
            promptText.gameObject.SetActive(true); // Enable prompt UI
            promptText.text = "[E] " + curInteractable.GetInteractPrompt(); // Show interact message
        }
    }

    void ClearPrompt() // Clears prompt and cached interactable
    {
        curInteractGameObject = null; // Reset current object
        curInteractable = null; // Reset current interactable
        promptText.gameObject.SetActive(false); // Hide prompt UI
    }
}

public interface IInteractable // Interface for all interactable objects
{
    string GetInteractPrompt(); // Returns interaction prompt
    void OnInteract(); // Executes interaction logic
}