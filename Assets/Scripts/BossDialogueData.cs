using UnityEngine;

[System.Serializable]
public class BossDialogueEntry
{
    [TextArea(2, 6)]
    public string text;
    public AudioClip voiceClip;
}

[CreateAssetMenu(fileName = "BossDialogue", menuName = "Game/Boss Dialogue Data")]
public class BossDialogueData : ScriptableObject
{
    public BossDialogueEntry[] entries = System.Array.Empty<BossDialogueEntry>();

    public bool HasEntries => entries != null && entries.Length > 0;
}
