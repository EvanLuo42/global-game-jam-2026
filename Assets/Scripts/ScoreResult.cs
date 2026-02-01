using UnityEngine;

namespace DefaultNamespace
{
    /// <summary>
    /// 单关结算结果
    /// </summary>
    [System.Serializable]
    public struct ScoreResult
    {
        [Header("面积统计 (像素数)")]
        [Tooltip("关卡中需要修改的总面积")]
        public int totalEditableArea;
        
        [Tooltip("玩家实际编辑的总面积 (Injustice + Justice)")]
        public int totalEditedArea;
        
        [Tooltip("按领导指示修改的面积 (Darken/Blur/Invert)")]
        public int injusticeArea;
        
        [Tooltip("使用提亮方式修改的面积 (Lighten)")]
        public int justiceArea;

        [Header("百分比数值 (0-1)")]
        [Tooltip("报纸销量 = 已修改面积 / 总面积")]
        [Range(0f, 1f)]
        public float salesRate;
        
        [Tooltip("领导满意度 = Injustice面积 / 总面积")]
        [Range(0f, 1f)]
        public float bossSatisfaction;
        
        [Tooltip("Justice值 = Justice面积 / 总面积")]
        [Range(0f, 1f)]
        public float justiceValue;

        /// <summary>
        /// 计算百分比数值
        /// </summary>
        public void CalculateRates()
        {
            if (totalEditableArea <= 0)
            {
                salesRate = 0f;
                bossSatisfaction = 0f;
                justiceValue = 0f;
                return;
            }

            salesRate = (float)totalEditedArea / totalEditableArea;
            bossSatisfaction = (float)injusticeArea / totalEditableArea;
            justiceValue = (float)justiceArea / totalEditableArea;
            
            // Clamp to valid range
            salesRate = Mathf.Clamp01(salesRate);
            bossSatisfaction = Mathf.Clamp01(bossSatisfaction);
            justiceValue = Mathf.Clamp01(justiceValue);
        }
    }

    /// <summary>
    /// 累计结算结果 (用于最终结局判定)
    /// </summary>
    [System.Serializable]
    public struct CumulativeScore
    {
        [Header("累计面积")]
        public int totalEditableArea;
        public int totalInjusticeArea;
        public int totalJusticeArea;

        [Header("计算数值")]
        [Tooltip("完成度 = (injustice + justice) / 总面积")]
        [Range(0f, 1f)]
        public float completionRate;
        
        [Tooltip("意识形态偏向 = injustice / (injustice + justice), 0=纯反贼, 1=纯走狗")]
        [Range(0f, 1f)]
        public float ideologyBias;

        /// <summary>
        /// 添加一关的结果
        /// </summary>
        public void AddLevelResult(ScoreResult result)
        {
            totalEditableArea += result.totalEditableArea;
            totalInjusticeArea += result.injusticeArea;
            totalJusticeArea += result.justiceArea;
            
            CalculateValues();
        }

        /// <summary>
        /// 计算累计数值
        /// </summary>
        public void CalculateValues()
        {
            // Y轴：完成度
            if (totalEditableArea > 0)
            {
                completionRate = (float)(totalInjusticeArea + totalJusticeArea) / totalEditableArea;
                completionRate = Mathf.Clamp01(completionRate);
            }
            else
            {
                completionRate = 0f;
            }

            // X轴：意识形态偏向
            var totalEdited = totalInjusticeArea + totalJusticeArea;
            if (totalEdited > 0)
            {
                ideologyBias = (float)totalInjusticeArea / totalEdited;
                ideologyBias = Mathf.Clamp01(ideologyBias);
            }
            else
            {
                ideologyBias = 0.5f; // 没有编辑时默认中立
            }
        }

        /// <summary>
        /// 重置累计数据
        /// </summary>
        public void Reset()
        {
            totalEditableArea = 0;
            totalInjusticeArea = 0;
            totalJusticeArea = 0;
            completionRate = 0f;
            ideologyBias = 0.5f;
        }
    }

    /// <summary>
    /// 游戏结局枚举
    /// </summary>
    public enum GameEnding
    {
        /// <summary>
        /// 结局1: 完成度不足60%，被解雇
        /// </summary>
        Fired = 1,
        
        /// <summary>
        /// 结局2: Justice倾向，你知道的太多了
        /// </summary>
        KnowTooMuch = 2,
        
        /// <summary>
        /// 结局3: 完美骑墙，跳槽N3 Media
        /// </summary>
        JumpShip = 3,
        
        /// <summary>
        /// 结局4: Injustice倾向，被加薪
        /// </summary>
        Promoted = 4
    }
}
