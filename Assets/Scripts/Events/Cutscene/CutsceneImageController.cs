using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using System.Collections;
using System.Collections.Generic;

public class CutsceneImageController : MonoBehaviour
{
    [System.Serializable] 
    public class NamedImage { public string name; public CanvasGroup canvasGroup; } // Represents an image with a name and CanvasGroup

    public List<NamedImage> images = new(); // Holds all image references to be managed
    private CanvasGroup currentImage; // Tracks the currently visible image

    void Awake() => AutoPopulateImages(); // Automatically find and store image references when the scene starts

    void Start()
    {
        Time.timeScale = 1f; // Ensure normal game speed (in case it was paused before)
        var first = FindImage("Image1"); // Try to find the image named "Image1"
        if (first != null) // If found...
        {
            currentImage = first; // Set it as the current image
            SetAlphaInstant(first, 0f); // Instantly make it invisible
            StartCoroutine(FadeIn(first, 1f)); // Start fading it in over 1 second
        }
    }

    void AutoPopulateImages() // Searches for GameObjects named "Image1" to "Image5" and stores their CanvasGroups
    {
        images.Clear(); // Clear any previous entries
        for (int i = 1; i <= 5; i++) // Loop from 1 to 5
        {
            string imageName = $"Image{i}"; // Construct name like "Image1", "Image2", etc.
            var go = GameObject.Find(imageName); // Try to find GameObject with that name
            var cg = go ? go.GetComponent<CanvasGroup>() : null; // If found, get its CanvasGroup
            if (cg != null) images.Add(new NamedImage { name = imageName, canvasGroup = cg }); // If it has a CanvasGroup, add it to the list
        }
    }

    [YarnCommand("fadeToImage")] // Allows Yarn script to call this method via command
    public static void FadeToImage(string imageName)
    {
        var controller = FindObjectOfType<CutsceneImageController>(); // Find the controller in the scene
        if (controller) controller.StartFade(imageName); // If found, start fading to the image
    }

    public void StartFade(string imageName) // Starts transition to a new image
    {
        var next = FindImage(imageName); // Look up the image by name
        if (next == null || next == currentImage) return; // Stop if image not found or already current
        StartCoroutine(FadeImages(currentImage, next)); // Begin fading between images
        currentImage = next; // Update the current image reference
    }

    CanvasGroup FindImage(string name) // Finds a CanvasGroup by name from the list
    {
        foreach (var img in images) // Loop through all images
            if (img.name == name) return img.canvasGroup; // Return the matching CanvasGroup
        return null; // If not found, return null
    }

    IEnumerator FadeImages(CanvasGroup from, CanvasGroup to) // Crossfades from one image to another
    {
        float duration = 1f, time = 0f; // Set fade duration and timer
        if (to) { to.gameObject.SetActive(true); to.alpha = 0f; } // If a target image exists, show it and start at 0 alpha
        if (from) from.gameObject.SetActive(true); // Ensure the source image is active
        while (time < duration) // Run until fade is complete
        {
            float t = time / duration; // Calculate normalized time
            if (from) from.alpha = Mathf.Lerp(1f, 0f, t); // Fade out the source
            if (to) to.alpha = Mathf.Lerp(0f, 1f, t); // Fade in the target
            time += Time.deltaTime; // Advance time
            yield return null; // Wait for next frame
        }
        if (from) { from.alpha = 0f; from.gameObject.SetActive(false); } // After fade, hide the old image
        if (to) { to.alpha = 1f; to.gameObject.SetActive(true); } // Ensure the new image is fully visible
    }

    IEnumerator FadeIn(CanvasGroup target, float duration) // Fades in a single image
    {
        float t = 0f; // Start timer
        target.alpha = 0f; target.gameObject.SetActive(true); // Make image visible and start at 0 alpha
        while (t < duration) // Fade loop
        {
            target.alpha = Mathf.Lerp(0f, 1f, t / duration); // Interpolate alpha over time
            t += Time.deltaTime; // Advance time
            yield return null; // Wait for next frame
        }
        target.alpha = 1f; // Ensure it's fully visible at the end
    }

    void SetAlphaInstant(CanvasGroup group, float value) // Immediately sets alpha and visibility of a CanvasGroup
    {
        if (!group) return; // Do nothing if null
        group.alpha = value; // Set alpha directly
        group.gameObject.SetActive(value > 0f); // Show or hide based on alpha
    }
}