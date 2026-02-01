using System;
using Mask;
using UnityEngine;

public class ViewImageRenderer : MonoBehaviour
{
    private static readonly int MaskTex0   = Shader.PropertyToID("_MaskTex0");
    private static readonly int MaskTex1   = Shader.PropertyToID("_MaskTex1");
    private static readonly int ActiveMask = Shader.PropertyToID("_ActiveMask");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    [Header("Materials")]
    public Material maskPreviewMat;
    public Material paintMat;
    public Material resultMat;

    [Header("Render Textures")] 
    public Texture2D sourceImage;
    public RenderTexture maskRT0;
    public RenderTexture maskRT1;
    public RenderTexture outputRT;

    private ViewMode _mode = ViewMode.Paint;

    private Material _currentMat;
    
    private RenderTexture _sourceRT;

    private void Start()
    {
        UploadTexture(sourceImage);
    }

    private void Update()
    {
        Render();
    }

    public void SetViewMode(ViewMode mode)
    {
        _mode = mode;
        SwitchMaterial();
    }

    public void SetMaskMode(MaskMode mode)
    {
        paintMat.SetInt(ActiveMask, (int)mode);
    }

    private void SwitchMaterial()
    {
        _currentMat = _mode switch
        {
            ViewMode.MaskPreview => maskPreviewMat,
            ViewMode.Paint       => paintMat,
            ViewMode.Result      => resultMat,
            _ => paintMat
        };

        _currentMat.SetTexture(MaskTex0, maskRT0);
        _currentMat.SetTexture(MaskTex1, maskRT1);
        _currentMat.SetTexture(MainTex, _sourceRT);
    }

    private void Render()
    {
        Graphics.Blit(_sourceRT, outputRT, _currentMat);
    }

    private void UploadTexture(Texture2D tex)
    {
        if (_sourceRT == null ||
            _sourceRT.width != tex.width ||
            _sourceRT.height != tex.height)
        {
            if (_sourceRT != null)
                _sourceRT.Release();

            _sourceRT = CreateSourceRT(tex.width, tex.height);
        }

        Graphics.Blit(tex, _sourceRT);
    }

    private static RenderTexture CreateSourceRT(int w, int h)
    {
        var rt = new RenderTexture(
            w, h, 0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear
        )
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        rt.Create();

        return rt;
    }
}