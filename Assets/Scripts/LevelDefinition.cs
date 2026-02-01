using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Scriptable Objects/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Meta")]
    public string displayName;
    
    [Tooltip("是否为教学关卡 (教学关卡不计入最终结局判定)")]
    public bool isTutorial;

    [Header("Scene Flow")]
    public LevelDefinition nextLevel;
    
    [Header("Assets")]
    [Tooltip("原始图片")]
    public Texture2D sourceImage;
    
    [Tooltip("可编辑区域遮罩 (白色=需要修改的区域, 用于计算总面积)")]
    public Texture2D editableAreaMask;
    
    [Tooltip("Injustice区域遮罩 (白色=领导要求用Darken/Blur/Invert修改的区域)")]
    public Texture2D injusticeMask;
    
    [Header("Dialogue")]
    public FungusIntroData introDialogue;
    
    [Header("Time Limit")]
    [Tooltip("关卡时间限制 (秒), 0表示无限制")]
    public float timeLimit = 120f; // 默认2分钟
}
