using UnityEngine;


public static class DialogueTriggerMemory
{
    public static bool HasTriggered(string id) // Checks if a dialogue with the given ID has been triggered before
    {
        return PlayerPrefs.GetInt("DialogueTrigger_" + id, 0) == 1; // Retrieves 1 if triggered, else 0 (default)
    }

    public static void MarkAsTriggered(string id) // Marks a dialogue with the given ID as triggered
    {
        PlayerPrefs.SetInt("DialogueTrigger_" + id, 1); // Stores 1 in PlayerPrefs for the given dialogue ID
        PlayerPrefs.Save(); // Saves PlayerPrefs to disk
    }

    public static void ResetAllTriggers() // Resets all saved dialogue triggers
    {
        PlayerPrefs.DeleteAll(); // Deletes all keys in PlayerPrefs (not just dialogues!)
        PlayerPrefs.Save(); // Saves changes to disk
    }
}