using Mask;
using UnityEngine;
using UnityEngine.UI;

public class ViewImageController : MonoBehaviour
{
    private static readonly int MaskTex0 = Shader.PropertyToID("_MaskTex0");
    private static readonly int MaskTex1 = Shader.PropertyToID("_MaskTex1");
    private static readonly int ActiveMask = Shader.PropertyToID("_ActiveMask");
    private Image _image;

    public Material maskPreviewMat;
    public Material paintMat;
    public Material resultMat;

    public RenderTexture maskRT0;
    public RenderTexture maskRT1;

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    public void OnSwitchMode(int index)
    {
        SwitchMode((ViewMode) index);
    }

    private void SwitchMode(ViewMode mode)
    {
        _image.material = mode switch
        {
            ViewMode.MaskPreview => maskPreviewMat,
            ViewMode.Paint       => paintMat,
            ViewMode.Result      => resultMat,
            _ => _image.material
        };
        
        _image.material.SetTexture(MaskTex0, maskRT0);
        _image.material.SetTexture(MaskTex1, maskRT1);
    }

    public void OnSwitchMaskMode(MaskMode mode)
    {
        if (_image.material == paintMat)
        {
            _image.material.SetInt(ActiveMask, (int) mode);
        }
    }
}
