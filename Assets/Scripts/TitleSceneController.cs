using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneController : MonoBehaviour
{
    public LevelDefinition initialLevel;

    [Header("Audio")]
    public AudioClip titleBGM;
    public AudioClip startGameAudio;
    public AudioClip galleryAudio;

    private void Start()
    {
        AudioManager.Instance.FadeToBGM(titleBGM);
    }

    public void OnClickStartGame()
    {
        AudioManager.Instance.PlaySFX(startGameAudio);
        GlobalState.CurrentLevel = initialLevel;
        SceneManager.LoadScene("IntroScene");
    }

    public void OnClickGallery()
    {
        AudioManager.Instance.PlaySFX(galleryAudio);
        SceneManager.LoadScene("GalleryScene");
    }
}