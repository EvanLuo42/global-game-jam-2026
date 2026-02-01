using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private UIDocument _doc;
    
    // UI 元素引用
    private VisualElement _standardSidebar;
    private VisualElement _maskToolsSidebar;
    private VisualElement _layerPanel;
    private Button _effectBtn1;
    private Button _mainActionBtn; // Submit/Apply 按钮

    private bool _isMaskMode = false;

    void OnEnable()
    {
        _doc = GetComponent<UIDocument>();
        var root = _doc.rootVisualElement;

        // 获取元素
        _standardSidebar = root.Q("SidebarStandard");
        _maskToolsSidebar = root.Q("SidebarMaskTools");
        _layerPanel = root.Q("LayerPanel");
        _effectBtn1 = root.Q<Button>("BtnEffect1");
        _mainActionBtn = root.Q<Button>("MainActionBtn");

        // 绑定事件
        if (_effectBtn1 != null)
            _effectBtn1.clicked += ToggleMaskMode;
            
        // 初始化状态
        UpdateUIState();
    }

    // 切换模式的核心逻辑
    void ToggleMaskMode()
    {
        _isMaskMode = !_isMaskMode;
        UpdateUIState();
    }

    void UpdateUIState()
    {
        if (_isMaskMode)
        {
            // Mask Mode
            
            // 隐藏普通侧边栏，显示工具栏
            _standardSidebar.style.display = DisplayStyle.None;
            _maskToolsSidebar.style.display = DisplayStyle.Flex;
            
            // 显示右侧图层面板
            _layerPanel.style.display = DisplayStyle.Flex;
            
            // 底部按钮改字
            _mainActionBtn.text = "Apply";

            // 高亮 Effect 1 按钮 (样式类切换)
            _effectBtn1.AddToClassList("btn-selected");
            
            // 可以在这里加具体的 Effect 逻辑： "Darken"
            _effectBtn1.text = "Effect 1:\nDarken"; 
        }
        else
        {
            // === 回到普通模式 (Standard Mode) ===
            
            _standardSidebar.style.display = DisplayStyle.Flex;
            _maskToolsSidebar.style.display = DisplayStyle.None;
            
            _layerPanel.style.display = DisplayStyle.None;
            
            _mainActionBtn.text = "Submit";
            
            _effectBtn1.RemoveFromClassList("btn-selected");
            _effectBtn1.text = "Effect 1";
        }
    }
}