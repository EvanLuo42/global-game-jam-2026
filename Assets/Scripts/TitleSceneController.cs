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
        // 确保游戏状态被重置
        GlobalState.ResetGameState();
        
        AudioManager.Instance?.FadeToBGM(titleBGM);
    }

    public void OnClickStartGame()
    {
        AudioManager.Instance?.PlaySFX(startGameAudio);
        
        // 重置游戏状态并设置初始关卡
        GlobalState.ResetGameState();
        GlobalState.CurrentLevel = initialLevel;
        TransitionController.Instance.TransitionToScene("IntroScene");
    }

    public void OnClickGallery()
    {
        AudioManager.Instance.PlaySFX(galleryAudio);
        TransitionController.Instance.TransitionToScene("GalleryScene");
    }
}