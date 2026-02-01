using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 结算场景控制器
/// 显示当前关卡的结算结果，以及最终结局（如果是最后一关）
/// </summary>
public class ScoreSceneController : MonoBehaviour
{
    [Header("Level Score UI")]
    [Tooltip("报纸销量显示")]
    public TextMeshProUGUI soldCopies;  // 保持原变量名
    
    [Tooltip("领导满意度显示")]
    public TextMeshProUGUI bossSatisfaction;  // 保持原变量名
    
    [Tooltip("Justice值显示")]
    public TextMeshProUGUI morality;  // 保持原变量名

    [Header("Level Info")]
    public TextMeshProUGUI levelNameText;

    [Header("Cumulative Score UI (可选)")]
    public TextMeshProUGUI cumulativeCompletionText;
    public TextMeshProUGUI cumulativeBiasText;

    [Header("Ending UI")]
    [Tooltip("结局面板 (仅在最后一关显示)")]
    public GameObject endingPanel;
    
    [Tooltip("结局标题")]
    public TextMeshProUGUI endingTitleText;
    
    [Tooltip("结局描述")]
    public TextMeshProUGUI endingDescriptionText;

    [Header("Buttons")]
    public Button nextLevelButton;
    public TextMeshProUGUI nextLevelButtonText;

    [Header("Audio")]
    public AudioClip scoreSceneBGM;
    public AudioClip endingBGM;

    private bool _isGameComplete;

    private void Start()
    {
        // 获取当前关卡结果
        var result = GlobalState.ScoreResult;
        var level = GlobalState.CurrentLevel;
        
        // 检查是否为最后一关
        _isGameComplete = GlobalState.IsGameComplete;

        // 调试输出
        Debug.Log($"[ScoreScene] Sales Rate: {result.salesRate:P1}, Boss: {result.bossSatisfaction:P1}, Justice: {result.justiceValue:P1}");

        // 显示关卡名称
        if (levelNameText != null && level != null)
        {
            levelNameText.text = level.displayName;
        }

        // 显示当前关卡分数
        DisplayLevelScore(result);
        
        // 显示累计分数
        DisplayCumulativeScore();

        // 处理结局
        if (_isGameComplete)
        {
            ShowEnding();
            if (endingBGM != null)
                AudioManager.Instance?.PlayBGM(endingBGM, volume: 0.3f);
        }
        else
        {
            HideEnding();
            if (scoreSceneBGM != null)
                AudioManager.Instance?.PlayBGM(scoreSceneBGM, volume: 0.2f);
        }

        // 设置按钮文本
        UpdateButtonText();
    }

    /// <summary>
    /// 显示当前关卡分数
    /// </summary>
    private void DisplayLevelScore(ScoreResult result)
    {
        if (soldCopies != null)
        {
            soldCopies.text = $"{result.salesRate * 100:F1}%";
        }
        else
        {
            Debug.LogWarning("[ScoreScene] soldCopies TextMeshProUGUI is not assigned!");
        }

        if (bossSatisfaction != null)
        {
            bossSatisfaction.text = $"{result.bossSatisfaction * 100:F1}%";
        }
        else
        {
            Debug.LogWarning("[ScoreScene] bossSatisfaction TextMeshProUGUI is not assigned!");
        }

        if (morality != null)
        {
            morality.text = $"{result.justiceValue * 100:F1}%";
        }
        else
        {
            Debug.LogWarning("[ScoreScene] morality TextMeshProUGUI is not assigned!");
        }
    }

    /// <summary>
    /// 显示累计分数
    /// </summary>
    private void DisplayCumulativeScore()
    {
        var cumulative = GlobalState.CumulativeScore;

        if (cumulativeCompletionText != null)
        {
            cumulativeCompletionText.text = $"Total Completion: {cumulative.completionRate * 100:F1}%";
        }

        if (cumulativeBiasText != null)
        {
            var biasLabel = GetBiasLabel(cumulative.ideologyBias);
            cumulativeBiasText.text = $"Ideology Bias: {biasLabel}";
        }
    }

    /// <summary>
    /// 获取偏向的文字描述
    /// </summary>
    private string GetBiasLabel(float bias)
    {
        if (bias < 0.2f) return "Strong Justice";
        if (bias < 0.4f) return "Leaning Justice";
        if (bias <= 0.6f) return "Neutral";
        if (bias <= 0.8f) return "Leaning Obedient";
        return "Absolute Obedience";
    }

    /// <summary>
    /// 显示最终结局
    /// </summary>
    private void ShowEnding()
    {
        // 计算最终结局
        var ending = EndingCalculator.CalculateEnding(GlobalState.CumulativeScore);
        GlobalState.FinalEnding = ending;

        // 调试输出
        EndingCalculator.DebugPrintJudgment(GlobalState.CumulativeScore);

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
        }

        if (endingTitleText != null)
        {
            endingTitleText.text = EndingCalculator.GetEndingTitle(ending);
        }

        if (endingDescriptionText != null)
        {
            endingDescriptionText.text = EndingCalculator.GetEndingDescription(ending);
        }
    }

    /// <summary>
    /// 隐藏结局面板
    /// </summary>
    private void HideEnding()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 更新按钮文本
    /// </summary>
    private void UpdateButtonText()
    {
        if (nextLevelButtonText == null) return;

        if (_isGameComplete)
        {
            nextLevelButtonText.text = "Return to Title";
        }
        else
        {
            nextLevelButtonText.text = "Next Level";
        }
    }

    /// <summary>
    /// 下一关按钮点击事件
    /// </summary>
    public void OnClickNextLevel()
    {
        if (_isGameComplete)
        {
            // 游戏结束，返回标题
            GlobalState.ResetGameState();
            SceneManager.LoadScene("TitlePage");
        }
        else
        {
            // 进入下一关
            GlobalState.CurrentLevel = GlobalState.CurrentLevel.nextLevel;
            SceneManager.LoadScene("IntroScene");
        }
    }

    /// <summary>
    /// 重新开始按钮（可选）
    /// </summary>
    public void OnClickRestart()
    {
        GlobalState.ResetGameState();
        SceneManager.LoadScene("TitlePage");
    }
}
