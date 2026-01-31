using Mask;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MaskPainter : MonoBehaviour
{
    private static readonly int Center = Shader.PropertyToID("_Center");
    private static readonly int Radius = Shader.PropertyToID("_Radius");
    private static readonly int Softness = Shader.PropertyToID("_Softness");
    private static readonly int Channel = Shader.PropertyToID("_Channel");
    private static readonly int Erase = Shader.PropertyToID("_Value");
    private static readonly int Aspect = Shader.PropertyToID("_Aspect");

    [Header("Target")]
    public Image viewImage;

    public ViewImageController viewImageController;
    
    public RenderTexture maskRT0;
    public RenderTexture maskRT1;

    public RenderTexture tempRT0;
    public RenderTexture tempRT1;
    
    public Material painterMaterial;

    [Header("Brush")]
    [Range(0.001f, 0.5f)] public float radius = 0.05f;
    [Range(0.0f, 0.2f)] public float softness = 0.02f;

    private RectTransform _rect;

    public MaskMode maskMode = MaskMode.Lighten;
    public PainterMode painterMode = PainterMode.Paint;

    private readonly MaskRTBinding[] _bindings = new MaskRTBinding[8];

    private void Awake()
    {
        _rect = viewImage.GetComponent<RectTransform>();
        
        ClearAllMasks();

        _bindings[0] = new MaskRTBinding { MaskRT = maskRT0, TempRT = tempRT0, Channel = 0 };
        _bindings[1] = new MaskRTBinding { MaskRT = maskRT0, TempRT = tempRT0, Channel = 1 };
        _bindings[2] = new MaskRTBinding { MaskRT = maskRT0, TempRT = tempRT0, Channel = 2 };
        _bindings[3] = new MaskRTBinding { MaskRT = maskRT0, TempRT = tempRT0, Channel = 3 };
        
        _bindings[4] = new MaskRTBinding { MaskRT = maskRT1, TempRT = tempRT1, Channel = 0 };
        _bindings[5] = new MaskRTBinding { MaskRT = maskRT1, TempRT = tempRT1, Channel = 1 };
        _bindings[6] = new MaskRTBinding { MaskRT = maskRT1, TempRT = tempRT1, Channel = 2 };
        _bindings[7] = new MaskRTBinding { MaskRT = maskRT1, TempRT = tempRT1, Channel = 3 };
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

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (!mouse.leftButton.isPressed) return;
        if (TryGetUV(_rect, mouse.position.value, out var uv))
        {
            Paint(uv);
        }
    }

    private void Paint(Vector2 uv)
    {
        var binding = _bindings[(int) maskMode];

        painterMaterial.SetVector(Center, new Vector4(uv.x, uv.y, 0, 0));
        painterMaterial.SetFloat(Radius, radius);
        painterMaterial.SetFloat(Softness, softness);
        painterMaterial.SetInt(Channel, binding.Channel);
        painterMaterial.SetInt(Erase, (int) painterMode);
        var aspect = (float)binding.MaskRT.width / binding.MaskRT.height;
        painterMaterial.SetFloat(Aspect, aspect);

        Graphics.Blit(binding.MaskRT, binding.TempRT, painterMaterial);
        Graphics.Blit(binding.TempRT, binding.MaskRT);
    }

    private static bool TryGetUV(RectTransform rect, Vector2 screenPos, out Vector2 uv)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, null, out var local))
        {
            uv = default;
            return false;
        }

        var r = rect.rect;
        uv = new Vector2(
            (local.x - r.xMin) / r.width,
            (local.y - r.yMin) / r.height
        );

        return uv.x is >= 0 and <= 1 && uv.y is >= 0 and <= 1;
    }

    public void OnSwitchPainterMode(int index)
    {
        SwitchPainterMode((PainterMode) index);
    }

    private void SwitchPainterMode(PainterMode mode)
    {
        painterMode = mode;
    }

    public void OnSwitchMaskMode(int index)
    {
        SwitchMaskMode((MaskMode) index);
    }

    private void SwitchMaskMode(MaskMode mode)
    {
        maskMode = mode;
        viewImageController.OnSwitchMaskMode(mode);
    }
}