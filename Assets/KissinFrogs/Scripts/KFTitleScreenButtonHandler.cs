using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class KFTitleScreenButtonHandler : MonoBehaviour
{
    [System.Serializable]
    public class ButtonData
    {
        public RectTransform buttonTransform;     // The button's RectTransform
        public Button uiButton;                   // The actual Button component
        public string sceneToLoad;                // Scene name (or "QUIT" for quitting)
        public AudioSource hoverSoundSource;      // Plays on hover
        public AudioSource clickSoundSource;      // Plays on click

        [HideInInspector] public Vector3 originalScale;
        [HideInInspector] public Vector3 targetScale;
    }

    [Header("Button Hover Settings")]
    public List<ButtonData> buttons = new List<ButtonData>();
    public float hoverScale = 1.05f;
    public float scaleDampTime = 0.08f;

    private Vector3[] scaleVelocity;

    void Start()
    {
        int count = buttons.Count;
        scaleVelocity = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            int index = i;
            var btn = buttons[i];

            if (btn.buttonTransform != null)
            {
                btn.originalScale = btn.buttonTransform.localScale;
                btn.targetScale = btn.originalScale;

                // Hover listeners
                EventTrigger trigger = btn.buttonTransform.GetComponent<EventTrigger>();
                if (trigger == null) trigger = btn.buttonTransform.gameObject.AddComponent<EventTrigger>();

                // OnPointerEnter
                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((data) => OnButtonHoverEnter(buttons[index]));
                trigger.triggers.Add(enterEntry);

                // OnPointerExit
                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) => OnButtonHoverExit(buttons[index]));
                trigger.triggers.Add(exitEntry);
            }

            // Click listeners
            if (btn.uiButton != null)
            {
                btn.uiButton.onClick.AddListener(() => OnButtonClicked(buttons[index]));
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            var btn = buttons[i];
            if (btn.buttonTransform != null)
            {
                btn.buttonTransform.localScale = Vector3.SmoothDamp(
                    btn.buttonTransform.localScale,
                    btn.targetScale,
                    ref scaleVelocity[i],
                    scaleDampTime
                );
            }
        }
    }

    void OnButtonHoverEnter(ButtonData btn)
    {
        btn.targetScale = btn.originalScale * hoverScale;

        if (btn.hoverSoundSource != null)
        {
            btn.hoverSoundSource.Stop();
            btn.hoverSoundSource.Play();
        }
    }

    void OnButtonHoverExit(ButtonData btn)
    {
        btn.targetScale = btn.originalScale;
    }

    void OnButtonClicked(ButtonData btn)
    {
        StartCoroutine(PlayClickAndThenAct(btn));
    }

    private IEnumerator PlayClickAndThenAct(ButtonData btn)
    {
        if (btn.clickSoundSource != null && btn.clickSoundSource.clip != null)
        {
            btn.clickSoundSource.Stop();
            btn.clickSoundSource.Play();
            yield return new WaitForSeconds(btn.clickSoundSource.clip.length);
        }

        if (btn.sceneToLoad == "QUIT")
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            SceneManager.LoadScene(btn.sceneToLoad);
        }
    }
}
