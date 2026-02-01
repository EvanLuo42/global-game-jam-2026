using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    [Header("UI Settings")]
    public UIDocument uiDocument;

    [Header("References")]
    public MonoBehaviour existingLevelScript;

    // 元素引用
    private VisualElement _root;
    private VisualElement _sidebarStandard;
    private VisualElement _sidebarMaskTools;
    private VisualElement _layerPanel;
    private Button _btnSubmit;
    
    // 这里改成了 Button，之前是 Label 导致报错
    private Button _layerActiveBtn; 

    // 按钮列表
    private List<Button> _effectButtons = new List<Button>();
    
    // 当前选中的按钮
    private Button _currentSelectedBtn = null;

    void OnEnable()
    {
        if (uiDocument == null) 
            uiDocument = FindObjectOfType<UIDocument>();
        // if (uiDocument == null) { Debug.LogError("缺少 UIDocument!"); return; }

        _root = uiDocument.rootVisualElement;
        if (_root == null) return;

        FindUIElements();
        RegisterCallbacks();
        
        // 初始化界面
        UpdateUIState();
    }

    void FindUIElements()
    {
        _sidebarStandard = _root.Q("SidebarStandard");
        _sidebarMaskTools = _root.Q("SidebarMaskTools");
        _layerPanel = _root.Q("LayerPanel");
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

    void AddButtonToList(string btnName)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null) _effectButtons.Add(btn);
    }

    void RegisterCallbacks()
    {
        foreach (var btn in _effectButtons)
        {
            Button targetBtn = btn; 
            targetBtn.clicked += () => OnEffectClicked(targetBtn);
        }

        if (_btnSubmit != null) _btnSubmit.clicked += OnSubmitClicked;
    }

    void OnEffectClicked(Button clickedBtn)
    {
        if (_currentSelectedBtn == clickedBtn)
            _currentSelectedBtn = null; // 取消选中
        else
            _currentSelectedBtn = clickedBtn; // 选中新按钮

        UpdateUIState();
    }

    void OnSubmitClicked()
    {
        if (_currentSelectedBtn != null)
            Debug.Log($"应用效果: {_currentSelectedBtn.text}");
        else
            Debug.Log("提交整个关卡");
    }
    void UpdateUIState()
    {
        // 按钮高亮处理
        foreach (var btn in _effectButtons)
        {
            if (btn == _currentSelectedBtn) btn.AddToClassList("btn-selected");
            else btn.RemoveFromClassList("btn-selected");
        }

        bool isEditingMode = (_currentSelectedBtn != null);

        // 控制显隐
        if (isEditingMode)
        {
            // 此时显示工具栏 (Pen/Eraser)
            if(_sidebarMaskTools != null) _sidebarMaskTools.RemoveFromClassList("hidden");
            
            // 确保旧的侧边栏是隐藏的
            if(_sidebarStandard != null) _sidebarStandard.AddToClassList("hidden");

            // 显示图层面板
            if(_layerPanel != null) _layerPanel.RemoveFromClassList("hidden");
            
            if(_btnSubmit != null) _btnSubmit.text = "Apply";

            // 修改图层名字
            if (_layerActiveBtn != null) 
                _layerActiveBtn.text = $"Layer 2: {_currentSelectedBtn.text}";
        }
        else
        {
            
            // 这里把 Standard Sidebar 也隐藏了
            // 之前这里是 RemoveFromClassList("hidden")，现在改成 Add，让它消失
            if(_sidebarStandard != null) _sidebarStandard.AddToClassList("hidden");
            
            // 隐藏工具栏
            if(_sidebarMaskTools != null) _sidebarMaskTools.AddToClassList("hidden");
            
            // 隐藏图层面板
            if(_layerPanel != null) _layerPanel.AddToClassList("hidden");

            if(_btnSubmit != null) _btnSubmit.text = "Submit";
        }
    }
}