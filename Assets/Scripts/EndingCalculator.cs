using DefaultNamespace;
using UnityEngine;

/// <summary>
/// 最终结局计算器
/// 
/// 基于累计分数判定最终结局：
/// 
/// Y轴（完成度）= (总injustice面积 + 总justice面积) / 总面积
/// X轴（意识形态偏向）= 总injustice面积 / (总injustice面积 + 总justice面积)
///   - 0 = 纯反贼（100% Justice）
///   - 0.5 = 绝对中立
///   - 1 = 纯走狗（100% Injustice）
/// 
/// 判定流程（漏斗式，从高优先级到低优先级）：
/// 1. 完成度 < 60% → 结局1: You're Fired
/// 2. 完成度 > 80% 且 0.40 <= 偏向 <= 0.60 → 结局3: 跳槽N3
/// 3. 偏向 > 0.5 → 结局4: 加薪
/// 4. 偏向 <= 0.5 → 结局2: 你知道的太多了
/// </summary>
public static class EndingCalculator
{
    [System.Serializable]
    public struct EndingThresholds
    {
        [Header("第一层：生存红线")]
        [Tooltip("最低完成度要求，低于此值被解雇")]
        [Range(0f, 1f)]
        public float minCompletionRate;

        [Header("第二层：完美平衡")]
        [Tooltip("跳槽结局要求的最低完成度")]
        [Range(0f, 1f)]
        public float perfectCompletionRate;
        
        [Tooltip("跳槽结局的意识形态偏向下限")]
        [Range(0f, 1f)]
        public float perfectBiasMin;
        
        [Tooltip("跳槽结局的意识形态偏向上限")]
        [Range(0f, 1f)]
        public float perfectBiasMax;

        /// <summary>
        /// 默认阈值
        /// </summary>
        public static EndingThresholds Default => new()
        {
            minCompletionRate = 0.6f,      // 60%
            perfectCompletionRate = 0.8f,   // 80%
            perfectBiasMin = 0.4f,          // 40%
            perfectBiasMax = 0.6f           // 60%
        };
    }

    /// <summary>
    /// 根据累计分数计算最终结局
    /// </summary>
    public static GameEnding CalculateEnding(CumulativeScore score)
    {
        return CalculateEnding(score, EndingThresholds.Default);
    }

    /// <summary>
    /// 根据累计分数和自定义阈值计算最终结局
    /// </summary>
    public static GameEnding CalculateEnding(CumulativeScore score, EndingThresholds thresholds)
    {
        var completion = score.completionRate;
        var bias = score.ideologyBias;

        // ========================================
        // 第一层：生存红线 (The Survival Check)
        // ========================================
        // 无论政治立场如何，完成度不足就被解雇
        if (completion < thresholds.minCompletionRate)
        {
            return GameEnding.Fired;
        }

        // ========================================
        // 第二层：完美平衡检查 (The Perfectionist Check)
        // ========================================
        // 高完成度 + 精确的骑墙 = 操纵大师
        if (completion > thresholds.perfectCompletionRate &&
            bias >= thresholds.perfectBiasMin &&
            bias <= thresholds.perfectBiasMax)
        {
            return GameEnding.JumpShip;
        }

        // ========================================
        // 第三层：阵营划分 (The Ideology Split)
        // ========================================
        // 偏向 > 0.5 = 走狗 (Injustice更多)
        if (bias > 0.5f)
        {
            return GameEnding.Promoted;
        }

        // 偏向 <= 0.5 = 反贼 (Justice更多或相等)
        return GameEnding.KnowTooMuch;
    }

    /// <summary>
    /// 获取结局的显示名称
    /// </summary>
    public static string GetEndingTitle(GameEnding ending)
    {
        return ending switch
        {
            GameEnding.Fired => "You're Fired!",
            GameEnding.KnowTooMuch => "You Know Too Much...",
            GameEnding.JumpShip => "Welcome to N3 Media Co, Ltd!",
            GameEnding.Promoted => "Congratulations! You Got a Raise!",
            _ => "Unknown Ending"
        };
    }

    /// <summary>
    /// 获取结局的描述文本
    /// </summary>
    public static string GetEndingDescription(GameEnding ending)
    {
        return ending switch
        {
            GameEnding.Fired => 
                "Your work performance was disappointing. " +
                "The newspaper sales plummeted, and the company had no choice but to let you go. " +
                "Maybe journalism isn't for you after all...",
            
            GameEnding.KnowTooMuch => 
                "Your persistent pursuit of justice has not gone unnoticed. " +
                "Unfortunately, those in power don't appreciate employees who ask too many questions. " +
                "One day, you simply... disappeared.",
            
            GameEnding.JumpShip => 
                "Your exceptional ability to balance all interests has caught the attention of headhunters. " +
                "N3 Media Co, Ltd offers you a position with triple the salary. " +
                "You've mastered the art of survival in this industry.",
            
            GameEnding.Promoted => 
                "Your dedication to following orders has impressed your superiors. " +
                "The boss personally announces your promotion and salary increase. " +
                "You've become a valuable asset to the company.",
            
            _ => "An unexpected ending occurred."
        };
    }

    /// <summary>
    /// 获取结局的中文名称
    /// </summary>
    public static string GetEndingTitleCN(GameEnding ending)
    {
        return ending switch
        {
            GameEnding.Fired => "你被解雇了！",
            GameEnding.KnowTooMuch => "你知道的太多了...",
            GameEnding.JumpShip => "欢迎加入 N3 传媒有限公司！",
            GameEnding.Promoted => "恭喜！你被加薪了！",
            _ => "未知结局"
        };
    }

    /// <summary>
    /// 获取结局的中文描述
    /// </summary>
    public static string GetEndingDescriptionCN(GameEnding ending)
    {
        return ending switch
        {
            GameEnding.Fired => 
                "你的工作表现令人失望。报纸销量一落千丈，公司不得不让你离开。" +
                "也许新闻行业并不适合你...",
            
            GameEnding.KnowTooMuch => 
                "你对正义的执着追求并非没有被注意到。" +
                "不幸的是，当权者并不欣赏问太多问题的员工。" +
                "某天，你就这样...消失了。",
            
            GameEnding.JumpShip => 
                "你平衡各方利益的卓越能力引起了猎头的注意。" +
                "N3 传媒有限公司向你提供了三倍薪资的职位。" +
                "你已经掌握了在这个行业生存的艺术。",
            
            GameEnding.Promoted => 
                "你服从命令的奉献精神给上级留下了深刻印象。" +
                "老板亲自宣布了你的晋升和加薪。" +
                "你已成为公司的宝贵资产。",
            
            _ => "发生了意想不到的结局。"
        };
    }

    /// <summary>
    /// 调试：打印判定过程
    /// </summary>
    public static void DebugPrintJudgment(CumulativeScore score, EndingThresholds? thresholds = null)
    {
        var t = thresholds ?? EndingThresholds.Default;
        
        Debug.Log("=== Ending Calculation Debug ===");
        Debug.Log($"Cumulative Stats:");
        Debug.Log($"  Total Editable Area: {score.totalEditableArea}");
        Debug.Log($"  Total Injustice Area: {score.totalInjusticeArea}");
        Debug.Log($"  Total Justice Area: {score.totalJusticeArea}");
        Debug.Log($"  Completion Rate (Y): {score.completionRate:P1}");
        Debug.Log($"  Ideology Bias (X): {score.ideologyBias:F3}");
        Debug.Log($"");
        Debug.Log($"Thresholds:");
        Debug.Log($"  Min Completion: {t.minCompletionRate:P0}");
        Debug.Log($"  Perfect Completion: {t.perfectCompletionRate:P0}");
        Debug.Log($"  Perfect Bias Range: [{t.perfectBiasMin:F2}, {t.perfectBiasMax:F2}]");
        Debug.Log($"");
        
        var ending = CalculateEnding(score, t);
        Debug.Log($"Judgment Process:");
        
        if (score.completionRate < t.minCompletionRate)
        {
            Debug.Log($"  [Layer 1] Completion {score.completionRate:P1} < {t.minCompletionRate:P0} → FIRED");
        }
        else if (score.completionRate > t.perfectCompletionRate &&
                 score.ideologyBias >= t.perfectBiasMin &&
                 score.ideologyBias <= t.perfectBiasMax)
        {
            Debug.Log($"  [Layer 1] Passed (Completion >= {t.minCompletionRate:P0})");
            Debug.Log($"  [Layer 2] Completion > {t.perfectCompletionRate:P0} AND Bias in [{t.perfectBiasMin:F2}, {t.perfectBiasMax:F2}] → JUMP SHIP");
        }
        else if (score.ideologyBias > 0.5f)
        {
            Debug.Log($"  [Layer 1] Passed (Completion >= {t.minCompletionRate:P0})");
            Debug.Log($"  [Layer 2] Skipped (Not in perfect range)");
            Debug.Log($"  [Layer 3] Bias {score.ideologyBias:F3} > 0.5 → PROMOTED");
        }
        else
        {
            Debug.Log($"  [Layer 1] Passed (Completion >= {t.minCompletionRate:P0})");
            Debug.Log($"  [Layer 2] Skipped (Not in perfect range)");
            Debug.Log($"  [Layer 3] Bias {score.ideologyBias:F3} <= 0.5 → KNOW TOO MUCH");
        }
        
        Debug.Log($"");
        Debug.Log($"Final Ending: {ending} - {GetEndingTitle(ending)}");
    }
}
