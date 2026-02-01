using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Fade")]
    public float defaultFadeTime = 0.8f;

    Coroutine _bgmFadeCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void FadeToBGM(AudioClip clip, float fadeTime = -1f)
    {
        if (fadeTime < 0) fadeTime = defaultFadeTime;

        if (_bgmFadeCoroutine != null)
            StopCoroutine(_bgmFadeCoroutine);

        _bgmFadeCoroutine = StartCoroutine(FadeBGMCoroutine(clip, fadeTime));
    }

    private IEnumerator FadeBGMCoroutine(AudioClip newClip, float fadeTime)
    {
        var startVol = bgmSource.volume;

        for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();

        if (!newClip)
        {
            _bgmFadeCoroutine = null;
            yield break;
        }

        bgmSource.clip = newClip;
        bgmSource.Play();

        for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, startVol, t / fadeTime);
            yield return null;
        }

        bgmSource.volume = startVol;
        _bgmFadeCoroutine = null;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }
    
    public void PlayLoopSFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource.isPlaying && sfxSource.clip == clip && sfxSource.loop)
            return;

        sfxSource.Stop();
        sfxSource.clip = clip;
        sfxSource.loop = true;
        sfxSource.volume = volume;
        sfxSource.Play();
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }
}
