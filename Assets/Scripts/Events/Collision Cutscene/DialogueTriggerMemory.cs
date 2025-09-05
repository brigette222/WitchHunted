using UnityEngine;

public static class DialogueTriggerMemory
{
    public static bool HasTriggered(string id)
    {
        return PlayerPrefs.GetInt("DialogueTrigger_" + id, 0) == 1;
    }

    public static void MarkAsTriggered(string id)
    {
        Debug.Log("Marking dialogue '" + id + "' as triggered.");
        PlayerPrefs.SetInt("DialogueTrigger_" + id, 1);
        PlayerPrefs.Save();
    }

    public static void ResetAllTriggers()
    {
        Debug.Log("Resetting all dialogue triggers.");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}