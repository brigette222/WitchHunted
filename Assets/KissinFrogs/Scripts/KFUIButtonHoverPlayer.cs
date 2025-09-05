using UnityEngine;
using UnityEngine.EventSystems;

// This script will play a hover sound when the mouse enters a button's area
public class KFUIButtonHoverPlayer : MonoBehaviour, IPointerEnterHandler    // Define a public class called KFUIButtonHoverPlayer    // It implements IPointerEnterHandler, which lets us react when the pointer enters a UI element
{
    public AudioSource hoverAudio; // Public reference to an AudioSource component that will play the hover sound

   
    public void OnPointerEnter(PointerEventData eventData)  // This method is automatically called when the pointer enters the UI element
    {
        
        if (hoverAudio != null && !hoverAudio.isPlaying)   // If we have an audio source and it's not currently playing, play it
        {
            hoverAudio.Play();   // Play the hover sound once
        }
    }
}