using UnityEngine;
using UnityEngine.InputSystem;
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

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse.leftButton.isPressed) SceneManager.LoadScene("LevelScene");
    }

    private static void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene("LevelScene");
    }
}