using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Mask;

public class LevelController : MonoBehaviour
{
    private static readonly int MaskTex0 = Shader.PropertyToID("_MaskTex0");
    private static readonly int MaskTex1 = Shader.PropertyToID("_MaskTex1");
    private static readonly int ActiveMask = Shader.PropertyToID("_ActiveMask");

    public ViewImageRenderer viewImageRenderer;
    public MaskPainter maskPainter;
    
    [Header("UI Settings")]
    public UIDocument uiDocument;

    [Header("References")]
    public MonoBehaviour existingLevelScript;

    [Header("Render Textures")]
    public RenderTexture targetRT;

    // 元素引用
    private VisualElement _root;
    private VisualElement _sidebarStandard;
    private VisualElement _sidebarMaskTools;
    private VisualElement _layerPanel;
    private Image _viewport;
    private Button _btnSubmit;
    
    // 这里改成了 Button，之前是 Label 导致报错
    private Button _layerActiveBtn; 

    // 按钮列表
    private readonly List<Button> _effectButtons = new();
    
    // 当前选中的按钮
    private Button _currentSelectedBtn;

    private void OnEnable()
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
    }

    private void Start()
    {
        viewImageRenderer.SetViewMode(ViewMode.Result);
        _viewport.image = targetRT;
    }

    private void FindUIElements()
    {
        _sidebarStandard = _root.Q("SidebarStandard");
        _sidebarMaskTools = _root.Q("SidebarMaskTools");
        _layerPanel = _root.Q("LayerPanel");
        _viewport = _root.Q<Image>("ViewportImage");
        _btnSubmit = _root.Q<Button>("MainActionBtn");
        
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

        if (_btnSubmit != null) _btnSubmit.clicked += OnSubmitClicked;
    }

    private void OnEffectClicked(Button clickedBtn)
    {
        _currentSelectedBtn = _currentSelectedBtn == clickedBtn ? null : // 取消选中
            clickedBtn; // 选中新按钮

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
                break;
        }

        UpdateUIState();
    }

    private void OnSubmitClicked()
    {
        if (_currentSelectedBtn != null)
            Debug.Log($"应用效果: {_currentSelectedBtn.text}");
        else
            Debug.Log("提交整个关卡");
    }

    private void UpdateUIState()
    {
        // 按钮高亮处理
        foreach (var btn in _effectButtons)
        {
            if (btn == _currentSelectedBtn) btn.AddToClassList("btn-selected");
            else btn.RemoveFromClassList("btn-selected");
        }

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