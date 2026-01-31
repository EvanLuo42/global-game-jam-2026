using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroSceneController : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private void Start()
    {
        videoPlayer.clip = GlobalState.CurrentLevel.introVideo;
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    private static void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene("LevelScene");
    }
}