using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Mask;

public class LevelController : MonoBehaviour
{
    public ViewImageRenderer viewImageRenderer;
    public MaskPainter maskPainter;
    
    [Header("UI Settings")]
    public UIDocument uiDocument;

    [Header("References")]
    public MonoBehaviour existingLevelScript;

    [Header("Render Textures")]
    public RenderTexture targetRT;

    [Header("Effect Audios")] 
    public AudioClip effectSwitch;
    public AudioClip paintEraser;
    public AudioClip effectApplyBack;
    public AudioClip levelBgm;

    [Header("Boss Dialogue (可选)")]
    [Tooltip("不赋值则自动查找或创建")]
    public BossDialogueOverlay bossDialogueOverlay;

    // 元素引用
    private VisualElement _root;
    private VisualElement _sidebarStandard;
    private VisualElement _sidebarMaskTools;
    private VisualElement _layerPanel;
    private Image _viewport;
    private Button _btnSubmit;
    private Button _paintButton;
    private Button _eraseButton;
    
    // 这里改成了 Button，之前是 Label 导致报错
    private Button _layerActiveBtn; 

    // 按钮列表
    private readonly List<Button> _effectButtons = new();
    
    // 当前选中的按钮
    private Button _currentSelectedBtn;

    private PainterMode _currentPainterMode = PainterMode.Paint;

    private bool _playAudio = false;

    private void Start()
    {
        if (uiDocument == null) 
            uiDocument = FindFirstObjectByType<UIDocument>();
        // if (uiDocument == null) { Debug.LogError("缺少 UIDocument!"); return; }

        _root = uiDocument.rootVisualElement;
        if (_root == null) return;

        FindUIElements();
        RegisterCallbacks();
        
        // 初始化界面
        UpdateUIState();
        
        viewImageRenderer.SetViewMode(ViewMode.Result);
        _viewport.image = targetRT;

        // 所有关卡：若有 Boss 对话则在右上角播放（前 N 句自动，其余需点 Apply 进入下一句）
        TryPlayBossDialogue();
    }

    private void TryPlayBossDialogue()
    {
        var level = GlobalState.CurrentLevel;
        if (level?.bossDialogue == null || !level.bossDialogue.HasEntries) return;

        var overlay = bossDialogueOverlay != null
            ? bossDialogueOverlay
            : FindFirstObjectByType<BossDialogueOverlay>();
        if (overlay == null)
        {
            var go = new GameObject("BossDialogueOverlay");
            overlay = go.AddComponent<BossDialogueOverlay>();
        }

        overlay.ShowDialogue(level.bossDialogue, OnBossDialogueFinished);
    }

    private void OnBossDialogueFinished() { }

    private void TryShowLightenToolDialogue()
    {
        var level = GlobalState.CurrentLevel;
        if (level == null) return;
        bool hasText = !string.IsNullOrEmpty(level.lightenToolDialogueText);
        bool hasVoice = level.lightenToolDialogueVoice != null;
        if (!hasText && !hasVoice) return;

        var overlay = bossDialogueOverlay != null
            ? bossDialogueOverlay
            : FindFirstObjectByType<BossDialogueOverlay>();
        if (overlay == null)
        {
            var go = new GameObject("BossDialogueOverlay");
            overlay = go.AddComponent<BossDialogueOverlay>();
        }
        overlay.ShowSubLine(level.lightenToolDialogueText, level.lightenToolDialogueVoice);
    }

    private void Update()
    {
        if (_playAudio) return;
        AudioManager.Instance.PlayBGM(levelBgm, volume: 0.1f);
        _playAudio = true;
    }

    private void FindUIElements()
    {
        _sidebarStandard = _root.Q("SidebarStandard");
        _sidebarMaskTools = _root.Q("SidebarMaskTools");
        _layerPanel = _root.Q("LayerPanel");
        _viewport = _root.Q<Image>("ViewportImage");
        _btnSubmit = _root.Q<Button>("MainActionBtn");
        _paintButton = _root.Q<Button>("Paint");
        _eraseButton = _root.Q<Button>("Erase");
        
        // 获取图层按钮，并赋值给 Button 类型的变量
        _layerActiveBtn = _root.Q<Button>(className: "layer-active");

        // 获取4个效果按钮
        _effectButtons.Clear();
        AddButtonToList("BtnDarken");
        AddButtonToList("BtnBlur");
        AddButtonToList("BtnNegative");
        AddButtonToList("BtnLighten");
    }

    private void AddButtonToList(string btnName)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null) _effectButtons.Add(btn);
    }

    private void RegisterCallbacks()
    {
        foreach (var btn in _effectButtons)
        {
            var targetBtn = btn; 
            targetBtn.clicked += () => OnEffectClicked(targetBtn);
        }

        _paintButton.clicked += OnPaintClicked;
        _eraseButton.clicked += OnEraseClicked;

        if (_btnSubmit != null) _btnSubmit.clicked += OnSubmitClicked;
    }

    private void OnPaintClicked()
    {
        AudioManager.Instance.PlaySFX(paintEraser);
        _currentPainterMode = PainterMode.Paint;
        maskPainter.OnSwitchPainterMode((int)PainterMode.Paint);
        UpdateUIState();
    }
    
    private void OnEraseClicked()
    {
        AudioManager.Instance.PlaySFX(paintEraser);
        _currentPainterMode = PainterMode.Erase;
        maskPainter.OnSwitchPainterMode((int)PainterMode.Erase);
        UpdateUIState();
    }

    private void OnEffectClicked(Button clickedBtn)
    {
        _currentSelectedBtn = _currentSelectedBtn == clickedBtn ? null : // 取消选中
            clickedBtn; // 选中新按钮
        
        AudioManager.Instance.PlaySFX(effectSwitch);

        var _maskPainter = FindFirstObjectByType<MaskPainter>();
        switch (clickedBtn.name)
        {
            case "BtnDarken":
                viewImageRenderer.SetMaskMode(MaskMode.Darken);
                _maskPainter.OnSwitchMaskMode((int) MaskMode.Darken);
                break;
            case "BtnBlur":
                viewImageRenderer.SetMaskMode(MaskMode.Blur);
                _maskPainter.OnSwitchMaskMode((int) MaskMode.Blur);
                break;
            case "BtnNegative":
                viewImageRenderer.SetMaskMode(MaskMode.Invert);
                _maskPainter.OnSwitchMaskMode((int) MaskMode.Invert);
                break;
            case "BtnLighten":
                viewImageRenderer.SetMaskMode(MaskMode.Lighten);
                _maskPainter.OnSwitchMaskMode((int) MaskMode.Lighten);
                TryShowLightenToolDialogue();
                break;
        }

        UpdateUIState();
    }

    private void OnSubmitClicked()
    {
        AudioManager.Instance.PlaySFX(effectApplyBack);
        FindFirstObjectByType<ScoreCalculator>().CalculateScore();
    }

    private void UpdateUIState()
    {
        // 按钮高亮处理
        foreach (var btn in _effectButtons)
        {
            if (btn == _currentSelectedBtn) btn.AddToClassList("btn-selected");
            else btn.RemoveFromClassList("btn-selected");
        }
        
        if (_currentPainterMode == PainterMode.Paint)
            _paintButton.AddToClassList("btn-selected");
        else
            _paintButton.RemoveFromClassList("btn-selected");

        if (_currentPainterMode == PainterMode.Erase)
            _eraseButton.AddToClassList("btn-selected");
        else
            _eraseButton.RemoveFromClassList("btn-selected");

        var isEditingMode = _currentSelectedBtn != null;

        // 控制显隐
        if (isEditingMode)
        {
            // 此时显示工具栏 (Pen/Eraser)
            _sidebarMaskTools?.RemoveFromClassList("hidden");

            // 确保旧的侧边栏是隐藏的
            _sidebarStandard?.AddToClassList("hidden");

            // 显示图层面板
            _layerPanel?.RemoveFromClassList("hidden");

            if(_btnSubmit != null) _btnSubmit.text = "Apply";

            // 修改图层名字
            if (_layerActiveBtn != null) 
                _layerActiveBtn.text = $"Layer 2: {_currentSelectedBtn.text}";
        }
        else
        {
            
            // 这里把 Standard Sidebar 也隐藏了
            // 之前这里是 RemoveFromClassList("hidden")，现在改成 Add，让它消失
            _sidebarStandard?.AddToClassList("hidden");

            // 隐藏工具栏
            _sidebarMaskTools?.AddToClassList("hidden");

            // 隐藏图层面板
            _layerPanel?.AddToClassList("hidden");

            if(_btnSubmit != null) _btnSubmit.text = "Submit";
        }
    }
}