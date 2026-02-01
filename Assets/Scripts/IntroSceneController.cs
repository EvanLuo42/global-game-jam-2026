using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroSceneController : MonoBehaviour
{

    private void Start()
    {
        AudioManager.Instance.FadeToBGM(null);
    }
}