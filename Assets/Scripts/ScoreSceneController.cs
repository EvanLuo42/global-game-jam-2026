using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TouchPhase = UnityEngine.TouchPhase;

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
    
    private bool _canProceed = false;
    private bool _hasClicked = false;

    private bool _pressed = false;

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
        
        AudioManager.Instance?.FadeToBGM(endingBGM);

        // 显示当前关卡分数
        DisplayLevelScore(result);
        
        // 显示累计分数
        DisplayCumulativeScore();

        // 处理结局
        if (_isGameComplete)
        {
            ShowEnding();
        }

        // 设置按钮文本
        UpdateButtonText();
        
        _canProceed = true;

    }
    
    private void GoToEnding()
    {
        TransitionController.Instance.TransitionToScene("EndingScene");
    }
    
    private void Update()
    {
        if (!_canProceed || _hasClicked) return;
        var mouse = Mouse.current;
        if (!_pressed && mouse.leftButton.isPressed)
        {
            TryGoToNextLevelOrEnding();
            _pressed = true;
        }
    }

    /// <summary>
    /// 进入下一关的 Intro 或结局；只执行一次（防止 Update 与按钮点击同时触发导致关卡被推进两次）。
    /// </summary>
    private void TryGoToNextLevelOrEnding()
    {
        if (!_canProceed || _hasClicked || GlobalState.CurrentLevel == null) return;
        if (GlobalState.CurrentLevel.displayName ==
            "The Big Green Factory Serves Up a Healthy Future For the People")
        {
            GoToEnding();
            return;
        }

        GlobalState.CurrentLevel = GlobalState.CurrentLevel.nextLevel;
        TransitionController.Instance.TransitionToScene("IntroScene");
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
    
    private void Proceed()
    {
        _hasClicked = true;

        GoToEnding();
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
    /// 下一关按钮点击事件（与 Update 点击共用 TryGoToNextLevelOrEnding，避免同一次点击推进两次关卡）。
    /// </summary>
    public void OnClickNextLevel()
    {
        TryGoToNextLevelOrEnding();
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
