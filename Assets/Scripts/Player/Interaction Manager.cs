using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public float checkRate = 0.05f;
    private float lastCheckTime;
    public float maxCheckDistance = 0.8f;
    public LayerMask layerMask;

    private GameObject curInteractGameObject;
    private IInteractable curInteractable;

    public TextMeshProUGUI promptText;

    void Start()
    {
        promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            Collider2D hit = Physics2D.OverlapCircle(transform.position, maxCheckDistance, layerMask);

            if (hit != null)
            {
                if (hit.gameObject != curInteractGameObject)
                {
                    curInteractGameObject = hit.gameObject;
                    curInteractable = hit.GetComponent<IInteractable>();

                    if (curInteractable != null)
                    {
                        SetPromptText();
                    }
                    else
                    {
                        ClearPrompt();
                    }
                }
            }
            else
            {
                ClearPrompt();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (curInteractable != null)
            {
                curInteractable.OnInteract();
                ClearPrompt();
            }
        }
    }

    void SetPromptText()
    {
        if (curInteractable != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = "[E] " + curInteractable.GetInteractPrompt();
        }
    }

    void ClearPrompt()
    {
        curInteractGameObject = null;
        curInteractable = null;
        promptText.gameObject.SetActive(false);
    }
}

public interface IInteractable
{
    string GetInteractPrompt();
    void OnInteract();
}