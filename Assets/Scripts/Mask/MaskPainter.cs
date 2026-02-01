using Mask;
using UnityEngine;
using UnityEngine.UIElements;

public class MaskPainter : MonoBehaviour
{
    private static readonly int Center  = Shader.PropertyToID("_Center");
    private static readonly int Radius  = Shader.PropertyToID("_Radius");
    private static readonly int Softness= Shader.PropertyToID("_Softness");
    private static readonly int Channel = Shader.PropertyToID("_Channel");
    private static readonly int Erase   = Shader.PropertyToID("_Value");
    private static readonly int Aspect  = Shader.PropertyToID("_Aspect");

    [Header("UI Settings")]
    public UIDocument uiDocument;

    [Header("Audios")] 
    public AudioClip pencil;

    private Image _viewport;
    private VisualElement _root;

    public ViewImageRenderer viewImageController;

    [Header("Mask RTs")]
    public RenderTexture maskRT0;
    public RenderTexture maskRT1;

    public RenderTexture tempRT0;
    public RenderTexture tempRT1;

    [Header("Materials")]
    public Material painterMaterial;

    [Header("Brush")]
    [Range(0.001f, 0.5f)] public float radius = 0.05f;
    [Range(0.0f, 0.2f)] public float softness = 0.02f;

    public MaskMode maskMode = MaskMode.Lighten;
    public PainterMode painterMode = PainterMode.Paint;

    private readonly MaskRTBinding[] _bindings = new MaskRTBinding[8];

    private bool _isPainting;
    private int _activePointerId = -1;

    private void Awake()
    {
        ClearAllMasks();

        _bindings[0] = new MaskRTBinding { MaskRT = maskRT0, TempRT = tempRT0, Channel = 0 };
        _bindings[1] = new MaskRTBinding { MaskRT = maskRT0, TempRT = tempRT0, Channel = 1 };
        _bindings[2] = new MaskRTBinding { MaskRT = maskRT0, TempRT = tempRT0, Channel = 2 };
        _bindings[3] = new MaskRTBinding { MaskRT = maskRT0, TempRT = tempRT0, Channel = 3 };

        _bindings[4] = new MaskRTBinding { MaskRT = maskRT1, TempRT = tempRT1, Channel = 0 };
        _bindings[5] = new MaskRTBinding { MaskRT = maskRT1, TempRT = tempRT1, Channel = 1 };
        _bindings[6] = new MaskRTBinding { MaskRT = maskRT1, TempRT = tempRT1, Channel = 2 };
        _bindings[7] = new MaskRTBinding { MaskRT = maskRT1, TempRT = tempRT1, Channel = 3 };

        _root = uiDocument.rootVisualElement;
        _viewport = _root.Q<Image>("ViewportImage");
    }

    private void OnEnable()
    {
        if (_viewport == null) return;

        // Pointer events (UI Toolkit)
        _viewport.RegisterCallback<PointerDownEvent>(OnPointerDown);
        _viewport.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        _viewport.RegisterCallback<PointerUpEvent>(OnPointerUp);
        _viewport.RegisterCallback<PointerCancelEvent>(OnPointerCancel);

        // 允许接收指针事件（有时很关键）
        _viewport.pickingMode = PickingMode.Position;
    }

    private void OnDisable()
    {
        if (_viewport == null) return;

        _viewport.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        _viewport.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        _viewport.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        _viewport.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);

        _isPainting = false;
        _activePointerId = -1;
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button != (int)MouseButton.LeftMouse) return;

        _isPainting = true;
        _activePointerId = evt.pointerId;

        _viewport.CapturePointer(_activePointerId);
        evt.StopPropagation();

        TryPaintAt(evt.position);
        
        AudioManager.Instance.PlayLoopSFX(pencil);
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_isPainting) return;
        if (evt.pointerId != _activePointerId) return;
        if (!_viewport.HasPointerCapture(_activePointerId)) return;

        evt.StopPropagation();
        TryPaintAt(evt.position);
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.pointerId != _activePointerId) return;

        _isPainting = false;
        evt.StopPropagation();

        if (_viewport.HasPointerCapture(_activePointerId))
            _viewport.ReleasePointer(_activePointerId);

        _activePointerId = -1;
        
        AudioManager.Instance.StopSFX();
    }

    private void OnPointerCancel(PointerCancelEvent evt)
    {
        _isPainting = false;

        if (_activePointerId != -1 && _viewport.HasPointerCapture(_activePointerId))
            _viewport.ReleasePointer(_activePointerId);

        _activePointerId = -1;
    }

    private void TryPaintAt(Vector2 pointerPosPanelSpace)
    {
        if (_viewport == null) return;

        if (TryGetUV(_viewport, pointerPosPanelSpace, out var uv))
        {
            Paint(uv);
        }
    }

    private void Paint(Vector2 uv)
    {
        var binding = _bindings[(int)maskMode];
        if (binding.MaskRT == null || binding.TempRT == null || painterMaterial == null) return;

        painterMaterial.SetVector(Center, new Vector4(uv.x, uv.y, 0, 0));
        painterMaterial.SetFloat(Radius, radius);
        painterMaterial.SetFloat(Softness, softness);
        painterMaterial.SetInt(Channel, binding.Channel);
        painterMaterial.SetInt(Erase, (int)painterMode);

        var aspect = (float)binding.MaskRT.width / binding.MaskRT.height;
        painterMaterial.SetFloat(Aspect, aspect);

        Graphics.Blit(binding.MaskRT, binding.TempRT, painterMaterial);
        Graphics.Blit(binding.TempRT, binding.MaskRT);
    }

    private void ClearAllMasks()
    {
        ClearRT(maskRT0);
        ClearRT(maskRT1);
        ClearRT(tempRT0);
        ClearRT(tempRT1);
    }

    private static void ClearRT(RenderTexture rt)
    {
        if (rt == null) return;

        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(false, true, Color.clear);
        RenderTexture.active = prev;
    }

    /// <summary>
    /// UI Toolkit: pointer position 和 worldBound 都是 panel space 坐标
    /// worldBound 原点在左上，所以 uv.y 需要翻转成“左下为原点”的 UV
    /// </summary>
    private static bool TryGetUV(VisualElement element, Vector2 pointerPosPanelSpace, out Vector2 uv)
    {
        var r = element.worldBound;

        if (!r.Contains(pointerPosPanelSpace))
        {
            uv = default;
            return false;
        }

        float u = (pointerPosPanelSpace.x - r.xMin) / r.width;
        float v = 1.0f - (pointerPosPanelSpace.y - r.yMin) / r.height; // Y flip

        // 防御性 clamp（避免边缘浮点误差）
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);

        uv = new Vector2(u, v);
        return true;
    }

    public void OnSwitchPainterMode(int index) => SwitchPainterMode((PainterMode)index);

    private void SwitchPainterMode(PainterMode mode) => painterMode = mode;

    public void OnSwitchMaskMode(int index) => SwitchMaskMode((MaskMode)index);

    private void SwitchMaskMode(MaskMode mode)
    {
        maskMode = mode;
        viewImageController?.SetMaskMode(mode);
    }
}
