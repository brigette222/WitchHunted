using UnityEngine;
using Yarn.Unity;

public class NodeTriggeredMusicSwitch : MonoBehaviour
{
    [Header("Music Switch Settings")]
    public string targetNodeName;             // The Yarn node to listen for
    public AudioSource newTrackToPlay;        // The new music track to fade in

    private void Awake()
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>();
        if (runner != null)
        {
            runner.onNodeComplete.AddListener(HandleNodeEnd);
        }
        else
        {
            Debug.LogWarning("[NodeTriggeredMusicSwitch] DialogueRunner not found.");
        }
    }

    private void HandleNodeEnd(string nodeName)
    {
        if (nodeName == targetNodeName)
        {
            MusicManager mm = FindObjectOfType<MusicManager>();
            if (mm != null && newTrackToPlay != null)
            {
                mm.SwitchTo(newTrackToPlay);
                Debug.Log($"[NodeTriggeredMusicSwitch] Node '{nodeName}' ended. Switched to new track.");
            }
        }
    }
}