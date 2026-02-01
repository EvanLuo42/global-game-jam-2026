using System.Collections.Generic;
using DefaultNamespace;

public static class GlobalState
{
    /// <summary>
    /// 当前关卡定义
    /// </summary>
    public static LevelDefinition CurrentLevel;
    
    /// <summary>
    /// 当前关卡的结算结果
    /// </summary>
    public static ScoreResult ScoreResult;
    
    /// <summary>
    /// 所有关卡的结算历史
    /// </summary>
    public static List<ScoreResult> LevelHistory = new();
    
    /// <summary>
    /// 累计分数 (用于最终结局判定)
    /// </summary>
    public static CumulativeScore CumulativeScore;
    
    /// <summary>
    /// 最终结局
    /// </summary>
    public static GameEnding FinalEnding;
    
    /// <summary>
    /// 当前是否为教学关卡
    /// </summary>
    public static bool IsTutorialLevel => CurrentLevel != null && CurrentLevel.isTutorial;
    
    /// <summary>
    /// 记录当前关卡结果并累计
    /// </summary>
    public static void RecordCurrentLevelResult()
    {
        LevelHistory.Add(ScoreResult);
        
        // 只有非教学关卡才计入累计分数
        if (!IsTutorialLevel)
        {
            CumulativeScore.AddLevelResult(ScoreResult);
        }
    }
    
    /// <summary>
    /// 重置所有游戏状态 (新游戏时调用)
    /// </summary>
    public static void ResetGameState()
    {
        CurrentLevel = null;
        ScoreResult = default;
        LevelHistory.Clear();
        CumulativeScore.Reset();
        FinalEnding = default;
    }
    
    /// <summary>
    /// 检查是否已完成所有关卡
    /// </summary>
    public static bool IsGameComplete => CurrentLevel != null && CurrentLevel.nextLevel == null;
}
