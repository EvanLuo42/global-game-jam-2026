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
    public Texture sourceImage;
    public Texture targetMask;
    public VideoClip introVideo;
}
