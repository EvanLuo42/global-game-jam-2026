using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Scriptable Objects/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Meta")]
    public string displayName;

    [Header("Scene Flow")]
    public LevelDefinition nextLevel;
    
    [Header("Assets")]
    public Texture2D sourceImage;
    public Texture2D injusticeMask;
    
    public FungusIntroData introDialogue;
}
