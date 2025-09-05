using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using System.Collections;
using System.Collections.Generic;

public class CutsceneImageController : MonoBehaviour
{
    [System.Serializable]
    public class NamedImage
    {
        public string name;
        public CanvasGroup canvasGroup;
    }

    public List<NamedImage> images = new List<NamedImage>();
    private CanvasGroup currentImage;

    void Awake()
    {
        AutoPopulateImages();
    }

    void Start()
    {
        Debug.Log($"[Intro] Time.timeScale = {Time.timeScale}");
        Time.timeScale = 1f;

        var first = FindImage("Image1");
        if (first != null)
        {
            currentImage = first;
            SetAlphaInstant(first, 0f);
            StartCoroutine(FadeIn(first, 1f));
        }
    }

    void AutoPopulateImages()
    {
        images.Clear();

        for (int i = 1; i <= 5; i++)
        {
            string imageName = $"Image{i}";
            var go = GameObject.Find(imageName);
            if (go != null)
            {
                var cg = go.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    images.Add(new NamedImage { name = imageName, canvasGroup = cg });
                    Debug.Log($"[CutsceneImageController] Added image: {imageName}");
                }
                else
                {
                    Debug.LogWarning($"[CutsceneImageController] No CanvasGroup on: {imageName}");
                }
            }
            else
            {
                Debug.LogWarning($"[CutsceneImageController] No GameObject named: {imageName}");
            }
        }
    }

    [YarnCommand("fadeToImage")]
    public static void FadeToImage(string imageName)
    {
        var controller = GameObject.FindObjectOfType<CutsceneImageController>();
        if (controller != null)
        {
            controller.StartFade(imageName);
        }
        else
        {
            Debug.LogError("[YARN] No CutsceneImageController found in scene!");
        }
    }

    public void StartFade(string imageName)
    {
        var nextImage = FindImage(imageName);

        if (nextImage == null)
        {
            Debug.LogWarning($"[YARN] Image '{imageName}' not found!");
            return;
        }

        if (nextImage == currentImage)
        {
            Debug.Log($"[YARN] Already on image: {imageName}");
            return;
        }

        StartCoroutine(FadeImages(currentImage, nextImage));
        currentImage = nextImage;
    }

    CanvasGroup FindImage(string name)
    {
        foreach (var entry in images)
            if (entry.name == name)
                return entry.canvasGroup;

        return null;
    }

    IEnumerator FadeImages(CanvasGroup from, CanvasGroup to)
    {
        float duration = 1f;
        float time = 0f;

        if (to != null)
        {
            to.gameObject.SetActive(true);
            to.alpha = 0f;
        }

        if (from != null)
            from.gameObject.SetActive(true);

        while (time < duration)
        {
            float t = time / duration;
            if (from != null) from.alpha = Mathf.Lerp(1f, 0f, t);
            if (to != null) to.alpha = Mathf.Lerp(0f, 1f, t);
            time += Time.deltaTime;
            yield return null;
        }

        if (from != null)
        {
            from.alpha = 0f;
            from.gameObject.SetActive(false);
        }

        if (to != null)
        {
            to.alpha = 1f;
            to.gameObject.SetActive(true);
        }
    }

    IEnumerator FadeIn(CanvasGroup target, float duration)
    {
        float t = 0f;
        target.alpha = 0f;
        target.gameObject.SetActive(true);
        while (t < duration)
        {
            target.alpha = Mathf.Lerp(0f, 1f, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        target.alpha = 1f;
    }

    void SetAlphaInstant(CanvasGroup group, float value)
    {
        if (group != null)
        {
            group.alpha = value;
            group.gameObject.SetActive(value > 0f);
        }
    }
}
