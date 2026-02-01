using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionController : MonoBehaviour
{
    public static TransitionController Instance { get; private set; }

    [Header("References")]
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float fadeOutDuration = 0.5f;
    public float fadeInDuration = 0.5f;

    private bool _isTransitioning;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 对外接口：淡出 → 加载 → 淡入
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        _isTransitioning = true;
        canvasGroup.blocksRaycasts = true;

        // Fade to black
        yield return Fade(1f, fadeOutDuration);

        // Load scene
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Fade from black
        yield return Fade(0f, fadeInDuration);

        canvasGroup.blocksRaycasts = false;
        _isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    public void SetBlackInstant()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// 立刻变亮
    /// </summary>
    public void SetClearInstant()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}
