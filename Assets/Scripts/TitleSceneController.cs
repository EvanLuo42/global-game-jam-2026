using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneController : MonoBehaviour
{
    public LevelDefinition initialLevel;
    
    public void OnClickStartGame()
    {
        GlobalState.CurrentLevel = initialLevel;    
        SceneManager.LoadScene("IntroScene");
    }

    public void OnClickGallery()
    {
        SceneManager.LoadScene("GalleryScene");
    }
}
