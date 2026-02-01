using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 右上角 Boss 对话框：每句文本渐显 + 音频同时播放，之后文本渐隐。
/// 不做状态检测：音频播完后过 5 秒进入下一句 text 和 audio。
/// </summary>
public class BossDialogueOverlay : MonoBehaviour
{
    [Header("Fade")]
    [Tooltip("文本渐显时长")]
    public float textFadeInDuration = 0.4f;
    [Tooltip("文本渐隐时长")]
    public float textFadeOutDuration = 0.4f;
    [Tooltip("音频播完后等待多少秒进入下一句")]
    public float delayAfterAudioSeconds = 5f;

    [Header("Font (主对话与子对话共用)")]
    [Tooltip("拖入 Times New Roman 的 TMP Font Asset；留空则会尝试从 Resources/Fonts/TimesNewRoman SDF 加载")]
    public TMP_FontAsset dialogueFont;

    [Header("Layout (右上角对话框，缩小不挡画布)")]
    public float dialogueTextFontSize = 24f;
    [Tooltip("对话框宽度占屏幕比例")]
    [Range(0.1f, 0.4f)]
    public float panelWidthRatio = 0.24f;
    [Tooltip("距离上边缘的比例 (0-1)")]
    [Range(0.02f, 0.25f)]
    public float panelTopRatio = 0.06f;
    [Tooltip("对话框高度占屏幕比例")]
    [Range(0.1f, 0.35f)]
    public float panelHeightRatio = 0.18f;
    [Tooltip("背景半透明黑")]
    public Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.88f);

    [Header("子对话 (如 Lighten 提示，显示在主对话下方)")]
    [Tooltip("子对话与主对话之间的间距（约几行）")]
    public float subLineSpacingRatio = 0.11f;
    public float subLineFontSize = 20f;

    private Canvas _canvas;
    private RectTransform _panelRect;
    private CanvasGroup _textGroup;
    private TextMeshProUGUI _dialogueText;
    private CanvasGroup _subLineGroup;
    private TextMeshProUGUI _subLineText;
    private AudioSource _voiceSource;
    private AudioSource _subLineVoiceSource;

    private BossDialogueData _data;
    private int _index;
    private System.Action _onFinished;
    private Coroutine _lineCoroutine;
    private Coroutine _subLineCoroutine;

    /// <summary> 是否正在显示对话 </summary>
    public bool IsShowing { get; private set; }

    private void Awake()
    {
        BuildUI();
        gameObject.SetActive(false);
    }

    private void BuildUI()
    {
        if (_canvas != null) return;
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 9999;
        _canvas.pixelPerfect = false;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // 右上角面板
        var panelGo = new GameObject("BossDialoguePanel");
        panelGo.transform.SetParent(transform, false);
        _panelRect = panelGo.AddComponent<RectTransform>();
        _panelRect.anchorMin = new Vector2(1f - panelWidthRatio, 1f - panelTopRatio - panelHeightRatio);
        _panelRect.anchorMax = new Vector2(1f - 0.02f, 1f - panelTopRatio);
        _panelRect.offsetMin = Vector2.zero;
        _panelRect.offsetMax = Vector2.zero;

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = panelBackgroundColor;
        panelImage.raycastTarget = false; // 不阻挡射线，玩家始终能在画布上画画

        // 主对话文本（上方，下方留空给子对话）
        var textGo = new GameObject("DialogueText");
        textGo.transform.SetParent(panelGo.transform, false);
        _textGroup = textGo.AddComponent<CanvasGroup>();
        _textGroup.alpha = 0f;
        _dialogueText = textGo.AddComponent<TextMeshProUGUI>();
        _dialogueText.fontSize = dialogueTextFontSize;
        _dialogueText.color = Color.white;
        _dialogueText.alignment = TextAlignmentOptions.TopLeft;
        _dialogueText.enableWordWrapping = true;
        _dialogueText.overflowMode = TextOverflowModes.Overflow;
        _dialogueText.raycastTarget = false;
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, subLineSpacingRatio);
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16, 16);
        textRect.offsetMax = new Vector2(-16, -16);

        // 子对话文本（主对话下方，约空两行；如 Lighten 提示，不打断主对话）
        var subGo = new GameObject("SubLineText");
        subGo.transform.SetParent(panelGo.transform, false);
        _subLineGroup = subGo.AddComponent<CanvasGroup>();
        _subLineGroup.alpha = 0f;
        _subLineText = subGo.AddComponent<TextMeshProUGUI>();
        _subLineText.fontSize = subLineFontSize;
        _subLineText.color = new Color(0.9f, 0.9f, 0.85f, 1f);
        _subLineText.alignment = TextAlignmentOptions.TopLeft;
        _subLineText.enableWordWrapping = true;
        _subLineText.overflowMode = TextOverflowModes.Overflow;
        _subLineText.raycastTarget = false;
        var subRect = subGo.GetComponent<RectTransform>();
        subRect.anchorMin = Vector2.zero;
        subRect.anchorMax = new Vector2(1f, subLineSpacingRatio - 0.02f);
        subRect.offsetMin = new Vector2(16, 8);
        subRect.offsetMax = new Vector2(-16, -8);

        _voiceSource = gameObject.AddComponent<AudioSource>();
        _voiceSource.playOnAwake = false;
        _voiceSource.loop = false;
        _subLineVoiceSource = gameObject.AddComponent<AudioSource>();
        _subLineVoiceSource.playOnAwake = false;
        _subLineVoiceSource.loop = false;

        ApplyFontToTexts();
    }

    private void ApplyFontToTexts()
    {
        var font = dialogueFont != null ? dialogueFont : Resources.Load<TMP_FontAsset>("Fonts/TimesNewRoman SDF");
        if (font == null) return;
        if (_dialogueText != null) _dialogueText.font = font;
        if (_subLineText != null) _subLineText.font = font;
    }

    /// <summary>
    /// 在主对话下方显示一句子对话（如点击 Lighten 时），不打断主对话；用独立区域和独立音轨。
    /// </summary>
    public void ShowSubLine(string text, AudioClip voiceClip)
    {
        if (string.IsNullOrEmpty(text) && voiceClip == null) return;
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        if (_subLineCoroutine != null) StopCoroutine(_subLineCoroutine);
        _subLineCoroutine = StartCoroutine(PlaySubLineSequence(text, voiceClip));
    }

    /// <summary>
    /// 显示单句对话（无主对话时用）：一句文案 + 一句语音，播完后渐隐。
    /// </summary>
    public void ShowSingleLine(string text, AudioClip voiceClip, System.Action onFinished = null)
    {
        if (_lineCoroutine != null) StopCoroutine(_lineCoroutine);
        _lineCoroutine = StartCoroutine(PlaySingleLineSequence(text, voiceClip, onFinished));
    }

    /// <summary>
    /// 显示 Boss 对话；每句音频播完后过 5 秒进入下一句；全部结束后调用 onFinished。
    /// </summary>
    public void ShowDialogue(BossDialogueData data, System.Action onFinished)
    {
        if (data == null || !data.HasEntries)
        {
            onFinished?.Invoke();
            return;
        }

        _data = data;
        _onFinished = onFinished;
        _index = 0;
        gameObject.SetActive(true);
        IsShowing = true;
        _textGroup.alpha = 0f;
        _dialogueText.text = "";

        if (_lineCoroutine != null) StopCoroutine(_lineCoroutine);
        _lineCoroutine = StartCoroutine(PlayLineSequence());
    }

    private IEnumerator PlaySingleLineSequence(string text, AudioClip voiceClip, System.Action onFinished)
    {
        gameObject.SetActive(true);
        IsShowing = true;
        _dialogueText.text = text ?? "";
        _textGroup.alpha = 0f;

        if (voiceClip != null)
        {
            _voiceSource.Stop();
            _voiceSource.clip = voiceClip;
            _voiceSource.Play();
        }

        for (float t = 0; t < textFadeInDuration; t += Time.unscaledDeltaTime)
        {
            _textGroup.alpha = Mathf.Clamp01(t / textFadeInDuration);
            yield return null;
        }
        _textGroup.alpha = 1f;

        float audioLength = voiceClip != null ? voiceClip.length : 0f;
        float waitTotal = audioLength + delayAfterAudioSeconds;
        yield return new WaitForSecondsRealtime(waitTotal);

        for (float t = 0; t < textFadeOutDuration; t += Time.unscaledDeltaTime)
        {
            _textGroup.alpha = 1f - Mathf.Clamp01(t / textFadeOutDuration);
            yield return null;
        }
        _textGroup.alpha = 0f;
        if (_voiceSource != null && _voiceSource.isPlaying)
            _voiceSource.Stop();

        _lineCoroutine = null;
        gameObject.SetActive(false);
        IsShowing = false;
        onFinished?.Invoke();
    }

    private IEnumerator PlaySubLineSequence(string text, AudioClip voiceClip)
    {
        _subLineText.text = text ?? "";
        _subLineGroup.alpha = 0f;

        if (voiceClip != null)
        {
            _subLineVoiceSource.Stop();
            _subLineVoiceSource.clip = voiceClip;
            _subLineVoiceSource.Play();
        }

        for (float t = 0; t < textFadeInDuration; t += Time.unscaledDeltaTime)
        {
            _subLineGroup.alpha = Mathf.Clamp01(t / textFadeInDuration);
            yield return null;
        }
        _subLineGroup.alpha = 1f;

        float audioLength = voiceClip != null ? voiceClip.length : 0f;
        float waitTotal = audioLength + Mathf.Min(2f, delayAfterAudioSeconds);
        yield return new WaitForSecondsRealtime(waitTotal);

        for (float t = 0; t < textFadeOutDuration; t += Time.unscaledDeltaTime)
        {
            _subLineGroup.alpha = 1f - Mathf.Clamp01(t / textFadeOutDuration);
            yield return null;
        }
        _subLineGroup.alpha = 0f;
        _subLineText.text = "";
        if (_subLineVoiceSource != null && _subLineVoiceSource.isPlaying)
            _subLineVoiceSource.Stop();

        _subLineCoroutine = null;
    }

    private IEnumerator PlayLineSequence()
    {
        while (true)
        {
            if (_data == null || _index < 0 || _index >= _data.entries.Length)
            {
                CloseAndFinish();
                yield break;
            }

            var entry = _data.entries[_index];
            _dialogueText.text = entry.text;
            _textGroup.alpha = 0f;

            // 渐显 + 同时播音频
            if (entry.voiceClip != null)
            {
                _voiceSource.Stop();
                _voiceSource.clip = entry.voiceClip;
                _voiceSource.Play();
            }

            for (float t = 0; t < textFadeInDuration; t += Time.unscaledDeltaTime)
            {
                _textGroup.alpha = Mathf.Clamp01(t / textFadeInDuration);
                yield return null;
            }
            _textGroup.alpha = 1f;

            // 等待：音频时长 + 5 秒，然后进入下一句
            float audioLength = entry.voiceClip != null ? entry.voiceClip.length : 0f;
            float waitTotal = audioLength + delayAfterAudioSeconds;
            yield return new WaitForSecondsRealtime(waitTotal);

            // 渐隐
            for (float t = 0; t < textFadeOutDuration; t += Time.unscaledDeltaTime)
            {
                _textGroup.alpha = 1f - Mathf.Clamp01(t / textFadeOutDuration);
                yield return null;
            }
            _textGroup.alpha = 0f;
            if (_voiceSource != null && _voiceSource.isPlaying)
                _voiceSource.Stop();

            _index++;
        }
    }

    private void CloseAndFinish()
    {
        _lineCoroutine = null;
        gameObject.SetActive(false);
        IsShowing = false;
        _onFinished?.Invoke();
        _onFinished = null;
    }
}
